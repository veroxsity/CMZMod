using System.IO;
using DNA.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class AddExplosiveFlashMessage : CastleMinerZMessage
	{
		public IntVector3 Position;

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.System;
			}
		}

		private AddExplosiveFlashMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 position)
		{
			AddExplosiveFlashMessage sendInstance = Message.GetSendInstance<AddExplosiveFlashMessage>();
			sendInstance.Position = position;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Position = IntVector3.Read(reader);
		}

		protected override void SendData(BinaryWriter writer)
		{
			Position.Write(writer);
		}
	}
}
