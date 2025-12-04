using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Remote Player Spawned (Global)")]
    [Description("Triggered when a remote player's character spawns. Can be placed on any GameObject in the scene.")]
    [Category("Network/Session/On Remote Player Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Remote", "Session")]

    [Serializable]
    public class EventNetworkOnRemotePlayerSpawnedGlobal : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            // Subscribe to global event from NetworkSessionEvents
            NetworkSessionEvents.EventRemotePlayerSpawned += OnRemotePlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            NetworkSessionEvents.EventRemotePlayerSpawned -= OnRemotePlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnRemotePlayerSpawned(NetworkCharacter character)
        {
            if (character == null) return;

            Debug.Log($"[EventNetworkOnRemotePlayerSpawnedGlobal] Remote player spawned: {character.name}");

            // Update args with the spawned character as target
            this.m_Args = new Args(this.m_Trigger.gameObject, character.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
