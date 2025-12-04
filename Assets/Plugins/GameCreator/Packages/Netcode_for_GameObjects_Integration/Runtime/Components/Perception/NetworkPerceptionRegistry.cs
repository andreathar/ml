using System.Collections.Generic;
using GameCreator.Netcode.Runtime.Components.Core;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Perception
{
    /// <summary>
    /// Static registry for efficient lookup of NetworkPerception components.
    /// Provides O(1) lookup by NetworkObjectId, InstanceId, or GameObject.
    /// </summary>
    public static class NetworkPerceptionRegistry
    {
        // STORAGE: -------------------------------------------------------------------------------

        private static readonly Dictionary<ulong, NetworkPerception> s_ByNetworkId = new();
        private static readonly Dictionary<int, NetworkPerception> s_ByInstanceId = new();
        private static readonly List<NetworkPerception> s_AllPerceptions = new();

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// Read-only list of all registered NetworkPerception components.
        /// </summary>
        public static IReadOnlyList<NetworkPerception> AllPerceptions => s_AllPerceptions;

        /// <summary>
        /// Number of registered NetworkPerception components.
        /// </summary>
        public static int Count => s_AllPerceptions.Count;

        // REGISTRATION: --------------------------------------------------------------------------

        /// <summary>
        /// Register a NetworkPerception with the registry.
        /// Called by NetworkPerception.OnNetworkSpawn().
        /// </summary>
        public static void Register(NetworkPerception perception)
        {
            if (perception == null || !perception.IsSpawned)
            {
                Debug.LogWarning("[NetworkPerceptionRegistry] Cannot register null or unspawned perception");
                return;
            }

            ulong networkId = perception.NetworkObjectId;
            int instanceId = perception.gameObject.GetInstanceID();

            // Avoid duplicates
            if (s_ByNetworkId.ContainsKey(networkId))
            {
                Debug.LogWarning($"[NetworkPerceptionRegistry] Already registered: NetworkId {networkId}");
                return;
            }

            s_ByNetworkId[networkId] = perception;
            s_ByInstanceId[instanceId] = perception;

            if (!s_AllPerceptions.Contains(perception))
            {
                s_AllPerceptions.Add(perception);
            }

            Debug.Log($"[NetworkPerceptionRegistry] Registered: {perception.name} (NetworkId: {networkId}, Total: {s_AllPerceptions.Count})");
        }

        /// <summary>
        /// Unregister a NetworkPerception from the registry.
        /// Called by NetworkPerception.OnNetworkDespawn().
        /// </summary>
        public static void Unregister(NetworkPerception perception)
        {
            if (perception == null) return;

            ulong networkId = perception.NetworkObjectId;
            int instanceId = perception.gameObject.GetInstanceID();

            s_ByNetworkId.Remove(networkId);
            s_ByInstanceId.Remove(instanceId);
            s_AllPerceptions.Remove(perception);

            Debug.Log($"[NetworkPerceptionRegistry] Unregistered: {perception.name} (Total: {s_AllPerceptions.Count})");
        }

        /// <summary>
        /// Clear all registrations. Called when network session ends.
        /// </summary>
        public static void Clear()
        {
            s_ByNetworkId.Clear();
            s_ByInstanceId.Clear();
            s_AllPerceptions.Clear();

            Debug.Log("[NetworkPerceptionRegistry] Cleared all registrations");
        }

        // LOOKUPS: -------------------------------------------------------------------------------

        /// <summary>
        /// Get NetworkPerception by NetworkObject.NetworkObjectId.
        /// </summary>
        public static NetworkPerception GetByNetworkId(ulong networkId)
        {
            return s_ByNetworkId.GetValueOrDefault(networkId);
        }

        /// <summary>
        /// Get NetworkPerception by GameObject reference.
        /// </summary>
        public static NetworkPerception GetByGameObject(GameObject go)
        {
            if (go == null) return null;
            return s_ByInstanceId.GetValueOrDefault(go.GetInstanceID());
        }

        /// <summary>
        /// Get NetworkPerception attached to a NetworkCharacter.
        /// </summary>
        public static NetworkPerception GetForCharacter(NetworkCharacter character)
        {
            if (character == null) return null;
            return character.GetComponent<NetworkPerception>();
        }

        /// <summary>
        /// Get NetworkPerception by InstanceID.
        /// </summary>
        public static NetworkPerception GetByInstanceId(int instanceId)
        {
            return s_ByInstanceId.GetValueOrDefault(instanceId);
        }

        /// <summary>
        /// Try to get a NetworkPerception by NetworkObjectId.
        /// </summary>
        public static bool TryGetByNetworkId(ulong networkId, out NetworkPerception perception)
        {
            return s_ByNetworkId.TryGetValue(networkId, out perception);
        }

        /// <summary>
        /// Check if a NetworkPerception is registered by NetworkObjectId.
        /// </summary>
        public static bool IsRegistered(ulong networkId)
        {
            return s_ByNetworkId.ContainsKey(networkId);
        }

        /// <summary>
        /// Check if a NetworkPerception is registered by GameObject.
        /// </summary>
        public static bool IsRegistered(GameObject go)
        {
            if (go == null) return false;
            return s_ByInstanceId.ContainsKey(go.GetInstanceID());
        }

        // UTILITY: -------------------------------------------------------------------------------

        /// <summary>
        /// Get GameObject for a NetworkObjectId (helper for target resolution).
        /// </summary>
        public static GameObject GetGameObjectByNetworkId(ulong networkId)
        {
            if (NetworkManager.Singleton == null) return null;

            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkId, out var networkObject))
            {
                return networkObject.gameObject;
            }

            return null;
        }

        /// <summary>
        /// Get NetworkObjectId for a GameObject (helper for network operations).
        /// </summary>
        public static ulong GetNetworkIdForGameObject(GameObject go)
        {
            if (go == null) return 0;

            var networkObject = go.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsSpawned)
                return 0;

            return networkObject.NetworkObjectId;
        }

        /// <summary>
        /// Find all NetworkPerceptions within a radius of a position.
        /// Useful for area-of-effect sensory events.
        /// </summary>
        public static List<NetworkPerception> GetPerceptionsInRadius(Vector3 position, float radius)
        {
            var result = new List<NetworkPerception>();
            float radiusSqr = radius * radius;

            foreach (var perception in s_AllPerceptions)
            {
                if (perception == null) continue;

                float distSqr = (perception.transform.position - position).sqrMagnitude;
                if (distSqr <= radiusSqr)
                {
                    result.Add(perception);
                }
            }

            return result;
        }

        /// <summary>
        /// Find the closest NetworkPerception to a position.
        /// </summary>
        public static NetworkPerception GetClosestPerception(Vector3 position, float maxDistance = float.MaxValue)
        {
            NetworkPerception closest = null;
            float closestDistSqr = maxDistance * maxDistance;

            foreach (var perception in s_AllPerceptions)
            {
                if (perception == null) continue;

                float distSqr = (perception.transform.position - position).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closest = perception;
                }
            }

            return closest;
        }
    }
}
