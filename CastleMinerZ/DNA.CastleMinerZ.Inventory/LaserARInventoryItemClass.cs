using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserARInventoryItemClass : LaserGunInventoryItemClass
	{
		public LaserARInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("SpaceWeapons\\AK"), name, description1, description2, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun3", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceAssault;
			ReloadTime = TimeSpan.FromSeconds(2.5);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 30);
			ShoulderMagnification = 2f;
			Innaccuracy = 0.02f;
		}
	}
}
