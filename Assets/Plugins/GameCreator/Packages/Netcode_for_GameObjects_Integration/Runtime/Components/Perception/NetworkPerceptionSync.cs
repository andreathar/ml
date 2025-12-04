using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.Components.Perception
{
    /// <summary>
    /// Helper component that forwards NetworkBehaviour callbacks to NetworkPerception.
    /// Similar to NetworkCharacterSync, this handles ownership changes and other callbacks.
    ///
    /// Auto-added by NetworkPerception if not present.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu - auto-added
    [RequireComponent(typeof(NetworkObject))]
    [RequireComponent(typeof(NetworkPerception))]
    public class NetworkPerceptionSync : NetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        private NetworkPerception m_NetworkPerception;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            m_NetworkPerception = GetComponent<NetworkPerception>();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            // NetworkPerception handles its own spawn logic
        }

        public override void OnNetworkDespawn()
        {
            // NetworkPerception handles its own despawn logic
            base.OnNetworkDespawn();
        }

        // OWNERSHIP CALLBACKS: -------------------------------------------------------------------

        /// <summary>
        /// Called when ownership of this NetworkObject changes.
        /// Forwards to NetworkPerception for handling.
        /// </summary>
        public override void OnGainedOwnership()
        {
            base.OnGainedOwnership();

            if (m_NetworkPerception != null)
            {
                Debug.Log($"[NetworkPerceptionSync] {gameObject.name}: Gained ownership");
                // NetworkPerception may need to re-sync local state
            }
        }

        /// <summary>
        /// Called when this client loses ownership of the NetworkObject.
        /// </summary>
        public override void OnLostOwnership()
        {
            base.OnLostOwnership();

            if (m_NetworkPerception != null)
            {
                Debug.Log($"[NetworkPerceptionSync] {gameObject.name}: Lost ownership");
                // NetworkPerception may need to switch to receiving state
            }
        }
    }
}
