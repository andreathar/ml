/*
 * HotReloadHandler.cs
 *
 * This script provides hot reload functionality between a Unity project and VS Code/Cursor.
 * It creates a TCP server that listens for messages from the VS Code extension,
 * then triggers Unity's asset database to refresh when code changes are detected.
 *
 * The handler supports three main commands:
 * - Start: Starts the server on the default or next available port
 * - Stop: Stops the server and disconnects all clients
 * - Reload: Restarts the server on the same port (maintains connection stability)
 *
 * Copyright (c) 2025 Rank Up Games LLC
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.Compilation;
#endif

// Suppress Unity domain-reload analyzer warnings for this editor-only hot reload handler.
// The handler relies on Editor callbacks (InitializeOnLoad/static constructor and
// AssemblyReloadEvents) rather than runtime RuntimeInitializeOnLoadMethod semantics.
#pragma warning disable UDR0001, UDR0002, UDR0003, UDR0005

namespace MLCreator.Editor.Tools
{
    /// <summary>
    /// Editor window that handles hot reload functionality between Unity and VS Code/Cursor.
    /// Implements a TCP server to listen for file change notifications.
    /// </summary>
    [InitializeOnLoad]
    public class HotReloadHandler : EditorWindow
    {
        // TCP server and communication components
        private static TcpListener s_Server;
        private static Thread s_ListenerThread;
        private static bool s_ShouldRequestRefresh = false;
        private static int s_CurrentPort = 55500; // Current port being used
        private static int s_LastSuccessfulPort = 55500; // Last port that successfully connected
        private static readonly Queue<string> s_MessageQueue = new Queue<string>();
        private static bool s_IsInitialized = false;
        private static bool s_IsServerRunning = false;
        private static readonly List<TcpClient> s_ConnectedClients = new List<TcpClient>();
        private static readonly object s_ClientListLock = new object();

        // Port configuration
        private const int DEFAULT_PORT = 55500;
        private static readonly int[] ALTERNATIVE_PORTS = { 55500, 55501, 55502, 55503, 55504 };

        // Add retry configuration for port binding
        private const int PORT_RETRY_ATTEMPTS = 5;
        private const int PORT_RETRY_DELAY_MS = 200;

        // Debug control flag - determines whether connection logs are shown
        private static bool s_ShowDebugLogs = false;
        private const string DEBUG_PREF_KEY = "UnityHotReloadHandler_ShowDebugLogs";
        private const string LAST_PORT_PREF_KEY = "UnityHotReloadHandler_LastPort";

        // Used to check if we're already running
        private static Mutex s_InstanceMutex;
        private static bool s_WasRunningBeforeReload = false;
        private const string WAS_RUNNING_PREF_KEY = "UnityHotReloadHandler_WasRunning";

        private static readonly Queue<Action> s_MainThreadActions = new Queue<Action>();
        private static readonly object s_MainThreadActionsLock = new object();

        /// <summary>
        /// Static constructor called when Unity editor loads.
        /// Initializes the server and registers for editor events.
        /// </summary>
        static HotReloadHandler()
        {
            // Load debug setting from EditorPrefs
            s_ShowDebugLogs = EditorPrefs.GetBool(DEBUG_PREF_KEY, false);
            s_LastSuccessfulPort = EditorPrefs.GetInt(LAST_PORT_PREF_KEY, DEFAULT_PORT);
            s_CurrentPort = s_LastSuccessfulPort;

            // Check if we were running before domain reload
            s_WasRunningBeforeReload = EditorPrefs.GetBool(WAS_RUNNING_PREF_KEY, false);

            // Register for domain reload completion to restart the server
            EditorApplication.update += OnEditorUpdate;

            // Register shutdown handler
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;

            // Auto-start on Unity load
            EditorApplication.delayCall += () =>
            {
                if (!s_IsInitialized && s_WasRunningBeforeReload)
                {
                    // Clear the flag
                    EditorPrefs.SetBool(WAS_RUNNING_PREF_KEY, false);
                    StartWithoutMutex();
                }
            };
        }

        /// <summary>
        /// Starts the hot reload server if not already running.
        /// Creates a mutex to ensure only one instance is active.
        /// </summary>
        [MenuItem("Tools/Hot Reload/Start")]
        public static void Start()
        {
            if (s_IsInitialized && s_IsServerRunning)
            {
                Debug.Log("Hot Reload server is already running.");
                return;
            }

            // Try to create or open existing mutex
            bool createdNew = false;
            try
            {
                s_InstanceMutex = new Mutex(true, "UnityHotReloadHandler", out createdNew);

                if (!createdNew)
                {
                    // Try to get ownership of existing mutex with a timeout
                    try
                    {
                        if (s_InstanceMutex.WaitOne(100))
                        {
                            // We got ownership, likely from a crashed instance
                            createdNew = true;
                        }
                    }
                    catch
                    {
                        // Mutex is abandoned or we can't get it
                        createdNew = false;
                    }
                }
            }
            catch (AbandonedMutexException)
            {
                // The mutex was abandoned, we can take ownership
                createdNew = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Mutex error: {ex.Message}. Starting without mutex protection.");
                s_InstanceMutex = null;
                createdNew = true; // Proceed anyway
            }

            if (!createdNew && s_InstanceMutex != null)
            {
                // Check if a server is actually running on our port
                if (!IsPortInUse(s_CurrentPort))
                {
                    Debug.Log(
                        "Previous instance mutex found but port is free. Proceeding with startup."
                    );
                    try
                    {
                        s_InstanceMutex.ReleaseMutex();
                        s_InstanceMutex.Close();
                    }
                    catch { }
                    s_InstanceMutex = null;
                    createdNew = true;
                }
                else
                {
                    Debug.Log(
                        "Another instance of Hot Reload server is already running. Skipping initialization."
                    );
                    s_InstanceMutex.Close();
                    s_InstanceMutex = null;
                    return;
                }
            }

            Debug.Log("Starting Unity Hot Reload server...");

            // Start TCP listener thread
            StartListenerThread();

            s_IsInitialized = true;

            // Mark that we're running for domain reload recovery
            EditorPrefs.SetBool(WAS_RUNNING_PREF_KEY, true);
        }

        /// <summary>
        /// Starts the server without mutex check, used after domain reload.
        /// </summary>
        private static void StartWithoutMutex()
        {
            if (s_IsInitialized && s_IsServerRunning)
            {
                return;
            }

            Debug.Log("Restarting Unity Hot Reload server after domain reload...");

            // Start TCP listener thread
            StartListenerThread();

            s_IsInitialized = true;
        }

        /// <summary>
        /// Checks if a specific port is in use.
        /// </summary>
        private static bool IsPortInUse(int port)
        {
            System.Net.Sockets.TcpListener tempListener = null;
            try
            {
                // Explicitly qualify TcpListener and IPAddress to ensure correct type resolution
                tempListener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, port);
                tempListener.Start();
                return false; // Port is available if we could start
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                // Check if the specific error is "Address already in use"
                // Common error codes for this are 10048 (WSAEADDRINUSE on Windows) or 48/98 on Unix-like systems.
                if (
                    ex.SocketErrorCode == System.Net.Sockets.SocketError.AddressAlreadyInUse
                    || ex.ErrorCode == 48
                    || ex.ErrorCode == 98
                    || ex.ErrorCode == 10048
                )
                {
                    return true; // Port is in use
                }
                // For other socket exceptions, we might not be sure, but typically means not usable.
                // Depending on desired behavior, could return true or re-throw. For IsPortInUse, true is safer.
                return true;
            }
            catch
            {
                // Any other exception (e.g., security, permissions) likely means port is not usable by us.
                return true;
            }
            finally
            {
                if (tempListener != null)
                {
                    tempListener.Stop(); // Stop also closes the listener socket
                }
            }
        }

        /// <summary>
        /// Stops the hot reload server and releases resources.
        /// </summary>
        [MenuItem("Tools/Hot Reload/Stop")]
        public static void Stop()
        {
            if (!s_IsInitialized)
            {
                Debug.Log("Hot Reload server is not running.");
                return;
            }

            Debug.Log("Stopping Unity Hot Reload server...");

            StopServer();

            // Release the mutex
            if (s_InstanceMutex != null)
            {
                try
                {
                    s_InstanceMutex.ReleaseMutex();
                    s_InstanceMutex.Close();
                }
                catch { }
                s_InstanceMutex = null;
            }

            s_IsInitialized = false;

            // Clear the running flag
            EditorPrefs.SetBool(WAS_RUNNING_PREF_KEY, false);
        }

        /// <summary>
        /// Reloads the hot reload server on the same port.
        /// This maintains connection stability with the VS Code extension.
        /// </summary>
        [MenuItem("Tools/Hot Reload/Reload")]
        public static void Reload()
        {
            if (!s_IsInitialized)
            {
                Debug.Log("Hot Reload server is not running. Starting fresh...");
                Start();
                return;
            }

            Debug.Log($"Reloading Unity Hot Reload server on port {s_CurrentPort}...");

            // Store the current port to reuse it
            int portToReuse = s_CurrentPort;

            // Stop the server without releasing the mutex
            StopServer();

            // Try to restart on the same port with retries
            bool restarted = false;
            for (int attempt = 0; attempt < PORT_RETRY_ATTEMPTS; attempt++)
            {
                // Increasing delay between attempts to allow OS to release the port
                Thread.Sleep(PORT_RETRY_DELAY_MS * (attempt + 1));

                // Force the port for this reload
                s_CurrentPort = portToReuse;

                if (TryStartOnSpecificPort(portToReuse))
                {
                    restarted = true;
                    Debug.Log($"Hot Reload server successfully reloaded on port {s_CurrentPort}");
                    break;
                }

                if (s_ShowDebugLogs)
                {
                    Debug.Log(
                        $"Port {portToReuse} not available yet, attempt {attempt + 1}/{PORT_RETRY_ATTEMPTS}"
                    );
                }
            }

            // If we couldn't restart on the same port, start normally
            if (!restarted)
            {
                Debug.LogWarning(
                    $"Could not reload on port {portToReuse}, starting on available port..."
                );
                StartListenerThread();
            }
        }

        /// <summary>
        /// Toggles whether debug logs are shown in the console.
        /// Setting is persisted between editor sessions.
        /// </summary>
        [MenuItem("Tools/Hot Reload/Toggle Debug Logs")]
        public static void ToggleDebugLogs()
        {
            s_ShowDebugLogs = !s_ShowDebugLogs;
            EditorPrefs.SetBool(DEBUG_PREF_KEY, s_ShowDebugLogs);
            Debug.Log($"Hot Reload: Debug logs are now {(s_ShowDebugLogs ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Validates the Toggle Debug Logs menu item and sets its checked state.
        /// </summary>
        [MenuItem("Tools/Hot Reload/Toggle Debug Logs", true)]
        public static bool ValidateToggleDebugLogs()
        {
            Menu.SetChecked("Tools/Hot Reload/Toggle Debug Logs", s_ShowDebugLogs);
            return true;
        }

        /// <summary>
        /// Starts the listener thread for TCP communication.
        /// Ensures any existing thread is stopped first.
        /// </summary>
        private static void StartListenerThread()
        {
            // Stop any existing thread first
            if (s_ListenerThread != null && s_ListenerThread.IsAlive)
            {
                StopServer();
            }

            // Start new thread
            s_ListenerThread = new Thread(ListenerThreadFunction);
            s_ListenerThread.IsBackground = true;
            s_ListenerThread.Start();
        }

        /// <summary>
        /// Stops the TCP server and cleans up the listener thread.
        /// Enhanced with better socket cleanup for port reuse.
        /// </summary>
        private static void StopServer()
        {
            s_IsServerRunning = false;

            // Disconnect all clients
            lock (s_ClientListLock)
            {
                foreach (var client in s_ConnectedClients)
                {
                    try
                    {
                        if (client.Connected)
                        {
                            // Shutdown the connection before closing
                            client.Client.Shutdown(SocketShutdown.Both);
                            client.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (s_ShowDebugLogs)
                        {
                            Debug.LogWarning($"Error closing client connection: {ex.Message}");
                        }
                    }
                }
                s_ConnectedClients.Clear();
            }

            // Stop TCP listener with proper cleanup
            if (s_Server != null)
            {
                try
                {
                    s_Server.Stop();

                    // Explicitly close and dispose the underlying socket
                    if (s_Server.Server != null)
                    {
                        s_Server.Server.Close();
                        s_Server.Server.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error stopping server: {ex.Message}");
                }
                s_Server = null;
            }

            // Stop thread
            if (s_ListenerThread != null && s_ListenerThread.IsAlive)
            {
                try
                {
                    s_ListenerThread.Join(1000); // Give it more time to exit gracefully

                    if (s_ListenerThread.IsAlive)
                    {
                        s_ListenerThread.Abort(); // Force abort if thread doesn't exit gracefully
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error stopping thread: {ex.Message}");
                }

                s_ListenerThread = null;
            }
        }

        /// <summary>
        /// Called before Unity reloads assemblies.
        /// Stops the server to prevent threading issues during domain reload.
        /// </summary>
        private static void OnBeforeAssemblyReload()
        {
            // Mark that we were running before reload
            if (s_IsInitialized && s_IsServerRunning)
            {
                EditorPrefs.SetBool(WAS_RUNNING_PREF_KEY, true);
            }

            // Release mutex before domain reload
            if (s_InstanceMutex != null)
            {
                try
                {
                    s_InstanceMutex.ReleaseMutex();
                    s_InstanceMutex.Close();
                }
                catch { }
                s_InstanceMutex = null;
            }

            StopServer();
        }

        /// <summary>
        /// Called when Unity editor is quitting.
        /// Performs cleanup of resources.
        /// </summary>
        private static void OnEditorQuitting()
        {
            // Clear the running flag
            EditorPrefs.SetBool(WAS_RUNNING_PREF_KEY, false);
            Stop();
        }

        /// <summary>
        /// Called every editor update frame.
        /// Processes any queued messages from the TCP listener and triggers refreshes.
        /// </summary>
        private static void OnEditorUpdate()
        {
            // Process any queued messages
            lock (s_MessageQueue)
            {
                while (s_MessageQueue.Count > 0)
                {
                    string message = s_MessageQueue.Dequeue();
                    ProcessMessage(message);
                }
            }

            // Process actions queued for the main thread
            lock (s_MainThreadActionsLock)
            {
                while (s_MainThreadActions.Count > 0)
                {
                    Action action = s_MainThreadActions.Dequeue();
                    action?.Invoke();
                }
            }

            // Check if we should refresh
            if (s_ShouldRequestRefresh)
            {
                s_ShouldRequestRefresh = false;
                RefreshAssets();
            }
        }

        /// <summary>
        /// Thread function that listens for TCP connections.
        /// Prioritizes the last successful port or current port for stability.
        /// </summary>
        private static void ListenerThreadFunction()
        {
            bool serverStarted = false;
            List<int> portsToTry = new List<int>();

            // Prioritize the current port (for reload scenarios)
            if (s_CurrentPort > 0 && !portsToTry.Contains(s_CurrentPort))
            {
                portsToTry.Add(s_CurrentPort);
            }

            // Then try the last successful port
            if (s_LastSuccessfulPort > 0 && !portsToTry.Contains(s_LastSuccessfulPort))
            {
                portsToTry.Add(s_LastSuccessfulPort);
            }

            // Finally, add all alternative ports
            foreach (int port in ALTERNATIVE_PORTS)
            {
                if (!portsToTry.Contains(port))
                {
                    portsToTry.Add(port);
                }
            }

            // Try each port until we find one that works
            foreach (int portToTry in portsToTry)
            {
                // If this is our preferred port (current or last successful), try harder
                bool isPreferredPort = (portToTry == s_CurrentPort || portToTry == s_LastSuccessfulPort);
                int maxAttempts = isPreferredPort ? PORT_RETRY_ATTEMPTS : 1;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    if (attempt > 0)
                    {
                        Thread.Sleep(PORT_RETRY_DELAY_MS);
                    }

                    try
                    {
                        s_Server = new System.Net.Sockets.TcpListener(
                            System.Net.IPAddress.Any,
                            portToTry
                        );

                        // Try to set socket options to allow port reuse
                        try
                        {
                            s_Server.Server.SetSocketOption(
                                SocketOptionLevel.Socket,
                                SocketOptionName.ReuseAddress,
                                true
                            );
                        }
                        catch (Exception ex)
                        {
                            if (s_ShowDebugLogs)
                            {
                                Debug.LogWarning($"Could not set ReuseAddress option: {ex.Message}");
                            }
                        }

                        s_Server.Start();

                        s_CurrentPort = portToTry;
                        s_LastSuccessfulPort = portToTry;
                        // Defer EditorPrefs call to main thread
                        lock (s_MainThreadActionsLock)
                        {
                            s_MainThreadActions.Enqueue(() =>
                                EditorPrefs.SetInt(LAST_PORT_PREF_KEY, s_LastSuccessfulPort)
                            );
                        }
                        serverStarted = true;

                        Debug.Log($"Unity Hot Reload server listening on port {s_CurrentPort}");
                        break;
                    }
                    catch (SocketException ex)
                    {
                        if (ex.ErrorCode == 10048 || ex.ErrorCode == 48 || ex.ErrorCode == 98)
                        {
                            if (s_ShowDebugLogs || (isPreferredPort && attempt == maxAttempts - 1))
                            {
                                Debug.LogWarning(
                                    $"Port {portToTry} is in use (attempt {attempt + 1}/{maxAttempts})"
                                );
                            }
                            continue;
                        }
                        else
                        {
                            Debug.LogError($"Socket error: {ex.Message} (ErrorCode: {ex.ErrorCode})");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error starting server: {ex.Message}");
                        return;
                    }
                }

                if (serverStarted)
                    break;
            }

            if (!serverStarted)
            {
                Debug.LogError("Failed to start Hot Reload server. All ports are in use.");
                return;
            }

            s_IsServerRunning = true;
            RunServerLoop();
        }

        /// <summary>
        /// Main server loop extracted to a separate method for reuse.
        /// Handles accepting connections and cleaning up disconnected clients.
        /// </summary>
        private static void RunServerLoop()
        {
            try
            {
                while (s_IsServerRunning)
                {
                    try
                    {
                        // Wait for a client connection with a timeout
                        if (s_Server.Pending())
                        {
                            TcpClient client = s_Server.AcceptTcpClient();

                            // Add client to list for tracking
                            lock (s_ClientListLock)
                            {
                                s_ConnectedClients.Add(client);
                            }

                            // Only log connection if debug logs are enabled
                            if (s_ShowDebugLogs)
                            {
                                Debug.Log(
                                    $"VS Code connected to Unity Hot Reload server on port {s_CurrentPort}"
                                );
                            }

                            // Handle client in a separate thread to allow multiple connections
                            Thread clientThread = new Thread(() => HandleClient(client));
                            clientThread.IsBackground = true;
                            clientThread.Start();
                        }
                        else
                        {
                            // No pending connections, sleep briefly to avoid busy waiting
                            Thread.Sleep(100);
                        }

                        // Clean up disconnected clients
                        lock (s_ClientListLock)
                        {
                            int removedCount = s_ConnectedClients.RemoveAll(c => !c.Connected);
                            if (removedCount > 0 && s_ShowDebugLogs)
                            {
                                Debug.Log($"Cleaned up {removedCount} disconnected client(s)");
                            }
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (!s_IsServerRunning)
                        {
                            // Server is shutting down, this is expected
                            break;
                        }

                        Debug.LogError($"Socket error while accepting client: {ex.Message}");
                    }
                }
            }
            catch (ThreadAbortException)
            {
                if (s_ShowDebugLogs)
                {
                    Debug.Log("Unity Hot Reload server thread aborted");
                }
            }
            catch (Exception e)
            {
                if (!s_IsServerRunning)
                {
                    // Server is shutting down, this is expected
                    if (s_ShowDebugLogs)
                    {
                        Debug.Log("Unity Hot Reload server stopped");
                    }
                }
                else
                {
                    Debug.LogError($"Unity Hot Reload server error: {e.Message}");
                }
            }
            finally
            {
                // Ensure server is stopped
                if (s_Server != null)
                {
                    try
                    {
                        s_Server.Stop();
                        s_Server = null;
                    }
                    catch (Exception)
                    {
                        // Ignore errors during cleanup
                    }
                }
                s_IsServerRunning = false;
            }
        }

        /// <summary>
        /// Handles communication with a single client connection.
        /// </summary>
        /// <param name="client">The connected TCP client</param>
        private static void HandleClient(TcpClient client)
        {
            try
            {
                byte[] buffer = new byte[1024];
                NetworkStream stream = client.GetStream();

                while (client.Connected && s_IsServerRunning)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            // Queue message for processing on main thread
                            lock (s_MessageQueue)
                            {
                                s_MessageQueue.Enqueue(message);
                            }
                        }
                        else
                        {
                            // Connection closed by client
                            break;
                        }
                    }
                    else
                    {
                        // No data available, sleep briefly
                        Thread.Sleep(50);
                    }
                }
            }
            catch (Exception ex)
            {
                if (s_ShowDebugLogs && s_IsServerRunning)
                {
                    Debug.LogWarning($"Client handler error: {ex.Message}");
                }
            }
            finally
            {
                // Remove client from list and close connection
                lock (s_ClientListLock)
                {
                    s_ConnectedClients.Remove(client);
                }

                try
                {
                    client.Close();
                }
                catch { }

                if (s_ShowDebugLogs)
                {
                    Debug.Log("Client disconnected from Unity Hot Reload server");
                }
            }
        }

        /// <summary>
        /// Processes messages received from the TCP client.
        /// Handles different command types from the VS Code extension.
        /// </summary>
        /// <param name="message">The message received from the client</param>
        private static void ProcessMessage(string message)
        {
            if (s_ShowDebugLogs)
            {
                Debug.Log($"Received message: {message}");
            }

            try
            {
                // Try to parse as JSON for more complex commands
                if (message.Contains("{") && message.Contains("}"))
                {
                    // Simple JSON parsing for command
                    if (message.Contains("\"command\""))
                    {
                        if (message.Contains("\"refresh\""))
                        {
                            s_ShouldRequestRefresh = true;
                        }
                        else if (message.Contains("\"ping\""))
                        {
                            // Could implement ping/pong in future
                            if (s_ShowDebugLogs)
                            {
                                Debug.Log("Received ping from VS Code");
                            }
                        }
                    }
                    else
                    {
                        // Default behavior - refresh
                        s_ShouldRequestRefresh = true;
                    }
                }
                else
                {
                    // Simple message - trigger refresh
                    s_ShouldRequestRefresh = true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing message: {ex.Message}");
                // Default to refresh on error
                s_ShouldRequestRefresh = true;
            }
        }

        /// <summary>
        /// Refreshes Unity's asset database and triggers script compilation.
        /// Called when changes are detected from VS Code/Cursor.
        /// </summary>
        private static void RefreshAssets()
        {
            try
            {
                Debug.Log("Hot Reload: Refreshing Unity assets...");

                // Refresh the asset database
                AssetDatabase.Refresh(ImportAssetOptions.Default);

                // Request script compilation (Unity 2019.1 or newer)
#if UNITY_2019_1_OR_NEWER
                CompilationPipeline.RequestScriptCompilation();
#endif

                if (s_ShowDebugLogs)
                {
                    Debug.Log("Hot Reload: Refresh complete");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Hot Reload refresh error: {e.Message}");
            }
        }

        /// <summary>
        /// Returns the current port being used by the hot reload server.
        /// Returns -1 if the server is not running.
        /// </summary>
        /// <returns>The current port number or -1 if not running</returns>
        public static int GetCurrentPort()
        {
            return s_IsServerRunning ? s_CurrentPort : -1;
        }

        /// <summary>
        /// Checks if the hot reload server is currently running.
        /// </summary>
        /// <returns>True if the server is running, false otherwise</returns>
        public static bool IsServerRunning()
        {
            return s_IsServerRunning;
        }

        /// <summary>
        /// Gets the number of currently connected clients.
        /// </summary>
        /// <returns>The number of connected clients</returns>
        public static int GetConnectedClientCount()
        {
            lock (s_ClientListLock)
            {
                return s_ConnectedClients.Count;
            }
        }

        /// <summary>
        /// Tries to start the server on a specific port.
        /// Returns true if successful, false otherwise.
        /// </summary>
        private static bool TryStartOnSpecificPort(int port)
        {
            try
            {
                // Create a new listener thread that will only try the specified port
                s_ListenerThread = new Thread(() => ListenerThreadFunctionSpecificPort(port));
                s_ListenerThread.IsBackground = true;
                s_ListenerThread.Start();

                // Wait a moment to see if it started successfully
                Thread.Sleep(100);

                return s_IsServerRunning;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Special listener thread function that only tries a specific port.
        /// Used for reload scenarios to maintain port consistency.
        /// </summary>
        private static void ListenerThreadFunctionSpecificPort(int specificPort)
        {
            try
            {
                s_Server = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Any, specificPort);

                // Try to set socket options to allow port reuse
                try
                {
                    s_Server.Server.SetSocketOption(
                        SocketOptionLevel.Socket,
                        SocketOptionName.ReuseAddress,
                        true
                    );
                }
                catch (Exception ex)
                {
                    if (s_ShowDebugLogs)
                    {
                        Debug.LogWarning($"Could not set ReuseAddress option: {ex.Message}");
                    }
                }

                s_Server.Start();

                s_CurrentPort = specificPort;
                s_LastSuccessfulPort = specificPort;
                // Defer EditorPrefs call to main thread
                lock (s_MainThreadActionsLock)
                {
                    s_MainThreadActions.Enqueue(() =>
                        EditorPrefs.SetInt(LAST_PORT_PREF_KEY, s_LastSuccessfulPort)
                    );
                }
                s_IsServerRunning = true;
                Debug.Log($"Unity Hot Reload server listening on port {s_CurrentPort}");
                RunServerLoop();
            }
            catch (Exception ex)
            {
                if (s_ShowDebugLogs)
                {
                    Debug.LogError($"Failed to start on port {specificPort}: {ex.Message}");
                }
                s_IsServerRunning = false;
            }
        }
    }
}

#endif // UNITY_EDITOR
