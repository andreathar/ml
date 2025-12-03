using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is Spawned")]
    [Description("Returns true if the specified NetworkObject is spawned on the network.")]
    [Category("Network/Is Spawned")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Spawned", "Active", "Exists")]

    [Serializable]
    public class ConditionNetworkIsSpawned : Condition
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private PropertyGetGameObject m_Target = GetGameObjectSelf.Create();

        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => $"{this.m_Target} Is Spawned";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            GameObject target = this.m_Target.Get(args);
            if (target == null) return false;

            NetworkObject networkObject = target.GetComponent<NetworkObject>();
            if (networkObject == null) return false;

            return networkObject.IsSpawned;
        }
    }
}
