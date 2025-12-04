using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Network Target Tracked")]
    [Description(
        "Triggered when a networked Perception starts tracking a new target. Can be placed ANYWHERE in the scene."
    )]
    [Category("Network/Perception/On Network Target Tracked")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEye), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Perception", "Track", "Target", "Detect", "See")]
    [Serializable]
    public class EventNetworkOnTargetTracked : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CompareGameObjectOrAny m_Perception = new CompareGameObjectOrAny();

        [SerializeField]
        private CompareGameObjectOrAny m_Target = new CompareGameObjectOrAny();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);

            NetworkPerceptionEvents.EventTargetTracked += OnTargetTracked;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventTargetTracked -= OnTargetTracked;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnTargetTracked(Perception perception, GameObject target)
        {
            if (perception == null || target == null)
                return;

            // Filter by perception
            if (!m_Perception.Match(perception.gameObject, m_Args))
                return;

            // Filter by target
            if (!m_Target.Match(target, m_Args))
                return;

            Debug.Log(
                $"[EventNetworkOnTargetTracked] Triggered: {perception.name} now tracking {target.name}"
            );

            // Set both perception and target accessible in Args
            var args = new Args(perception.gameObject, target);
            _ = m_Trigger.Execute(args);
        }
    }
}
