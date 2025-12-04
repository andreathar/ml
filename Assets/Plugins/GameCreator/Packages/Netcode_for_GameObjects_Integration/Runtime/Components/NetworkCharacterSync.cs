using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime
{
    /// <summary>
    /// NetworkBehaviour helper component for NetworkCharacter.
    /// Handles network callbacks that require NetworkBehaviour inheritance:
    /// - OnNetworkSpawn/Despawn for registry registration
    /// - OnOwnershipChanged for ownership tracking
    ///
    /// This component is automatically added when NetworkCharacter is present.
    /// </summary>
    [AddComponentMenu("")] // Hide from menu - auto-added by NetworkCharacter
    [RequireComponent(typeof(NetworkCharacter))]
    public class NetworkCharacterSync : NetworkBehaviour
    {
        // MEMBERS: -------------------------------------------------------------------------------

        private NetworkCharacter m_NetworkCharacter;

        // INITIALIZERS: --------------------------------------------------------------------------

        private void Awake()
        {
            this.m_NetworkCharacter = GetComponent<NetworkCharacter>();
        }

        // NETWORK CALLBACKS: ---------------------------------------------------------------------

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.OnNetworkSpawn();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.OnNetworkDespawn();
            }

            base.OnNetworkDespawn();
        }

        protected override void OnOwnershipChanged(ulong previousOwnerId, ulong newOwnerId)
        {
            base.OnOwnershipChanged(previousOwnerId, newOwnerId);

            if (this.m_NetworkCharacter != null)
            {
                this.m_NetworkCharacter.HandleOwnershipChanged(previousOwnerId, newOwnerId);
            }
        }
    }
}
