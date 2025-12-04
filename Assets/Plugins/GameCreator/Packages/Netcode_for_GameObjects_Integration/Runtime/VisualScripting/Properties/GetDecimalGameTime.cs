using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Network Game Time")]
    [Category("Network/Network Game Time")]
    [Description("Returns the elapsed game time since the game started (seconds).")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Serializable]
    public class GetDecimalGameTime : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkGameStateManager.Instance?.GameTime ?? 0;
        }

        public override double Get(GameObject gameObject)
        {
            return NetworkGameStateManager.Instance?.GameTime ?? 0;
        }

        public static PropertyGetDecimal Create()
        {
            GetDecimalGameTime instance = new GetDecimalGameTime();
            return new PropertyGetDecimal(instance);
        }

        public override string String => "Network Game Time";
    }
}
