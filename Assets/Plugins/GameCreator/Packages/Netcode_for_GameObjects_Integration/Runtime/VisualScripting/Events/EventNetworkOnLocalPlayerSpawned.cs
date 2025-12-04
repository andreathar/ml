using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Local Player Spawned")]
    [Description("Triggered when YOUR player spawns on the network. The spawned player is available as the Target. Can be placed ANYWHERE in the scene.")]
    [Category("Network/Characters/On Local Player Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Local", "Own")]

    [Serializable]
    public class EventNetworkOnLocalPlayerSpawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventLocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnLocalPlayerSpawned(NetworkCharacter player)
        {
            if (player == null) return;

            Debug.Log($"[EventNetworkOnLocalPlayerSpawned] Local player spawned: {player.name}");

            // Set the player as the target so it can be referenced in instructions
            this.m_Args = new Args(this.m_Trigger.gameObject, player.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
