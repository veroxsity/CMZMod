using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class DetonateRocketMessage : CastleMinerZMessage
	{
		public Vector3 Location;

		public ExplosiveTypes ExplosiveType;

		public InventoryItemIDs ItemType;

		public bool HitDragon;

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

		private DetonateRocketMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, Vector3 location, ExplosiveTypes explosiveType, InventoryItemIDs itemType, bool hitDragon)
		{
			DetonateRocketMessage sendInstance = Message.GetSendInstance<DetonateRocketMessage>();
			sendInstance.Location = location;
			sendInstance.HitDragon = hitDragon;
			sendInstance.ExplosiveType = explosiveType;
			sendInstance.ItemType = itemType;
			sendInstance.DoSend(from);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write(Location);
			writer.Write(HitDragon);
			writer.Write((byte)ExplosiveType);
			writer.Write((byte)ItemType);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			Location = reader.ReadVector3();
			HitDragon = reader.ReadBoolean();
			ExplosiveType = (ExplosiveTypes)reader.ReadByte();
			ItemType = (InventoryItemIDs)reader.ReadByte();
		}
	}
}
