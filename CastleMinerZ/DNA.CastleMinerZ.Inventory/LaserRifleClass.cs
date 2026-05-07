using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserRifleClass : LaserGunInventoryItemClass
	{
		public LaserRifleClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("SpaceWeapons\\BoltRifle"), name, description1, description2, TimeSpan.FromSeconds(0.25999999046325684), material, bulletdamage, durabilitydamage, ammotype, "LaserGun1", "AssaultReload")
		{
			_playerMode = PlayerMode.SpaceBoltRifle;
			ReloadTime = TimeSpan.FromSeconds(2.950000047683716);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 10);
			ShoulderMagnification = 2.5f;
			Innaccuracy = 0f;
			Scoped = true;
		}
	}
}
