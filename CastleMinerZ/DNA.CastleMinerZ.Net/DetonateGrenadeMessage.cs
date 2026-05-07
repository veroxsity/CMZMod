using System.IO;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateGrenadeMessage : CastleMinerZMessage
	{
		public Vector3 Location;

		public GrenadeTypeEnum GrenadeType;

		public bool OnGround;

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
				return SendDataOptions.Reliable;
			}
		}

		private DetonateGrenadeMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, GrenadeTypeEnum grenadeType, bool onGround)
		{
			DetonateGrenadeMessage sendInstance = Message.GetSendInstance<DetonateGrenadeMessage>();
			sendInstance.Location = location;
			sendInstance.GrenadeType = grenadeType;
			sendInstance.OnGround = onGround;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Location);
			writer.Write((byte)GrenadeType);
			writer.Write(OnGround);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Location = reader.ReadVector3();
			GrenadeType = (GrenadeTypeEnum)reader.ReadByte();
			OnGround = reader.ReadBoolean();
		}
	}
}
