using System;
using System.Linq;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Change Network Ownership")]
    [Description("Transfers ownership of a NetworkObject to another client.")]
    [Category("Network/Change Network Ownership")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Ownership", "Transfer", "Authority")]
    [Serializable]
    public class InstructionNetworkChangeOwnership : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectInstance.Create();

        [SerializeField]
        [Tooltip("Client ID to transfer ownership to.")]
        private PropertyGetInteger m_NewOwnerClientId = new PropertyGetInteger(0);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title =>
            $"Transfer ownership of {this.m_Target} to Client {this.m_NewOwnerClientId}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning(
                    "[InstructionNetworkChangeOwnership] Only server can change ownership."
                );
                return Task.CompletedTask;
            }

            GameObject target = this.m_Target.Get(args);
            if (target == null)
            {
                Debug.LogWarning("[InstructionNetworkChangeOwnership] Target is null.");
                return Task.CompletedTask;
            }

            NetworkObject networkObject = target.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogWarning(
                    "[InstructionNetworkChangeOwnership] Target has no NetworkObject component."
                );
                return Task.CompletedTask;
            }

            if (!networkObject.IsSpawned)
            {
                Debug.LogWarning(
                    "[InstructionNetworkChangeOwnership] Target is not spawned on network."
                );
                return Task.CompletedTask;
            }

            ulong newOwner = (ulong)this.m_NewOwnerClientId.Get(args);

            // Validate client exists
            if (!NetworkManager.Singleton.ConnectedClientsIds.Contains(newOwner))
            {
                Debug.LogWarning(
                    $"[InstructionNetworkChangeOwnership] Client {newOwner} is not connected."
                );
                return Task.CompletedTask;
            }

            ulong oldOwner = networkObject.OwnerClientId;
            networkObject.ChangeOwnership(newOwner);

            Debug.Log(
                $"[InstructionNetworkChangeOwnership] Transferred ownership from {oldOwner} to {newOwner}"
            );

            return Task.CompletedTask;
        }
    }
}
