using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Coordinates scene loading/unloading across the network.
    /// Features:
    /// - Synchronized scene transitions
    /// - Loading screen coordination
    /// - Scene-specific spawn points
    /// - Additive scene management
    /// - Scene object registration
    ///
    /// Note: Named "Coordinator" to avoid conflict with Unity's NetworkSceneManager
    /// Execution Order: Should run after NetworkManager (-55)
    /// </summary>
    [DefaultExecutionOrder(-55)]
    [AddComponentMenu("Game Creator/Network/Network Scene Coordinator")]
    public class NetworkSceneCoordinator : NetworkBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkSceneCoordinator s_Instance;
        public static NetworkSceneCoordinator Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Scene Settings")]
        [SerializeField]
        [Tooltip("Scene to load when returning to lobby")]
        private string m_LobbySceneName = "Lobby";

        [SerializeField]
        [Tooltip("Default gameplay scene")]
        private string m_DefaultGameSceneName = "Game";

        [Header("Loading")]
        [SerializeField]
        [Tooltip("Minimum time to show loading screen (seconds)")]
        private float m_MinLoadingTime = 1f;

        [SerializeField]
        [Tooltip("Use async scene loading")]
        private bool m_UseAsyncLoading = true;

        // NETWORK VARIABLES: ---------------------------------------------------------------------

        private NetworkVariable<SceneState> m_SceneState = new(
            SceneState.None,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private NetworkVariable<int> m_LoadedClientsCount = new(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        // MEMBERS: -------------------------------------------------------------------------------

        private string m_CurrentSceneName;
        private string m_PendingSceneName;
        private readonly HashSet<ulong> m_ClientsReadyForScene = new();
        private readonly List<string> m_AdditiveScenes = new();
        private bool m_IsLoading;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired when scene loading starts. Passes scene name.</summary>
        public static event Action<string> EventSceneLoadStarted;

        /// <summary>Fired during scene loading. Passes progress (0-1).</summary>
        public static event Action<float> EventSceneLoadProgress;

        /// <summary>Fired when scene loading completes. Passes scene name.</summary>
        public static event Action<string> EventSceneLoadCompleted;

        /// <summary>Fired when all clients have loaded the scene.</summary>
        public static event Action<string> EventAllClientsReady;

        /// <summary>Fired when scene unloading starts.</summary>
        public static event Action<string> EventSceneUnloadStarted;

        /// <summary>Fired when scene unloading completes.</summary>
        public static event Action<string> EventSceneUnloadCompleted;

        /// <summary>Fired when scene state changes.</summary>
        public static event Action<SceneState> EventSceneStateChanged;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum SceneState
        {
            None = 0,
            Loading = 1,
            Loaded = 2,
            Unloading = 3,
            Transitioning = 4
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>Current scene state.</summary>
        public SceneState CurrentSceneState => m_SceneState.Value;

        /// <summary>Currently loaded scene name.</summary>
        public string CurrentSceneName => m_CurrentSceneName;

        /// <summary>Is a scene currently loading?</summary>
        public bool IsLoading => m_IsLoading;

        /// <summary>Number of clients that have finished loading.</summary>
        public int LoadedClientsCount => m_LoadedClientsCount.Value;

        /// <summary>Are all connected clients ready?</summary>
        public bool AreAllClientsReady
        {
            get
            {
                if (NetworkManager.Singleton == null) return false;
                return m_ClientsReadyForScene.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count;
            }
        }

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[NetworkSceneCoordinator] Duplicate instance detected. Destroying.");
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        public override void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
            base.OnDestroy();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            m_SceneState.OnValueChanged += OnSceneStateChanged;

            if (IsServer)
            {
                // Track current scene
                m_CurrentSceneName = SceneManager.GetActiveScene().name;

                // Subscribe to NGO scene events
                NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientSceneLoadComplete;
                NetworkManager.Singleton.SceneManager.OnUnloadComplete += OnClientSceneUnloadComplete;

                Debug.Log($"[NetworkSceneCoordinator] Initialized. Current scene: {m_CurrentSceneName}");
            }
        }

        public override void OnNetworkDespawn()
        {
            m_SceneState.OnValueChanged -= OnSceneStateChanged;

            if (IsServer && NetworkManager.Singleton?.SceneManager != null)
            {
                NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnClientSceneLoadComplete;
                NetworkManager.Singleton.SceneManager.OnUnloadComplete -= OnClientSceneUnloadComplete;
            }

            base.OnNetworkDespawn();
        }

        // SCENE STATE CALLBACKS: -----------------------------------------------------------------

        private void OnSceneStateChanged(SceneState oldState, SceneState newState)
        {
            Debug.Log($"[NetworkSceneCoordinator] State changed: {oldState} -> {newState}");
            EventSceneStateChanged?.Invoke(newState);
        }

        private void OnClientSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
        {
            if (!IsServer) return;

            m_ClientsReadyForScene.Add(clientId);
            m_LoadedClientsCount.Value = m_ClientsReadyForScene.Count;

            Debug.Log($"[NetworkSceneCoordinator] Client {clientId} loaded scene: {sceneName} ({m_ClientsReadyForScene.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

            if (AreAllClientsReady)
            {
                OnAllClientsLoaded(sceneName);
            }
        }

        private void OnClientSceneUnloadComplete(ulong clientId, string sceneName)
        {
            Debug.Log($"[NetworkSceneCoordinator] Client {clientId} unloaded scene: {sceneName}");
        }

        private void OnAllClientsLoaded(string sceneName)
        {
            m_SceneState.Value = SceneState.Loaded;
            m_IsLoading = false;

            EventAllClientsReady?.Invoke(sceneName);
            NotifyAllClientsReadyClientRpc(sceneName);

            Debug.Log($"[NetworkSceneCoordinator] All clients loaded scene: {sceneName}");
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Load a scene for all clients. Server-only.
        /// </summary>
        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkSceneCoordinator] Only server can load scenes");
                return;
            }

            if (m_IsLoading)
            {
                Debug.LogWarning("[NetworkSceneCoordinator] Already loading a scene");
                return;
            }

            StartCoroutine(LoadSceneCoroutine(sceneName, mode));
        }

        /// <summary>
        /// Load the lobby scene. Server-only.
        /// </summary>
        public void LoadLobbyScene()
        {
            LoadScene(m_LobbySceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Load the default game scene. Server-only.
        /// </summary>
        public void LoadGameScene()
        {
            LoadScene(m_DefaultGameSceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Load an additive scene. Server-only.
        /// </summary>
        public void LoadAdditiveScene(string sceneName)
        {
            if (!IsServer) return;

            if (m_AdditiveScenes.Contains(sceneName))
            {
                Debug.LogWarning($"[NetworkSceneCoordinator] Scene already loaded: {sceneName}");
                return;
            }

            StartCoroutine(LoadAdditiveSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Unload an additive scene. Server-only.
        /// </summary>
        public void UnloadAdditiveScene(string sceneName)
        {
            if (!IsServer) return;

            if (!m_AdditiveScenes.Contains(sceneName))
            {
                Debug.LogWarning($"[NetworkSceneCoordinator] Scene not loaded: {sceneName}");
                return;
            }

            StartCoroutine(UnloadAdditiveSceneCoroutine(sceneName));
        }

        /// <summary>
        /// Get all loaded additive scenes.
        /// </summary>
        public IReadOnlyList<string> GetAdditiveScenes()
        {
            return m_AdditiveScenes;
        }

        /// <summary>
        /// Notify server that this client is ready for the scene.
        /// Called automatically, but can be called manually for custom loading.
        /// </summary>
        public void NotifyClientReady()
        {
            NotifyClientReadyRpc();
        }

        // COROUTINES: ----------------------------------------------------------------------------

        private IEnumerator LoadSceneCoroutine(string sceneName, LoadSceneMode mode)
        {
            m_IsLoading = true;
            m_PendingSceneName = sceneName;
            m_ClientsReadyForScene.Clear();
            m_LoadedClientsCount.Value = 0;
            m_SceneState.Value = SceneState.Loading;

            EventSceneLoadStarted?.Invoke(sceneName);
            NotifySceneLoadStartedClientRpc(sceneName);

            float startTime = Time.time;

            // Use NGO's scene manager for synchronized loading
            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, mode);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"[NetworkSceneCoordinator] Failed to start scene load: {status}");
                m_IsLoading = false;
                m_SceneState.Value = SceneState.None;
                yield break;
            }

            // Wait for minimum loading time
            float elapsed = Time.time - startTime;
            if (elapsed < m_MinLoadingTime)
            {
                yield return new WaitForSeconds(m_MinLoadingTime - elapsed);
            }

            m_CurrentSceneName = sceneName;

            EventSceneLoadCompleted?.Invoke(sceneName);
            NotifySceneLoadCompletedClientRpc(sceneName);

            Debug.Log($"[NetworkSceneCoordinator] Scene load initiated: {sceneName}");
        }

        private IEnumerator LoadAdditiveSceneCoroutine(string sceneName)
        {
            EventSceneLoadStarted?.Invoke(sceneName);
            NotifySceneLoadStartedClientRpc(sceneName);

            var status = NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"[NetworkSceneCoordinator] Failed to load additive scene: {status}");
                yield break;
            }

            m_AdditiveScenes.Add(sceneName);

            EventSceneLoadCompleted?.Invoke(sceneName);
            NotifySceneLoadCompletedClientRpc(sceneName);

            Debug.Log($"[NetworkSceneCoordinator] Additive scene loaded: {sceneName}");
        }

        private IEnumerator UnloadAdditiveSceneCoroutine(string sceneName)
        {
            EventSceneUnloadStarted?.Invoke(sceneName);
            NotifySceneUnloadStartedClientRpc(sceneName);

            var status = NetworkManager.Singleton.SceneManager.UnloadScene(SceneManager.GetSceneByName(sceneName));

            if (status != SceneEventProgressStatus.Started)
            {
                Debug.LogError($"[NetworkSceneCoordinator] Failed to unload scene: {status}");
                yield break;
            }

            m_AdditiveScenes.Remove(sceneName);

            EventSceneUnloadCompleted?.Invoke(sceneName);
            NotifySceneUnloadCompletedClientRpc(sceneName);

            Debug.Log($"[NetworkSceneCoordinator] Additive scene unloaded: {sceneName}");
        }

        // RPCs: ----------------------------------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void NotifyClientReadyRpc(RpcParams rpcParams = default)
        {
            ulong clientId = rpcParams.Receive.SenderClientId;
            m_ClientsReadyForScene.Add(clientId);
            m_LoadedClientsCount.Value = m_ClientsReadyForScene.Count;

            Debug.Log($"[NetworkSceneCoordinator] Client {clientId} ready ({m_ClientsReadyForScene.Count}/{NetworkManager.Singleton.ConnectedClientsIds.Count})");

            if (AreAllClientsReady && m_SceneState.Value == SceneState.Loading)
            {
                OnAllClientsLoaded(m_CurrentSceneName);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifySceneLoadStartedClientRpc(string sceneName)
        {
            if (!IsServer)
            {
                EventSceneLoadStarted?.Invoke(sceneName);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifySceneLoadCompletedClientRpc(string sceneName)
        {
            if (!IsServer)
            {
                EventSceneLoadCompleted?.Invoke(sceneName);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifySceneUnloadStartedClientRpc(string sceneName)
        {
            if (!IsServer)
            {
                EventSceneUnloadStarted?.Invoke(sceneName);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifySceneUnloadCompletedClientRpc(string sceneName)
        {
            if (!IsServer)
            {
                EventSceneUnloadCompleted?.Invoke(sceneName);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyAllClientsReadyClientRpc(string sceneName)
        {
            if (!IsServer)
            {
                EventAllClientsReady?.Invoke(sceneName);
            }
        }

        // STATIC RESET: --------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Instance = null;
        }
    }
}
