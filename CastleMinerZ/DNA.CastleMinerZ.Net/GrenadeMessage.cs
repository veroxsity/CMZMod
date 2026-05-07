using System.IO;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class GrenadeMessage : CastleMinerZMessage
	{
		public Vector3 Position;

		public Vector3 Direction;

		public GrenadeTypeEnum GrenadeType;

		public float SecondsLeft;

		public override MessageTypes MessageType
		{
			get
			{
				return MessageTypes.PlayerUpdate;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.Reliable;
			}
		}

		private GrenadeMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix orientation, GrenadeTypeEnum grenadeType, float secondsLeft)
		{
			GrenadeMessage sendInstance = Message.GetSendInstance<GrenadeMessage>();
			sendInstance.Direction = orientation.Forward;
			sendInstance.Position = orientation.Translation + sendInstance.Direction;
			sendInstance.GrenadeType = grenadeType;
			sendInstance.SecondsLeft = secondsLeft;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Direction = reader.ReadVector3();
			Position = reader.ReadVector3();
			GrenadeType = (GrenadeTypeEnum)reader.ReadByte();
			SecondsLeft = reader.ReadSingle();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Direction);
			writer.Write(Position);
			writer.Write((byte)GrenadeType);
			writer.Write(SecondsLeft);
		}
	}
}
