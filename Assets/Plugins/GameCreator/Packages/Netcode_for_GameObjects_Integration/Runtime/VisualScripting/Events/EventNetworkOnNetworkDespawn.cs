using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Network Despawn")]
    [Description("Triggered when this NetworkObject is despawned from the network.")]
    [Category("Network/On Network Despawn")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Despawn", "Destroy", "Remove")]
    [Serializable]
    public class EventNetworkOnNetworkDespawn : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        [NonSerialized]
        private NetworkCharacterAdapter m_Adapter;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            this.m_Adapter = trigger.GetComponent<NetworkCharacterAdapter>();
            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventNetworkDespawned += OnNetworkDespawned;
            }
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventNetworkDespawned -= OnNetworkDespawned;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnNetworkDespawned()
        {
            Debug.Log("[EventNetworkOnNetworkDespawn] Network object despawned");
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
