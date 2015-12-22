using UnityEngine;
using System.Collections;
using DarkRift;

namespace Lockstep.DarkRift
{
    public class DarkRiftNetworkHelper : NetworkHelper
    {
        public DarkRiftServerWrapper ServerWrapper {get; private set;}
        int port = 4296;

        public DarkRiftNetworkHelper () {
            DarkRiftAPI.onData += DarkRiftAPI_onData;
            DarkRiftAPI.onDataDetailed += DarkRiftAPI_onDataDetailed;
            ServerWrapper = new DarkRiftServerWrapper();
        }

        void DarkRiftAPI_onData (byte tag, ushort subject, object data)
        {
            Debug.Log ("a");
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
            if (this._isServer)
            {
                ServerWrapper.OnApplicationQuit();
                this._isServer = false;
            }

            DarkRiftAPI.Disconnect();


        }

        public override void Simulate()
        {
            ServerWrapper.FixedUpdate();
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
            ServerWrapper.Awake();
            this._isServer = true;

            this.Connect("127.0.0.1");
        }

        public override bool IsConnected
        {
            get
            {
                return DarkRiftAPI.isConnected;
            }
        }

        private bool _isServer;

        public override bool IsServer
        {
            get
            {
                return _isServer;
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
            if (this.IsServer)
                this.Receive(messageType, data);
        }

        public override void SendMessageToServer(MessageType messageType, byte[] data)
        {
            if (this.IsServer)
                this.Receive(messageType, data);
            else
                DarkRiftAPI.SendMessageToServer((byte)messageType,0,data);
        }

    }
}