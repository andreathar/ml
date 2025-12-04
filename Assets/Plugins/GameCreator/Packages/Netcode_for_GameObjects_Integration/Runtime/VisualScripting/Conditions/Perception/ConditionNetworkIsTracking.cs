using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Conditions.Perception
{
    [Version(1, 0, 0)]
    [Title("Is Network Tracking")]
    [Description("Returns true if the networked Perception is tracking the specified target")]
    [Category("Network/Perception/Is Network Tracking")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEye), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Perception", "Track", "Target", "Check")]
    [Serializable]
    public class ConditionNetworkIsTracking : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{m_Perception} is tracking {m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null)
                return false;

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null)
                return false;

            var target = m_Target.Get(args);
            if (target == null)
                return false;

            return networkPerception.IsTracking(target);
        }
    }
}
