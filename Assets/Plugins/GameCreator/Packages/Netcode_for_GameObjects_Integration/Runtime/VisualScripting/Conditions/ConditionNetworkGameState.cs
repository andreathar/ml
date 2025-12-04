using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is Game State")]
    [Description("Returns true if the current game state matches the specified state.")]
    [Category("Network/Game State/Is Game State")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "State", "Lobby", "Playing", "Game")]

    [Serializable]
    public class ConditionNetworkGameState : Condition
    {
        [SerializeField]
        private NetworkGameStateManager.GameState m_State = NetworkGameStateManager.GameState.Playing;

        protected override string Summary => $"is Game State {m_State}";

        protected override bool Run(Args args)
        {
            if (NetworkGameStateManager.Instance == null) return false;
            return NetworkGameStateManager.Instance.CurrentState == m_State;
        }
    }
}
