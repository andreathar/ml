using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Load Network Scene")]
    [Description("Loads a scene for all network clients (Server-only).")]
    [Category("Network/Scene/Load Network Scene")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Scene", "Load", "Level")]
    [Serializable]
    public class InstructionNetworkLoadScene : Instruction
    {
        [SerializeField]
        private PropertyGetString m_SceneName = new PropertyGetString("Game");

        [SerializeField]
        private UnityEngine.SceneManagement.LoadSceneMode m_LoadMode = UnityEngine
            .SceneManagement
            .LoadSceneMode
            .Single;

        public override string Title => $"Load Network Scene '{m_SceneName}'";

        protected override Task Run(Args args)
        {
            if (NetworkSceneCoordinator.Instance == null)
            {
                Debug.LogWarning("[InstructionNetworkLoadScene] NetworkSceneCoordinator not found");
                return DefaultResult;
            }

            string sceneName = m_SceneName.Get(args);
            NetworkSceneCoordinator.Instance.LoadScene(sceneName, m_LoadMode);

            return DefaultResult;
        }
    }
}
