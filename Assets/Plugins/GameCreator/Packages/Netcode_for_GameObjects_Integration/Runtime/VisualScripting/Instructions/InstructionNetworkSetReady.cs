using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Set Player Ready")]
    [Description("Sets the local player's ready state.")]
    [Category("Network/Game State/Set Player Ready")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Ready", "Player", "Lobby")]

    [Serializable]
    public class InstructionNetworkSetReady : Instruction
    {
        [SerializeField]
        private PropertyGetBool m_IsReady = new PropertyGetBool(true);

        public override string Title => "Set Local Player Ready";

        protected override Task Run(Args args)
        {
            if (NetworkGameStateManager.Instance != null)
            {
                bool isReady = m_IsReady.Get(args);
                NetworkGameStateManager.Instance.SetReady(isReady);
            }
            else
            {
                Debug.LogWarning("[InstructionNetworkSetReady] NetworkGameStateManager not found");
            }

            return DefaultResult;
        }
    }
}
