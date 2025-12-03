using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// NetworkCharacter extends the standard GameCreator Character with network-aware properties.
    /// Key features:
    /// - IsNetworkSpawned flag prevents CharacterController conflicts with NetworkTransform
    /// - Properly handles initialization for network-spawned characters
    /// - Disables input control for non-owner remote characters
    /// - Registers with NetworkCharacterRegistry for efficient lookup
    ///
    /// Host vs Client topology:
    /// - Host (ClientId 0): IsHost=true, IsServer=true, IsClient=true
    /// - Client (ClientId 1+): IsHost=false, IsServer=false, IsClient=true
    /// Both have a "local player" - the character they own (IsOwner=true)
    /// </summary>
    [AddComponentMenu("Game Creator/Characters/Network Character")]
    [Icon(RuntimePaths.GIZMOS + "GizmoCharacter.png")]
    [RequireComponent(typeof(NetworkObject))]
    public class NetworkCharacter : Character
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Network Settings")]
        [SerializeField]
        [Tooltip(
            "When true, the CharacterController.center will not be modified, allowing NetworkTransform to control positioning."
        )]
        private bool m_IsNetworkSpawned;

        [SerializeField]
        [Tooltip("Disable player input processing for remote (non-owner) characters.")]
        private bool m_DisableInputForRemote = true;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private NetworkObject m_NetworkObject;

        [NonSerialized]
        private bool m_IsInitialized;

        [NonSerialized]
        private bool m_WasPlayerBeforeNetwork;

        [NonSerialized]
        private bool m_IsRegistered;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// When true, indicates this character is spawned via network and should not have
        /// its CharacterController.center modified by the driver system.
        /// This prevents conflicts with NetworkTransform positioning.
        /// </summary>
        public bool IsNetworkSpawned
        {
            get => this.m_IsNetworkSpawned;
            set
            {
                if (this.m_IsNetworkSpawned == value)
                    return;
                this.m_IsNetworkSpawned = value;
                this.EventNetworkSpawnedChanged?.Invoke(value);

                // Update controllability based on ownership
                this.UpdateControllability();
            }
        }

        /// <summary>
        /// Returns true if this character is owned by the local client.
        /// Works for both Host (ClientId 0) and Client (ClientId 1+).
        /// </summary>
        public bool IsLocalOwner
        {
            get
            {
                if (this.m_NetworkObject == null)
                    return true; // Not networked, treat as local
                return this.m_NetworkObject.IsOwner;
            }
        }

        /// <summary>
        /// The ClientId of the owner of this character.
        /// 0 for Host, 1+ for Clients.
        /// </summary>
        public ulong OwnerClientId
        {
            get
            {
                if (this.m_NetworkObject == null)
                    return 0;
                return this.m_NetworkObject.OwnerClientId;
            }
        }

        /// <summary>
        /// Returns true if this character should process input.
        /// </summary>
        public bool ShouldProcessInput
        {
            get
            {
                if (!this.m_IsNetworkSpawned)
                    return true;
                if (!this.m_DisableInputForRemote)
                    return true;
                return this.IsLocalOwner;
            }
        }

        /// <summary>
        /// The underlying NetworkObject component.
        /// </summary>
        public NetworkObject NetworkObject => this.m_NetworkObject;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Fired when IsNetworkSpawned changes. Passes the new value.
        /// </summary>
        public event Action<bool> EventNetworkSpawnedChanged;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void Awake()
        {
            // Cache NetworkObject reference
            this.m_NetworkObject = GetComponent<NetworkObject>();

            // Call base Character.Awake() for standard initialization
            base.Awake();

            this.m_IsInitialized = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Ensure NetworkCharacterSync component exists for network callbacks
            if (GetComponent<NetworkCharacterSync>() == null)
            {
                gameObject.AddComponent<NetworkCharacterSync>();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            // Unregister from registry
            if (this.m_IsRegistered)
            {
                NetworkCharacterRegistry.Unregister(this);
                this.m_IsRegistered = false;
            }

            base.OnDestroy();
        }

        // NETWORK CALLBACKS: ---------------------------------------------------------------------

        /// <summary>
        /// Called when this NetworkObject is spawned on the network.
        /// Registers with NetworkCharacterRegistry for efficient lookup.
        /// </summary>
        public void OnNetworkSpawn()
        {
            this.m_IsNetworkSpawned = true;

            // Register with the registry
            NetworkCharacterRegistry.Register(this);
            this.m_IsRegistered = true;

            // Set up local vs remote player
            if (this.IsLocalOwner)
            {
                this.BecomeLocalPlayer();
            }
            else
            {
                this.BecomeRemotePlayer();
            }

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} spawned on network "
                    + $"(ClientId: {this.OwnerClientId}, IsLocalOwner: {this.IsLocalOwner}, "
                    + $"IsHost: {NetworkCharacterRegistry.IsHost})"
            );
        }

        /// <summary>
        /// Called when this NetworkObject is despawned from the network.
        /// Unregisters from NetworkCharacterRegistry.
        /// </summary>
        public void OnNetworkDespawn()
        {
            // Unregister from the registry
            if (this.m_IsRegistered)
            {
                NetworkCharacterRegistry.Unregister(this);
                this.m_IsRegistered = false;
            }

            this.m_IsNetworkSpawned = false;

            Debug.Log($"[NetworkCharacter] {gameObject.name} despawned from network");
        }

        /// <summary>
        /// Called by NetworkCharacterSync when ownership of this NetworkObject changes.
        /// Updates registry mapping and local player state.
        /// </summary>
        public void HandleOwnershipChanged(ulong previousOwner, ulong newOwner)
        {
            // Update the registry mapping
            NetworkCharacterRegistry.UpdateClientIdMapping(this, newOwner);

            // Update local vs remote player state
            if (this.IsLocalOwner)
            {
                this.BecomeLocalPlayer();
            }
            else
            {
                this.BecomeRemotePlayer();
            }

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} ownership changed: "
                    + $"{previousOwner} -> {newOwner} (IsLocalOwner: {this.IsLocalOwner})"
            );
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        protected override void Update()
        {
            // Skip update if not properly initialized (animation system not ready)
            if (!this.m_IsInitialized)
                return;

            // Check if animation system is ready (prevents NullReferenceException)
            if (this.Animim?.Animator == null)
                return;
            if (this.Gestures == null)
                return;

            base.Update();
        }

        protected override void LateUpdate()
        {
            if (!this.m_IsInitialized)
                return;
            base.LateUpdate();
        }

        protected override void FixedUpdate()
        {
            if (!this.m_IsInitialized)
                return;
            base.FixedUpdate();
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Called when this character becomes the local player (gains ownership).
        /// Sets IsPlayer = true and enables input processing.
        /// </summary>
        public void BecomeLocalPlayer()
        {
            this.IsPlayer = true;
            this.UpdateControllability();

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} became local player (ShortcutPlayer set)"
            );
        }

        /// <summary>
        /// Called when this character is no longer the local player (remote character).
        /// Sets the internal IsPlayer flag to false WITHOUT clearing ShortcutPlayer.
        /// This ensures remote characters don't interfere with the local player reference.
        /// </summary>
        public void BecomeRemotePlayer()
        {
            // Set the backing field directly to avoid triggering ShortcutPlayer.Change(null)
            // which would clear the local player reference when remote players spawn.
            // The m_IsPlayer field is protected in the base Character class.
            this.m_IsPlayer = false;

            this.UpdateControllability();

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} became remote player (ShortcutPlayer preserved)"
            );
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Updates the controllability of the character based on network ownership.
        /// </summary>
        private void UpdateControllability()
        {
            if (this.Player == null)
                return;

            if (this.m_IsNetworkSpawned && this.m_DisableInputForRemote)
            {
                // Only owner can control the character
                this.Player.IsControllable = this.IsLocalOwner;
            }
        }

        // GIZMOS: --------------------------------------------------------------------------------

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw network indicator when spawned
            if (this.m_IsNetworkSpawned && Application.isPlaying)
            {
                // Blue for local owner, red for remote
                Gizmos.color = this.IsLocalOwner
                    ? new Color(0.2f, 0.8f, 1f, 0.5f)
                    : new Color(1f, 0.3f, 0.2f, 0.5f);
                Gizmos.DrawWireSphere(this.transform.position + Vector3.up * 2f, 0.15f);
            }
        }
    }
}
