using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Perception
{
    [Version(1, 0, 0)]
    [Title("On Network Awareness Stage")]
    [Description(
        "Triggered when a networked Perception's awareness stage changes (None/Suspicious/Alert/Aware). Can be placed ANYWHERE in the scene."
    )]
    [Category("Network/Perception/On Network Awareness Stage")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Yellow)]
    [Keywords(
        "Network",
        "Multiplayer",
        "Perception",
        "Awareness",
        "Stage",
        "Alert",
        "Suspicious",
        "Aware"
    )]
    [Serializable]
    public class EventNetworkOnAwarenessStage : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CompareGameObjectOrAny m_Perception = new CompareGameObjectOrAny();

        [SerializeField]
        private CompareGameObjectOrAny m_Target = new CompareGameObjectOrAny();

        [SerializeField]
        [Tooltip("Which awareness stages trigger this event")]
        private AwareMask m_Stage = AwareMask.Alert | AwareMask.Aware;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);

            NetworkPerceptionEvents.EventAwarenessStageChanged += OnAwarenessStageChanged;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventAwarenessStageChanged -= OnAwarenessStageChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnAwarenessStageChanged(
            Perception perception,
            GameObject target,
            AwareStage stage
        )
        {
            if (perception == null || target == null)
                return;

            // Filter by perception
            if (!m_Perception.Match(perception.gameObject, m_Args))
                return;

            // Filter by target
            if (!m_Target.Match(target, m_Args))
                return;

            // Filter by stage
            if (!m_Stage.HasFlag((AwareMask)stage))
                return;

            Debug.Log(
                $"[EventNetworkOnAwarenessStage] Triggered: {perception.name} -> {target.name} = {stage}"
            );

            // Set both perception and target accessible in Args
            var args = new Args(perception.gameObject, target);
            _ = m_Trigger.Execute(args);
        }
    }
}
