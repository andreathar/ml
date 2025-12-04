using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Conditions.Perception
{
    [Version(1, 0, 0)]
    [Title("Network Awareness Level")]
    [Description(
        "Compares the networked Perception's awareness level for a target against a value"
    )]
    [Category("Network/Perception/Network Awareness Level")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Perception", "Awareness", "Level", "Compare")]
    [Serializable]
    public class ConditionNetworkAwarenessLevel : Condition
    {
        private enum Comparison
        {
            LessThan,
            LessOrEqual,
            Equal,
            GreaterOrEqual,
            GreaterThan,
        }

        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        [SerializeField]
        private Comparison m_Comparison = Comparison.GreaterOrEqual;

        [SerializeField]
        [Tooltip("Awareness level to compare against (0-1)")]
        private PropertyGetDecimal m_Value = GetDecimalDecimal.Create(0.5f);

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary
        {
            get
            {
                string op = m_Comparison switch
                {
                    Comparison.LessThan => "<",
                    Comparison.LessOrEqual => "<=",
                    Comparison.Equal => "==",
                    Comparison.GreaterOrEqual => ">=",
                    Comparison.GreaterThan => ">",
                    _ => "?",
                };
                return $"{m_Perception} awareness of {m_Target} {op} {m_Value}";
            }
        }

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

            float awareness = networkPerception.GetAwareness(target);
            float compareValue = (float)m_Value.Get(args);

            return m_Comparison switch
            {
                Comparison.LessThan => awareness < compareValue,
                Comparison.LessOrEqual => awareness <= compareValue,
                Comparison.Equal => Mathf.Approximately(awareness, compareValue),
                Comparison.GreaterOrEqual => awareness >= compareValue,
                Comparison.GreaterThan => awareness > compareValue,
                _ => false,
            };
        }
    }
}
