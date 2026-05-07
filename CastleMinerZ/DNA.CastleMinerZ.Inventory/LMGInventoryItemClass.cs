using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LMGInventoryItemClass : GunInventoryItemClass
	{
		public LMGInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, string name, string description1, string description2, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Weapons\\M294\\Model"), name, description1, description2, TimeSpan.FromSeconds(0.1120000034570694), material, damage, durabilitydamage, ammotype, "GunShot2", "Reload")
		{
			_playerMode = PlayerMode.LMG;
			ShoulderMagnification = 1.3f;
			Recoil = Angle.FromDegrees(3f);
			ReloadTime = TimeSpan.FromSeconds(9.699999809265137);
			Automatic = true;
			RoundsPerReload = (ClipCapacity = 100);
			FlightTime = 1f;
			Innaccuracy = 0.1f;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			ModelEntity modelEntity = (ModelEntity)base.CreateEntity(use, attachedToLocalPlayer);
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f) * Quaternion.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				Matrix localToParent2 = Matrix.Transform(Matrix.CreateScale(32f / modelEntity.GetLocalBoundingSphere().Radius), rotation);
				localToParent2.Translation = new Vector3(12f, -19f, -16f);
				modelEntity.LocalToParent = localToParent2;
				break;
			}
			case ItemUse.Pickup:
			{
				Matrix localToParent = Matrix.CreateFromYawPitchRoll(0f, -(float)Math.PI / 2f, 0f);
				modelEntity.LocalToParent = localToParent;
				break;
			}
			}
			return modelEntity;
		}
	}
}
