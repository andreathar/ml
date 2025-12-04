using System;
using GameCreator.Runtime.Common;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Properties.Core
{
    [Title("Network Character Position by Client ID")]
    [Category("Network/Network Character Position by Client ID")]
    [Description("Returns the world position of a NetworkCharacter by their owner's Client ID")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Yellow)]
    [Serializable]
    public class GetPositionNetworkCharacterByClientId : PropertyTypeGetPosition
    {
        [SerializeField]
        private PropertyGetInteger m_ClientId = new PropertyGetInteger(0);

        public override Vector3 Get(Args args)
        {
            ulong clientId = (ulong)this.m_ClientId.Get(args);
            return NetworkCharacterRegistry.GetPositionByClientId(clientId);
        }

        public override Vector3 Get(GameObject gameObject)
        {
            ulong clientId = (ulong)this.m_ClientId.Get(gameObject);
            return NetworkCharacterRegistry.GetPositionByClientId(clientId);
        }

        public static PropertyGetPosition Create(int clientId = 0)
        {
            var instance = new GetPositionNetworkCharacterByClientId
            {
                m_ClientId = new PropertyGetInteger(clientId),
            };
            return new PropertyGetPosition(instance);
        }

        public override string String => $"Network Player (Client {this.m_ClientId}) Position";
    }
}
