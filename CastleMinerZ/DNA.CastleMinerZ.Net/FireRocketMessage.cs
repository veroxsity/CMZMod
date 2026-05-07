using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class FireRocketMessage : CastleMinerZMessage
	{
		public Vector3 Position;

		public Vector3 Direction;

		public InventoryItemIDs WeaponType;

		public bool Guided;

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

		private FireRocketMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Matrix orientation, InventoryItemIDs weaponType, bool guided)
		{
			FireRocketMessage sendInstance = Message.GetSendInstance<FireRocketMessage>();
			sendInstance.Direction = orientation.Forward;
			sendInstance.Position = orientation.Translation + sendInstance.Direction;
			sendInstance.WeaponType = weaponType;
			sendInstance.Guided = guided;
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Direction = reader.ReadVector3();
			Position = reader.ReadVector3();
			Guided = reader.ReadBoolean();
			WeaponType = (InventoryItemIDs)reader.ReadInt16();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Direction);
			writer.Write(Position);
			writer.Write(Guided);
			writer.Write((short)WeaponType);
		}
	}
}
