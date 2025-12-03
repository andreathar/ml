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
    /// </summary>
    [AddComponentMenu("Game Creator/Characters/Network Character")]
    [Icon(RuntimePaths.GIZMOS + "GizmoCharacter.png")]
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
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
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
