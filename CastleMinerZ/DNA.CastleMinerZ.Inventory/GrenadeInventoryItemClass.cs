using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeInventoryItemClass : ModelInventoryItemClass
	{
		public GrenadeTypeEnum GrenadeType;

		public override bool IsMeleeWeapon
		{
			get
			{
				return false;
			}
		}

		public GrenadeInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, GrenadeTypeEnum grenadeType)
			: base(id, model, name, description1, description2, 10, TimeSpan.FromSeconds(1.0), Color.White)
		{
			_playerMode = PlayerMode.Grenade;
			GrenadeType = grenadeType;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new GrenadeItem(this, stackCount);
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(_model);
			castleMinerToolModel.EnablePerPixelLighting();
			switch (use)
			{
			case ItemUse.UI:
			{
				float value = 25.6f / castleMinerToolModel.GetLocalBoundingSphere().Radius;
				castleMinerToolModel.LocalPosition = new Vector3(-25f, -12.5f, 0f);
				castleMinerToolModel.LocalScale = new Vector3(value);
				castleMinerToolModel.LocalRotation = Quaternion.Concatenate(Quaternion.CreateFromYawPitchRoll(-1.5f, -1.2f, -0.5f), Quaternion.CreateFromYawPitchRoll(0f, 0.2f, 0f));
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			}
			return castleMinerToolModel;
		}
	}
}
