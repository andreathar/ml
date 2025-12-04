using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Ready Player Count")]
    [Category("Network/Ready Player Count")]
    [Description("Returns the number of players who have marked themselves as ready.")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Serializable]
    public class GetDecimalReadyPlayerCount : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkGameStateManager.Instance?.ReadyPlayerCount ?? 0;
        }

        public override double Get(GameObject gameObject)
        {
            return NetworkGameStateManager.Instance?.ReadyPlayerCount ?? 0;
        }

        public static PropertyGetDecimal Create()
        {
            GetDecimalReadyPlayerCount instance = new GetDecimalReadyPlayerCount();
            return new PropertyGetDecimal(instance);
        }

        public override string String => "Ready Player Count";
    }
}
