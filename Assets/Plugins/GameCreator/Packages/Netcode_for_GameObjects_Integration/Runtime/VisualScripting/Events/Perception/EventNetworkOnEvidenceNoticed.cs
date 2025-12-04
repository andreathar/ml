using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting.Events.Perception
{
    [Version(1, 0, 0)]
    [Title("On Network Evidence Noticed")]
    [Description(
        "Triggered when a networked Perception notices evidence (tampering detected). Can be placed ANYWHERE in the scene."
    )]
    [Category("Network/Perception/On Network Evidence Noticed")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEvidence), ColorTheme.Type.Red)]
    [Keywords(
        "Network",
        "Multiplayer",
        "Perception",
        "Evidence",
        "Notice",
        "Tamper",
        "Investigate"
    )]
    [Serializable]
    public class EventNetworkOnEvidenceNoticed : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CompareGameObjectOrAny m_Perception = new CompareGameObjectOrAny();

        [SerializeField]
        private CompareGameObjectOrAny m_Evidence = new CompareGameObjectOrAny();

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized]
        private Args m_Args;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            m_Args = new Args(trigger.gameObject);

            NetworkPerceptionEvents.EventEvidenceNoticed += OnEvidenceNoticed;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkPerceptionEvents.EventEvidenceNoticed -= OnEvidenceNoticed;
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnEvidenceNoticed(Perception perception, Evidence evidence)
        {
            if (perception == null)
                return;

            // Filter by perception
            if (!m_Perception.Match(perception.gameObject, m_Args))
                return;

            // Filter by evidence
            if (evidence != null && !m_Evidence.Match(evidence.gameObject, m_Args))
                return;

            Debug.Log(
                $"[EventNetworkOnEvidenceNoticed] Triggered: {perception.name} noticed evidence {evidence?.name ?? "null"}"
            );

            // Set perception as Self, evidence as Target
            var args =
                evidence != null
                    ? new Args(perception.gameObject, evidence.gameObject)
                    : new Args(perception.gameObject);

            _ = m_Trigger.Execute(args);
        }
    }
}
