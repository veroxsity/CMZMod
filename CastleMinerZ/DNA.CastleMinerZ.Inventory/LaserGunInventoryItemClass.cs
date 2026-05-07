using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class LaserGunInventoryItemClass : GunInventoryItemClass
	{
		public override bool NeedsDropCompensation
		{
			get
			{
				return false;
			}
		}

		public LaserGunInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, TimeSpan fireRate, ToolMaterialTypes material, float bulletDamage, float itemSelfDamage, InventoryItem.InventoryItemClass ammoClass, string shootSound, string reloadSound)
			: base(id, model, name, description1, description2, fireRate, material, bulletDamage, itemSelfDamage, ammoClass, shootSound, reloadSound)
		{
			TracerColor = CMZColors.GetLaserMaterialcColor(Material).ToVector4();
			ToolColor = CMZColors.GetLaserMaterialcColor(Material);
			ToolColor2 = ToolColor;
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
				localToParent2.Translation = new Vector3(9f, -17f, -16f);
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
