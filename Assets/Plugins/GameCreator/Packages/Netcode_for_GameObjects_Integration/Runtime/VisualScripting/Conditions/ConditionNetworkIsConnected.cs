using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Conditions.Core
{
    [Version(1, 0, 0)]
    [Title("Is Connected")]
    [Description("Returns true if connected to a network session (as server, host, or client).")]
    [Category("Network/Is Connected")]
    [Image(typeof(IconSignal), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Connected", "Online", "Active")]
    [Serializable]
    public class ConditionNetworkIsConnected : Condition
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        protected override string Summary => "Is Connected";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override bool Run(Args args)
        {
            if (NetworkManager.Singleton == null)
                return false;
            return NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient;
        }
    }
}
