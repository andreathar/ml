using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Start Countdown")]
    [Description("Starts the game countdown timer (Server-only).")]
    [Category("Network/Game State/Start Countdown")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Countdown", "Start", "Timer")]

    [Serializable]
    public class InstructionNetworkStartCountdown : Instruction
    {
        [SerializeField]
        [Tooltip("Custom countdown duration. Use 0 or negative for default.")]
        private PropertyGetDecimal m_Duration = new PropertyGetDecimal(3f);

        public override string Title => "Start Game Countdown";

        protected override Task Run(Args args)
        {
            if (NetworkGameStateManager.Instance != null)
            {
                float duration = (float)m_Duration.Get(args);
                if (duration > 0)
                {
                    NetworkGameStateManager.Instance.StartCountdown(duration);
                }
                else
                {
                    NetworkGameStateManager.Instance.StartCountdown();
                }
            }
            else
            {
                Debug.LogWarning("[InstructionNetworkStartCountdown] NetworkGameStateManager not found");
            }

            return DefaultResult;
        }
    }
}
