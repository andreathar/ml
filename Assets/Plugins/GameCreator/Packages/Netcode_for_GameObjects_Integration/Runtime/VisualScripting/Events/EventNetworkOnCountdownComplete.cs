using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Core
{
    [Version(1, 0, 0)]
    [Title("On Countdown Complete")]
    [Description("Triggered when the game countdown completes and game starts.")]
    [Category("Network/Game State/On Countdown Complete")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Countdown", "Complete", "Start", "Game")]
    [Serializable]
    public class EventNetworkOnCountdownComplete : TriggerEvent
    {
        [NonSerialized]
        private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkGameStateManager.EventCountdownComplete += OnCountdownComplete;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkGameStateManager.EventCountdownComplete -= OnCountdownComplete;
        }

        private void OnCountdownComplete()
        {
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
