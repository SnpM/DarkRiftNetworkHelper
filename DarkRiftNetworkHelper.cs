using UnityEngine;
using System.Collections;
using DarkRift;
using Lockstep.NetworkHelpers.DarkRift;
namespace Lockstep.NetworkHelpers
{
	public class DarkRiftNetworkHelper : NetworkHelper
	{

		void Start()
		{

		}




		int port = 4296;


		void HandleData (byte tag, ushort subject, object data)
		{
			Debug.Log ("received" + ", " + data.ToString());
			byte [] byteData = data as byte [];
			if (byteData != null) {
				base.Receive ((MessageType)tag, byteData);
			}
		}
		public override void Connect (string ip)
		{
			DarkRiftAPI.Connect (ip);

		}

		public override void Disconnect ()
		{
			if (this._isServer) {
				this._isServer = false;
			} else {
				DarkRiftAPI.Disconnect ();
			}

		}

		bool first = true;
		public override void Simulate ()
		{

			if (this.IsConnected) {
				if (first) {
					Debug.Log ("first");
					DarkRiftAPI.connection.onData += (tag, subject, data) => Debug.Log("RECEIVED");;
					first = false;
				}
				DarkRiftAPI.SendMessageToAll (1, 2, 3);

			}
			
		}

		public override int ID {
			get {
				return (int)DarkRiftAPI.id;
			}
		}
		public override void Host (int roomSize)
		{
			this._isServer = true;

		}

		public override bool IsConnected {
			get {
				return this.IsServer || DarkRiftAPI.isConnected;
			}
		}

		private bool _isServer;

		public override bool IsServer {
			get {
				return _isServer;
			}
		}
		public override int PlayerCount {
			get {
				return 0;
			}
		}
		protected override void OnSendMessageToServer (MessageType messageType, byte [] data)
		{
			DarkRiftAPI.SendMessageToServer ((byte)messageType, 0, data);
		}

		protected override void OnSendMessageToAll (MessageType messageType, byte [] data)
		{
		}


	}
}