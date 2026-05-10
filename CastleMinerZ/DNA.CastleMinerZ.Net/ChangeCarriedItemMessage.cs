using System.IO;
using DNA.CastleMinerZ.Inventory;
using DNA.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Net
{
	public class ChangeCarriedItemMessage : CastleMinerZMessage
	{
		public InventoryItemIDs ItemID;

		// For mod items: the mod's namespaced ID (e.g. "you.diamond-sword").
		// Empty string for vanilla items. Wire format always writes this string
		// after the enum; readers always read it. Keeps the protocol symmetric.
		public string ModItemId = "";

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

		private ChangeCarriedItemMessage()
		{
		}

		public static void Send(LocalNetworkGamer from, InventoryItemIDs id)
		{
			Send(from, id, null);
		}

		public static void Send(LocalNetworkGamer from, InventoryItemIDs id, string modItemId)
		{
			ChangeCarriedItemMessage sendInstance = Message.GetSendInstance<ChangeCarriedItemMessage>();
			sendInstance.ItemID = id;
			sendInstance.ModItemId = modItemId ?? "";
			sendInstance.DoSend(from);
		}

		protected override void RecieveData(BinaryReader reader)
		{
			ItemID = (InventoryItemIDs)reader.ReadInt16();
			ModItemId = reader.ReadString();
		}

		protected override void SendData(BinaryWriter writer)
		{
			writer.Write((short)ItemID);
			writer.Write(ModItemId ?? "");
		}
	}
}
