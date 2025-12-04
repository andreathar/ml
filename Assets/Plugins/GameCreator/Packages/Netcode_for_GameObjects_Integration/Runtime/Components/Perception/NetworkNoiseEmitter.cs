using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Perception
{
    /// <summary>
    /// NetworkNoiseEmitter handles network-synchronized noise emission.
    /// When a client emits noise, it sends a ServerRpc to validate and broadcast
    /// the noise to all clients, ensuring NPCs on all machines respond consistently.
    ///
    /// Usage:
    /// - Spawn this component once in the scene (typically on NetworkManagers GameObject)
    /// - Use InstructionNetworkEmitNoise or call NetworkNoiseEmitter.Instance.EmitNoise() directly
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Noise Emitter")]
    [Icon(RuntimePaths.GIZMOS + "GizmoPerception.png")]
    [RequireComponent(typeof(NetworkObject))]
    [DisallowMultipleComponent]
    public class NetworkNoiseEmitter : NetworkBehaviour
    {
        // CONSTANTS: -----------------------------------------------------------------------------

        private const float MIN_RADIUS = 0.1f;
        private const float MAX_RADIUS = 100f;
        private const float MIN_INTENSITY = 0f;
        private const float MAX_INTENSITY = 10f;
        private const int MAX_EMISSIONS_PER_SECOND = 10;
        private const float RATE_LIMIT_WINDOW = 1f;

        // SINGLETON: -----------------------------------------------------------------------------

        private static NetworkNoiseEmitter s_Instance;

        /// <summary>
        /// Singleton instance of the NetworkNoiseEmitter.
        /// </summary>
        public static NetworkNoiseEmitter Instance => s_Instance;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Rate Limiting")]
        [SerializeField]
        [Tooltip("Maximum noise emissions per second per client")]
        private int m_MaxEmissionsPerSecond = MAX_EMISSIONS_PER_SECOND;

        [SerializeField]
        [Tooltip("Enable rate limiting to prevent spam")]
        private bool m_EnableRateLimiting = true;

        [Header("Debug")]
        [SerializeField]
        [Tooltip("Log noise emissions to console")]
        private bool m_DebugLogging = true;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Dictionary<ulong, List<float>> m_ClientEmissionTimes = new();

        [NonSerialized]
        private List<ISpatialHash> m_PerceptionCache = new();

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Fired when a noise is emitted through the network.
        /// Parameters: (Vector3 position, float radius, string tag, float intensity, ulong sourceClientId)
        /// </summary>
        public static event Action<Vector3, float, string, float, ulong> EventNoiseEmitted;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            if (s_Instance != null && s_Instance != this)
            {
                Debug.LogWarning("[NetworkNoiseEmitter] Duplicate instance detected, destroying");
                Destroy(gameObject);
                return;
            }

            s_Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (s_Instance == null)
            {
                s_Instance = this;
            }

            if (this.m_DebugLogging)
            {
                Debug.Log($"[NetworkNoiseEmitter] Spawned (IsServer={IsServer}, IsHost={IsHost})");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }

            this.m_ClientEmissionTimes.Clear();

            base.OnNetworkDespawn();
        }

        protected new void OnDestroy()
        {
            if (s_Instance == this)
            {
                s_Instance = null;
            }
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Emit a noise that will be synchronized across all clients.
        /// Call this from any client - it will be validated by the server and broadcast.
        /// </summary>
        /// <param name="position">World position of the noise</param>
        /// <param name="radius">Radius of the noise effect</param>
        /// <param name="tag">Tag identifier for the noise type</param>
        /// <param name="intensity">Intensity of the noise (0-10)</param>
        public void EmitNoise(Vector3 position, float radius, string tag, float intensity)
        {
            if (!IsSpawned)
            {
                Debug.LogWarning("[NetworkNoiseEmitter] Cannot emit noise - not spawned");
                return;
            }

            // Clamp values
            radius = Mathf.Clamp(radius, MIN_RADIUS, MAX_RADIUS);
            intensity = Mathf.Clamp(intensity, MIN_INTENSITY, MAX_INTENSITY);

            // Convert tag to fixed string for network serialization
            FixedString32Bytes fixedTag = new FixedString32Bytes();
            if (!string.IsNullOrEmpty(tag))
            {
                fixedTag = new FixedString32Bytes(tag.Length > 29 ? tag.Substring(0, 29) : tag);
            }

            // Send to server for validation and broadcast
            EmitNoiseServerRpc(position, radius, fixedTag, intensity);
        }

        /// <summary>
        /// Check if a noise emission would be rate limited.
        /// </summary>
        public bool WouldBeRateLimited(ulong clientId)
        {
            if (!this.m_EnableRateLimiting)
                return false;

            if (!this.m_ClientEmissionTimes.TryGetValue(clientId, out var times))
                return false;

            CleanupOldEmissions(times);
            return times.Count >= this.m_MaxEmissionsPerSecond;
        }

        // SERVER RPCS: ---------------------------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void EmitNoiseServerRpc(
            Vector3 position,
            float radius,
            FixedString32Bytes tag,
            float intensity,
            RpcParams rpcParams = default
        )
        {
            ulong senderClientId = rpcParams.Receive.SenderClientId;

            // Rate limit check
            if (this.m_EnableRateLimiting && IsRateLimited(senderClientId))
            {
                if (this.m_DebugLogging)
                {
                    Debug.LogWarning(
                        $"[NetworkNoiseEmitter] Rate limited noise from client {senderClientId}"
                    );
                }
                return;
            }

            // Validate parameters
            radius = Mathf.Clamp(radius, MIN_RADIUS, MAX_RADIUS);
            intensity = Mathf.Clamp(intensity, MIN_INTENSITY, MAX_INTENSITY);

            // Record emission for rate limiting
            RecordEmission(senderClientId);

            if (this.m_DebugLogging)
            {
                Debug.Log(
                    $"[NetworkNoiseEmitter] Server validated noise from client {senderClientId}: "
                        + $"pos={position}, radius={radius}, tag={tag}, intensity={intensity}"
                );
            }

            // Broadcast to all clients (including host)
            BroadcastNoiseClientRpc(position, radius, tag, intensity, senderClientId);
        }

        // CLIENT RPCS: ---------------------------------------------------------------------------

        [Rpc(SendTo.ClientsAndHost)]
        private void BroadcastNoiseClientRpc(
            Vector3 position,
            float radius,
            FixedString32Bytes tag,
            float intensity,
            ulong sourceClientId
        )
        {
            string tagString = tag.ToString();

            if (this.m_DebugLogging)
            {
                Debug.Log(
                    $"[NetworkNoiseEmitter] Processing noise: pos={position}, radius={radius}, "
                        + $"tag={tagString}, intensity={intensity}, source={sourceClientId}"
                );
            }

            // Process through local SpatialHashPerception
            ProcessNoiseLocally(position, radius, tagString, intensity, sourceClientId);

            // Fire global event
            EventNoiseEmitted?.Invoke(position, radius, tagString, intensity, sourceClientId);
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void ProcessNoiseLocally(
            Vector3 position,
            float radius,
            string tag,
            float intensity,
            ulong sourceClientId
        )
        {
            // Clear cache
            this.m_PerceptionCache.Clear();

            // Find all perceptions in range
            SpatialHashPerception.Find(position, radius, this.m_PerceptionCache);

            // Add gizmo visualization
            HearManager.Instance?.AddGizmoNoise(position, radius);

            // Create stimulus
            StimulusNoise stimulus = new StimulusNoise(tag, position, radius, intensity);

            int processedCount = 0;
            foreach (ISpatialHash spatialHash in this.m_PerceptionCache)
            {
                Perception perception = spatialHash as Perception;
                if (perception == null)
                    continue;

                SensorHear sensorHear = perception.GetSensor<SensorHear>();
                if (sensorHear == null)
                    continue;

                // Process the noise
                sensorHear.OnReceiveNoise(stimulus);
                processedCount++;

                // Notify global events for UI
                NetworkPerceptionEvents.NotifyNoiseHeard(perception, stimulus);
            }

            if (this.m_DebugLogging && processedCount > 0)
            {
                Debug.Log(
                    $"[NetworkNoiseEmitter] Noise processed by {processedCount} perceptions"
                );
            }
        }

        private bool IsRateLimited(ulong clientId)
        {
            if (!this.m_ClientEmissionTimes.TryGetValue(clientId, out var times))
            {
                return false;
            }

            CleanupOldEmissions(times);
            return times.Count >= this.m_MaxEmissionsPerSecond;
        }

        private void RecordEmission(ulong clientId)
        {
            if (!this.m_ClientEmissionTimes.TryGetValue(clientId, out var times))
            {
                times = new List<float>();
                this.m_ClientEmissionTimes[clientId] = times;
            }

            CleanupOldEmissions(times);
            times.Add(Time.time);
        }

        private void CleanupOldEmissions(List<float> times)
        {
            float cutoff = Time.time - RATE_LIMIT_WINDOW;
            times.RemoveAll(t => t < cutoff);
        }
    }
}
