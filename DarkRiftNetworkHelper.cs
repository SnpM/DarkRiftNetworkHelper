using UnityEngine;
using System.Collections;
using DarkRift;

namespace Lockstep
{
    public class DarkRiftNetworkHelper : NetworkHelper
    {
        public DarkRiftNetworkHelper () {
            DarkRiftAPI.onDataDetailed += DarkRiftAPI_onDataDetailed;
        }

        void DarkRiftAPI_onDataDetailed (ushort sender, byte tag, ushort subject, object data)
        {
            byte[] byteData = data as byte[];
            if (byteData != null) {
                base.Receive ((MessageType)tag,byteData);
            }
        }
        public override void Connect(string ip)
        {
            DarkRiftAPI.Connect(ip);
        }

        public override void Disconnect()
        {
            DarkRiftAPI.Disconnect();
        }

        public override ushort ID
        {
            get
            {
                return DarkRiftAPI.id;
            }
        }
        public override void Host(int roomSize)
        {
            Debug.Log ("DarkRift integration does not support client-hosted servers yet.");
        }

        public override bool IsConnected
        {
            get
            {
                return DarkRiftAPI.isConnected;
            }
        }

        public override bool IsServer
        {
            get
            {
                return false;
            }
        }
        public override int PlayerCount
        {
            get
            {
                return 0;
            }
        }

        public override void SendMessageToAll(MessageType messageType, byte[] data)
        {
            //Implemented for possible client-hosted server in the future
            DarkRiftAPI.SendMessageToAll ((byte)messageType,0,data);
        }

        public override void SendMessageToServer(MessageType messageType, byte[] data)
        {
            DarkRiftAPI.SendMessageToServer((byte)messageType,0,data);
        }

    }
}