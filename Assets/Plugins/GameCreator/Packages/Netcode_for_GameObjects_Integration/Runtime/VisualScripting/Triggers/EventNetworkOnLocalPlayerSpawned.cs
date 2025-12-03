using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Local Player Spawned")]
    [Description("Triggered when the local player's character spawns on the network.")]
    [Category("Network/On Local Player Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Local")]

    [Serializable]
    public class EventNetworkOnLocalPlayerSpawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;
        [NonSerialized] private NetworkCharacterAdapter m_Adapter;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            // Find adapter on this object
            this.m_Adapter = trigger.GetComponent<NetworkCharacterAdapter>();
            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventNetworkSpawned += OnNetworkSpawned;
            }
        }

        protected override void OnDisable(Trigger trigger)
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
            // Only trigger for local player
            NetworkObject networkObject = this.m_Adapter.GetComponent<NetworkObject>();
            if (networkObject == null || !networkObject.IsOwner) return;

            Debug.Log("[EventNetworkOnLocalPlayerSpawned] Local player spawned");
            this.m_Args = new Args(this.m_Adapter.gameObject); // Update args with spawned object
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
