using System;
using Unity.Netcode;
using GameCreator.Runtime.Characters;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// NetworkCharacterAdapter bridges GameCreator's Character system with Unity Netcode.
    /// It manages the IsNetworkSpawned flag lifecycle and provides network state access.
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Character Adapter")]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacterAdapter : NetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Character m_Character;
        [NonSerialized] private NetworkCharacter m_NetworkCharacter;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// The Character component on this GameObject.
        /// </summary>
        public Character Character => this.m_Character;

        /// <summary>
        /// The NetworkCharacter component if available (null if using standard Character).
        /// </summary>
        public NetworkCharacter NetworkCharacter => this.m_NetworkCharacter;

        /// <summary>
        /// Returns true if this is the local player's character.
        /// </summary>
        public new bool IsLocalPlayer => this.IsOwner && this.m_Character != null && this.m_Character.IsPlayer;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Fired when this character spawns on the network.
        /// </summary>
        public event Action EventNetworkSpawned;

        /// <summary>
        /// Fired when this character despawns from the network.
        /// </summary>
        public event Action EventNetworkDespawned;

        /// <summary>
        /// Fired when ownership of this character changes.
        /// </summary>
        public event Action<ulong, ulong> EventOwnershipChanged;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_Character = GetComponent<Character>();
            this.m_NetworkCharacter = GetComponent<NetworkCharacter>();

            if (this.m_Character == null)
            {
                Debug.LogWarning($"[NetworkCharacterAdapter] No Character component found on {gameObject.name}. Add Character or NetworkCharacter component.");
            }
        }

        // NETWORK LIFECYCLE: ---------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Set IsNetworkSpawned flag if using NetworkCharacter
            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.IsNetworkSpawned = true;
            }

            // For standard Character, we can still work but warn about potential issues
            if (this.m_Character != null && this.m_NetworkCharacter == null)
            {
                Debug.LogWarning($"[NetworkCharacterAdapter] Using standard Character component on {gameObject.name}. Consider using NetworkCharacter for full network support.");
            }

            this.EventNetworkSpawned?.Invoke();

            if (this.IsOwner)
            {
                this.OnLocalPlayerSpawned();
            }
            else
            {
                this.OnRemotePlayerSpawned();
            }
        }

        public override void OnNetworkDespawn()
        {
            // Clear IsNetworkSpawned flag
            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.IsNetworkSpawned = false;
            }

            this.EventNetworkDespawned?.Invoke();

            base.OnNetworkDespawn();
        }

        public override void OnGainedOwnership()
        {
            base.OnGainedOwnership();
            this.OnLocalPlayerSpawned();
        }

        public override void OnLostOwnership()
        {
            base.OnLostOwnership();
            this.OnRemotePlayerSpawned();
        }

        // PROTECTED METHODS: ---------------------------------------------------------------------

        /// <summary>
        /// Called when the local player's character spawns or gains ownership.
        /// Override to add custom local player initialization.
        /// </summary>
        protected virtual void OnLocalPlayerSpawned()
        {
            if (this.m_Character == null) return;

            // Use NetworkCharacter's method if available, otherwise set IsPlayer directly
            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.BecomeLocalPlayer();
            }
            else
            {
                this.m_Character.IsPlayer = true;
            }

            Debug.Log($"[NetworkCharacterAdapter] Local player spawned: {gameObject.name} (ClientId: {OwnerClientId})");
        }

        /// <summary>
        /// Called when a remote player's character spawns or when we lose ownership.
        /// Override to add custom remote player initialization.
        /// </summary>
        protected virtual void OnRemotePlayerSpawned()
        {
            if (this.m_Character == null) return;

            // Use NetworkCharacter's method if available
            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.BecomeRemotePlayer();
            }
            // Note: For standard Character without NetworkCharacter, we intentionally
            // do NOT set IsPlayer = false here because that would clear ShortcutPlayer
            // and break all Player-targeting functionality for the local player.
            // Remote characters should never have IsPlayer = true anyway.

            Debug.Log($"[NetworkCharacterAdapter] Remote player spawned: {gameObject.name} (OwnerClientId: {OwnerClientId})");
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Request ownership of this character from the server.
        /// </summary>
        public void RequestOwnership()
        {
            if (IsServer) return;
            if (IsOwner) return;

            RequestOwnershipRpc();
        }

        /// <summary>
        /// Transfer ownership to another client (server only).
        /// </summary>
        /// <param name="newOwnerClientId">The client ID to transfer ownership to</param>
        public void TransferOwnership(ulong newOwnerClientId)
        {
            if (!IsServer)
            {
                Debug.LogWarning("[NetworkCharacterAdapter] Only server can transfer ownership.");
                return;
            }

            ulong oldOwner = OwnerClientId;
            NetworkObject.ChangeOwnership(newOwnerClientId);
            this.EventOwnershipChanged?.Invoke(oldOwner, newOwnerClientId);
        }

        // RPCs: ----------------------------------------------------------------------------------

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
        private void RequestOwnershipRpc(RpcParams rpcParams = default)
        {
            ulong requestingClientId = rpcParams.Receive.SenderClientId;

            // Simple ownership request - server can add validation logic here
            ulong oldOwner = OwnerClientId;
            NetworkObject.ChangeOwnership(requestingClientId);
            this.EventOwnershipChanged?.Invoke(oldOwner, requestingClientId);

            Debug.Log($"[NetworkCharacterAdapter] Ownership transferred from {oldOwner} to {requestingClientId}");
        }
    }
}
