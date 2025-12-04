using System;
using GameCreator.Runtime.Characters;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// NetworkCharacterMotion synchronizes character motion state across the network.
    /// It replicates movement direction, speed, and grounded state from owner to all clients.
    /// </summary>
    [AddComponentMenu("Game Creator/Network/Network Character Motion")]
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(Character))]
    public class NetworkCharacterMotion : NetworkBehaviour
    {
        // CONSTANTS: -----------------------------------------------------------------------------

        private const float SYNC_THRESHOLD = 0.01f;
        private const float INTERPOLATION_SPEED = 10f;

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [Header("Synchronization Settings")]
        [SerializeField]
        [Tooltip("How often to sync motion state (times per second).")]
        private float m_SyncRate = 30f;

        [SerializeField]
        [Tooltip("Interpolate motion on remote clients for smoother movement.")]
        private bool m_InterpolateMotion = true;

        // NETWORK VARIABLES: ---------------------------------------------------------------------

        private NetworkVariable<Vector3> m_NetworkMoveDirection = new NetworkVariable<Vector3>(
            Vector3.zero,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private NetworkVariable<float> m_NetworkLinearSpeed = new NetworkVariable<float>(
            0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private NetworkVariable<bool> m_NetworkIsGrounded = new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private NetworkVariable<bool> m_NetworkIsMoving = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Character m_Character;

        [NonSerialized]
        private float m_LastSyncTime;

        [NonSerialized]
        private Vector3 m_TargetMoveDirection;

        [NonSerialized]
        private float m_TargetLinearSpeed;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// The synchronized movement direction.
        /// </summary>
        public Vector3 MoveDirection => this.m_NetworkMoveDirection.Value;

        /// <summary>
        /// The synchronized linear speed.
        /// </summary>
        public float LinearSpeed => this.m_NetworkLinearSpeed.Value;

        /// <summary>
        /// The synchronized grounded state.
        /// </summary>
        public bool IsGrounded => this.m_NetworkIsGrounded.Value;

        /// <summary>
        /// The synchronized moving state.
        /// </summary>
        public bool IsMoving => this.m_NetworkIsMoving.Value;

        /// <summary>
        /// Time interval between syncs.
        /// </summary>
        private float SyncInterval => 1f / this.m_SyncRate;

        // EVENTS: --------------------------------------------------------------------------------

        /// <summary>
        /// Fired when motion state changes on remote clients.
        /// </summary>
        public event Action<Vector3, float> EventMotionChanged;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_Character = GetComponent<Character>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // Subscribe to network variable changes for remote clients
            if (!IsOwner)
            {
                this.m_NetworkMoveDirection.OnValueChanged += OnMoveDirectionChanged;
                this.m_NetworkLinearSpeed.OnValueChanged += OnLinearSpeedChanged;
            }

            this.m_TargetMoveDirection = Vector3.zero;
            this.m_TargetLinearSpeed = 0f;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner)
            {
                this.m_NetworkMoveDirection.OnValueChanged -= OnMoveDirectionChanged;
                this.m_NetworkLinearSpeed.OnValueChanged -= OnLinearSpeedChanged;
            }

            base.OnNetworkDespawn();
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        private void Update()
        {
            if (!IsSpawned)
                return;
            if (this.m_Character == null)
                return;

            if (IsOwner)
            {
                this.UpdateOwnerSync();
            }
            else if (this.m_InterpolateMotion)
            {
                this.UpdateRemoteInterpolation();
            }
        }

        /// <summary>
        /// Owner: Sync local motion state to network variables.
        /// /// </summary>
        private void UpdateOwnerSync()
        {
            // Rate limit syncing
            if (Time.time - this.m_LastSyncTime < this.SyncInterval)
                return;
            this.m_LastSyncTime = Time.time;

            IUnitMotion motion = this.m_Character.Motion;
            if (motion == null)
                return;

            // Sync move direction if changed
            Vector3 currentDirection = motion.MoveDirection;
            if (
                Vector3.Distance(currentDirection, this.m_NetworkMoveDirection.Value)
                > SYNC_THRESHOLD
            )
            {
                this.m_NetworkMoveDirection.Value = currentDirection;
            }

            // Sync linear speed if changed
            float currentSpeed = motion.LinearSpeed;
            if (Mathf.Abs(currentSpeed - this.m_NetworkLinearSpeed.Value) > SYNC_THRESHOLD)
            {
                this.m_NetworkLinearSpeed.Value = currentSpeed;
            }

            // Sync grounded state
            bool isGrounded = this.m_Character.Driver.IsGrounded;
            if (isGrounded != this.m_NetworkIsGrounded.Value)
            {
                this.m_NetworkIsGrounded.Value = isGrounded;
            }

            // Sync moving state
            bool isMoving = motion.MoveDirection.sqrMagnitude > 0.01f;
            if (isMoving != this.m_NetworkIsMoving.Value)
            {
                this.m_NetworkIsMoving.Value = isMoving;
            }
        }

        /// <summary>
        /// Remote: Interpolate towards received motion state.
        /// </summary>
        private void UpdateRemoteInterpolation()
        {
            // Smoothly interpolate movement direction
            this.m_TargetMoveDirection = Vector3.Lerp(
                this.m_TargetMoveDirection,
                this.m_NetworkMoveDirection.Value,
                Time.deltaTime * INTERPOLATION_SPEED
            );

            // Smoothly interpolate speed
            this.m_TargetLinearSpeed = Mathf.Lerp(
                this.m_TargetLinearSpeed,
                this.m_NetworkLinearSpeed.Value,
                Time.deltaTime * INTERPOLATION_SPEED
            );

            // Apply interpolated motion to character
            if (this.m_Character.Motion != null && this.m_TargetMoveDirection.sqrMagnitude > 0.001f)
            {
                // MoveToDirection takes (velocity, space, priority)
                // We multiply direction by speed to get velocity
                this.m_Character.Motion.MoveToDirection(
                    this.m_TargetMoveDirection * this.m_TargetLinearSpeed,
                    Space.World,
                    0 // priority
                );
            }
        }

        // CALLBACKS: -----------------------------------------------------------------------------

        private void OnMoveDirectionChanged(Vector3 oldValue, Vector3 newValue)
        {
            this.EventMotionChanged?.Invoke(newValue, this.m_NetworkLinearSpeed.Value);
        }

        private void OnLinearSpeedChanged(float oldValue, float newValue)
        {
            this.EventMotionChanged?.Invoke(this.m_NetworkMoveDirection.Value, newValue);
        }

        // PUBLIC METHODS: ------------------------------------------------------------------------

        /// <summary>
        /// Request the character to jump (owner only, sent to server).
        /// </summary>
        public void RequestJump(float force)
        {
            if (!IsOwner)
                return;
            RequestJumpRpc(force);
        }

        /// <summary>
        /// Request the character to dash (owner only, sent to server).
        /// </summary>
        /// <param name="direction">Dash direction</param>
        /// <param name="speed">Dash speed</param>
        /// <param name="gravity">Gravity during dash</param>
        /// <param name="duration">Dash duration in seconds</param>
        /// <param name="fade">Fade out time</param>
        public void RequestDash(
            Vector3 direction,
            float speed,
            float gravity = 0f,
            float duration = 0.5f,
            float fade = 0.2f
        )
        {
            if (!IsOwner)
                return;
            RequestDashRpc(direction, speed, gravity, duration, fade);
        }

        // RPCs: ----------------------------------------------------------------------------------

        [Rpc(SendTo.Server)]
        private void RequestJumpRpc(float force)
        {
            // Server validates and executes jump
            if (this.m_Character.Jump != null)
            {
                this.m_Character.Jump.Do(force);
            }

            // Broadcast to all clients
            ExecuteJumpRpc(force);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ExecuteJumpRpc(float force)
        {
            if (IsOwner)
                return; // Owner already executed locally

            if (this.m_Character.Jump != null)
            {
                this.m_Character.Jump.Do(force);
            }
        }

        [Rpc(SendTo.Server)]
        private void RequestDashRpc(
            Vector3 direction,
            float speed,
            float gravity,
            float duration,
            float fade
        )
        {
            // Server validates and executes dash
            if (this.m_Character.Dash != null)
            {
                _ = this.m_Character.Dash.Execute(direction, speed, gravity, duration, fade);
            }

            // Broadcast to all clients
            ExecuteDashRpc(direction, speed, gravity, duration, fade);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void ExecuteDashRpc(
            Vector3 direction,
            float speed,
            float gravity,
            float duration,
            float fade
        )
        {
            if (IsOwner)
                return; // Owner already executed locally

            if (this.m_Character.Dash != null)
            {
                _ = this.m_Character.Dash.Execute(direction, speed, gravity, duration, fade);
            }
        }
    }
}
