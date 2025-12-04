using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("NPC Count")]
    [Category("Network/NPC Count")]
    [Description("Returns the number of spawned NPCs (server-authoritative characters)")]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetDecimalNPCCount : PropertyTypeGetDecimal
    {
        public override double Get(Args args)
        {
            return NetworkCharacterRegistry.NPCCount;
        }

        public override double Get(GameObject gameObject)
        {
            return NetworkCharacterRegistry.NPCCount;
        }

        public static PropertyGetDecimal Create()
        {
            GetDecimalNPCCount instance = new GetDecimalNPCCount();
            return new PropertyGetDecimal(instance);
        }

        public override string String => "NPC Count";
    }
}
