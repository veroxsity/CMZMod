using System;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherBaseInventoryItemClass : GunInventoryItemClass
	{
		public class RocketLauncherGrenadeModel : ModelEntity
		{
			private bool _recolor;

			public RocketLauncherGrenadeModel(Model model, bool recolor)
				: base(model)
			{
				_recolor = recolor;
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (_recolor)
				{
					dNAEffect.DiffuseColor = ColorF.FromRGB(1.5f, 0f, 0f);
				}
				else
				{
					dNAEffect.DiffuseColor = Color.White;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		public class RocketLauncherModel : ModelEntity
		{
			public bool Darken;

			public RocketLauncherModel(Model model)
				: base(model)
			{
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				DNAEffect dNAEffect = (DNAEffect)effect;
				if (Darken)
				{
					dNAEffect.DiffuseColor = Color.Gray;
				}
				else
				{
					dNAEffect.DiffuseColor = Color.White;
				}
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		private static Model RPGGrenadeModel;

		private static readonly Vector3 cBarrelTipOffset;

		private static readonly Vector3 cBarrelTipScale;

		static RocketLauncherBaseInventoryItemClass()
		{
			cBarrelTipOffset = new Vector3(0.01f, -0.005f, 0.01f);
			cBarrelTipScale = new Vector3(0.65f);
			RPGGrenadeModel = CastleMinerZGame.Instance.Content.Load<Model>("Weapons\\RPGGrenade");
		}

		public RocketLauncherBaseInventoryItemClass(InventoryItemIDs id, Model model, string name, string description1, string description2, TimeSpan reloadTime, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype, string shootSound, string reloadSound)
			: base(id, model, name, description1, description2, reloadTime, ToolMaterialTypes.BloodStone, damage, durabilitydamage, ammotype, shootSound, reloadSound)
		{
			_playerMode = PlayerMode.RPG;
			Recoil = Angle.FromDegrees(10f);
			ReloadTime = TimeSpan.FromSeconds(0.567);
			Angle.FromDegrees(0f);
			Automatic = false;
			ClipCapacity = 1;
			RoundsPerReload = 1;
			FlightTime = 0.4f;
			Innaccuracy = 0.1f;
			Velocity = 50f;
			EnemyDamageType = DamageType.SHOTGUN;
			Scoped = true;
		}

		public virtual bool IsGuided()
		{
			return false;
		}

		public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			GunEntity gunEntity = (GunEntity)base.CreateEntity(use, attachedToLocalPlayer);
			if (ID == InventoryItemIDs.RocketLauncherGuidedShotFired || ID == InventoryItemIDs.RocketLauncherGuided)
			{
				gunEntity.DiffuseColor = Color.Gray;
			}
			else
			{
				gunEntity.DiffuseColor = Color.White;
			}
			switch (use)
			{
			case ItemUse.UI:
				gunEntity.LocalPosition += new Vector3(-5f, 5f, 0f);
				break;
			case ItemUse.Hand:
				if (ID == InventoryItemIDs.RocketLauncherGuided || ID == InventoryItemIDs.RocketLauncher)
				{
					ModelBone modelBone = _model.Bones["BarrelTip"];
					Vector3 position;
					Vector3 fwd;
					if (modelBone != null)
					{
						position = Vector3.Transform(cBarrelTipOffset, modelBone.Transform);
						fwd = Vector3.Normalize(modelBone.Transform.Left);
					}
					else
					{
						position = new Vector3(0f, 0f, -0.5f);
						fwd = Vector3.Forward;
					}
					RocketLauncherGrenadeModel rocketLauncherGrenadeModel = ((ID != InventoryItemIDs.RocketLauncherGuided) ? new RocketLauncherGrenadeModel(RPGGrenadeModel, false) : new RocketLauncherGrenadeModel(RPGGrenadeModel, true));
					gunEntity.Children.Add(rocketLauncherGrenadeModel);
					rocketLauncherGrenadeModel.LocalToParent = MathTools.CreateWorld(position, fwd);
					rocketLauncherGrenadeModel.LocalScale = cBarrelTipScale;
				}
				break;
			}
			return gunEntity;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new RocketLauncherBaseItem(this, stackCount);
		}
	}
}
