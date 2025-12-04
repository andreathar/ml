using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Are All Players Ready")]
    [Description("Returns true if all connected players have marked themselves as ready.")]
    [Category("Network/Game State/Are All Players Ready")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Ready", "Players", "All", "Lobby")]
    [Serializable]
    public class ConditionNetworkAllPlayersReady : Condition
    {
        protected override string Summary => "are All Players Ready";

        protected override bool Run(Args args)
        {
            if (NetworkGameStateManager.Instance == null)
                return false;
            return NetworkGameStateManager.Instance.AreAllPlayersReady;
        }
    }
}
