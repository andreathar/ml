using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Instructions.Core
{
    [Version(1, 0, 0)]
    [Title("Despawn Network Object")]
    [Description("Despawns a NetworkObject from the network.")]
    [Category("Network/Despawn Network Object")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Despawn", "Remove", "Destroy")]
    [Serializable]
    public class InstructionNetworkDespawn : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectInstance.Create();

        [SerializeField]
        [Tooltip("Whether to destroy the GameObject after despawning.")]
        private PropertyGetBool m_DestroyAfterDespawn = new PropertyGetBool(true);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Despawn {this.m_Target}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[InstructionNetworkDespawn] Only server can despawn objects.");
                return Task.CompletedTask;
            }

            GameObject target = this.m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkDespawn] Target is null.");
                return Task.CompletedTask;
            }

            NetworkObject networkObject = target.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogWarning(
                    "[InstructionNetworkDespawn] Target has no NetworkObject component."
                );
                return Task.CompletedTask;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogWarning("[InstructionNetworkDespawn] Target is not spawned on network.");
                return Task.CompletedTask;
            }

            bool destroy = this.m_DestroyAfterDespawn.Get(args);
            networkObject.Despawn(destroy);

            Debug.Log($"[InstructionNetworkDespawn] Despawned {target.name}");

            return Task.CompletedTask;
        }
    }
}
