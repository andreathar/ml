using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Local Player Spawned (Global)")]
    [Description("Triggered when the local player's character spawns. Can be placed on any GameObject in the scene.")]
    [Category("Network/Session/On Local Player Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Local", "Session")]

    [Serializable]
    public class EventNetworkOnLocalPlayerSpawnedGlobal : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            // Subscribe to global event from NetworkSessionEvents
            NetworkSessionEvents.EventLocalPlayerSpawned += OnLocalPlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            NetworkSessionEvents.EventLocalPlayerSpawned -= OnLocalPlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnLocalPlayerSpawned(NetworkCharacter character)
        {
            if (character == null) return;

            Debug.Log($"[EventNetworkOnLocalPlayerSpawnedGlobal] Local player spawned: {character.name}");

            // Update args with the spawned character as target
            this.m_Args = new Args(this.m_Trigger.gameObject, character.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
