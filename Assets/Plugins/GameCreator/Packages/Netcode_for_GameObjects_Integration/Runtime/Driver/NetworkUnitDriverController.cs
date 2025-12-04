using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Core
{
    /// <summary>
    /// NetworkUnitDriverController extends UnitDriverController to support networked characters.
    /// The key difference is that it skips CharacterController.center modifications when
    /// the character is network-spawned, allowing NetworkTransform to control positioning.
    /// </summary>
    [Title("Network Character Controller")]
    [Image(typeof(IconCapsuleSolid), ColorTheme.Type.Blue)]
    [Category("Network Character Controller")]
    [Description(
        "Moves the Character using Unity's Character Controller with network awareness. Skips center modifications for networked characters."
    )]
    [Serializable]
    public class NetworkUnitDriverController : UnitDriverController
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private NetworkCharacter m_NetworkCharacter;

        [NonSerialized]
        private bool m_CachedNetworkCheck;

        // PROPERTIES: ----------------------------------------------------------------------------

        /// <summary>
        /// Returns true if this character should skip CharacterController.center modifications.
        /// </summary>
        protected bool ShouldSkipCenterModification
        {
            get
            {
                if (this.m_NetworkCharacter == null)
                    return false;
                return this.m_NetworkCharacter.IsNetworkSpawned;
            }
        }

        // INITIALIZERS: --------------------------------------------------------------------------

        public override void OnStartup(Character character)
        {
            base.OnStartup(character);

            // Cache reference to NetworkCharacter if available
            this.m_NetworkCharacter = character as NetworkCharacter;
        }

        public override void OnDispose(Character character)
        {
            base.OnDispose(character);
            this.m_NetworkCharacter = null;
        }

        // UPDATE METHODS: ------------------------------------------------------------------------

        public override void OnFixedUpdate()
        {
            if (this.Character == null)
                return;
            if (this.Character.IsDead)
                return;

            // For networked characters, use our custom physics update
            if (this.ShouldSkipCenterModification)
            {
                this.UpdatePhysicPropertiesNetworked();
            }
            else
            {
                base.OnFixedUpdate();
            }
        }

        /// <summary>
        /// Updates physics properties for networked characters.
        /// This version skips both CharacterController.center reset AND Transform.localPosition
        /// modifications that conflict with NetworkTransform.
        /// </summary>
        protected virtual void UpdatePhysicPropertiesNetworked()
        {
            // Get the CharacterController via reflection or direct access
            CharacterController controller = this.Character.GetComponent<CharacterController>();
            if (controller == null)
                return;

            float height = this.Character.Motion.Height;
            float radius = this.Character.Motion.Radius;

            // Update height if changed (still needed for character scaling)
            if (Math.Abs(controller.height - height) > float.Epsilon)
            {
                // CRITICAL FIX: Do NOT modify Transform.localPosition for networked characters!
                // The original code did: this.Transform.localPosition += Vector3.down * offset;
                // This conflicts with NetworkTransform and causes leg animation jittering.
                // Instead, just update the controller height and let ApplyMannequinPosition handle visuals.
                controller.height = height;
                this.Character.Animim.ApplyMannequinPosition();
            }

            // Update radius if changed
            if (Math.Abs(controller.radius - radius) > float.Epsilon)
            {
                controller.radius = radius;
            }

            // CRITICAL: Do NOT reset controller.center for networked characters!
            // The original UnitDriverController does this:
            //   if (controller.center != Vector3.zero) controller.center = Vector3.zero;
            // We skip this to allow NetworkTransform to position the character correctly.
        }

        // STRING: --------------------------------------------------------------------------------

        public override string ToString() => "Network Character Controller";
    }
}
