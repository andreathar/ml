using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Spawn Network Player")]
    [Description("Spawns a player prefab on the network with ownership assigned to a client.")]
    [Category("Network/Spawn Network Player")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Spawn", "Player", "Instantiate")]
    [Serializable]
    public class InstructionNetworkSpawnPlayer : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_PlayerPrefab = GetGameObjectInstance.Create();

        [SerializeField]
        private PropertyGetPosition m_SpawnPosition = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetRotation m_SpawnRotation = GetRotationCharactersPlayer.Create;

        [SerializeField]
        [Tooltip("Client ID to assign ownership. Use -1 for server/host.")]
        private PropertyGetInteger m_OwnerClientId = new PropertyGetInteger(-1);

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => $"Spawn Network Player at {this.m_SpawnPosition}";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Debug.LogWarning("[InstructionNetworkSpawnPlayer] Only server can spawn players.");
                return Task.CompletedTask;
            }

            GameObject prefab = this.m_PlayerPrefab.Get(args);
            if (prefab == null)
            {
                Debug.LogError("[InstructionNetworkSpawnPlayer] Player prefab is null.");
                return Task.CompletedTask;
            }

            Vector3 position = this.m_SpawnPosition.Get(args);
            Quaternion rotation = this.m_SpawnRotation.Get(args);
            int ownerClientId = (int)this.m_OwnerClientId.Get(args);

            // Instantiate the prefab
            GameObject instance = UnityEngine.Object.Instantiate(prefab, position, rotation);

            // Get NetworkObject component
            NetworkObject networkObject = instance.GetComponent<NetworkObject>();
            if (networkObject == null)
            {
                Debug.LogError(
                    "[InstructionNetworkSpawnPlayer] Prefab must have NetworkObject component."
                );
                UnityEngine.Object.Destroy(instance);
                return Task.CompletedTask;
            }

            // Spawn with ownership
            if (ownerClientId >= 0)
            {
                networkObject.SpawnWithOwnership((ulong)ownerClientId);
            }
            else
            {
                networkObject.Spawn();
            }

            Debug.Log(
                $"[InstructionNetworkSpawnPlayer] Spawned player at {position} with owner {ownerClientId}"
            );

            return Task.CompletedTask;
        }
    }
}
