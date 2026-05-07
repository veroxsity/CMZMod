using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class PickInventoryItemClass : ModelInventoryItemClass
	{
		public class PickModelEntity : CastleMinerToolModel
		{
			public PickModelEntity(Model model)
				: base(model)
			{
				DrawPriority = 900;
			}

			public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
			{
				int count = base.Model.Meshes.Count;
				for (int i = 0; i < count; i++)
				{
					ModelMesh modelMesh = base.Model.Meshes[i];
					Matrix world = _worldBoneTransforms[modelMesh.ParentBone.Index];
					int count2 = modelMesh.Effects.Count;
					int num = 0;
					while (true)
					{
						if (num < count2)
						{
							if (!SetEffectParams(modelMesh, modelMesh.Effects[num], gameTime, world, view, projection))
							{
								break;
							}
							num++;
							continue;
						}
						if (modelMesh.Name.Contains("Beam"))
						{
							BlendState blendState = device.BlendState;
							device.BlendState = BlendState.Additive;
							modelMesh.Draw();
							device.BlendState = blendState;
						}
						else
						{
							modelMesh.Draw();
						}
						break;
					}
				}
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = oeffect as DNAEffect;
				if (dNAEffect != null && mesh.Name.Contains("Beam"))
				{
					if (ToolColor.A == 0)
					{
						return false;
					}
					dNAEffect.DiffuseColor = ToolColor;
				}
				return base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection);
			}
		}

		public ToolMaterialTypes Material;

		private Cue _activeSoundCue;

		private string _activeSound;

		private bool _laserSword;

		public PickInventoryItemClass(InventoryItemIDs id, ToolMaterialTypes material, Model model, string name, string description1, string description2, float meleeDamage)
			: base(id, model, name, description1, description2, 1, TimeSpan.FromSeconds(0.30000001192092896), Color.White)
		{
			_playerMode = PlayerMode.Pick;
			EnemyDamage = meleeDamage;
			EnemyDamageType = DamageType.BLADE;
			Material = material;
			if (id == InventoryItemIDs.IronLaserSword || id == InventoryItemIDs.GoldLaserSword || id == InventoryItemIDs.DiamondLaserSword || id == InventoryItemIDs.BloodStoneLaserSword || id == InventoryItemIDs.CopperLaserSword)
			{
				ToolColor = CMZColors.GetLaserMaterialcColor(Material);
				_activeSound = "LightSaber";
				_laserSword = true;
			}
			else
			{
				ToolColor = CMZColors.GetMaterialcColor(Material);
			}
			switch (Material)
			{
			case ToolMaterialTypes.Wood:
				ItemSelfDamagePerUse = 0.005f;
				break;
			case ToolMaterialTypes.Stone:
				ItemSelfDamagePerUse = 0.0025f;
				break;
			case ToolMaterialTypes.Copper:
				ItemSelfDamagePerUse = 0.00125f;
				break;
			case ToolMaterialTypes.Iron:
				ItemSelfDamagePerUse = 0.0005f;
				break;
			case ToolMaterialTypes.Gold:
				ItemSelfDamagePerUse = 0.00033333333f;
				break;
			case ToolMaterialTypes.Diamond:
				ItemSelfDamagePerUse = 0.00025f;
				break;
			case ToolMaterialTypes.BloodStone:
				ItemSelfDamagePerUse = 0.0002f;
				break;
			}
			if (id == InventoryItemIDs.IronLaserSword || id == InventoryItemIDs.GoldLaserSword || id == InventoryItemIDs.DiamondLaserSword || id == InventoryItemIDs.BloodStoneLaserSword || id == InventoryItemIDs.CopperLaserSword)
			{
				ItemSelfDamagePerUse = 0.005f;
			}
		}

		public override void OnItemEquipped()
		{
			if (_activeSound != null)
			{
				_activeSoundCue = SoundManager.Instance.PlayInstance(_activeSound);
			}
			base.OnItemEquipped();
		}

		public override void OnItemUnequipped()
		{
			if (_activeSoundCue != null && _activeSoundCue.IsPlaying)
			{
				_activeSoundCue.Stop(AudioStopOptions.Immediate);
			}
			base.OnItemUnequipped();
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			PickModelEntity pickModelEntity = new PickModelEntity(_model);
			pickModelEntity.EnablePerPixelLighting();
			pickModelEntity.ToolColor = ToolColor;
			switch (use)
			{
			case ItemUse.UI:
			{
				Quaternion rotation = Quaternion.CreateFromYawPitchRoll(0f, 0f, -(float)Math.PI / 4f);
				Matrix localToParent = Matrix.Transform(Matrix.CreateScale(32f / pickModelEntity.GetLocalBoundingSphere().Radius), rotation);
				pickModelEntity.LocalToParent = localToParent;
				pickModelEntity.EnableDefaultLighting();
				break;
			}
			case ItemUse.Hand:
				if (!_laserSword)
				{
					pickModelEntity.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 2f) * Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI / 4f);
					pickModelEntity.LocalPosition = new Vector3(0f, 0.11126215f, 0f);
				}
				break;
			}
			return pickModelEntity;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new PickInventoryItem(this, stackCount);
		}
	}
}
