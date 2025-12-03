using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Network Local Player")]
    [Category("Network/Network Local Player")]

    [Description("Returns the local client's NetworkCharacter - the character owned by this client")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]

    [Serializable]
    public class GetGameObjectNetworkLocalPlayer : PropertyTypeGetGameObject
    {
        public override GameObject Get(Args args)
        {
            return GetLocalPlayer();
        }

        public override GameObject Get(GameObject gameObject)
        {
            return GetLocalPlayer();
        }

        private static GameObject GetLocalPlayer()
        {
            // First try ShortcutPlayer (most efficient)
            if (ShortcutPlayer.Instance != null)
            {
                return ShortcutPlayer.Instance;
            }

            // Fallback: Find the NetworkCharacter owned by local client
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsClient)
            {
                return null;
            }

            // Search for local player's NetworkCharacter
            NetworkCharacter[] networkCharacters = UnityEngine.Object.FindObjectsByType<NetworkCharacter>(FindObjectsSortMode.None);
            foreach (NetworkCharacter character in networkCharacters)
            {
                if (character.IsLocalOwner && character.IsNetworkSpawned)
                {
                    return character.gameObject;
                }
            }

            return null;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectNetworkLocalPlayer instance = new GetGameObjectNetworkLocalPlayer();
            return new PropertyGetGameObject(instance);
        }

        public override string String => "Network Local Player";

        public override GameObject EditorValue
        {
            get
            {
                // In editor, find any character marked as player
                Character[] instances = UnityEngine.Object.FindObjectsByType<Character>(FindObjectsSortMode.None);
                foreach (Character instance in instances)
                {
                    if (instance.IsPlayer) return instance.gameObject;
                }

                return null;
            }
        }
    }
}
