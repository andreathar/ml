using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.Variables;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Title("Collect NPCs")]
    [Description("Collects all spawned NPC NetworkCharacters (server-authoritative) into a List Variable")]

    [Image(typeof(IconBust), ColorTheme.Type.Yellow, typeof(OverlayListVariable))]

    [Category("Network/Collect NPCs")]

    [Parameter("Store In", "List where the collected NPC GameObjects are saved")]

    [Keywords("Gather", "Get", "Set", "Array", "List", "Variables", "Network", "Multiplayer", "NPC", "AI")]

    [Serializable]
    public class InstructionCollectNPCs : Instruction
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        private CollectorListVariable m_StoreIn = new CollectorListVariable();

        // PROPERTIES: ----------------------------------------------------------------------------

        public override string Title => "Collect all NPCs";

        // RUN METHOD: ----------------------------------------------------------------------------

        protected override Task Run(Args args)
        {
            List<GameObject> elements = new List<GameObject>();

            foreach (var npc in NetworkCharacterRegistry.NPCs)
            {
                if (npc == null) continue;
                elements.Add(npc.gameObject);
            }

            this.m_StoreIn.Fill(elements.ToArray(), args);
            return DefaultResult;
        }
    }
}
