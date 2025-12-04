using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// NetworkPerception synchronizes GameCreator Perception state across the network.
    /// Key features:
    /// - Server-authoritative awareness levels
    /// - Synchronized tracker data (per-target awareness)
    /// - Evidence state replication
    /// - Global event broadcasting for visual scripting
    ///
    /// Usage:
    /// - Add to any GameObject with Perception component that needs network sync
    /// - NPCs: Server owns perception, all clients see synced awareness
    /// - Players: Owner controls perception (for local predictions)
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Perception")]
    [Icon(RuntimePaths.GIZMOS + "GizmoPerception.png")]
    [RequireComponent(typeof(NetworkObject))]
    [DisallowMultipleComponent]
    public class NetworkPerception : NetworkBehaviour
    {
        // CONSTANTS: -----------------------------------------------------------------------------

        private const float SYNC_THRESHOLD = 0.01f; // Minimum awareness change to trigger sync
        private const float SYNC_INTERVAL = 0.1f;   // Minimum time between syncs

        // SERIALIZABLE STRUCTS: ------------------------------------------------------------------

        /// <summary>
        /// Network-serializable data for tracked target awareness.
        /// </summary>
        public struct TrackedTargetData : INetworkSerializable, IEquatable<TrackedTargetData>
        {
            public ulong TargetNetworkId;
            public float Awareness;
            public byte Stage;
            public float LastIncreaseTime;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref TargetNetworkId);
                serializer.SerializeValue(ref Awareness);
                serializer.SerializeValue(ref Stage);
                serializer.SerializeValue(ref LastIncreaseTime);
            }

            public bool Equals(TrackedTargetData other) =>
                TargetNetworkId == other.TargetNetworkId;

            public override int GetHashCode() => TargetNetworkId.GetHashCode();

            public override bool Equals(object obj) =>
                obj is TrackedTargetData other && Equals(other);
        }

        /// <summary>
        /// Network-serializable data for evidence state.
        /// </summary>
        public struct EvidenceData : INetworkSerializable, IEquatable<EvidenceData>
        {
            public FixedString32Bytes EvidenceTag;
            public bool IsTampered;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref EvidenceTag);
                serializer.SerializeValue(ref IsTampered);
            }

            public bool Equals(EvidenceData other) =>
                EvidenceTag.Equals(other.EvidenceTag);

            public override int GetHashCode() => EvidenceTag.GetHashCode();

            public override bool Equals(object obj) =>
                obj is EvidenceData other && Equals(other);
        }

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Perception m_Perception;
        [NonSerialized] private NetworkObject m_NetworkObject;
        [NonSerialized] private bool m_IsInitialized;
        [NonSerialized] private bool m_EventsHooked;
        [NonSerialized] private float m_LastSyncTime;

        // Cache for tracking local awareness changes
        [NonSerialized] private Dictionary<ulong, float> m_LastSyncedAwareness = new();
        [NonSerialized] private Dictionary<ulong, AwareStage> m_LastSyncedStage = new();

        // NETWORK VARIABLES: ---------------------------------------------------------------------

        private NetworkList<TrackedTargetData> m_TrackedTargets;
        private NetworkList<EvidenceData> m_Evidences;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// The local Perception component this NetworkPerception synchronizes.
        /// </summary>
        public Perception Perception => m_Perception;

        /// <summary>
        /// The NetworkObject component on this GameObject.
        /// </summary>
        public NetworkObject NetworkObject => m_NetworkObject;

        /// <summary>
        /// True if this perception is server-authoritative (typical for NPCs).
        /// </summary>
        public bool IsServerAuthoritative => IsServer || (IsSpawned && !IsOwner);

        /// <summary>
        /// Number of tracked targets.
        /// </summary>
        public int TrackedTargetCount => m_TrackedTargets?.Count ?? 0;

        /// <summary>
        /// Get tracked target data by index.
        /// </summary>
        public TrackedTargetData GetTrackedTarget(int index) => m_TrackedTargets[index];

        /// <summary>
        /// Number of evidence entries.
        /// </summary>
        public int EvidenceCount => m_Evidences?.Count ?? 0;

        /// <summary>
        /// Get evidence data by index.
        /// </summary>
        public EvidenceData GetEvidence(int index) => m_Evidences[index];

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            m_TrackedTargets = new NetworkList<TrackedTargetData>();
            m_Evidences = new NetworkList<EvidenceData>();

            m_Perception = GetComponent<Perception>();
            m_NetworkObject = GetComponent<NetworkObject>();

            if (m_Perception == null)
            {
                Debug.LogError($"[NetworkPerception] {gameObject.name}: Missing Perception component!");
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Register with global registry
            NetworkPerceptionRegistry.Register(this);

            // Hook into local Perception events
            HookPerceptionEvents();

            m_IsInitialized = true;

            if (IsServer)
            {
                // Server: Push current local state to NetworkLists
                SyncFromLocalPerception();
                Debug.Log($"[NetworkPerception] {gameObject.name}: Spawned as server-authoritative");
            }
            else
            {
                // Client: Subscribe to NetworkList changes
                m_TrackedTargets.OnListChanged += OnTrackedTargetsChanged;
                m_Evidences.OnListChanged += OnEvidencesChanged;

                // Apply current networked state to local Perception
                ApplyNetworkStateToLocal();
                Debug.Log($"[NetworkPerception] {gameObject.name}: Spawned as client");
            }
        }

        public override void OnNetworkDespawn()
        {
            // Unhook events
            UnhookPerceptionEvents();

            if (!IsServer)
            {
                m_TrackedTargets.OnListChanged -= OnTrackedTargetsChanged;
                m_Evidences.OnListChanged -= OnEvidencesChanged;
            }

            // Unregister from global registry
            NetworkPerceptionRegistry.Unregister(this);

            m_IsInitialized = false;

            Debug.Log($"[NetworkPerception] {gameObject.name}: Despawned");

            base.OnNetworkDespawn();
        }

        // EVENT HOOKS: ---------------------------------------------------------------------------

        private void HookPerceptionEvents()
        {
            if (m_Perception == null || m_EventsHooked) return;

            m_Perception.EventChangeAwarenessLevel += OnLocalAwarenessLevelChange;
            m_Perception.EventChangeAwarenessStage += OnLocalAwarenessStageChange;
            m_Perception.EventTrack += OnLocalTrack;
            m_Perception.EventUntrack += OnLocalUntrack;
            m_Perception.EventNoticeEvidence += OnLocalNoticeEvidence;

            m_EventsHooked = true;
        }

        private void UnhookPerceptionEvents()
        {
            if (m_Perception == null || !m_EventsHooked) return;

            m_Perception.EventChangeAwarenessLevel -= OnLocalAwarenessLevelChange;
            m_Perception.EventChangeAwarenessStage -= OnLocalAwarenessStageChange;
            m_Perception.EventTrack -= OnLocalTrack;
            m_Perception.EventUntrack -= OnLocalUntrack;
            m_Perception.EventNoticeEvidence -= OnLocalNoticeEvidence;

            m_EventsHooked = false;
        }

        // LOCAL EVENT HANDLERS: ------------------------------------------------------------------

        private void OnLocalAwarenessLevelChange(GameObject target, float level)
        {
            if (!IsServer || target == null) return;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return;

            // Check if change exceeds threshold
            if (m_LastSyncedAwareness.TryGetValue(targetNetworkId, out float lastLevel))
            {
                if (Mathf.Abs(level - lastLevel) < SYNC_THRESHOLD) return;
            }

            // Update NetworkList
            UpdateTrackedTargetInList(targetNetworkId, level);
            m_LastSyncedAwareness[targetNetworkId] = level;

            // Broadcast to all clients
            var stage = Tracker.GetStage(level);
            NotifyAwarenessChangeRpc(targetNetworkId, level, (byte)stage);
        }

        private void OnLocalAwarenessStageChange(GameObject target, AwareStage stage)
        {
            if (!IsServer || target == null) return;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return;

            // Check if stage actually changed
            if (m_LastSyncedStage.TryGetValue(targetNetworkId, out var lastStage))
            {
                if (stage == lastStage) return;
            }

            m_LastSyncedStage[targetNetworkId] = stage;

            // Broadcast stage change to all clients
            NotifyAwarenessStageChangeRpc(targetNetworkId, (byte)stage);
        }

        private void OnLocalTrack(GameObject target)
        {
            if (!IsServer || target == null) return;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return;

            // Add to NetworkList if not exists
            if (!HasTrackedTarget(targetNetworkId))
            {
                m_TrackedTargets.Add(new TrackedTargetData
                {
                    TargetNetworkId = targetNetworkId,
                    Awareness = 0f,
                    Stage = (byte)AwareStage.None,
                    LastIncreaseTime = Time.time
                });
            }

            // Broadcast
            NotifyTargetTrackedRpc(targetNetworkId);
        }

        private void OnLocalUntrack(GameObject target)
        {
            if (!IsServer || target == null) return;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return;

            // Remove from NetworkList
            RemoveTrackedTarget(targetNetworkId);

            // Broadcast
            NotifyTargetUntrackedRpc(targetNetworkId);
        }

        private void OnLocalNoticeEvidence(GameObject evidence)
        {
            if (!IsServer || evidence == null) return;

            var evidenceNetworkId = GetNetworkIdForGameObject(evidence);
            if (evidenceNetworkId == 0) return;

            NotifyEvidenceNoticedRpc(evidenceNetworkId);
        }

        // SERVER RPCS: ---------------------------------------------------------------------------

        /// <summary>
        /// Request to set awareness level for a target (called by clients or server).
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void SetAwarenessRpc(ulong targetNetworkId, float awareness, RpcParams rpcParams = default)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            // Apply to local Perception
            if (m_Perception != null)
            {
                m_Perception.SetAwareness(target, awareness);
            }
        }

        /// <summary>
        /// Request to add awareness for a target.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void AddAwarenessRpc(ulong targetNetworkId, float delta, float maxAwareness = 1f, RpcParams rpcParams = default)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            if (m_Perception != null)
            {
                m_Perception.AddAwareness(target, delta, maxAwareness);
            }
        }

        /// <summary>
        /// Request to start tracking a target.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void TrackTargetRpc(ulong targetNetworkId, RpcParams rpcParams = default)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            if (m_Perception != null)
            {
                m_Perception.Track(target);
            }
        }

        /// <summary>
        /// Request to stop tracking a target.
        /// </summary>
        [Rpc(SendTo.Server, RequireOwnership = false)]
        public void UntrackTargetRpc(ulong targetNetworkId, RpcParams rpcParams = default)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            if (m_Perception != null)
            {
                m_Perception.Untrack(target);
            }
        }

        // CLIENT RPCS: ---------------------------------------------------------------------------

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyAwarenessChangeRpc(ulong targetNetworkId, float level, byte stage)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            // Broadcast to global event system for visual scripting
            NetworkPerceptionEvents.NotifyAwarenessChanged(m_Perception, target, level);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyAwarenessStageChangeRpc(ulong targetNetworkId, byte stage)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            NetworkPerceptionEvents.NotifyAwarenessStageChanged(m_Perception, target, (AwareStage)stage);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyTargetTrackedRpc(ulong targetNetworkId)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            NetworkPerceptionEvents.NotifyTargetTracked(m_Perception, target);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyTargetUntrackedRpc(ulong targetNetworkId)
        {
            var target = GetGameObjectForNetworkId(targetNetworkId);
            if (target == null) return;

            NetworkPerceptionEvents.NotifyTargetUntracked(m_Perception, target);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void NotifyEvidenceNoticedRpc(ulong evidenceNetworkId)
        {
            var evidence = GetGameObjectForNetworkId(evidenceNetworkId);
            if (evidence == null) return;

            var evidenceComponent = evidence.GetComponent<Evidence>();
            NetworkPerceptionEvents.NotifyEvidenceNoticed(m_Perception, evidenceComponent);
        }

        // NETWORK LIST CALLBACKS: ----------------------------------------------------------------

        private void OnTrackedTargetsChanged(NetworkListEvent<TrackedTargetData> changeEvent)
        {
            // Client-side: apply changes to local Perception
            if (IsServer) return;

            switch (changeEvent.Type)
            {
                case NetworkListEvent<TrackedTargetData>.EventType.Add:
                    ApplyTrackedTargetToLocal(changeEvent.Value);
                    break;

                case NetworkListEvent<TrackedTargetData>.EventType.Remove:
                    RemoveTrackedTargetFromLocal(changeEvent.Value);
                    break;

                case NetworkListEvent<TrackedTargetData>.EventType.Value:
                    ApplyTrackedTargetToLocal(changeEvent.Value);
                    break;
            }
        }

        private void OnEvidencesChanged(NetworkListEvent<EvidenceData> changeEvent)
        {
            // Client-side: handle evidence state changes
            if (IsServer) return;

            // Evidence sync handled through events, not direct application
        }

        // HELPER METHODS: ------------------------------------------------------------------------

        private void SyncFromLocalPerception()
        {
            if (m_Perception == null) return;

            // Sync tracked targets
            foreach (var kvp in m_Perception.TrackerList)
            {
                var tracker = kvp.Value;
                if (tracker.Target == null) continue;

                var targetNetworkId = GetNetworkIdForGameObject(tracker.Target);
                if (targetNetworkId == 0) continue;

                m_TrackedTargets.Add(new TrackedTargetData
                {
                    TargetNetworkId = targetNetworkId,
                    Awareness = tracker.Awareness,
                    Stage = (byte)tracker.Stage,
                    LastIncreaseTime = Time.time
                });

                m_LastSyncedAwareness[targetNetworkId] = tracker.Awareness;
                m_LastSyncedStage[targetNetworkId] = tracker.Stage;
            }
        }

        private void ApplyNetworkStateToLocal()
        {
            if (m_Perception == null) return;

            foreach (var data in m_TrackedTargets)
            {
                ApplyTrackedTargetToLocal(data);
            }
        }

        private void ApplyTrackedTargetToLocal(TrackedTargetData data)
        {
            if (m_Perception == null) return;

            var target = GetGameObjectForNetworkId(data.TargetNetworkId);
            if (target == null) return;

            // Ensure target is tracked
            if (!m_Perception.IsTracking(target))
            {
                m_Perception.Track(target);
            }

            // Apply awareness level
            m_Perception.SetAwareness(target, data.Awareness);
        }

        private void RemoveTrackedTargetFromLocal(TrackedTargetData data)
        {
            if (m_Perception == null) return;

            var target = GetGameObjectForNetworkId(data.TargetNetworkId);
            if (target == null) return;

            if (m_Perception.IsTracking(target))
            {
                m_Perception.Untrack(target);
            }
        }

        private bool HasTrackedTarget(ulong targetNetworkId)
        {
            foreach (var data in m_TrackedTargets)
            {
                if (data.TargetNetworkId == targetNetworkId)
                    return true;
            }
            return false;
        }

        private void UpdateTrackedTargetInList(ulong targetNetworkId, float awareness)
        {
            for (int i = 0; i < m_TrackedTargets.Count; i++)
            {
                if (m_TrackedTargets[i].TargetNetworkId == targetNetworkId)
                {
                    var data = m_TrackedTargets[i];
                    data.Awareness = awareness;
                    data.Stage = (byte)Tracker.GetStage(awareness);
                    data.LastIncreaseTime = Time.time;
                    m_TrackedTargets[i] = data;
                    return;
                }
            }

            // Not found, add new
            m_TrackedTargets.Add(new TrackedTargetData
            {
                TargetNetworkId = targetNetworkId,
                Awareness = awareness,
                Stage = (byte)Tracker.GetStage(awareness),
                LastIncreaseTime = Time.time
            });
        }

        private void RemoveTrackedTarget(ulong targetNetworkId)
        {
            for (int i = m_TrackedTargets.Count - 1; i >= 0; i--)
            {
                if (m_TrackedTargets[i].TargetNetworkId == targetNetworkId)
                {
                    m_TrackedTargets.RemoveAt(i);
                    m_LastSyncedAwareness.Remove(targetNetworkId);
                    m_LastSyncedStage.Remove(targetNetworkId);
                    return;
                }
            }
        }

        private ulong GetNetworkIdForGameObject(GameObject go)
        {
            if (go == null) return 0;

            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
                return 0;

            return networkObject.NetworkObjectId;
        }

        private GameObject GetGameObjectForNetworkId(ulong networkId)
        {
            if (NetworkManager.Singleton == null) return null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
            {
                return networkObject.gameObject;
            }

            return null;
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Get the synchronized awareness level for a target.
        /// </summary>
        public float GetAwareness(GameObject target)
        {
            if (target == null) return 0f;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return 0f;

            foreach (var data in m_TrackedTargets)
            {
                if (data.TargetNetworkId == targetNetworkId)
                    return data.Awareness;
            }

            return 0f;
        }

        /// <summary>
        /// Get the synchronized awareness stage for a target.
        /// </summary>
        public AwareStage GetAwarenessStage(GameObject target)
        {
            if (target == null) return AwareStage.None;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            if (targetNetworkId == 0) return AwareStage.None;

            foreach (var data in m_TrackedTargets)
            {
                if (data.TargetNetworkId == targetNetworkId)
                    return (AwareStage)data.Stage;
            }

            return AwareStage.None;
        }

        /// <summary>
        /// Check if a target is being tracked (network-synchronized).
        /// </summary>
        public bool IsTracking(GameObject target)
        {
            if (target == null) return false;

            var targetNetworkId = GetNetworkIdForGameObject(target);
            return HasTrackedTarget(targetNetworkId);
        }
    }
}
