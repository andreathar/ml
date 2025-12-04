using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting.Instructions.Core
{
    [Version(1, 0, 0)]
    [Title("Start Network Host")]
    [Description("Starts the NetworkManager as a host (server + client).")]
    [Category("Network/Start Host")]
    [Image(typeof(IconPlay), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Host", "Server", "Start")]
    [Serializable]
    public class InstructionNetworkStartHost : Instruction
    {
        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Start Network Host";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[InstructionNetworkStartHost] NetworkManager not found in scene.");
                return Task.CompletedTask;
            }

            if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsClient)
            {
                Debug.LogWarning("[InstructionNetworkStartHost] Network already started.");
                return Task.CompletedTask;
            }

            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                Debug.Log("[InstructionNetworkStartHost] Host started successfully.");
            }
            else
            {
                Debug.LogError("[InstructionNetworkStartHost] Failed to start host.");
            }

            return Task.CompletedTask;
        }
    }
}
