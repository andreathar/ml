using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Network Local Player")]
    [Category("Network/Network Local Player")]
    [Description(
        "Returns the local client's NetworkCharacter - the character owned by this client"
    )]
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
            // Use registry for efficient cached lookup
            var localPlayer = NetworkCharacterRegistry.LocalPlayer;
            if (localPlayer != null)
            {
                return localPlayer.gameObject;
            }

            // Fallback to ShortcutPlayer for single-player or editor preview
            if (ShortcutPlayer.Instance != null)
            {
                return ShortcutPlayer.Instance;
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
                Character[] instances = UnityEngine.Object.FindObjectsByType<Character>(
                    FindObjectsSortMode.None
                );
                foreach (Character instance in instances)
                {
                    if (instance.IsPlayer)
                        return instance.gameObject;
                }

                return null;
            }
        }
    }
}
