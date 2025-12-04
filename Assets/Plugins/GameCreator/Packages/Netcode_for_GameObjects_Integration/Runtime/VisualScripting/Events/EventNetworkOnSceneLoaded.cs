using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Network Scene Loaded")]
    [Description("Triggered when a network scene finishes loading for all clients.")]
    [Category("Network/Scene/On Network Scene Loaded")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Green)]
    [Keywords("Network", "Multiplayer", "Scene", "Load", "Ready")]
    [Serializable]
    public class EventNetworkOnSceneLoaded : TriggerEvent
    {
        [SerializeField]
        [Tooltip("Scene name to filter (empty = any scene)")]
        private string m_SceneName = "";

        [NonSerialized]
        private Args m_Args;

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);
            NetworkSceneCoordinator.EventAllClientsReady += OnAllClientsReady;
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);
            NetworkSceneCoordinator.EventAllClientsReady -= OnAllClientsReady;
        }

        private void OnAllClientsReady(string sceneName)
        {
            // Filter by scene name
            if (!string.IsNullOrEmpty(m_SceneName) && sceneName != m_SceneName)
                return;

            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
