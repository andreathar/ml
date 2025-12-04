using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Closest Network Character")]
    [Category("Network/Closest Network Character")]
    [Description(
        "Returns the closest NetworkCharacter (player or NPC) to the specified target, optionally excluding the local player"
    )]
    [Image(typeof(IconBust), ColorTheme.Type.Green)]
    [Serializable]
    public class GetGameObjectClosestCharacter : PropertyTypeGetGameObject
    {
        [SerializeField]
        private PropertyGetGameObject m_From = GetGameObjectPlayer.Create();

        [SerializeField]
        private bool m_ExcludeLocalPlayer = true;

        public override GameObject Get(Args args)
        {
            GameObject from = this.m_From.Get(args);
            if (from == null) return null;

            var closest = NetworkCharacterRegistry.GetClosestCharacter(
                from.transform.position,
                this.m_ExcludeLocalPlayer
            );
            return closest != null ? closest.gameObject : null;
        }

        public override GameObject Get(GameObject gameObject)
        {
            if (gameObject == null) return null;

            var closest = NetworkCharacterRegistry.GetClosestCharacter(
                gameObject.transform.position,
                this.m_ExcludeLocalPlayer
            );
            return closest != null ? closest.gameObject : null;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectClosestCharacter instance = new GetGameObjectClosestCharacter();
            return new PropertyGetGameObject(instance);
        }

        public override string String => this.m_ExcludeLocalPlayer
            ? $"Closest Character to {this.m_From} (exclude local)"
            : $"Closest Character to {this.m_From}";

        public override GameObject EditorValue
        {
            get
            {
                // In editor, find any character
                Character[] instances = UnityEngine.Object.FindObjectsByType<Character>(
                    FindObjectsSortMode.None
                );
                return instances.Length > 0 ? instances[0].gameObject : null;
            }
        }
    }
}
