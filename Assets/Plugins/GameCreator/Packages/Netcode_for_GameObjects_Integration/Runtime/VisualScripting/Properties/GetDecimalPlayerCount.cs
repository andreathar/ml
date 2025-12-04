using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Player Count")]
    [Category("Network/Player Count")]
    [Description("Returns the number of spawned player characters")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Serializable]
    public class GetDecimalPlayerCount : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkCharacterRegistry.PlayerCount;
        }

        public override double Get(GameObject gameObject)
        {
            return NetworkCharacterRegistry.PlayerCount;
        }

        public static PropertyGetDecimal Create()
        {
            GetDecimalPlayerCount instance = new GetDecimalPlayerCount();
            return new PropertyGetDecimal(instance);
        }

        public override string String => "Player Count";
    }
}
