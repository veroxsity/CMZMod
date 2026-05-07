using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class ModelInventoryItemClass : InventoryItem.InventoryItemClass
	{
		protected Model _model;

		public Color ToolColor = Color.White;

		public Color ToolColor2 = Color.White;

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, int maxStack, TimeSpan ts, Color recolor1)
			: base(id, name, description1, description2, maxStack, ts)
		{
			ToolColor = recolor1;
			ToolColor2 = Color.White;
			_model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, int maxStack, TimeSpan ts, Color recolor1, Color recolor2)
			: base(id, name, description1, description2, maxStack, ts)
		{
			ToolColor = recolor1;
			ToolColor2 = recolor2;
			_model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, int maxStack, TimeSpan ts, Color recolor1, string useSound)
			: base(id, name, description1, description2, maxStack, ts, useSound)
		{
			ToolColor = recolor1;
			ToolColor2 = Color.White;
			_model = model;
		}

		public ModelInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, int maxStack, TimeSpan ts, Color recolor1, Color recolor2, string useSound)
			: base(id, name, description1, description2, maxStack, ts, useSound)
		{
			ToolColor = recolor1;
			ToolColor2 = recolor2;
			_model = model;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			CastleMinerToolModel castleMinerToolModel = new CastleMinerToolModel(_model);
			castleMinerToolModel.EnablePerPixelLighting();
			castleMinerToolModel.ToolColor = ToolColor;
			castleMinerToolModel.ToolColor2 = ToolColor2;
			switch (use)
			{
			case ItemUse.UI:
			{
				Matrix localToParent2;
				if (ID == InventoryItemIDs.GunPowder || ID == InventoryItemIDs.ExplosivePowder)
				{
					Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, (float)Math.PI / 4f, 0f);
					float scale = 28.8f / castleMinerToolModel.GetLocalBoundingSphere().Radius;
					localToParent2 = Matrix.Transform(Matrix.CreateScale(scale), rotation);
				}
				else
				{
					Quaternion rotation2 = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f);
					float num = 28.8f / castleMinerToolModel.GetLocalBoundingSphere().Radius;
					if (ID >= InventoryItemIDs.Iron && ID <= InventoryItemIDs.Gold)
					{
						num *= 1.5f;
					}
					localToParent2 = Matrix.Transform(Matrix.CreateScale(num), rotation2);
					if (ID >= InventoryItemIDs.BrassCasing && ID <= InventoryItemIDs.BloodStoneBullets)
					{
						Vector3 translation = castleMinerToolModel.GetLocalBoundingSphere().Center * num;
						translation.X -= 7f;
						localToParent2.Translation = translation;
					}
					else if (ID == InventoryItemIDs.Diamond)
					{
						Vector3 translation2 = localToParent2.Translation;
						translation2.X -= 3f;
						translation2.Y -= 10f;
						localToParent2.Translation = translation2;
					}
					else if (ID >= InventoryItemIDs.Iron && ID <= InventoryItemIDs.Gold)
					{
						Vector3 translation3 = localToParent2.Translation;
						translation3.X -= 5f;
						translation3.Y -= 5f;
						localToParent2.Translation = translation3;
					}
					else if (ID >= InventoryItemIDs.Coal && ID < InventoryItemIDs.Diamond)
					{
						Vector3 translation4 = localToParent2.Translation;
						translation4.X += 10f;
						localToParent2.Translation = translation4;
					}
					else if (ID == InventoryItemIDs.RocketAmmo)
					{
						localToParent2 *= Matrix.CreateScale(0.5f);
					}
				}
				castleMinerToolModel.LocalToParent = localToParent2;
				castleMinerToolModel.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				castleMinerToolModel.LocalRotation = new Quaternion(0.4816553f, 0.05900274f, 0.8705468f, -0.08170173f);
				castleMinerToolModel.LocalPosition = new Vector3(0f, 0.1119255f, 0f);
				if (ID >= InventoryItemIDs.Coal && ID <= InventoryItemIDs.GoldOre)
				{
					castleMinerToolModel.LocalScale = new Vector3(0.35f, 0.35f, 0.35f);
					castleMinerToolModel.LocalPosition = new Vector3(0f, 0.0719255f, 0f);
				}
				else
				{
					castleMinerToolModel.LocalScale = new Vector3(0.5f, 0.5f, 0.5f);
				}
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			case ItemUse.Pickup:
				if (ID >= InventoryItemIDs.BrassCasing && ID <= InventoryItemIDs.BloodStoneBullets)
				{
					Matrix localToParent = Matrix.CreateScale(2.5f);
					castleMinerToolModel.LocalToParent = localToParent;
				}
				castleMinerToolModel.EnablePerPixelLighting();
				break;
			}
			return castleMinerToolModel;
		}
	}
}
