using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Network Local Player Location")]
    [Category("Network/Network Local Player Location")]
    [Description(
        "Returns the position and rotation of the local client's NetworkCharacter as a Location"
    )]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Serializable]
    public class GetLocationNetworkLocalPlayer : PropertyTypeGetLocation
    {
        public override Location Get(Args args)
        {
            return GetLocalPlayerLocation();
        }

        public override Location Get(GameObject gameObject)
        {
            return GetLocalPlayerLocation();
        }

        private static Location GetLocalPlayerLocation()
        {
            Transform transform = GetLocalPlayerTransform();
            if (transform == null)
            {
                return new Location(Vector3.zero, Quaternion.identity);
            }

            return new Location(transform.position, transform.rotation);
        }

        private static Transform GetLocalPlayerTransform()
        {
            // Use registry for efficient lookup
            var localPlayer = NetworkCharacterRegistry.LocalPlayer;
            if (localPlayer != null)
            {
                return localPlayer.transform;
            }

            // Fallback to ShortcutPlayer for single-player or editor preview
            return ShortcutPlayer.Transform;
        }

        public static PropertyGetLocation Create =>
            new PropertyGetLocation(new GetLocationNetworkLocalPlayer());

        public override string String => "Network Local Player";

        public override Location EditorValue
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
                    {
                        return new Location(
                            instance.transform.position,
                            instance.transform.rotation
                        );
                    }
                }

                return new Location(Vector3.zero, Quaternion.identity);
            }
        }
    }
}
