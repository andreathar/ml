using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Countdown Started")]
    [Description("Triggered when the game countdown starts.")]
    [Category("Network/Game State/On Countdown Started")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Countdown", "Start", "Timer")]
    [Serializable]
    public class EventNetworkOnCountdownStarted : TriggerEvent
    {
        [NonSerialized]
        private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkGameStateManager.EventCountdownStarted += OnCountdownStarted;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkGameStateManager.EventCountdownStarted -= OnCountdownStarted;
        }

        private void OnCountdownStarted(float duration)
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
