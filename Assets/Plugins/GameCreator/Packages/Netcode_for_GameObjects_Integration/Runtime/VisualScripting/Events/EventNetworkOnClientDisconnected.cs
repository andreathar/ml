using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Client Disconnected")]
    [Description("Triggered when a client disconnects from the network session.")]
    [Category("Network/On Client Disconnected")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Client", "Disconnect", "Leave")]
    [Serializable]
    public class EventNetworkOnClientDisconnected : TriggerEvent
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
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnClientDisconnected(ulong clientId)
        {
            if (this.m_ServerOnly && !NetworkManager.Singleton.IsServer)
                return;

            Debug.Log($"[EventNetworkOnClientDisconnected] Client {clientId} disconnected");
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
