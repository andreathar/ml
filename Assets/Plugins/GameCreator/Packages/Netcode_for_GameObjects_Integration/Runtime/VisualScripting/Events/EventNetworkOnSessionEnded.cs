using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Session Ended")]
    [Description("Triggered when the network session ends (disconnected from host/server or server stopped).")]
    [Category("Network/Session/On Session Ended")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Disconnect", "Stop", "End", "Session")]

    [Serializable]
    public class EventNetworkOnSessionEnded : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventSessionEnded += OnSessionEnded;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventSessionEnded -= OnSessionEnded;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnSessionEnded()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
