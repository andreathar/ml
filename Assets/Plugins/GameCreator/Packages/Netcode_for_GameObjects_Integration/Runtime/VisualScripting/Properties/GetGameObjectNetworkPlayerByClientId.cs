using System;
using GameCreator.Runtime.Common;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Network Player by Client ID")]
    [Category("Network/Network Player by Client ID")]

    [Description("Returns a NetworkCharacter by their Client ID")]
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
            if (NetworkManager.Singleton == null)
            {
                return null;
            }

            // Search for NetworkCharacter with matching owner client ID
            NetworkCharacter[] networkCharacters = UnityEngine.Object.FindObjectsByType<NetworkCharacter>(FindObjectsSortMode.None);
            foreach (NetworkCharacter character in networkCharacters)
            {
                NetworkObject networkObject = character.GetComponent<NetworkObject>();
                if (networkObject != null &&
                    networkObject.IsSpawned &&
                    networkObject.OwnerClientId == clientId)
                {
                    return character.gameObject;
                }
            }

            return null;
        }

        public static PropertyGetGameObject Create(int clientId = 0)
        {
            GetGameObjectNetworkPlayerByClientId instance = new GetGameObjectNetworkPlayerByClientId
            {
                m_ClientId = new PropertyGetInteger(clientId)
            };
            return new PropertyGetGameObject(instance);
        }

        public override string String => $"Network Player (Client {this.m_ClientId})";
    }
}
