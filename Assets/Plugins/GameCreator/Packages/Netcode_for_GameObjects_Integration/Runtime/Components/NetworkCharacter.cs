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

        [NonSerialized]
        private bool m_BaseOnEnablePending;

        [NonSerialized]
        private bool m_BaseAwakePending;

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

            // Ensure Animator reference is set before calling base.Awake()
            // When Netcode instantiates prefabs, the serialized Animator reference may not be resolved yet
            // Try to find and assign it if the Animim unit exists but Animator is null
            this.EnsureAnimatorReference();

            // Only call base.Awake() if Animator is ready
            // base.Awake() initializes AnimimGraph and InverseKinematics which require Animator
            if (this.Animim?.Animator != null)
            {
                base.Awake();
                this.m_IsInitialized = true;
                this.m_BaseAwakePending = false;
            }
            else
            {
                // Defer base.Awake() until Animator is ready
                // This can happen with network-spawned prefabs
                this.m_BaseAwakePending = true;
                this.m_IsInitialized = false;
                Debug.LogWarning(
                    $"[NetworkCharacter] {gameObject.name}: Animator not ready during Awake, deferring initialization"
                );
            }
        }

        /// <summary>
        /// Ensures the Animator reference is set on the Animim unit.
        /// If the serialized reference is null, attempts to find the Animator in children (Mannequin).
        /// </summary>
        private void EnsureAnimatorReference()
        {
            // Check if Animim exists and has a null Animator
            if (this.Animim != null && this.Animim.Animator == null)
            {
                // Try to find Animator in children (typically on Mannequin child object)
                Animator animator = GetComponentInChildren<Animator>(true);
                if (animator != null)
                {
                    this.Animim.Animator = animator;
                    Debug.Log(
                        $"[NetworkCharacter] {gameObject.name}: Found and assigned Animator from child '{animator.gameObject.name}'"
                    );
                }
            }
        }

        protected override void OnEnable()
        {
            // Ensure NetworkCharacterSync component exists for network callbacks
            if (GetComponent<NetworkCharacterSync>() == null)
            {
                gameObject.AddComponent<NetworkCharacterSync>();
            }

            // If Awake hasn't completed yet, defer OnEnable as well
            if (this.m_BaseAwakePending)
            {
                this.m_BaseOnEnablePending = true;
                return;
            }

            // Only call base.OnEnable if Animator is ready
            // This prevents NullReferenceException in IK system when network-spawned
            // The Animator is on a child object (Mannequin) and may not be resolved yet
            if (this.Animim?.Animator != null)
            {
                base.OnEnable();
                this.m_BaseOnEnablePending = false;
            }
            else
            {
                // Defer base.OnEnable until Animator is ready
                this.m_BaseOnEnablePending = true;
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
        /// Handles both player characters and NPCs appropriately.
        /// </summary>
        public void OnNetworkSpawn()
        {
            this.m_IsNetworkSpawned = true;

            // Register with the registry (handles players vs NPCs internally)
            NetworkCharacterRegistry.Register(this);
            this.m_IsRegistered = true;

            // Determine if this is a player character or NPC
            // NPCs have IsPlayer = false and are server-authoritative
            bool isNPC = !this.IsPlayer;

            if (isNPC)
            {
                // Clean up NPC name (remove "(Clone)" suffix from network-spawned objects)
                this.SetNetworkNPCName();

                // NPC - server controls, all clients see it as remote
                this.BecomeNPC();

                // Notify session events
                NetworkSessionEvents.NotifyNPCSpawned(this);

                Debug.Log(
                    $"[NetworkCharacter] {gameObject.name} spawned as NPC "
                        + $"(Server-authoritative, NetworkObjectId: {this.m_NetworkObject?.NetworkObjectId ?? 0})"
                );
            }
            else
            {
                // Set unique player name based on role and client ID
                this.SetNetworkPlayerName();

                // Player character - set up local vs remote player
                if (this.IsLocalOwner)
                {
                    this.BecomeLocalPlayer();
                }
                else
                {
                    this.BecomeRemotePlayer();
                }

                // Notify session events
                NetworkSessionEvents.NotifyPlayerSpawned(this, this.IsLocalOwner);

                Debug.Log(
                    $"[NetworkCharacter] {gameObject.name} spawned as PLAYER "
                        + $"(ClientId: {this.OwnerClientId}, IsLocalOwner: {this.IsLocalOwner}, "
                        + $"IsHost: {NetworkCharacterRegistry.IsHost})"
                );
            }
        }

        /// <summary>
        /// Called when this NetworkObject is despawned from the network.
        /// Unregisters from NetworkCharacterRegistry.
        /// </summary>
        public void OnNetworkDespawn()
        {
            // Notify session events before unregistering
            NetworkSessionEvents.NotifyCharacterDespawned(this);

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
        /// Note: NPCs typically don't change ownership, but this handles the case if they do.
        /// </summary>
        public void HandleOwnershipChanged(ulong previousOwner, ulong newOwner)
        {
            // NPCs don't need ClientId mapping updates - they're not player-owned
            if (!this.IsNPC)
            {
                // Update the registry mapping for players
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
            }

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} ownership changed: "
                    + $"{previousOwner} -> {newOwner} (IsLocalOwner: {this.IsLocalOwner}, IsNPC: {this.IsNPC})"
            );
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        protected override void Update()
        {
            // Handle deferred Awake initialization
            if (this.m_BaseAwakePending)
            {
                // Try to ensure Animator reference again
                this.EnsureAnimatorReference();

                if (this.Animim?.Animator != null)
                {
                    // Animator is now ready, complete deferred Awake
                    base.Awake();
                    this.m_IsInitialized = true;
                    this.m_BaseAwakePending = false;
                    Debug.Log(
                        $"[NetworkCharacter] {gameObject.name}: Completed deferred Awake initialization"
                    );
                }
                else
                {
                    // Still waiting for Animator
                    return;
                }
            }

            // Skip update if not properly initialized
            if (!this.m_IsInitialized)
                return;

            // Check if animation system is ready (prevents NullReferenceException)
            if (this.Animim?.Animator == null)
                return;
            if (this.Gestures == null)
                return;

            // Complete deferred OnEnable once Animator is ready
            if (this.m_BaseOnEnablePending)
            {
                base.OnEnable();
                this.m_BaseOnEnablePending = false;
            }

            base.Update();
        }

        protected override void LateUpdate()
        {
            if (!this.m_IsInitialized || this.m_BaseAwakePending)
                return;
            base.LateUpdate();
        }

        protected override void FixedUpdate()
        {
            if (!this.m_IsInitialized || this.m_BaseAwakePending)
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

        /// <summary>
        /// Called when this character is an NPC (server-authoritative).
        /// NPCs are not player-controlled and are synchronized by the server.
        /// </summary>
        public void BecomeNPC()
        {
            // NPCs should never be IsPlayer = true
            this.m_IsPlayer = false;

            // Disable input for NPCs - they're controlled by AI/server
            if (this.Player != null)
            {
                this.Player.IsControllable = false;
            }

            Debug.Log(
                $"[NetworkCharacter] {gameObject.name} configured as NPC (server-authoritative)"
            );
        }

        /// <summary>
        /// Returns true if this character is an NPC (not a player character).
        /// NPCs are server-authoritative and controlled by AI.
        /// </summary>
        public bool IsNPC => !this.IsPlayer;

        // PRIVATE METHODS: -----------------------------------------------------------------------

        /// <summary>
        /// Cleans up the NPC name by removing "(Clone)" suffix from network-spawned objects.
        /// This ensures GameCreator instructions can find NPCs by their original prefab name.
        /// </summary>
        private void SetNetworkNPCName()
        {
            // Remove "(Clone)" suffix if present
            string currentName = gameObject.name;
            if (currentName.EndsWith("(Clone)"))
            {
                string newName = currentName.Substring(0, currentName.Length - "(Clone)".Length);
                gameObject.name = newName;
                Debug.Log($"[NetworkCharacter] Renamed NPC '{currentName}' -> '{newName}'");
            }
        }

        /// <summary>
        /// Sets a unique name for the player based on their role and client ID.
        /// Format: "Player_Host" for ClientId 0, "Player_Client1", "Player_Client2", etc.
        /// </summary>
        private void SetNetworkPlayerName()
        {
            if (this.m_NetworkObject == null)
                return;

            ulong clientId = this.OwnerClientId;
            bool isHost =
                clientId == 0
                && NetworkManager.Singleton != null
                && NetworkManager.Singleton.IsHost;

            string newName;
            if (isHost)
            {
                newName = "Player_Host";
            }
            else
            {
                newName = $"Player_Client{clientId}";
            }

            // Only rename if different (avoid unnecessary updates)
            if (gameObject.name != newName)
            {
                string oldName = gameObject.name;
                gameObject.name = newName;
                Debug.Log($"[NetworkCharacter] Renamed '{oldName}' -> '{newName}'");
            }
        }

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
