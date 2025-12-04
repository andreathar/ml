using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Host Started")]
    [Description("Triggered when the network session starts as Host (Server + Client combined).")]
    [Category("Network/Session/On Host Started")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Host", "Server", "Session", "Start")]
    [Serializable]
    public class EventNetworkOnHostStarted : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventHostStarted += OnHostStarted;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventHostStarted -= OnHostStarted;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnHostStarted()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
