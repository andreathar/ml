using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Client Started")]
    [Description(
        "Triggered when the network session starts as Client (connecting to a remote host/server)."
    )]
    [Category("Network/Session/On Client Started")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Client", "Connect", "Session", "Start")]
    [Serializable]
    public class EventNetworkOnClientStarted : TriggerEvent
    {
        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            NetworkSessionEvents.EventClientStarted += OnClientStarted;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSessionEvents.EventClientStarted -= OnClientStarted;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnClientStarted()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
