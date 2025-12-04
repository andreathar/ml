using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Start Network Client")]
    [Description("Starts the NetworkManager as a client and connects to a host.")]
    [Category("Network/Start Client")]
    [Image(typeof(IconPlay), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "Client", "Connect", "Join")]
    [Serializable]
    public class InstructionNetworkStartClient : Instruction
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Start Network Client";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError(
                    "[InstructionNetworkStartClient] NetworkManager not found in scene."
                );
                return Task.CompletedTask;
            }

            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("[InstructionNetworkStartClient] Network already started.");
                return Task.CompletedTask;
            }

            bool success = NetworkManager.Singleton.StartClient();

            if (success)
            {
                Debug.Log("[InstructionNetworkStartClient] Client started, connecting...");
            }
            else
            {
                Debug.LogError("[InstructionNetworkStartClient] Failed to start client.");
            }

            return Task.CompletedTask;
        }
    }
}
