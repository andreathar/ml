using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Perception
{
    [Version(1, 0, 0)]
    [Title("On Network Awareness Change")]
    [Description(
        "Triggered when a networked Perception's awareness level changes for any target. Can be placed ANYWHERE in the scene."
    )]
    [Category("Network/Perception/On Network Awareness Change")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Perception", "Awareness", "Level", "Detect")]
    [Serializable]
    public class EventNetworkOnAwarenessChange : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CompareGameObjectOrAny m_Perception = new CompareGameObjectOrAny();

        [SerializeField]
        private CompareGameObjectOrAny m_Target = new CompareGameObjectOrAny();

        [SerializeField]
        [Tooltip("Minimum awareness level change to trigger (0-1)")]
        private float m_MinChange = 0f;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        [NonSerialized]
        private float m_LastLevel;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);

            NetworkPerceptionEvents.EventAwarenessChanged += OnAwarenessChanged;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventAwarenessChanged -= OnAwarenessChanged;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnAwarenessChanged(Perception perception, GameObject target, float level)
        {
            if (perception == null || target == null)
                return;

            // Filter by perception
            if (!m_Perception.Match(perception.gameObject, m_Args))
                return;

            // Filter by target
            if (!m_Target.Match(target, m_Args))
                return;

            // Filter by minimum change
            if (Mathf.Abs(level - m_LastLevel) < m_MinChange)
                return;

            m_LastLevel = level;

            Debug.Log(
                $"[EventNetworkOnAwarenessChange] Triggered: {perception.name} -> {target.name} = {level:F2}"
            );

            // Set both perception and target accessible in Args
            var args = new Args(perception.gameObject, target);
            _ = m_Trigger.Execute(args);
        }
    }
}
