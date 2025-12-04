using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Game State Changed")]
    [Description("Triggered when the network game state changes (Lobby, Countdown, Playing, etc).")]
    [Category("Network/Game State/On Game State Changed")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "State", "Lobby", "Playing", "Game")]

    [Serializable]
    public class EventNetworkOnGameStateChanged : TriggerEvent
    {
        [SerializeField]
        [Tooltip("Only trigger for specific state (None = any state)")]
        private NetworkGameStateManager.GameState m_FilterState = NetworkGameStateManager.GameState.None;

        [NonSerialized] private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkGameStateManager.EventStateChanged += OnStateChanged;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkGameStateManager.EventStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(NetworkGameStateManager.GameState oldState, NetworkGameStateManager.GameState newState)
        {
            // Filter if specific state requested
            if (m_FilterState != NetworkGameStateManager.GameState.None && newState != m_FilterState)
                return;

            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
