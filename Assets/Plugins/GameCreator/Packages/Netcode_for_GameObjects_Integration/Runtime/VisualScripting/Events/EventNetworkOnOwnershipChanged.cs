using System;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using Unity.Netcode;
using UnityEngine;
using TriggerEvent = GameCreator.Runtime.VisualScripting.Event;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("On Ownership Changed")]
    [Description("Triggered when ownership of this NetworkObject changes.")]
    [Category("Network/On Ownership Changed")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Yellow)]
    [Keywords("Network", "Multiplayer", "Owner", "Transfer", "Authority")]

    [Serializable]
    public class EventNetworkOnOwnershipChanged : TriggerEvent
    {
        // EXPOSED MEMBERS: -----------------------------------------------------------------------

        [SerializeField]
        [Tooltip("If true, only triggers when becoming the owner. If false, triggers on any ownership change.")]
        private bool m_OnlyWhenGainingOwnership = false;

        // MEMBERS: -------------------------------------------------------------------------------

        [NonSerialized] private Args m_Args;
        [NonSerialized] private NetworkCharacterAdapter m_Adapter;

        // INITIALIZERS: --------------------------------------------------------------------------

        protected internal override void OnEnable(Trigger trigger)
        {
            base.OnEnable(trigger);
            this.m_Args = new Args(trigger.gameObject);

            this.m_Adapter = trigger.GetComponent<NetworkCharacterAdapter>();
            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventOwnershipChanged += OnOwnershipChanged;
            }
        }

        protected internal override void OnDisable(Trigger trigger)
        {
            base.OnDisable(trigger);

            if (this.m_Adapter != null)
            {
                this.m_Adapter.EventOwnershipChanged -= OnOwnershipChanged;
            }
        }

        // PRIVATE METHODS: -----------------------------------------------------------------------

        private void OnOwnershipChanged(ulong oldOwner, ulong newOwner)
        {
            if (this.m_OnlyWhenGainingOwnership)
            {
                // Only trigger if we are the new owner
                if (NetworkManager.Singleton == null) return;
                if (newOwner != NetworkManager.Singleton.LocalClientId) return;
            }

            Debug.Log($"[EventNetworkOnOwnershipChanged] Ownership changed from {oldOwner} to {newOwner}");
            _ = this.m_Trigger.Execute(this.m_Args);
        }
    }
}
