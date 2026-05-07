using System.IO;
using DNA.Net;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class PlayerExistsMessage : CastleMinerZMessage
	{
		public byte[] AvatarDescriptionData;

		public bool RequestResponse;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.System;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		private PlayerExistsMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, AvatarDescription description, bool requestResponse)
		{
			PlayerExistsMessage sendInstance = Message.GetSendInstance<PlayerExistsMessage>();
			sendInstance.AvatarDescriptionData = description.Description;
			sendInstance.RequestResponse = requestResponse;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			RequestResponse = reader.ReadBoolean();
			int count = reader.ReadInt32();
			AvatarDescriptionData = reader.ReadBytes(count);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(RequestResponse);
			writer.Write(AvatarDescriptionData.Length);
			writer.Write(AvatarDescriptionData);
		}
	}
}
