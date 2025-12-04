using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On NPC Spawned (Global)")]
    [Description("Triggered when an NPC character spawns on the network. Can be placed on any GameObject in the scene.")]
    [Category("Network/Session/On NPC Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "NPC", "Spawn", "Session")]

    [Serializable]
    public class EventNetworkOnNPCSpawnedGlobal : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            // Subscribe to global event from NetworkSessionEvents
            NetworkSessionEvents.EventNPCSpawned += OnNPCSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            NetworkSessionEvents.EventNPCSpawned -= OnNPCSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnNPCSpawned(NetworkCharacter character)
        {
            if (character == null) return;

            Debug.Log($"[EventNetworkOnNPCSpawnedGlobal] NPC spawned: {character.name}");

            // Update args with the spawned character as target
            this.m_Args = new Args(this.m_Trigger.gameObject, character.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
