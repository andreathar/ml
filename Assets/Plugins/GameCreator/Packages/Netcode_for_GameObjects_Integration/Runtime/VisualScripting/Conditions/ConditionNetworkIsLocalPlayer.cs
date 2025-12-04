using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Conditions.Core
{
    [Version(1, 0, 0)]
    [Title("Is Local Network Player")]
    [Description(
        "Returns true if the specified character is the local client's player (owned by this client)"
    )]
    [Category("Network/Is Local Network Player")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Player", "Local", "Owner", "Client")]
    [Serializable]
    public class ConditionNetworkIsLocalPlayer : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Character = GetGameObjectInstance.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"is {this.m_Character} Local Player";

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
                return networkCharacter.IsLocalOwner && networkCharacter.IsNetworkSpawned;
            }

            // Check NetworkObject directly
            NetworkObject networkObject = target.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                return networkObject.IsOwner && networkObject.IsSpawned;
            }

            // No network components - check if marked as player in GameCreator
            var character = target.GetComponent<GameCreator.Runtime.Characters.Character>();
            return character != null && character.IsPlayer;
        }
    }
}
