using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Centralized RPC management for GameCreator + Netcode integration.
    /// Features:
    /// - Generic RPC channels for custom events
    /// - Message routing without requiring NetworkBehaviour on every object
    /// - Event-based RPC system for visual scripting
    /// - Reliable and unreliable message options
    /// - Target filtering (All, Server, Specific Client, Nearby)
    ///
    /// Execution Order: Should run after NetworkManager (-45)
    /// </summary>
    [DefaultExecutionOrder(-45)]
    [AddComponentMenu("Game Creator/Network/Network RPC Manager")]
    public class NetworkRPCManager : NetworkBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkRPCManager s_Instance;
        public static NetworkRPCManager Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Settings")]
        [SerializeField]
        [Tooltip("Maximum message size in bytes")]
        private int m_MaxMessageSize = 1024;

        [SerializeField]
        [Tooltip("Enable debug logging for RPCs")]
        private bool m_DebugLogging = false;

        // MEMBERS: -------------------------------------------------------------------------------

        private readonly Dictionary<string, Action<RPCMessage>> m_Handlers = new();
        private readonly Dictionary<string, List<Action<RPCMessage>>> m_Listeners = new();
        private readonly Queue<RPCMessage> m_MessageQueue = new();

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired when any RPC message is received.</summary>
        public static event Action<RPCMessage> EventMessageReceived;

        /// <summary>Fired when an RPC message is sent.</summary>
        public static event Action<RPCMessage> EventMessageSent;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum RPCTarget
        {
            Server, // Send to server only
            AllClients, // Send to all clients (including host)
            AllClientsExceptSender, // Send to all except the sender
            SpecificClient, // Send to a specific client
            NearbyClients, // Send to clients within range (requires position)
        }

        public enum RPCDelivery
        {
            Reliable, // Guaranteed delivery, ordered
            Unreliable, // Fast, may be lost or out of order
        }

        // STRUCTS: -------------------------------------------------------------------------------

        [Serializable]
        public struct RPCMessage : INetworkSerializable
        {
            public string Channel;
            public string EventName;
            public ulong SenderClientId;
            public ulong TargetClientId;
            public Vector3 Position;
            public float Range;
            public string StringData;
            public int IntData;
            public float FloatData;
            public bool BoolData;
            public Vector3 VectorData;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer)
                where T : IReaderWriter
            {
                serializer.SerializeValue(ref Channel);
                serializer.SerializeValue(ref EventName);
                serializer.SerializeValue(ref SenderClientId);
                serializer.SerializeValue(ref TargetClientId);
                serializer.SerializeValue(ref Position);
                serializer.SerializeValue(ref Range);
                serializer.SerializeValue(ref StringData);
                serializer.SerializeValue(ref IntData);
                serializer.SerializeValue(ref FloatData);
                serializer.SerializeValue(ref BoolData);
                serializer.SerializeValue(ref VectorData);
            }
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>Is the RPC manager ready to send/receive?</summary>
        public bool IsReady => IsSpawned;

        /// <summary>Local client ID.</summary>
        public ulong LocalClientId => NetworkManager.Singleton?.LocalClientId ?? 0;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[NetworkRPCManager] Duplicate instance detected. Destroying.");
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
            Debug.Log("[NetworkRPCManager] Ready for RPC communication");
        }

        // REGISTRATION: --------------------------------------------------------------------------

        /// <summary>
        /// Register a handler for a specific channel. Only one handler per channel.
        /// </summary>
        public void RegisterHandler(string channel, Action<RPCMessage> handler)
        {
            if (string.IsNullOrEmpty(channel))
            {
                Debug.LogError("[NetworkRPCManager] Channel cannot be null or empty");
                return;
            }

            m_Handlers[channel] = handler;

            if (m_DebugLogging)
            {
                Debug.Log($"[NetworkRPCManager] Registered handler for channel: {channel}");
            }
        }

        /// <summary>
        /// Unregister a handler for a channel.
        /// </summary>
        public void UnregisterHandler(string channel)
        {
            m_Handlers.Remove(channel);
        }

        /// <summary>
        /// Add a listener for a channel. Multiple listeners allowed.
        /// </summary>
        public void AddListener(string channel, Action<RPCMessage> listener)
        {
            if (!m_Listeners.TryGetValue(channel, out var list))
            {
                list = new List<Action<RPCMessage>>();
                m_Listeners[channel] = list;
            }

            list.Add(listener);
        }

        /// <summary>
        /// Remove a listener from a channel.
        /// </summary>
        public void RemoveListener(string channel, Action<RPCMessage> listener)
        {
            if (m_Listeners.TryGetValue(channel, out var list))
            {
                list.Remove(listener);
            }
        }

        /// <summary>
        /// Remove all listeners for a channel.
        /// </summary>
        public void RemoveAllListeners(string channel)
        {
            m_Listeners.Remove(channel);
        }

        // SEND METHODS: --------------------------------------------------------------------------

        /// <summary>
        /// Send an RPC message to the server.
        /// </summary>
        public void SendToServer(
            string channel,
            string eventName,
            string data = "",
            int intData = 0,
            float floatData = 0f,
            bool boolData = false,
            Vector3 vectorData = default
        )
        {
            var message = CreateMessage(
                channel,
                eventName,
                data,
                intData,
                floatData,
                boolData,
                vectorData
            );
            SendToServerRpc(message);

            if (m_DebugLogging)
            {
                Debug.Log($"[NetworkRPCManager] Sent to server: {channel}/{eventName}");
            }
        }

        /// <summary>
        /// Send an RPC message to all clients (server-only).
        /// </summary>
        public void SendToAllClients(
            string channel,
            string eventName,
            string data = "",
            int intData = 0,
            float floatData = 0f,
            bool boolData = false,
            Vector3 vectorData = default
        )
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkRPCManager] Only server can send to all clients");
                return;
            }

            var message = CreateMessage(
                channel,
                eventName,
                data,
                intData,
                floatData,
                boolData,
                vectorData
            );
            BroadcastToClientsRpc(message);

            if (m_DebugLogging)
            {
                Debug.Log($"[NetworkRPCManager] Broadcast to all: {channel}/{eventName}");
            }
        }

        /// <summary>
        /// Send an RPC message to a specific client (server-only).
        /// </summary>
        public void SendToClient(
            ulong targetClientId,
            string channel,
            string eventName,
            string data = "",
            int intData = 0,
            float floatData = 0f,
            bool boolData = false,
            Vector3 vectorData = default
        )
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkRPCManager] Only server can send to specific client");
                return;
            }

            var message = CreateMessage(
                channel,
                eventName,
                data,
                intData,
                floatData,
                boolData,
                vectorData
            );
            message.TargetClientId = targetClientId;

            SendToSpecificClientRpc(message, RpcTarget.Single(targetClientId, RpcTargetUse.Temp));

            if (m_DebugLogging)
            {
                Debug.Log(
                    $"[NetworkRPCManager] Sent to client {targetClientId}: {channel}/{eventName}"
                );
            }
        }

        /// <summary>
        /// Send an RPC message to clients within range of a position (server-only).
        /// </summary>
        public void SendToNearbyClients(
            Vector3 position,
            float range,
            string channel,
            string eventName,
            string data = "",
            int intData = 0,
            float floatData = 0f,
            bool boolData = false,
            Vector3 vectorData = default
        )
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkRPCManager] Only server can send to nearby clients");
                return;
            }

            var message = CreateMessage(
                channel,
                eventName,
                data,
                intData,
                floatData,
                boolData,
                vectorData
            );
            message.Position = position;
            message.Range = range;

            // Find nearby players and send individually
            foreach (var character in NetworkCharacterRegistry.Players)
            {
                if (character == null)
                    continue;

                float distance = Vector3.Distance(position, character.transform.position);
                if (distance <= range)
                {
                    ulong clientId = character.OwnerClientId;
                    SendToSpecificClientRpc(message, RpcTarget.Single(clientId, RpcTargetUse.Temp));
                }
            }

            if (m_DebugLogging)
            {
                Debug.Log(
                    $"[NetworkRPCManager] Sent to nearby clients at {position} (range: {range}): {channel}/{eventName}"
                );
            }
        }

        /// <summary>
        /// Request the server to broadcast a message to all clients.
        /// Can be called from any client.
        /// </summary>
        public void RequestBroadcast(
            string channel,
            string eventName,
            string data = "",
            int intData = 0,
            float floatData = 0f,
            bool boolData = false,
            Vector3 vectorData = default
        )
        {
            var message = CreateMessage(
                channel,
                eventName,
                data,
                intData,
                floatData,
                boolData,
                vectorData
            );
            RequestBroadcastRpc(message);
        }

        // CONVENIENCE METHODS: -------------------------------------------------------------------

        /// <summary>
        /// Send a simple string event to server.
        /// </summary>
        public void SendEventToServer(string eventName, string data = "")
        {
            SendToServer("events", eventName, data);
        }

        /// <summary>
        /// Broadcast a simple string event to all clients (server-only).
        /// </summary>
        public void BroadcastEvent(string eventName, string data = "")
        {
            SendToAllClients("events", eventName, data);
        }

        /// <summary>
        /// Send a gameplay action to server for validation.
        /// </summary>
        public void SendGameplayAction(string actionName, Vector3 position, int targetId = 0)
        {
            SendToServer("gameplay", actionName, "", targetId, 0f, false, position);
        }

        /// <summary>
        /// Send a chat message to all clients.
        /// </summary>
        public void SendChatMessage(string message)
        {
            RequestBroadcast("chat", "message", message);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private RPCMessage CreateMessage(
            string channel,
            string eventName,
            string data,
            int intData,
            float floatData,
            bool boolData,
            Vector3 vectorData
        )
        {
            return new RPCMessage
            {
                Channel = channel ?? "",
                EventName = eventName ?? "",
                SenderClientId = LocalClientId,
                TargetClientId = 0,
                Position = Vector3.zero,
                Range = 0f,
                StringData = data ?? "",
                IntData = intData,
                FloatData = floatData,
                BoolData = boolData,
                VectorData = vectorData,
            };
        }

        private void ProcessMessage(RPCMessage message)
        {
            // Invoke global event
            EventMessageReceived?.Invoke(message);

            // Invoke registered handler
            if (m_Handlers.TryGetValue(message.Channel, out var handler))
            {
                try
                {
                    handler.Invoke(message);
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        $"[NetworkRPCManager] Handler error for {message.Channel}: {e.Message}"
                    );
                }
            }

            // Invoke all listeners
            if (m_Listeners.TryGetValue(message.Channel, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    try
                    {
                        listener.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"[NetworkRPCManager] Listener error for {message.Channel}: {e.Message}"
                        );
                    }
                }
            }

            if (m_DebugLogging)
            {
                Debug.Log(
                    $"[NetworkRPCManager] Received: {message.Channel}/{message.EventName} from {message.SenderClientId}"
                );
            }
        }

        // RPCs: ----------------------------------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void SendToServerRpc(RPCMessage message, RpcParams rpcParams = default)
        {
            message.SenderClientId = rpcParams.Receive.SenderClientId;
            ProcessMessage(message);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void BroadcastToClientsRpc(RPCMessage message)
        {
            ProcessMessage(message);
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void SendToSpecificClientRpc(RPCMessage message, RpcParams rpcParams = default)
        {
            ProcessMessage(message);
        }

        [Rpc(SendTo.Server)]
        private void RequestBroadcastRpc(RPCMessage message, RpcParams rpcParams = default)
        {
            message.SenderClientId = rpcParams.Receive.SenderClientId;

            // Server processes first
            ProcessMessage(message);

            // Then broadcast to all clients
            BroadcastToClientsRpc(message);
        }

        // STATIC RESET: --------------------------------------------------------------------------

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_Instance = null;
        }
    }
}
