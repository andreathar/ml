using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Perception
{
    [Version(1, 0, 0)]
    [Title("On Network Noise Heard")]
    [Description(
        "Triggered when a networked Perception hears a noise (server-validated). Can be placed ANYWHERE in the scene."
    )]
    [Category("Network/Perception/On Network Noise Heard")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEar), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Perception", "Hear", "Noise", "Sound", "Audio")]
    [Serializable]
    public class EventNetworkOnNoiseHeard : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CompareGameObjectOrAny m_Perception = new CompareGameObjectOrAny();

        [SerializeField]
        [Tooltip("Filter by noise tag (leave empty for any noise)")]
        private string m_NoiseTag = "";

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);

            NetworkPerceptionEvents.EventNoiseHeard += OnNoiseHeard;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventNoiseHeard -= OnNoiseHeard;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnNoiseHeard(Perception perception, StimulusNoise noise)
        {
            if (perception == null)
                return;

            // Filter by perception
            if (!m_Perception.Match(perception.gameObject, m_Args))
                return;

            // Filter by noise tag
            if (!string.IsNullOrEmpty(m_NoiseTag))
            {
                if (noise.Tag != m_NoiseTag)
                    return;
            }

            Debug.Log(
                $"[EventNetworkOnNoiseHeard] Triggered: {perception.name} heard noise (tag: {noise.Tag ?? "null"})"
            );

            // Set perception as Self, noise source position could be used for Target
            var args = new Args(perception.gameObject);
            _ = m_Trigger.Execute(args);
        }
    }
}
