using System;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserPistolClass : LaserGunInventoryItemClass
	{
		public LaserPistolClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("SpaceWeapons\\Colt"), name, description1, description2, TimeSpan.FromSeconds(0.05999999865889549), material, bulletdamage, durabilitydamage, ammotype, "LaserGun4", "Reload")
		{
			_playerMode = PlayerMode.SpacePistol;
			ReloadTime = TimeSpan.FromSeconds(1.25);
			Automatic = false;
			RoundsPerReload = (ClipCapacity = 7);
			ShoulderMagnification = 1.2f;
			Innaccuracy = 0.05f;
		}
	}
}
