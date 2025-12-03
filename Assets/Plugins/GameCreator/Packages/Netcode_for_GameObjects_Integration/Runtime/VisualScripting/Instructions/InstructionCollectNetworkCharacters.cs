using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Collect Network Characters")]
    [Description("Collects all spawned NetworkCharacters into a List Variable")]

    [Image(typeof(IconBust), ColorTheme.Type.Green, typeof(OverlayListVariable))]

    [Category("Network/Collect Network Characters")]

    [Parameter("Store In", "List where the collected NetworkCharacter GameObjects are saved")]
    [Parameter("Include Local Player", "Whether to include the local player's character in the list")]

    [Keywords("Gather", "Get", "Set", "Array", "List", "Variables", "Network", "Multiplayer", "Player")]

    [Serializable]
    public class InstructionCollectNetworkCharacters : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CollectorListVariable m_StoreIn = new CollectorListVariable();

        [SerializeField]
        private bool m_IncludeLocalPlayer = true;

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => this.m_IncludeLocalPlayer
            ? "Collect all Network Characters"
            : "Collect other Network Characters (exclude local)";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            List<GameObject> elements = new List<GameObject>();

            var localPlayer = NetworkCharacterRegistry.LocalPlayer;

            foreach (var character in NetworkCharacterRegistry.All)
            {
                if (character == null) continue;

                // Skip local player if not including
                if (!this.m_IncludeLocalPlayer && character == localPlayer)
                {
                    continue;
                }

                elements.Add(character.gameObject);
            }

            this.m_StoreIn.Fill(elements.ToArray(), args);
            return DefaultResult;
        }
    }
}
