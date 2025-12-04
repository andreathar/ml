using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Network Local Player Rotation")]
    [Category("Network/Network Local Player Rotation")]
    [Description("Returns the rotation of the local client's NetworkCharacter")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Green)]
    [Serializable]
    public class GetRotationNetworkLocalPlayer : PropertyTypeGetRotation
    {
        [SerializeField]
        private RotationSpace m_Space = RotationSpace.Global;

        public override Quaternion Get(Args args)
        {
            return GetLocalPlayerRotation();
        }

        public override Quaternion Get(GameObject gameObject)
        {
            return GetLocalPlayerRotation();
        }

        private Quaternion GetLocalPlayerRotation()
        {
            Transform transform = GetLocalPlayerTransform();
            if (transform == null)
                return Quaternion.identity;

            return this.m_Space switch
            {
                RotationSpace.Local => transform.localRotation,
                RotationSpace.Global => transform.rotation,
                _ => throw new ArgumentOutOfRangeException(),
            };
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

        public static PropertyGetRotation Create =>
            new PropertyGetRotation(new GetRotationNetworkLocalPlayer());

        public override string String => $"{this.m_Space} Network Local Player";

        public override Quaternion EditorValue
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
                        return instance.transform.rotation;
                }

                return Quaternion.identity;
            }
        }
    }
}
