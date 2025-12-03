using System;
using System.Collections.Generic;
using GameCreator.Runtime.Common;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// Synchronization mode for network variables.
    /// </summary>
    public enum NetworkSyncMode
    {
        /// <summary>All clients can read the value.</summary>
        Everyone,

        /// <summary>Only the owner can read and write.</summary>
        OwnerOnly,

        /// <summary>Only the server can read and write.</summary>
        ServerOnly,
    }

    /// <summary>
    /// Configuration for a synced variable.
    /// </summary>
    [Serializable]
    public class SyncedVariableConfig
    {
        public string variableName;
        public NetworkSyncMode syncMode = NetworkSyncMode.Everyone;
        public float syncRate = 10f; // Hz
    }

    /// <summary>
    /// NetworkVariablesSync provides synchronization for GameCreator Local Variables
    /// across the network using Unity Netcode NetworkVariables.
    /// </summary>
    /// [AddComponentMenu("Game Creator/Network/Network Variables Sync")]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkVariablesSync : NetworkBehaviour
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Variable Configuration")]
        [SerializeField]
        [Tooltip("List of GameCreator variables to synchronize.")]
        private List<SyncedVariableConfig> m_SyncedVariables = new List<SyncedVariableConfig>();

        // NETWORK VARIABLES: ---------------------------------------------------------------------

        // Generic storage for synchronized values (using strings as JSON for flexibility)
        private NetworkList<NetworkString64> m_SyncedValues;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Dictionary<string, int> m_VariableIndexMap;

        [NonSerialized]
        private Dictionary<string, float> m_LastSyncTimes;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// Number of configured synced variables.
        /// </summary>
        public int SyncedVariableCount => this.m_SyncedVariables.Count;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Fired when a synced variable value changes.
        /// </summary>
        public event Action<string, string> EventVariableChanged;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_VariableIndexMap = new Dictionary<string, int>();
            this.m_LastSyncTimes = new Dictionary<string, float>();

            // Initialize the NetworkList
            this.m_SyncedValues = new NetworkList<NetworkString64>();

            // Build index map
            for (int i = 0; i < this.m_SyncedVariables.Count; i++)
            {
                var config = this.m_SyncedVariables[i];
                if (!string.IsNullOrEmpty(config.variableName))
                {
                    this.m_VariableIndexMap[config.variableName] = i;
                    this.m_LastSyncTimes[config.variableName] = 0f;
                }
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to list changes
            this.m_SyncedValues.OnListChanged += OnSyncedValuesChanged;

            // Initialize list with empty values if server
            if (IsServer)
            {
                for (int i = 0; i < this.m_SyncedVariables.Count; i++)
                {
                    this.m_SyncedValues.Add(new NetworkString64(""));
                }
            }
        }

        public override void OnNetworkDespawn()
        {
            this.m_SyncedValues.OnListChanged -= OnSyncedValuesChanged;
            base.OnNetworkDespawn();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Set a synced variable value.
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <param name="value">Value as string</param>
        public void SetVariable(string variableName, string value)
        {
            if (!this.m_VariableIndexMap.TryGetValue(variableName, out int index))
            {
                Debug.LogWarning(
                    $"[NetworkVariablesSync] Variable '{variableName}' is not configured for sync."
                );
                return;
            }

            var config = this.m_SyncedVariables[index];

            // Check sync mode permissions
            if (!CanWrite(config.syncMode))
            {
                Debug.LogWarning(
                    $"[NetworkVariablesSync] Cannot write to '{variableName}' with current sync mode."
                );
                return;
            }

            // Rate limiting
            float interval = 1f / config.syncRate;
            if (Time.time - this.m_LastSyncTimes[variableName] < interval)
            {
                return;
            }
            this.m_LastSyncTimes[variableName] = Time.time;

            // Update via appropriate method
            if (IsServer)
            {
                if (index < this.m_SyncedValues.Count)
                {
                    this.m_SyncedValues[index] = new NetworkString64(value);
                }
            }
            else
            {
                SetVariableServerRpc(variableName, new NetworkString64(value));
            }
        }

        /// <summary>
        /// Get a synced variable value.
        /// </summary>
        /// <param name="variableName">Name of the variable</param>
        /// <returns>Value as string, or null if not found</returns>
        public string GetVariable(string variableName)
        {
            if (!this.m_VariableIndexMap.TryGetValue(variableName, out int index))
            {
                return null;
            }

            if (index >= this.m_SyncedValues.Count)
            {
                return null;
            }

            return this.m_SyncedValues[index].ToString();
        }

        /// <summary>
        /// Set a float variable.
        /// </summary>
        public void SetFloat(string variableName, float value)
        {
            SetVariable(variableName, value.ToString("G9"));
        }

        /// <summary>
        /// Get a float variable.
        /// </summary>
        public float GetFloat(string variableName, float defaultValue = 0f)
        {
            string value = GetVariable(variableName);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return float.TryParse(value, out float result) ? result : defaultValue;
        }

        /// <summary>
        /// Set an int variable.
        /// </summary>
        public void SetInt(string variableName, int value)
        {
            SetVariable(variableName, value.ToString());
        }

        /// <summary>
        /// Get an int variable.
        /// </summary>
        public int GetInt(string variableName, int defaultValue = 0)
        {
            string value = GetVariable(variableName);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Set a bool variable.
        /// </summary>
        public void SetBool(string variableName, bool value)
        {
            SetVariable(variableName, value ? "1" : "0");
        }

        /// <summary>
        /// Get a bool variable.
        /// </summary>
        public bool GetBool(string variableName, bool defaultValue = false)
        {
            string value = GetVariable(variableName);
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return value == "1" || value.ToLower() == "true";
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private bool CanWrite(NetworkSyncMode mode)
        {
            return mode switch
            {
                NetworkSyncMode.Everyone => true,
                NetworkSyncMode.OwnerOnly => IsOwner,
                NetworkSyncMode.ServerOnly => IsServer,
                _ => false,
            };
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        private void OnSyncedValuesChanged(NetworkListEvent<NetworkString64> changeEvent)
        {
            // Find which variable changed
            int index = changeEvent.Index;
            if (index < 0 || index >= this.m_SyncedVariables.Count)
                return;

            string variableName = this.m_SyncedVariables[index].variableName;
            string newValue = changeEvent.Value.ToString();

            this.EventVariableChanged?.Invoke(variableName, newValue);
        }

        // RPCs: ----------------------------------------------------------------------------------

        [ServerRpc(RequireOwnership = false)]
        private void SetVariableServerRpc(
            string variableName,
            NetworkString64 value,
            ServerRpcParams rpcParams = default
        )
        {
            if (!this.m_VariableIndexMap.TryGetValue(variableName, out int index))
            {
                return;
            }

            var config = this.m_SyncedVariables[index];

            // Validate permissions based on sync mode
            bool canWrite = config.syncMode switch
            {
                NetworkSyncMode.Everyone => true,
                NetworkSyncMode.OwnerOnly => rpcParams.Receive.SenderClientId == OwnerClientId,
                NetworkSyncMode.ServerOnly => false, // Clients can't write server-only
                _ => false,
            };

            if (!canWrite)
            {
                Debug.LogWarning(
                    $"[NetworkVariablesSync] Client {rpcParams.Receive.SenderClientId} denied write to '{variableName}'"
                );
                return;
            }

            if (index < this.m_SyncedValues.Count)
            {
                this.m_SyncedValues[index] = value;
            }
        }
    }

    /// <summary>
    /// Fixed-size network string for NetworkList compatibility.
    /// </summary>
    public struct NetworkString64 : INetworkSerializable, IEquatable<NetworkString64>
    {
        private FixedString64Bytes m_Value;

        public NetworkString64(string value)
        {
            this.m_Value = new FixedString64Bytes(value ?? "");
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer)
            where T : IReaderWriter
        {
            serializer.SerializeValue(ref this.m_Value);
        }

        public override string ToString() => this.m_Value.ToString();

        public bool Equals(NetworkString64 other) => this.m_Value.Equals(other.m_Value);

        public override bool Equals(object obj) => obj is NetworkString64 other && Equals(other);

        public override int GetHashCode() => this.m_Value.GetHashCode();
    }
}
