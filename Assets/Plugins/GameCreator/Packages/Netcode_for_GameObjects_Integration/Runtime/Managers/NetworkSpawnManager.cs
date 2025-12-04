using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Manages network spawning with proper timing, naming, and coordination.
    /// Features:
    /// - Unique names per prefab type (Guard_001, Guard_002, Enemy_001, etc.)
    /// - Name synchronization across all clients
    /// - Spawn queue to prevent flooding
    /// - Player spawn coordination
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
        private readonly Dictionary<string, int> m_PrefabSpawnCounters = new();
        private int m_NextSpawnPointIndex;
        private float m_LastNPCSpawnTime;

        // EVENTS: --------------------------------------------------------------------------------

        public static event Action<ulong, SpawnData> EventBeforePlayerSpawn;
        public static event Action<ulong, NetworkCharacter> EventAfterPlayerSpawn;
        public static event Action<SpawnData> EventBeforeNPCSpawn;
        public static event Action<NetworkCharacter> EventAfterNPCSpawn;
        public static event Action EventAllPlayersSpawned;

        // ENUMS: ---------------------------------------------------------------------------------

        public enum SpawnPointMode
        {
            RoundRobin,
            Random,
            Closest,
            ByClientId,
        }

        public enum SpawnType
        {
            Player,
            NPC,
        }

        // STRUCTS: -------------------------------------------------------------------------------

        public struct SpawnData
        {
            public GameObject Prefab;
            public Vector3 Position;
            public Quaternion Rotation;
            public ulong OwnerClientId;
            public SpawnType Type;
            public string CustomName;
        }

        private struct SpawnRequest
        {
            public SpawnData Data;
            public Action<NetworkCharacter> Callback;
        }

        // PROPERTIES: ----------------------------------------------------------------------------

        public GameObject PlayerPrefab
        {
            get => m_PlayerPrefab;
            set => m_PlayerPrefab = value;
        }

        public int SpawnedPlayerCount => m_SpawnedPlayers.Count;
        public int SpawnedNPCCount => m_SpawnedNPCs.Count;
        public IReadOnlyDictionary<ulong, NetworkCharacter> SpawnedPlayers => m_SpawnedPlayers;
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

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                Debug.Log("[NetworkSpawnManager] Server started");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer && NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            m_PrefabSpawnCounters.Clear();
            base.OnNetworkDespawn();
        }

        // UPDATE: --------------------------------------------------------------------------------

        private void Update()
        {
            if (!IsServer)
                return;

            ProcessSpawnQueue();
        }

        // CLIENT CALLBACKS: ----------------------------------------------------------------------

        private void OnClientConnected(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"[NetworkSpawnManager] Client {clientId} connected");

            if (m_AutoSpawnPlayer)
            {
                StartCoroutine(DelayedPlayerSpawn(clientId));
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (!IsServer)
                return;

            Debug.Log($"[NetworkSpawnManager] Client {clientId} disconnected");

            if (m_SpawnedPlayers.TryGetValue(clientId, out var player))
            {
                m_SpawnedPlayers.Remove(clientId);
            }
        }

        private System.Collections.IEnumerator DelayedPlayerSpawn(ulong clientId)
        {
            yield return new WaitForSeconds(m_PlayerSpawnDelay);

            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                yield break;

            if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(clientId))
                yield break;

            if (m_SpawnedPlayers.ContainsKey(clientId))
                yield break;

            SpawnPlayerForClient(clientId);
        }

        // PUBLIC SPAWN METHODS: ------------------------------------------------------------------

        /// <summary>
        /// Spawn a player for a specific client. Server-only.
        /// </summary>
        public NetworkCharacter SpawnPlayerForClient(
            ulong clientId,
            Vector3? position = null,
            Quaternion? rotation = null
        )
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

            Vector3 spawnPos = position ?? GetNextSpawnPoint();
            Quaternion spawnRot = rotation ?? Quaternion.identity;

            // Generate unique player name
            bool isHost = clientId == NetworkManager.ServerClientId;
            string playerName = isHost ? "Player_Host" : $"Player_Client{clientId}";

            var spawnData = new SpawnData
            {
                Prefab = m_PlayerPrefab,
                Position = spawnPos,
                Rotation = spawnRot,
                OwnerClientId = clientId,
                Type = SpawnType.Player,
                CustomName = playerName,
            };

            EventBeforePlayerSpawn?.Invoke(clientId, spawnData);

            GameObject instance = Instantiate(spawnData.Prefab, spawnData.Position, spawnData.Rotation);
            instance.name = spawnData.CustomName;

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[NetworkSpawnManager] Player prefab must have NetworkObject");
                Destroy(instance);
                return null;
            }

            networkObject.SpawnAsPlayerObject(clientId);

            // Sync name to all clients
            SyncObjectNameClientRpc(networkObject.NetworkObjectId, spawnData.CustomName);

            NetworkCharacter networkCharacter = instance.GetComponent<NetworkCharacter>();
            if (networkCharacter != null)
            {
                m_SpawnedPlayers[clientId] = networkCharacter;
                EventAfterPlayerSpawn?.Invoke(clientId, networkCharacter);
            }

            Debug.Log($"[NetworkSpawnManager] Spawned '{spawnData.CustomName}' at {spawnPos}");
            CheckAllPlayersSpawned();

            return networkCharacter;
        }

        /// <summary>
        /// Queue an NPC for spawning. Server-only.
        /// </summary>
        public void QueueNPCSpawn(
            GameObject prefab,
            Vector3 position,
            Quaternion rotation,
            Action<NetworkCharacter> callback = null
        )
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
                    Type = SpawnType.NPC,
                },
                Callback = callback,
            };

            m_SpawnQueue.Enqueue(request);
        }

        /// <summary>
        /// Spawn an NPC immediately. Server-only.
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
                Type = SpawnType.NPC,
            };

            return SpawnNPCInternal(spawnData);
        }

        /// <summary>
        /// Despawn a network character. Server-only.
        /// </summary>
        public void DespawnCharacter(NetworkCharacter character)
        {
            if (!IsServer)
            {
                Debug.LogError("[NetworkSpawnManager] Only server can despawn");
                return;
            }

            if (character == null)
                return;

            var networkObject = character.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.IsSpawned)
            {
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
        /// Despawn all NPCs. Server-only.
        /// </summary>
        public void DespawnAllNPCs()
        {
            if (!IsServer)
                return;

            var npcsToRemove = new List<NetworkCharacter>(m_SpawnedNPCs);
            foreach (var npc in npcsToRemove)
            {
                DespawnCharacter(npc);
            }

            m_PrefabSpawnCounters.Clear();
        }

        /// <summary>
        /// Get the current spawn count for a prefab type.
        /// </summary>
        public int GetSpawnCountForPrefab(string prefabName)
        {
            return m_PrefabSpawnCounters.TryGetValue(prefabName, out int count) ? count : 0;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ProcessSpawnQueue()
        {
            if (m_SpawnQueue.Count == 0)
                return;

            if (Time.time - m_LastNPCSpawnTime < m_NPCSpawnDelay)
                return;

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

            // Generate unique name per prefab type
            string baseName = data.Prefab.name;
            if (!m_PrefabSpawnCounters.TryGetValue(baseName, out int counter))
            {
                counter = 0;
            }

            counter++;
            m_PrefabSpawnCounters[baseName] = counter;

            string uniqueName = $"{baseName}_{counter:D3}";

            EventBeforeNPCSpawn?.Invoke(data);

            GameObject instance = Instantiate(data.Prefab, data.Position, data.Rotation);
            instance.name = uniqueName;

            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError("[NetworkSpawnManager] NPC prefab must have NetworkObject");
                Destroy(instance);
                return null;
            }

            networkObject.Spawn();

            // Sync name to all clients
            SyncObjectNameClientRpc(networkObject.NetworkObjectId, uniqueName);

            NetworkCharacter networkCharacter = instance.GetComponent<NetworkCharacter>();
            if (networkCharacter != null)
            {
                m_SpawnedNPCs.Add(networkCharacter);
                EventAfterNPCSpawn?.Invoke(networkCharacter);
            }

            Debug.Log($"[NetworkSpawnManager] Spawned NPC '{uniqueName}' at {data.Position}");

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
            if (!m_WaitForAllPlayers)
                return;

            int connectedCount = NetworkManager.Singleton.ConnectedClientsIds.Count;
            if (m_SpawnedPlayers.Count >= connectedCount)
            {
                EventAllPlayersSpawned?.Invoke();
                Debug.Log($"[NetworkSpawnManager] All {connectedCount} players spawned");
            }
        }

        // RPCS: ----------------------------------------------------------------------------------

        [Rpc(SendTo.ClientsAndHost)]
        private void SyncObjectNameClientRpc(ulong networkObjectId, string objectName)
        {
            // Find the NetworkObject and update its name
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                    networkObjectId,
                    out NetworkObject networkObject
                ))
            {
                networkObject.gameObject.name = objectName;
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
