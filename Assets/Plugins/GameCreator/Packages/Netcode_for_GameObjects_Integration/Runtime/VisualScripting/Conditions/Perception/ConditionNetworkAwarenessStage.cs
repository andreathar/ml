using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Perception;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Network Awareness Stage Is")]
    [Description("Returns true if the networked Perception's awareness stage for a target matches the specified stage")]
    [Category("Network/Perception/Network Awareness Stage Is")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Perception", "Awareness", "Stage", "Alert", "Suspicious", "Aware")]

    [Serializable]
    public class ConditionNetworkAwarenessStage : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        [SerializeField]
        [Tooltip("The awareness stage to check for")]
        private AwareMask m_Stage = AwareMask.Alert;

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{m_Perception} awareness of {m_Target} is {m_Stage}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null) return false;

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null) return false;

            var target = m_Target.Get(args);
            if (target == null) return false;

            AwareStage currentStage = networkPerception.GetAwarenessStage(target);
            return m_Stage.HasFlag((AwareMask)currentStage);
        }
    }
}
