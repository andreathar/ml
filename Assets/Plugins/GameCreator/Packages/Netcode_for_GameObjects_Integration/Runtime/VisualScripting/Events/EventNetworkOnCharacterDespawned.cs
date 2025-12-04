using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Character Despawned")]
    [Description("Triggered when any NetworkCharacter (player or NPC) despawns from the network. The despawned character is available as the Target.")]
    [Category("Network/Characters/On Character Despawned")]
    [Image(typeof(IconBust), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Character", "Despawn", "Remove", "Disconnect")]

    [Serializable]
    public class EventNetworkOnCharacterDespawned : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventCharacterDespawned += OnCharacterDespawned;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventCharacterDespawned -= OnCharacterDespawned;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnCharacterDespawned(NetworkCharacter character)
        {
            if (character == null) return;

            // Set the character as the target so it can be referenced in instructions
            this.m_Args = new Args(this.m_Trigger.gameObject, character.gameObject);
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
