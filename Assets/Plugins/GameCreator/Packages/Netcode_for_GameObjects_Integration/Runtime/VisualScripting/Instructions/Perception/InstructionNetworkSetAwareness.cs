using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Set Network Awareness")]
    [Description("Sets the awareness level for a target on a networked Perception. Server-authoritative.")]
    [Category("Network/Perception/Set Network Awareness")]
    [Image(typeof(GameCreator.Runtime.Perception.IconAwareness), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Perception", "Awareness", "Set", "Level")]

    [Serializable]
    public class InstructionNetworkSetAwareness : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        [SerializeField]
        [Tooltip("Awareness level (0-1)")]
        private PropertyGetDecimal m_Awareness = GetDecimalDecimal.Create(1f);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Set {m_Target} awareness to {m_Awareness}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null)
            {
                Debug.LogWarning("[InstructionNetworkSetAwareness] Perception GameObject is null");
                return DefaultResult;
            }

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null)
            {
                Debug.LogWarning($"[InstructionNetworkSetAwareness] {perceptionGO.name} has no NetworkPerception component");
                return DefaultResult;
            }

            var target = m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkSetAwareness] Target is null");
                return DefaultResult;
            }

            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject == null || !targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning($"[InstructionNetworkSetAwareness] Target {target.name} has no spawned NetworkObject");
                return DefaultResult;
            }

            float awareness = Mathf.Clamp01((float)m_Awareness.Get(args));

            // Send to server for authoritative update
            networkPerception.SetAwarenessRpc(targetNetworkObject.NetworkObjectId, awareness);

            Debug.Log($"[InstructionNetworkSetAwareness] Requested awareness for {target.name} = {awareness:F2}");

            return DefaultResult;
        }
    }
}
