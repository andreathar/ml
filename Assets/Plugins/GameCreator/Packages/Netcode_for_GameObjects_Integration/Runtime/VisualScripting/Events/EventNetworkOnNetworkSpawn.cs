using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Network Spawn")]
    [Description("Triggered when this NetworkObject is spawned on the network.")]
    [Category("Network/On Network Spawn")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Spawn", "Initialize")]
    [Serializable]
    public class EventNetworkOnNetworkSpawn : TriggerEvent
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
                this.m_Adapter.EventNetworkSpawned += OnNetworkSpawned;
            }
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventNetworkSpawned -= OnNetworkSpawned;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnNetworkSpawned()
        {
            Debug.Log("[EventNetworkOnNetworkSpawn] Network object spawned");
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
