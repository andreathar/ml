using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Instructions.Core
{
    [Version(1, 0, 0)]
    [Title("Disconnect Network")]
    [Description("Disconnects from the network (shuts down NetworkManager).")]
    [Category("Network/Disconnect")]
    [Image(typeof(IconStop), ColorTheme.Type.Red)]
    [Keywords("Network", "Multiplayer", "Disconnect", "Stop", "Shutdown")]
    [Serializable]
    public class InstructionNetworkDisconnect : Instruction
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Disconnect Network";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[InstructionNetworkDisconnect] NetworkManager not found.");
                return Task.CompletedTask;
            }

            if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("[InstructionNetworkDisconnect] Not connected to network.");
                return Task.CompletedTask;
            }

            NetworkManager.Singleton.Shutdown();
            Debug.Log("[InstructionNetworkDisconnect] Network shutdown complete.");

            return Task.CompletedTask;
        }
    }
}
