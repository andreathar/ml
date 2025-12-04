using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is Owner")]
    [Description("Returns true if the local client owns the specified NetworkObject.")]
    [Category("Network/Is Owner")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Owner", "Authority", "Local")]
    [Serializable]
    public class ConditionNetworkIsOwner : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_Target} Is Owner";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null)
                return false;

            NetworkObject networkObject = target.GetComponent<NetworkObject>();
            if (networkObject == null)
                return false;

            return networkObject.IsOwner;
        }
    }
}
