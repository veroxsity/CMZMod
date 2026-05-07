using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateExplosiveMessage : CastleMinerZMessage
	{
		public IntVector3 Location;

		public bool OriginalExplosion;

		public ExplosiveTypes ExplosiveType;

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

		private DetonateExplosiveMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, IntVector3 location, bool originalExplosion, ExplosiveTypes explosiveType)
		{
			DetonateExplosiveMessage sendInstance = Message.GetSendInstance<DetonateExplosiveMessage>();
			sendInstance.Location = location;
			sendInstance.OriginalExplosion = originalExplosion;
			sendInstance.ExplosiveType = explosiveType;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Location);
			writer.Write(OriginalExplosion);
			writer.Write((byte)ExplosiveType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Location = reader.ReadIntVector3();
			OriginalExplosion = reader.ReadBoolean();
			ExplosiveType = (ExplosiveTypes)reader.ReadByte();
		}
	}
}
