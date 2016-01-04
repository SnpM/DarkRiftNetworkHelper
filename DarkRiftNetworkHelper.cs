using UnityEngine;
using System.Collections;
using DarkRift;
using Lockstep.NetworkHelpers.DarkRift;
namespace Lockstep.NetworkHelpers
{
    public class DarkRiftNetworkHelper : NetworkHelper
    {
        public DarkRiftNetworkHelper () {
            Awake ();
        }
       
        void Awake () {
            DarkRiftAPI.onData += HandleData;
            ServerWrapper = new DarkRiftServerWrapper();
            ConnectionService.onPlayerConnect += ConnectionService_onPlayerConnect;
            ConnectionService.onPlayerDisconnect += ConnectionService_onPlayerDisconnect;
            ConnectionService.onServerMessage += this.HandleServerData;
        }
        void HandleServerData (ConnectionService service, NetworkMessage message) {
            this.HandleData(message.tag,message.subject,message.data);
        }



        void ConnectionService_onPlayerDisconnect (ConnectionService con)
        {
            this.Connections.Add(con);
        }

        void ConnectionService_onPlayerConnect (ConnectionService con)
        {
            Connections.Add(con);
        }
        public DarkRiftServerWrapper ServerWrapper {get; private set;}
        int port = 4296;

        FastList<ConnectionService> Connections = new FastList<ConnectionService>();


        void HandleData (byte tag, ushort subject, object data)
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
            else {
            DarkRiftAPI.Disconnect();
            }

        }

        public override void Simulate()
        {
            ServerWrapper.FixedUpdate();
        }

        public override int ID
        {
            get
            {
                return (int)DarkRiftAPI.id;
            }
        }
        public override void Host(int roomSize)
        {
            ServerWrapper.Awake();
            this._isServer = true;

        }

        public override bool IsConnected
        {
            get
            {
                return this.IsServer || DarkRiftAPI.isConnected;
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
        protected override void OnSendMessageToServer(MessageType messageType, byte[] data)
        {
            DarkRiftAPI.SendMessageToServer((byte)messageType,0,data);
        }

        protected override void OnSendMessageToAll(MessageType messageType, byte[] data)
        {
            for (int i = 0; i < Connections.Count; i++) {
                Connections[i].SendReply((byte)messageType,0,data);
            }
            this.Receive(messageType, data);
        }


    }
}