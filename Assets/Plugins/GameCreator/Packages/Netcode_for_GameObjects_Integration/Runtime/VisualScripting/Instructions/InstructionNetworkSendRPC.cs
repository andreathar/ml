using System;
using System.Threading.Tasks;
using GameCreator.Runtime.Common;
using GameCreator.Runtime.VisualScripting;
using UnityEngine;

namespace GameCreator.Netcode.Runtime.VisualScripting
{
    [Version(1, 0, 0)]
    [Title("Send RPC Message")]
    [Description("Sends an RPC message through the NetworkRPCManager.")]
    [Category("Network/RPC/Send RPC Message")]
    [Image(typeof(IconCharacter), ColorTheme.Type.Blue)]
    [Keywords("Network", "Multiplayer", "RPC", "Message", "Send", "Event")]
    [Serializable]
    public class InstructionNetworkSendRPC : Instruction
    {
        public enum SendTarget
        {
            Server,
            AllClients,
            Broadcast,
        }

        [SerializeField]
        private SendTarget m_Target = SendTarget.Server;

        [SerializeField]
        private PropertyGetString m_Channel = new PropertyGetString("events");

        [SerializeField]
        private PropertyGetString m_EventName = new PropertyGetString("custom_event");

        [SerializeField]
        private PropertyGetString m_StringData = new PropertyGetString("");

        [SerializeField]
        private PropertyGetInteger m_IntData = new PropertyGetInteger(0);

        [SerializeField]
        private PropertyGetDecimal m_FloatData = new PropertyGetDecimal(0f);

        public override string Title => $"Send RPC '{m_EventName}' to {m_Target}";

        protected override Task Run(Args args)
        {
            if (NetworkRPCManager.Instance == null)
            {
                Debug.LogWarning("[InstructionNetworkSendRPC] NetworkRPCManager not found");
                return DefaultResult;
            }

            string channel = m_Channel.Get(args);
            string eventName = m_EventName.Get(args);
            string stringData = m_StringData.Get(args);
            int intData = (int)m_IntData.Get(args);
            float floatData = (float)m_FloatData.Get(args);

            switch (m_Target)
            {
                case SendTarget.Server:
                    NetworkRPCManager.Instance.SendToServer(
                        channel,
                        eventName,
                        stringData,
                        intData,
                        floatData
                    );
                    break;

                case SendTarget.AllClients:
                    NetworkRPCManager.Instance.SendToAllClients(
                        channel,
                        eventName,
                        stringData,
                        intData,
                        floatData
                    );
                    break;

                case SendTarget.Broadcast:
                    NetworkRPCManager.Instance.RequestBroadcast(
                        channel,
                        eventName,
                        stringData,
                        intData,
                        floatData
                    );
                    break;
            }

            return DefaultResult;
        }
    }
}
