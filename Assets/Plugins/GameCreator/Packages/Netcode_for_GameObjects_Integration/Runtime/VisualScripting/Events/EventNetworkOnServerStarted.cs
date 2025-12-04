using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Server Started")]
    [Description(
        "Triggered when the network session starts as dedicated Server (no local client)."
    )]
    [Category("Network/Session/On Server Started")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Server", "Dedicated", "Session", "Start")]
    [Serializable]
    public class EventNetworkOnServerStarted : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventServerStarted += OnServerStarted;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventServerStarted -= OnServerStarted;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnServerStarted()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
