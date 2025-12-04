using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Instructions.Perception
{
    [Version(1, 0, 0)]
    [Title("Untrack Network Target")]
    [Description("Stops tracking a target on a networked Perception. Server-authoritative.")]
    [Category("Network/Perception/Untrack Network Target")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEye), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Perception", "Untrack", "Target", "Stop", "Forget")]

    [Serializable]
    public class InstructionNetworkUntrackTarget : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Untrack {m_Target} on network";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null)
            {
                Debug.LogWarning("[InstructionNetworkUntrackTarget] Perception GameObject is null");
                return DefaultResult;
            }

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null)
            {
                Debug.LogWarning($"[InstructionNetworkUntrackTarget] {perceptionGO.name} has no NetworkPerception component");
                return DefaultResult;
            }

            var target = m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkUntrackTarget] Target is null");
                return DefaultResult;
            }

            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject == null || !targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning($"[InstructionNetworkUntrackTarget] Target {target.name} has no spawned NetworkObject");
                return DefaultResult;
            }

            // Send to server for authoritative untracking
            networkPerception.UntrackTargetRpc(targetNetworkObject.NetworkObjectId);

            Debug.Log($"[InstructionNetworkUntrackTarget] Requested untrack {target.name}");

            return DefaultResult;
        }
    }
}
