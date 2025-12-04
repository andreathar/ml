using System;
using GameCreator.Runtime.Characters;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Closest NPC")]
    [Category("Network/Closest NPC")]
    [Description(
        "Returns the closest NPC (server-authoritative character) to the specified target"
    )]
    [Image(typeof(IconBust), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetGameObjectClosestNPC : PropertyTypeGetGameObject
    {
        [SerializeField]
        private PropertyGetGameObject m_From = GetGameObjectPlayer.Create();

        public override GameObject Get(Args args)
        {
            GameObject from = this.m_From.Get(args);
            if (from == null)
                return null;

            var closest = NetworkCharacterRegistry.GetClosestNPC(from.transform.position);
            return closest != null ? closest.gameObject : null;
        }

        public override GameObject Get(GameObject gameObject)
        {
            if (gameObject == null)
                return null;

            var closest = NetworkCharacterRegistry.GetClosestNPC(gameObject.transform.position);
            return closest != null ? closest.gameObject : null;
        }

        public static PropertyGetGameObject Create()
        {
            GetGameObjectClosestNPC instance = new GetGameObjectClosestNPC();
            return new PropertyGetGameObject(instance);
        }

        public override string String => $"Closest NPC to {this.m_From}";

        public override GameObject EditorValue
        {
            get
            {
                // In editor, find any character NOT marked as player
                Character[] instances = UnityEngine.Object.FindObjectsByType<Character>(
                    FindObjectsSortMode.None
                );
                foreach (Character instance in instances)
                {
                    if (!instance.IsPlayer)
                        return instance.gameObject;
                }

                return null;
            }
        }
    }
}
