using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Any Player Spawned (Global)")]
    [Description("Triggered when any player's character spawns (local or remote). Can be placed on any GameObject in the scene.")]
    [Category("Network/Session/On Any Player Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Player", "Spawn", "Any", "Session")]

    [Serializable]
    public class EventNetworkOnAnyPlayerSpawnedGlobal : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            // Subscribe to global event from NetworkSessionEvents
            NetworkSessionEvents.EventPlayerSpawned += OnPlayerSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            NetworkSessionEvents.EventPlayerSpawned -= OnPlayerSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnPlayerSpawned(NetworkCharacter character)
        {
            if (character == null) return;

            Debug.Log($"[EventNetworkOnAnyPlayerSpawnedGlobal] Player spawned: {character.name} (IsLocal: {character.IsLocalOwner})");

            // Update args with the spawned character as target
            this.m_Args = new Args(this.m_Trigger.gameObject, character.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
