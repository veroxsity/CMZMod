using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherInventoryItemClass : RocketLauncherBaseInventoryItemClass
	{
		public RocketLauncherInventoryItemClass(InventoryItemIDs id, string name, string description1, string description2, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Weapons\\Rpg"), name, description1, description2, TimeSpan.FromMinutes(1.0 / 60.0), damage, durabilitydamage, ammotype, "RPGLaunch", "ShotGunReload")
		{
		}
	}
}
