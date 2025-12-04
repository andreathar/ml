using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Set Game State")]
    [Description("Sets the network game state (Server-only).")]
    [Category("Network/Game State/Set Game State")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "State", "Lobby", "Playing", "Game")]
    [Serializable]
    public class InstructionNetworkSetGameState : Instruction
    {
        [SerializeField]
        private NetworkGameStateManager.GameState m_State = NetworkGameStateManager
            .GameState
            .Playing;

        public override string Title => $"Set Game State to {m_State}";

        protected override Task Run(Args args)
        {
            if (NetworkGameStateManager.Instance != null)
            {
                NetworkGameStateManager.Instance.SetState(m_State);
            }
            else
            {
                Debug.LogWarning(
                    "[InstructionNetworkSetGameState] NetworkGameStateManager not found"
                );
            }

            return DefaultResult;
        }
    }
}
