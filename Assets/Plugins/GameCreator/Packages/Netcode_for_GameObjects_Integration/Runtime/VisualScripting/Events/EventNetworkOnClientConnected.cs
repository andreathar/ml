using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Client Connected")]
    [Description("Triggered when a client connects to the network session.")]
    [Category("Network/On Client Connected")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Client", "Connect", "Join")]
    [Serializable]
    public class EventNetworkOnClientConnected : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        [Tooltip("If true, only triggers on the server. If false, triggers on all clients.")]
        private bool m_ServerOnly = true;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            }
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnClientConnected(ulong clientId)
        {
            if (this.m_ServerOnly && !NetworkManager.Singleton.IsServer)
                return;

            Debug.Log($"[EventNetworkOnClientConnected] Client {clientId} connected");
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
