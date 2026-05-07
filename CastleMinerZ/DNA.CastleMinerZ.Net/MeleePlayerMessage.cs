using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.IO;
using DNA.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class MeleePlayerMessage : Message
	{
		public InventoryItemIDs ItemID;

		public Vector3 DamageSource;

		public override bool Echo
		{
			get
			{
				return false;
			}
		}

		protected override SendDataOptions SendDataOptions
		{
			get
			{
				return SendDataOptions.ReliableInOrder;
			}
		}

		private MeleePlayerMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, NetworkGamer to, InventoryItemIDs itemID, Vector3 damageSource)
		{
			MeleePlayerMessage sendInstance = Message.GetSendInstance<MeleePlayerMessage>();
			sendInstance.ItemID = itemID;
			sendInstance.DamageSource = damageSource;
			sendInstance.DoSend(from, to);
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((short)ItemID);
			writer.Write(DamageSource);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			ItemID = (InventoryItemIDs)reader.ReadInt16();
			DamageSource = reader.ReadVector3();
		}
	}
}
