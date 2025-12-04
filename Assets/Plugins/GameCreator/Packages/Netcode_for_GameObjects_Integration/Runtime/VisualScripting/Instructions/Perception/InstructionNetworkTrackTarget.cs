using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Instructions.Perception
{
    [Version(1, 0, 0)]
    [Title("Track Network Target")]
    [Description("Starts tracking a target on a networked Perception. Server-authoritative.")]
    [Category("Network/Perception/Track Network Target")]
    [Image(typeof(GameCreator.Runtime.Perception.IconEye), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Perception", "Track", "Target", "Start", "Follow")]

    [Serializable]
    public class InstructionNetworkTrackTarget : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Perception = GetGameObjectSelf.Create();

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectTarget.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Track {m_Target} on network";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            var perceptionGO = m_Perception.Get(args);
            if (perceptionGO == null)
            {
                Debug.LogWarning("[InstructionNetworkTrackTarget] Perception GameObject is null");
                return DefaultResult;
            }

            var networkPerception = perceptionGO.GetComponent<NetworkPerception>();
            if (networkPerception == null)
            {
                Debug.LogWarning($"[InstructionNetworkTrackTarget] {perceptionGO.name} has no NetworkPerception component");
                return DefaultResult;
            }

            var target = m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkTrackTarget] Target is null");
                return DefaultResult;
            }

            var targetNetworkObject = target.GetComponent<NetworkObject>();
            if (targetNetworkObject == null || !targetNetworkObject.IsSpawned)
            {
                Debug.LogWarning($"[InstructionNetworkTrackTarget] Target {target.name} has no spawned NetworkObject");
                return DefaultResult;
            }

            // Send to server for authoritative tracking
            networkPerception.TrackTargetRpc(targetNetworkObject.NetworkObjectId);

            Debug.Log($"[InstructionNetworkTrackTarget] Requested track {target.name}");

            return DefaultResult;
        }
    }
}
