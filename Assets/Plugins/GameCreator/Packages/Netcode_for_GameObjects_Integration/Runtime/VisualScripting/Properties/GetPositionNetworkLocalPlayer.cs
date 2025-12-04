using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Network Local Player Position")]
    [Category("Network/Network Local Player Position")]
    [Description("Returns the world position of the local client's NetworkCharacter")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Serializable]
    public class GetPositionNetworkLocalPlayer : PropertyTypeGetPosition
    {
        public override Vector3 Get(Args args)
        {
            return GetLocalPlayerPosition();
        }

        public override Vector3 Get(GameObject gameObject)
        {
            return GetLocalPlayerPosition();
        }

        private static Vector3 GetLocalPlayerPosition()
        {
            // Use registry for efficient lookup
            var localPlayer = NetworkCharacterRegistry.LocalPlayer;
            if (localPlayer != null)
            {
                return localPlayer.transform.position;
            }

            // Fallback to ShortcutPlayer for single-player or editor preview
            Transform transform = ShortcutPlayer.Transform;
            return transform != null ? transform.position : Vector3.zero;
        }

        public static PropertyGetPosition Create =>
            new PropertyGetPosition(new GetPositionNetworkLocalPlayer());

        public override string String => "Network Local Player";

        public override Vector3 EditorValue
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
                        return instance.transform.position;
                }

                return Vector3.zero;
            }
        }
    }
}
