using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Countdown Timer")]
    [Category("Network/Countdown Timer")]
    [Description("Returns the remaining countdown time in seconds.")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetDecimalCountdown : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkGameStateManager.Instance?.CountdownRemaining ?? 0;
        }

        public override double Get(GameObject gameObject)
        {
            return NetworkGameStateManager.Instance?.CountdownRemaining ?? 0;
        }

        public static PropertyGetDecimal Create()
        {
            GetDecimalCountdown instance = new GetDecimalCountdown();
            return new PropertyGetDecimal(instance);
        }

        public override string String => "Countdown Timer";
    }
}
