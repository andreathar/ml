using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Add Network Awareness")]
    [Description("Adds to the awareness level for a target on a networked Perception. Server-authoritative.")]
    [Category("Network/Perception/Add Network Awareness")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Perception", "Awareness", "Add", "Increase", "Level")]

    [Serializable]
    public class InstructionNetworkAddAwareness : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        [SerializeField]
        [Tooltip("Amount to add to awareness (can be negative to subtract)")]
        private PropertyGetDecimal m_Amount = GetDecimalDecimal.Create(0.1f);

        [SerializeField]
        [Tooltip("Maximum awareness level (0-1)")]
        private PropertyGetDecimal m_MaxAwareness = GetDecimalDecimal.Create(1f);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Add {m_Amount} awareness for {m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null)
            {
                Debug.LogWarning("[InstructionNetworkAddAwareness] Perception GameObject is null");
                return DefaultResult;
            }

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null)
            {
                Debug.LogWarning($"[InstructionNetworkAddAwareness] {perceptionGO.name} has no NetworkPerception component");
                return DefaultResult;
            }

            var target = m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkAddAwareness] Target is null");
                return DefaultResult;
            }

            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject == null || !targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning($"[InstructionNetworkAddAwareness] Target {target.name} has no spawned NetworkObject");
                return DefaultResult;
            }

            float amount = (float)m_Amount.Get(args);
            float maxAwareness = Mathf.Clamp01((float)m_MaxAwareness.Get(args));

            // Send to server for authoritative update
            networkPerception.AddAwarenessRpc(targetNetworkObject.NetworkObjectId, amount, maxAwareness);

            Debug.Log($"[InstructionNetworkAddAwareness] Requested add {amount:F2} awareness for {target.name}");

            return DefaultResult;
        }
    }
}
