using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On RPC Message Received")]
    [Description("Triggered when an RPC message is received on a specific channel.")]
    [Category("Network/RPC/On RPC Message Received")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "RPC", "Message", "Event", "Channel")]
    [Serializable]
    public class EventNetworkOnRPCReceived : TriggerEvent
    {
        [SerializeField]
        [Tooltip("Channel to listen on (empty = all channels)")]
        private string m_Channel = "";

        [SerializeField]
        [Tooltip("Event name to filter (empty = all events)")]
        private string m_EventName = "";

        [NonSerialized]
        private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkRPCManager.EventMessageReceived += OnMessageReceived;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkRPCManager.EventMessageReceived -= OnMessageReceived;
        }

        private void OnMessageReceived(NetworkRPCManager.RPCMessage message)
        {
            // Filter by channel
            if (!string.IsNullOrEmpty(m_Channel) && message.Channel != m_Channel)
                return;

            // Filter by event name
            if (!string.IsNullOrEmpty(m_EventName) && message.EventName != m_EventName)
                return;

            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
