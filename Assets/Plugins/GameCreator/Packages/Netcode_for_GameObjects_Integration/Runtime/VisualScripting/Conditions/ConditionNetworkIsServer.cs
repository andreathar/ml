using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Is Server")]
    [Description("Returns true if this instance is running as the server.")]
    [Category("Network/Is Server")]
    [Image(typeof(IconSignal), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Server", "Host", "Authority")]
    [Serializable]
    public class ConditionNetworkIsServer : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => "Is Server";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (NetworkManager.Singleton == null)
                return false;
            return NetworkManager.Singleton.IsServer;
        }
    }
}
