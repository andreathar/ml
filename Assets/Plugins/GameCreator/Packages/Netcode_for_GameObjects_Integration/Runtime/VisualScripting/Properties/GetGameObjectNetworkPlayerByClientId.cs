using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Network Player by Client ID")]
    [Category("Network/Network Player by Client ID")]
    [Description("Returns a NetworkCharacter by their Client ID (0 = Host, 1+ = Clients)")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetGameObjectNetworkPlayerByClientId : PropertyTypeGetGameObject
    {
        [SerializeField]
        private PropertyGetInteger m_ClientId = new PropertyGetInteger(0);

        public override GameObject Get(Args args)
        {
            ulong clientId = (ulong)this.m_ClientId.Get(args);
            return GetPlayerByClientId(clientId);
        }

        public override GameObject Get(GameObject gameObject)
        {
            ulong clientId = (ulong)this.m_ClientId.Get(gameObject);
            return GetPlayerByClientId(clientId);
        }

        private static GameObject GetPlayerByClientId(ulong clientId)
        {
            // Use registry for efficient cached lookup
            var character = NetworkCharacterRegistry.GetByClientId(clientId);
            return character != null ? character.gameObject : null;
        }

        public static PropertyGetGameObject Create(int clientId = 0)
        {
            GetGameObjectNetworkPlayerByClientId instance = new GetGameObjectNetworkPlayerByClientId
            {
                m_ClientId = new PropertyGetInteger(clientId),
            };
            return new PropertyGetGameObject(instance);
        }

        public override string String => $"Network Player (Client {this.m_ClientId})";
    }
}
