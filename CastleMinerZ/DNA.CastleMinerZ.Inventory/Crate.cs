using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Inventory
{
	public class Crate
	{
		private const int Columns = 8;

		public bool Destroyed;

		private IntVector3 _location;

		public InventoryItem[] Inventory = new InventoryItem[32];

		public IntVector3 Location
		{
			get
			{
				return _location;
			}
		}

		public Crate(IntVector3 location)
		{
			_location = location;
		}

		public void EjectContents()
		{
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null && !IsSlotLocked(i))
				{
					Vector3 location = IntVector3.ToVector3(Location) + new Vector3(0.5f);
					PickupManager.Instance.CreateUpwardPickup(Inventory[i], location, 3f);
					Inventory[i] = null;
				}
			}
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer allGamer in CastleMinerZGame.Instance.CurrentNetworkSession.AllGamers)
			{
				if (allGamer.Tag != null)
				{
					Player player = (Player)allGamer.Tag;
					int num = player.FocusCrateItem.X + player.FocusCrateItem.Y * 8;
					if (player.FocusCrate == Location && num == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public Crate(BinaryReader reader)
		{
			Read(reader);
		}

		public void Write(BinaryWriter writer)
		{
			_location.Write(writer);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == null)
				{
					writer.Write(false);
					continue;
				}
				writer.Write(true);
				Inventory[i].Write(writer);
			}
		}

		public void Read(BinaryReader reader)
		{
			_location = IntVector3.Read(reader);
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (reader.ReadBoolean())
				{
					Inventory[i] = InventoryItem.Create(reader);
					if (Inventory[i] != null && !Inventory[i].IsValid())
					{
						Inventory[i] = null;
					}
				}
				else
				{
					Inventory[i] = null;
				}
			}
		}
	}
}
