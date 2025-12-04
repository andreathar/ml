using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Spawn Network NPC")]
    [Description("Spawns an NPC prefab on the network (Server-only).")]
    [Category("Network/Spawn/Spawn Network NPC")]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Spawn", "NPC", "AI", "Instantiate")]

    [Serializable]
    public class InstructionNetworkSpawnNPC : Instruction
    {
        [SerializeField]
        private PropertyGetGameObject m_Prefab = GetGameObjectInstance.Create();

        [SerializeField]
        private PropertyGetPosition m_Position = GetPositionCharactersPlayer.Create;

        [SerializeField]
        private PropertyGetRotation m_Rotation = GetRotationCharactersPlayer.Create;

        [SerializeField]
        [Tooltip("Use spawn queue to prevent flooding")]
        private bool m_UseQueue = true;

        public override string Title => $"Spawn NPC {m_Prefab}";

        protected override Task Run(Args args)
        {
            if (NetworkSpawnManager.Instance == null)
            {
                Debug.LogWarning("[InstructionNetworkSpawnNPC] NetworkSpawnManager not found");
                return DefaultResult;
            }

            GameObject prefab = m_Prefab.Get(args);
            if (prefab == null)
            {
                Debug.LogWarning("[InstructionNetworkSpawnNPC] Prefab is null");
                return DefaultResult;
            }

            Vector3 position = m_Position.Get(args);
            Quaternion rotation = m_Rotation.Get(args);

            if (m_UseQueue)
            {
                NetworkSpawnManager.Instance.QueueNPCSpawn(prefab, position, rotation);
            }
            else
            {
                NetworkSpawnManager.Instance.SpawnNPCImmediate(prefab, position, rotation);
            }

            return DefaultResult;
        }
    }
}
