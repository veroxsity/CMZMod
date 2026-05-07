using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserSMGClass : LaserGunInventoryItemClass
	{
		public LaserSMGClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("SpaceWeapons\\M11"), name, description1, description2, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun2", "Reload")
		{
			_playerMode = PlayerMode.SpaceSMG;
			ReloadTime = TimeSpan.FromSeconds(2.05);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 20);
			ShoulderMagnification = 1.1f;
			Innaccuracy = 0.1f;
		}
	}
}
