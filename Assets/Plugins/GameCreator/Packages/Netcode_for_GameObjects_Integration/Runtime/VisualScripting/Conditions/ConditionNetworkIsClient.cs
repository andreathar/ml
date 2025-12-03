using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is Client")]
    [Description("Returns true if this instance is running as a client.")]
    [Category("Network/Is Client")]
    [Image(typeof(IconPlayer), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Client", "Connected")]

    [Serializable]
    public class ConditionNetworkIsClient : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => "Is Client";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (NetworkManager.Singleton == null) return false;
            return NetworkManager.Singleton.IsClient;
        }
    }
}
