using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On NPC Spawned")]
    [Description(
        "Triggered when any NPC (server-authoritative character) spawns on the network. The spawned NPC is available as the Target."
    )]
    [Category("Network/Characters/On NPC Spawned")]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "NPC", "AI", "Spawn", "Server")]
    [Serializable]
    public class EventNetworkOnNPCSpawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventNPCSpawned += OnNPCSpawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventNPCSpawned -= OnNPCSpawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnNPCSpawned(NetworkCharacter npc)
        {
            if (npc == null)
                return;

            // Set the NPC as the target so it can be referenced in instructions
            this.m_Args = new Args(this.m_Trigger.gameObject, npc.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
