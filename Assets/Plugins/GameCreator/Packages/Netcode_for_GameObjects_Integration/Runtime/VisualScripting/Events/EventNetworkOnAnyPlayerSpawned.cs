using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Any Player Spawned")]
    [Description(
        "Triggered when any player character (local or remote) spawns on the network. The spawned player is available as the Target."
    )]
    [Category("Network/Characters/On Any Player Spawned")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Any")]
    [Serializable]
    public class EventNetworkOnAnyPlayerSpawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventPlayerSpawned += OnPlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventPlayerSpawned -= OnPlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnPlayerSpawned(NetworkCharacter player)
        {
            if (player == null)
                return;

            // Set the player as the target so it can be referenced in instructions
            this.m_Args = new Args(this.m_Trigger.gameObject, player.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
