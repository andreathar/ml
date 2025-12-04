using System;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Core
{
    /// <summary>
    /// Global network session event manager.
    /// Place this component in your scene to receive network session events.
    /// Broadcasts events that can be listened to by GameCreator visual scripting triggers.
    ///
    /// Events:
    /// - Host/Server/Client started
    /// - Player spawned (any player, local player, remote player)
    /// - NPC spawned
    /// - Session ended
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Session Events")]
    public class NetworkSessionEvents : MonoBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkSessionEvents s_Instance;
        public static NetworkSessionEvents Instance => s_Instance;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired when Host mode starts (Server + Client)</summary>
        public static event Action EventHostStarted;

        /// <summary>Fired when Server mode starts (dedicated server)</summary>
        public static event Action EventServerStarted;

        /// <summary>Fired when Client mode starts (connecting to server)</summary>
        public static event Action EventClientStarted;

        /// <summary>Fired when network session ends (disconnect)</summary>
        public static event Action EventSessionEnded;

        /// <summary>Fired when any player character spawns. Passes the NetworkCharacter.</summary>
        public static event Action<NetworkCharacter> EventPlayerSpawned;

        /// <summary>Fired when the local player spawns. Passes the NetworkCharacter.</summary>
        public static event Action<NetworkCharacter> EventLocalPlayerSpawned;

        /// <summary>Fired when a remote player spawns. Passes the NetworkCharacter.</summary>
        public static event Action<NetworkCharacter> EventRemotePlayerSpawned;

        /// <summary>Fired when an NPC spawns. Passes the NetworkCharacter.</summary>
        public static event Action<NetworkCharacter> EventNPCSpawned;

        /// <summary>Fired when any NetworkCharacter despawns. Passes the NetworkCharacter.</summary>
        public static event Action<NetworkCharacter> EventCharacterDespawned;

        // MEMBERS: -------------------------------------------------------------------------------

        private bool m_WasHosting;
        private bool m_WasServer;
        private bool m_WasClient;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        private void OnEnable()
        {
            // Subscribe to NetworkManager events if available
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
                NetworkManager.Singleton.OnClientStarted += HandleClientStarted;
                NetworkManager.Singleton.OnServerStopped += HandleServerStopped;
                NetworkManager.Singleton.OnClientStopped += HandleClientStopped;
            }
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnServerStarted -= HandleServerStarted;
                NetworkManager.Singleton.OnClientStarted -= HandleClientStarted;
                NetworkManager.Singleton.OnServerStopped -= HandleServerStopped;
                NetworkManager.Singleton.OnClientStopped -= HandleClientStopped;
            }
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        // NETWORK CALLBACKS: ---------------------------------------------------------------------

        private void HandleServerStarted()
        {
            if (NetworkManager.Singleton.IsHost)
            {
                m_WasHosting = true;
                Debug.Log("[NetworkSessionEvents] Host started");
                EventHostStarted?.Invoke();
            }
            else
            {
                m_WasServer = true;
                Debug.Log("[NetworkSessionEvents] Server started (dedicated)");
                EventServerStarted?.Invoke();
            }
        }

        private void HandleClientStarted()
        {
            // For Host, both ServerStarted and ClientStarted fire
            // We only want to fire ClientStarted for pure clients
            if (!NetworkManager.Singleton.IsHost)
            {
                m_WasClient = true;
                Debug.Log("[NetworkSessionEvents] Client started");
                EventClientStarted?.Invoke();
            }
        }

        private void HandleServerStopped(bool wasHost)
        {
            if (m_WasHosting || m_WasServer)
            {
                m_WasHosting = false;
                m_WasServer = false;
                Debug.Log("[NetworkSessionEvents] Session ended (server stopped)");
                EventSessionEnded?.Invoke();
            }
        }

        private void HandleClientStopped(bool wasHost)
        {
            if (m_WasClient)
            {
                m_WasClient = false;
                Debug.Log("[NetworkSessionEvents] Session ended (client stopped)");
                EventSessionEnded?.Invoke();
            }
        }

        // PUBLIC STATIC METHODS (called by NetworkCharacter): ------------------------------------

        /// <summary>
        /// Called by NetworkCharacter when a player spawns on the network.
        /// </summary>
        public static void NotifyPlayerSpawned(NetworkCharacter character, bool isLocalOwner)
        {
            if (character == null)
                return;

            Debug.Log(
                $"[NetworkSessionEvents] Player spawned: {character.name} (IsLocal: {isLocalOwner})"
            );

            EventPlayerSpawned?.Invoke(character);

            if (isLocalOwner)
            {
                EventLocalPlayerSpawned?.Invoke(character);
            }
            else
            {
                EventRemotePlayerSpawned?.Invoke(character);
            }
        }

        /// <summary>
        /// Called by NetworkCharacter when an NPC spawns on the network.
        /// </summary>
        public static void NotifyNPCSpawned(NetworkCharacter character)
        {
            if (character == null)
                return;

            Debug.Log($"[NetworkSessionEvents] NPC spawned: {character.name}");
            EventNPCSpawned?.Invoke(character);
        }

        /// <summary>
        /// Called by NetworkCharacter when any character despawns.
        /// </summary>
        public static void NotifyCharacterDespawned(NetworkCharacter character)
        {
            if (character == null)
                return;

            Debug.Log($"[NetworkSessionEvents] Character despawned: {character.name}");
            EventCharacterDespawned?.Invoke(character);
        }
    }
}
