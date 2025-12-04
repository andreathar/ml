using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Manages network spawning with proper timing and coordination.
    /// Features:
    /// - Spawn points management
    /// - Spawn queue to prevent flooding
    /// - Player spawn coordination (waits for all players ready)
    /// - NPC spawn scheduling
    /// - Prefab validation
    ///
    /// Execution Order: Should run after NetworkManager (-50)
    /// </summary>
    [DefaultExecutionOrder(-50)]
    [AddComponentMenu("Game Creator/Network/Network Spawn Manager")]
    public class NetworkSpawnManager : NetworkBehaviour
    {
        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkSpawnManager s_Instance;
        public static NetworkSpawnManager Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Player Spawning")]
        [SerializeField]
        [Tooltip("Player prefab to spawn for each client")]
        private GameObject m_PlayerPrefab;

        [SerializeField]
        [Tooltip("Spawn points for players. If empty, uses default position.")]
        private Transform[] m_PlayerSpawnPoints;

        [SerializeField]
        [Tooltip("How to assign spawn points to players")]
        private SpawnPointMode m_SpawnPointMode = SpawnPointMode.RoundRobin;

        [SerializeField]
        [Tooltip("Auto-spawn player when client connects")]
        private bool m_AutoSpawnPlayer = true;

        [Header("NPC Spawning")]
        [SerializeField]
        [Tooltip("Delay between NPC spawns to prevent flooding (seconds)")]
        private float m_NPCSpawnDelay = 0.1f;

        [SerializeField]
        [Tooltip("Maximum NPCs to spawn per frame")]
        private int m_MaxNPCsPerFrame = 3;

        [Header("Spawn Timing")]
        [SerializeField]
        [Tooltip("Delay after network spawn before spawning player (seconds)")]
        private float m_PlayerSpawnDelay = 0.5f;

        [SerializeField]
        [Tooltip("Wait for all connected clients before spawning players")]
        private bool m_WaitForAllPlayers = false;

        // MEMBERS: -------------------------------------------------------------------------------

        private readonly Queue<SpawnRequest> m_SpawnQueue = new();
        private readonly Dictionary<ulong, NetworkCharacter> m_SpawnedPlayers = new();
        private readonly List<NetworkCharacter> m_SpawnedNPCs = new();
        private int m_NextSpawnPointIndex;
        private float m_LastNPCSpawnTime;
        private bool m_IsProcessingQueue;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>Fired before a player is spawned. Can be used to modify spawn position.</summary>
        public static event Action<ulong, SpawnData> EventBeforePlayerSpawn;

        /// <summary>Fired after a player is spawned.</summary>
        public static event Action<ulong, NetworkCharacter> EventAfterPlayerSpawn;

        /// <summary>Fired before an NPC is spawned.</summary>
        public static event Action<SpawnData> EventBeforeNPCSpawn;

        /// <summary>Fired after an NPC is spawned.</summary>
        public static event Action<NetworkCharacter> EventAfterNPCSpawn;

        /// <summary>Fired when all expected players have spawned.</summary>
        public static event Action EventAllPlayersSpawned;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum SpawnPointMode
        {
            RoundRobin,     // Cycle through spawn points
            Random,         // Random spawn point
            Closest,        // Closest to some reference point
            ByClientId      // Index matches ClientId (mod spawn point count)
        }

        public enum SpawnType
        {
            Player,
            NPC
        }

        // STRUCTS: -------------------------------------------------------------------------------

        public struct SpawnData
        {
            public GameObject Prefab;
            public Vector3 Position;
            public Quaternion Rotation;
            public ulong OwnerClientId;
            public SpawnType Type;
            public object CustomData;
        }

        private struct SpawnRequest
        {
            public SpawnData Data;
            public Action<NetworkCharacter> Callback;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>The player prefab used for spawning.</summary>
        public GameObject PlayerPrefab
        {
            get => m_PlayerPrefab;
            set => m_PlayerPrefab = value;
        }

        /// <summary>Number of players currently spawned.</summary>
        public int SpawnedPlayerCount => m_SpawnedPlayers.Count;

        /// <summary>Number of NPCs currently spawned.</summary>
        public int SpawnedNPCCount => m_SpawnedNPCs.Count;

        /// <summary>All spawned players.</summary>
        public IReadOnlyDictionary<ulong, NetworkCharacter> SpawnedPlayers => m_SpawnedPlayers;

        /// <summary>All spawned NPCs.</summary>
        public IReadOnlyList<NetworkCharacter> SpawnedNPCs => m_SpawnedNPCs;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[NetworkSpawnManager] Duplicate instance detected. Destroying.");
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        private void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                // Subscribe to client connections for auto-spawn
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

                Debug.Log("[NetworkSpawnManager] Server started, listening for client connections");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            base.OnNetworkDespawn();
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            if (!IsServer) return;

            ProcessSpawnQueue();
        }

        // CLIENT CALLBACKS: ----------------------------------------------------------------------

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkSpawnManager] Client {clientId} connected");

            if (m_AutoSpawnPlayer)
            {
                // Delay spawn slightly to ensure client is fully ready
                StartCoroutine(DelayedPlayerSpawn(clientId));
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer) return;

            Debug.Log($"[NetworkSpawnManager] Client {clientId} disconnected");

            // Clean up spawned player reference
            if (m_SpawnedPlayers.TryGetValue(clientId, out var player))
            {
                m_SpawnedPlayers.Remove(clientId);
                // NetworkObject handles despawning automatically
            }
        }

        private System.Collections.IEnumerator DelayedPlayerSpawn(ulong clientId)
        {
            yield return new WaitForSeconds(m_PlayerSpawnDelay);

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                yield break;

            // Check if client is still connected
            if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
                yield break;

            // Check if player already spawned (e.g., by manual spawn)
            if (m_SpawnedPlayers.ContainsKey(clientId))
                yield break;

            SpawnPlayerForClient(clientId);
        }

        // PUBLIC SPAWN METHODS: ------------------------------------------------------------------

        /// <summary>
        /// Spawn a player for a specific client.
        /// Server-only.
        /// </summary>
        public NetworkCharacter SpawnPlayerForClient(ulong clientId, Vector3? position = null, Quaternion? rotation = null)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkSpawnManager] Only server can spawn players");
                return null;
            }

            if (m_PlayerPrefab == null)
            {
                Debug.LogError("[NetworkSpawnManager] Player prefab is not set");
                return null;
            }

            // Get spawn point
            Vector3 spawnPos = position ?? GetNextSpawnPoint();
            Quaternion spawnRot = rotation ?? Quaternion.identity;

            var spawnData = new SpawnData
            {
                Prefab = m_PlayerPrefab,
                Position = spawnPos,
                Rotation = spawnRot,
                OwnerClientId = clientId,
                Type = SpawnType.Player
            };

            // Allow modification before spawn
            EventBeforePlayerSpawn?.Invoke(clientId, spawnData);

            // Instantiate and spawn
            GameObject instance = Instantiate(spawnData.Prefab, spawnData.Position, spawnData.Rotation);
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError("[NetworkSpawnManager] Player prefab must have NetworkObject component");
                Destroy(instance);
                return null;
            }

            // Spawn with ownership
            networkObject.SpawnAsPlayerObject(clientId);

            // Get NetworkCharacter
            NetworkCharacter networkCharacter = instance.GetComponent<NetworkCharacter>();
            if (networkCharacter != null)
            {
                m_SpawnedPlayers[clientId] = networkCharacter;
                EventAfterPlayerSpawn?.Invoke(clientId, networkCharacter);
            }

            Debug.Log($"[NetworkSpawnManager] Spawned player for client {clientId} at {spawnPos}");

            // Check if all players spawned
            CheckAllPlayersSpawned();

            return networkCharacter;
        }

        /// <summary>
        /// Queue an NPC for spawning. Uses spawn queue to prevent flooding.
        /// Server-only.
        /// </summary>
        public void QueueNPCSpawn(GameObject prefab, Vector3 position, Quaternion rotation, Action<NetworkCharacter> callback = null)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkSpawnManager] Only server can spawn NPCs");
                return;
            }

            var request = new SpawnRequest
            {
                Data = new SpawnData
                {
                    Prefab = prefab,
                    Position = position,
                    Rotation = rotation,
                    OwnerClientId = NetworkManager.ServerClientId,
                    Type = SpawnType.NPC
                },
                Callback = callback
            };

            m_SpawnQueue.Enqueue(request);
        }

        /// <summary>
        /// Spawn an NPC immediately (bypasses queue).
        /// Server-only.
        /// </summary>
        public NetworkCharacter SpawnNPCImmediate(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkSpawnManager] Only server can spawn NPCs");
                return null;
            }

            var spawnData = new SpawnData
            {
                Prefab = prefab,
                Position = position,
                Rotation = rotation,
                OwnerClientId = NetworkManager.ServerClientId,
                Type = SpawnType.NPC
            };

            return SpawnNPCInternal(spawnData);
        }

        /// <summary>
        /// Despawn and destroy a network character.
        /// Server-only.
        /// </summary>
        public void DespawnCharacter(NetworkCharacter character)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkSpawnManager] Only server can despawn characters");
                return;
            }

            if (character == null) return;

            var networkObject = character.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
                // Remove from tracking
                if (character.IsPlayer)
                {
                    var clientId = networkObject.OwnerClientId;
                    m_SpawnedPlayers.Remove(clientId);
                }
                else
                {
                    m_SpawnedNPCs.Remove(character);
                }

                networkObject.Despawn();
            }
        }

        /// <summary>
        /// Despawn all NPCs.
        /// Server-only.
        /// </summary>
        public void DespawnAllNPCs()
        {
            if (!IsServer) return;

            var npcsToRemove = new List<NetworkCharacter>(m_SpawnedNPCs);
            foreach (var npc in npcsToRemove)
            {
                DespawnCharacter(npc);
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ProcessSpawnQueue()
        {
            if (m_SpawnQueue.Count == 0) return;

            // Check spawn delay
            if (Time.time - m_LastNPCSpawnTime < m_NPCSpawnDelay) return;

            int spawnsThisFrame = 0;
            while (m_SpawnQueue.Count > 0 && spawnsThisFrame < m_MaxNPCsPerFrame)
            {
                var request = m_SpawnQueue.Dequeue();
                var character = SpawnNPCInternal(request.Data);
                request.Callback?.Invoke(character);
                spawnsThisFrame++;
            }

            if (spawnsThisFrame > 0)
            {
                m_LastNPCSpawnTime = Time.time;
            }
        }

        private NetworkCharacter SpawnNPCInternal(SpawnData data)
        {
            if (data.Prefab == null)
            {
                Debug.LogError("[NetworkSpawnManager] NPC prefab is null");
                return null;
            }

            // Allow modification before spawn
            EventBeforeNPCSpawn?.Invoke(data);

            // Instantiate
            GameObject instance = Instantiate(data.Prefab, data.Position, data.Rotation);
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();

            if (networkObject == null)
            {
                Debug.LogError("[NetworkSpawnManager] NPC prefab must have NetworkObject component");
                Destroy(instance);
                return null;
            }

            // Spawn (server-owned)
            networkObject.Spawn();

            // Get NetworkCharacter
            NetworkCharacter networkCharacter = instance.GetComponent<NetworkCharacter>();
            if (networkCharacter != null)
            {
                m_SpawnedNPCs.Add(networkCharacter);
                EventAfterNPCSpawn?.Invoke(networkCharacter);
            }

            Debug.Log($"[NetworkSpawnManager] Spawned NPC '{data.Prefab.name}' at {data.Position}");

            return networkCharacter;
        }

        private Vector3 GetNextSpawnPoint()
        {
            if (m_PlayerSpawnPoints == null || m_PlayerSpawnPoints.Length == 0)
            {
                return Vector3.zero;
            }

            Transform spawnPoint = null;

            switch (m_SpawnPointMode)
            {
                case SpawnPointMode.RoundRobin:
                    spawnPoint = m_PlayerSpawnPoints[m_NextSpawnPointIndex];
                    m_NextSpawnPointIndex = (m_NextSpawnPointIndex + 1) % m_PlayerSpawnPoints.Length;
                    break;

                case SpawnPointMode.Random:
                    spawnPoint = m_PlayerSpawnPoints[UnityEngine.Random.Range(0, m_PlayerSpawnPoints.Length)];
                    break;

                case SpawnPointMode.ByClientId:
                    int index = (int)(NetworkManager.Singleton.ConnectedClientsIds.Count % m_PlayerSpawnPoints.Length);
                    spawnPoint = m_PlayerSpawnPoints[index];
                    break;

                default:
                    spawnPoint = m_PlayerSpawnPoints[0];
                    break;
            }

            return spawnPoint != null ? spawnPoint.position : Vector3.zero;
        }

        private void CheckAllPlayersSpawned()
        {
            if (!m_WaitForAllPlayers) return;

            int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
            if (m_SpawnedPlayers.Count >= connectedCount)
            {
                EventAllPlayersSpawned?.Invoke();
                Debug.Log($"[NetworkSpawnManager] All {connectedCount} players spawned");
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
