using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On All Players Ready")]
    [Description("Triggered when all connected players have marked themselves as ready.")]
    [Category("Network/Game State/On All Players Ready")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Ready", "Players", "All", "Lobby")]

    [Serializable]
    public class EventNetworkOnAllPlayersReady : TriggerEvent
    {
        [NonSerialized] private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkGameStateManager.EventAllPlayersReady += OnAllPlayersReady;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkGameStateManager.EventAllPlayersReady -= OnAllPlayersReady;
        }

        private void OnAllPlayersReady()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
