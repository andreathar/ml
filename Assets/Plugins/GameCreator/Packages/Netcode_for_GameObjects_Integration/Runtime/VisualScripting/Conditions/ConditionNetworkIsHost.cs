using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Conditions.Core
{
    [Version(1, 0, 0)]
    [Title("Is Host")]
    [Description("Returns true if this instance is running as both server and client (host).")]
    [Category("Network/Is Host")]
    [Image(typeof(IconSignal), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Host", "Server", "Client")]
    [Serializable]
    public class ConditionNetworkIsHost : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => "Is Host";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (NetworkManager.Singleton == null)
                return false;
            return NetworkManager.Singleton.IsHost;
        }
    }
}
