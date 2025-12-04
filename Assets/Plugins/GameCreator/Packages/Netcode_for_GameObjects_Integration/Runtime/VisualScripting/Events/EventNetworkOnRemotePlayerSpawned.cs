using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Remote Player Spawned")]
    [Description(
        "Triggered when a remote player (owned by another client) spawns on the network. The spawned player is available as the Target."
    )]
    [Category("Network/Characters/On Remote Player Spawned")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Remote", "Other")]
    [Serializable]
    public class EventNetworkOnRemotePlayerSpawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventRemotePlayerSpawned += OnRemotePlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventRemotePlayerSpawned -= OnRemotePlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnRemotePlayerSpawned(NetworkCharacter player)
        {
            if (player == null)
                return;

            // Set the player as the target so it can be referenced in instructions
            this.m_Args = new Args(this.m_Trigger.gameObject, player.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
