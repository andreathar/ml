using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is NPC")]
    [Description(
        "Returns true if the specified character is an NPC (server-authoritative, not a player)"
    )]
    [Category("Network/Is NPC")]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "NPC", "AI", "Server", "Character")]
    [Serializable]
    public class ConditionNetworkIsNPC : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectInstance.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is {this.m_Character} NPC";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Character.Get(args);
            if (target == null)
                return false;

            // Check if it's a NetworkCharacter
            NetworkCharacter networkCharacter = target.GetComponent<NetworkCharacter>();
            if (networkCharacter != null)
            {
                return networkCharacter.IsNPC;
            }

            // Fallback to checking GameCreator Character
            var character = target.GetComponent<GameCreator.Runtime.Characters.Character>();
            return character != null && !character.IsPlayer;
        }
    }
}
