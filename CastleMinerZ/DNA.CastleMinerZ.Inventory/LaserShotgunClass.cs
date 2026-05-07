using System;
using DNA.CastleMinerZ.AI;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserShotgunClass : LaserGunInventoryItemClass
	{
		public LaserShotgunClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float bulletdamage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("SpaceWeapons\\PumpShotgun"), name, description1, description2, TimeSpan.FromSeconds(0.1899999976158142), material, bulletdamage, durabilitydamage, ammotype, "LaserGun5", "ShotGunReload")
		{
			_playerMode = PlayerMode.SpacePumpShotgun;
			ReloadTime = TimeSpan.FromSeconds(0.5699999928474426);
			Automatic = false;
			RoundsPerReload = 1;
			ClipCapacity = 8;
			ShoulderMagnification = 1f;
			EnemyDamageType = DamageType.SHOTGUN;
			Innaccuracy = 0.1f;
		}
	}
}
