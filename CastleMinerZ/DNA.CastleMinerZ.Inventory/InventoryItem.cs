using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.ModAPI.Internal;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class InventoryItem
	{
		public abstract class InventoryItemClass
		{
			public InventoryItemIDs ID;
			public string ModItemId;
			public int ModIconIndex = -1;
			public string IconTextureName;

			protected string _name;

			protected string _description1;

			protected string _description2;

			public int MaxStackCount;

			public float EnemyDamage;

			public DamageType EnemyDamageType;

			protected TimeSpan _coolDownTime;

			public float ItemSelfDamagePerUse;

			private string _useSoundCue;

			protected PlayerMode _playerMode;

			public string UseSound
			{
				get
				{
					return _useSoundCue;
				}
			}

			public TimeSpan CoolDownTime
			{
				get
				{
					return _coolDownTime;
				}
			}

			public string Name
			{
				get
				{
					return _name;
				}
			}

			public string Description1
			{
				get
				{
					return _description1;
				}
			}

			public string Description2
			{
				get
				{
					return _description2;
				}
			}

			public PlayerMode PlayerAnimationMode
			{
				get
				{
					return _playerMode;
				}
			}

			public virtual bool IsMeleeWeapon
			{
				get
				{
					return true;
				}
			}

			public virtual float PickupTimeoutLength
			{
				get
				{
					return 30f;
				}
			}

			public void SetName(string name) { _name = name; }
			public void SetDescription(string d1, string d2) { _description1 = d1; _description2 = d2; }
			public void SetCoolDown(TimeSpan cd) { _coolDownTime = cd; }
			public void SetUseSound(string cue) { _useSoundCue = cue; }
			public void SetAnimationMode(PlayerMode mode) { _playerMode = mode; }

			public InventoryItemClass(InventoryItemIDs id, string name, string description1, string description2, int maxStack, TimeSpan coolDownTime)
			{
				_playerMode = PlayerMode.Generic;
				_useSoundCue = null;
				_coolDownTime = coolDownTime;
				_name = name;
				_description1 = description1;
				_description2 = description2;
				MaxStackCount = maxStack;
				EnemyDamage = 0.1f;
				EnemyDamageType = DamageType.BLUNT;
				ID = id;
			}

			public InventoryItemClass(InventoryItemIDs id, string name, string description1, string description2, int maxStack, TimeSpan coolDownTime, string useSound)
			{
				_useSoundCue = useSound;
				_coolDownTime = coolDownTime;
				_name = name;
				_description1 = description1;
				_description2 = description2;
				MaxStackCount = maxStack;
				EnemyDamage = 0.1f;
				EnemyDamageType = DamageType.BLUNT;
				ID = id;
			}

			public abstract Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer);

			public virtual InventoryItem CreateItem(int stackCount)
			{
				return new InventoryItem(this, stackCount);
			}

			public virtual void OnItemEquipped()
			{
			}

			public virtual void OnItemUnequipped()
			{
			}

			public void Draw2D(SpriteBatch batch, Rectangle destRect, Color color)
			{
				if (ModItemId != null)
				{
					if (_2DModImages == null)
						FinishInitialization(batch.GraphicsDevice);
					int idx = ModIconIndex;
					batch.Draw(sourceRectangle: new Rectangle((idx & 7) * 64, idx / 8 * 64, 64, 64),
						texture: _2DModImages, destinationRectangle: destRect, color: color);
					return;
				}
				if (_2DImages == null)
				{
					FinishInitialization(batch.GraphicsDevice);
				}
				int iD = (int)ID;
				batch.Draw(sourceRectangle: new Rectangle((iD & 7) * 64, iD / 8 * 64, 64, 64), texture: _2DImages, destinationRectangle: destRect, color: color);
			}

			public void Draw2D(SpriteBatch batch, Rectangle destRect)
			{
				Draw2D(batch, destRect, Color.White);
			}
		}

		public const int UIItemsPerRow = 8;

		public const int UIItemSize = 64;

		public const int UIMapWidth = 512;

		public const int UIMapHeight = 1024;

		protected static Dictionary<InventoryItemIDs, InventoryItemClass> AllItems = new Dictionary<InventoryItemIDs, InventoryItemClass>();

		public static RenderTarget2D _2DImages = null;
		public static RenderTarget2D _2DModImages = null;

		private InventoryItemClass _class;

		private OneShotTimer _coolDownTimer;

		private int _stackCount;

		public float ItemHealthLevel = 1f;

		public TimeSpan DigTime = TimeSpan.Zero;

		public IntVector3 DigLocation;

		private StringBuilder sbuilder = new StringBuilder();

		protected OneShotTimer CoolDownTimer
		{
			get
			{
				return _coolDownTimer;
			}
		}

		public InventoryItemClass ItemClass
		{
			get
			{
				return _class;
			}
		}

		public int StackCount
		{
			get
			{
				return _stackCount;
			}
			set
			{
				_stackCount = value;
			}
		}

		public int MaxStackCount
		{
			get
			{
				return _class.MaxStackCount;
			}
		}

		public string Name
		{
			get
			{
				return _class.Name;
			}
		}

		public string Description1
		{
			get
			{
				return _class.Description1;
			}
		}

		public string Description2
		{
			get
			{
				return _class.Description2;
			}
		}

		public bool IsMeleeWeapon
		{
			get
			{
				return _class.IsMeleeWeapon;
			}
		}

		public float EnemyDamage
		{
			get
			{
				return _class.EnemyDamage;
			}
		}

		public DamageType EnemyDamageType
		{
			get
			{
				return _class.EnemyDamageType;
			}
		}

		public PlayerMode PlayerMode
		{
			get
			{
				return _class.PlayerAnimationMode;
			}
		}

		public static InventoryItem CreateItem(InventoryItemIDs id, int stackCount)
		{
			return AllItems[id].CreateItem(stackCount);
		}

		public static InventoryItemClass GetClass(InventoryItemIDs id)
		{
			return AllItems[id];
		}

		public static Entity CreateEntity(InventoryItemIDs id, ItemUse use, bool attachedToLocalPlayer)
		{
			InventoryItemClass inventoryItemClass = GetClass(id);
			return inventoryItemClass.CreateEntity(use, attachedToLocalPlayer);
		}

		public static InventoryItemClass GetClass(IItemId id)
		{
			if (id.IsVanilla)
				return AllItems[id.VanillaId];
			InventoryItemClass cls = ModAPI.Internal.ItemRegistry.GetClass(id.ModId);
			if (cls != null)
				return cls;
			return ModAPI.Internal.PlaceholderItemClass.Instance;
		}

		public static InventoryItem CreateItem(IItemId id, int stackCount)
		{
			return GetClass(id).CreateItem(stackCount);
		}

		public static void Initalize(ContentManager content)
		{
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.WoodBlock, BlockTypeEnum.Wood, "Made from logs", "This is a raw material that must be found", 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.RockBlock, BlockTypeEnum.Rock, "Commonly found underground", "This is a raw material that must be found", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SandBlock, BlockTypeEnum.Sand, "Found on the surface", "This is a raw material that must be found", 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.DirtBlock, BlockTypeEnum.Dirt, "Found on the surface", "This is a raw material that must be found", 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.LogBlock, BlockTypeEnum.Log, "Comes from trees", "This is a raw material that must be found", 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.LanternBlock, BlockTypeEnum.Lantern, "Lights the world", "More durable than a torch", 0.075f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.BloodStoneBlock, BlockTypeEnum.BloodStone, "Found in hell", "Bloodstone is very hard", 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SpaceRock, BlockTypeEnum.SpaceRock, "Comes from space", "Junk", 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.SpaceRockInventory, BlockTypeEnum.SpaceRockInventory, "Comes from space", "Used to make alien tools and weapons", 0.15f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.IronWall, BlockTypeEnum.IronWall, "Strong walls for building", "Prevents some monsters from digging", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.CopperWall, BlockTypeEnum.CopperWall, "Strong walls for building", "Prevents some monsters from digging", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.GoldenWall, BlockTypeEnum.GoldenWall, "Strong walls for building", "Prevents some monsters from digging", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.DiamondWall, BlockTypeEnum.DiamondWall, "Strong walls for building", "Prevents some monsters from digging", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Crate, BlockTypeEnum.Crate, "Used for storing items", "", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Snow, BlockTypeEnum.Snow, "Found on the surface", "This is a raw material that must be found", 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Ice, BlockTypeEnum.Ice, "Found on the surface", "This is a raw material that must be found", 0.01f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.TNT, BlockTypeEnum.TNT, "Used to blow up large areas", "Only destroys certain materials", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.C4, BlockTypeEnum.C4, "Used to blow up large areas", "Destroys everything", 0.1f));
			RegisterItemClass(new BlockInventoryItemClass(InventoryItemIDs.Slime, BlockTypeEnum.Slime, "Space Goo", "Used to make alien weapons", 0.075f));
			Model model = content.Load<Model>("PickAxe");
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.StonePickAxe, ToolMaterialTypes.Stone, model, "Stone PickAxe", "Used for breaking certain stones and ores", "", 0.1f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.CopperPickAxe, ToolMaterialTypes.Copper, model, "Copper PickAxe", "Used for breaking certain stones and ores", "", 0.2f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.IronPickAxe, ToolMaterialTypes.Iron, model, "Iron PickAxe", "Used for breaking certain stones and ores", "", 0.4f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.GoldPickAxe, ToolMaterialTypes.Gold, model, "Gold PickAxe", "Used for breaking certain stones and ores", "", 0.8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.DiamondPickAxe, ToolMaterialTypes.Diamond, model, "Diamond PickAxe", "Used for breaking certain stones and ores", "", 1.6f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.BloodstonePickAxe, ToolMaterialTypes.BloodStone, model, "BloodStone PickAxe", "Used for breaking certain stones and ores", "", 3f));
			Model model2 = content.Load<Model>("Saber");
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.CopperLaserSword, ToolMaterialTypes.Copper, model2, "Copper Laser Sword", "Advanced melee and mining tool", "", 8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.IronLaserSword, ToolMaterialTypes.Iron, model2, "Iron Laser Sword", "Advanced melee and mining tool", "", 8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.GoldLaserSword, ToolMaterialTypes.Gold, model2, "Gold Laser Sword", "Advanced melee and mining tool", "", 8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.DiamondLaserSword, ToolMaterialTypes.Diamond, model2, "Diamond Laser Sword", "Advanced melee and mining tool", "", 8f));
			RegisterItemClass(new PickInventoryItemClass(InventoryItemIDs.BloodStoneLaserSword, ToolMaterialTypes.BloodStone, model2, "BloodStone Laser Sword", "Advanced melee and mining tool", "", 8f));
			Model model3 = content.Load<Model>("Spade");
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.StoneSpade, ToolMaterialTypes.Stone, model3, "Stone Spade", "Used for digging dirt and sand", "Also removes C4 and TNT", 0.1f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.CopperSpade, ToolMaterialTypes.Copper, model3, "Copper Spade", "Used for digging dirt and sand", "Also removes C4 and TNT", 0.2f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.IronSpade, ToolMaterialTypes.Iron, model3, "Iron Spade", "Used for digging dirt and sand", "Also removes C4 and TNT", 0.4f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.GoldSpade, ToolMaterialTypes.Gold, model3, "Gold Spade", "Used for digging dirt and sand", "Also removes C4 and TNT", 0.8f));
			RegisterItemClass(new SpadeInventoryClass(InventoryItemIDs.DiamondSpade, ToolMaterialTypes.Diamond, model3, "Diamond Spade", "Used for digging dirt and sand", "Also removes C4 and TNT", 1.6f));
			Model model4 = content.Load<Model>("Axe");
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.StoneAxe, ToolMaterialTypes.Stone, model4, "Stone Axe", "Used for chopping wood", "Can also be used for basic melee defense", 0.15f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.CopperAxe, ToolMaterialTypes.Copper, model4, "Copper Axe", "Used for chopping wood", "Can also be used for basic melee defense", 0.3f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.IronAxe, ToolMaterialTypes.Iron, model4, "Iron Axe", "Used for chopping wood", "Can also be used for basic melee defense", 0.5f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.GoldAxe, ToolMaterialTypes.Gold, model4, "Gold Axe", "Used for chopping wood", "Can also be used for basic melee defense", 1f));
			RegisterItemClass(new AxeInventoryClass(InventoryItemIDs.DiamondAxe, ToolMaterialTypes.Diamond, model4, "Diamond Axe", "Used for chopping wood", "Can also be used for basic melee defense", 2f));
			Model model5 = content.Load<Model>("Ammo");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.BrassCasing, model5, "Brass Casing", "Used for making ammunition", "", 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronCasing, model5, "Iron Casing", "Used for making ammunition", "", 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Iron));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldCasing, model5, "Gold Casing", "Used for making ammunition", "", 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Gold));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.DiamondCasing, model5, "Diamond Casing", "Used for making ammunition", "", 5000, TimeSpan.FromSeconds(0.3), Color.Transparent, CMZColors.Diamond));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Bullets, model5, "Bullets", "Ammo for conventional weapons", "", 5000, TimeSpan.FromSeconds(0.3), Color.DarkGray, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronBullets, model5, "Iron Bullets", "Ammo for gold weapons", "", 5000, TimeSpan.FromSeconds(0.3), Color.LightGray, CMZColors.Brass));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldBullets, model5, "Gold Bullets", "Ammo for diamond weapons", "", 5000, TimeSpan.FromSeconds(0.3), new Color(255, 215, 0), CMZColors.Iron));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.DiamondBullets, model5, "Diamond Bullets", "Ammo for bloodstone", "", 5000, TimeSpan.FromSeconds(0.3), Color.Cyan, CMZColors.Gold));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.BloodStoneBullets, model5, "BloodStone Bullets", "", "", 5000, TimeSpan.FromSeconds(0.3), Color.DarkRed, CMZColors.Diamond));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.LaserBullets, model5, "Laser Bullets", "Ammo for laser weapons", "", 5000, TimeSpan.FromSeconds(0.3), Color.LimeGreen, CMZColors.Stone));
			Model model6 = content.Load<Model>("weapons\\RPGGrenade");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.RocketAmmo, model6, "Rockets", "Ammo for rocket launchers", "", 5000, TimeSpan.FromSeconds(0.3), Color.DarkGray, CMZColors.Brass));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncher, "Rocket Launcher", "Dumb fired projectile grenade", "Uses Rockets", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherGuidedInventoryItemClass(InventoryItemIDs.RocketLauncherGuided, "Anti Dragon Guided Missile", "Guided missile used for killing dragons", "Uses Rockets", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncherShotFired, "", "", "", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			RegisterItemClass(new RocketLauncherInventoryItemClass(InventoryItemIDs.RocketLauncherGuidedShotFired, "", "", "", 100f, 1f, GetClass(InventoryItemIDs.RocketAmmo)));
			Model model7 = content.Load<Model>("Grenade");
			RegisterItemClass(new GrenadeInventoryItemClass(InventoryItemIDs.Grenade, model7, "Grenade", "Blow up Zombies", "", GrenadeTypeEnum.HE));
			RegisterItemClass(new StickInventoryItemClass(InventoryItemIDs.Stick, Color.White, model, "Wood Stick", "Use this to make various items", "Such as a pickaxe or a torch", 0.05f));
			RegisterItemClass(new TorchInventoryItemClass());
			RegisterItemClass(new DoorInventoryItemClass());
			Model model8 = content.Load<Model>("GunPowder");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GunPowder, model8, "Gun Powder", "Used to craft ammunition", "This is a raw material that must be found", 64, TimeSpan.FromSeconds(0.30000001192092896), Color.White));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.ExplosivePowder, model8, "Explosive Powder", "Used to craft explosives", "Dropped by dragons and demons", 64, TimeSpan.FromSeconds(0.30000001192092896), Color.Red));
			Model model9 = content.Load<Model>("Ore");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Coal, model9, "Coal", "Used to craft items", "This is a raw material that must be found", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Coal, CMZColors.Coal));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.IronOre, model9, "Iron Ore", "Can be made into iron", "This is a raw material that must be found", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.IronOre, Color.White));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.CopperOre, model9, "Copper Ore", "Can be made into copper", "This is a raw material that must be found", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.CopperOre, Color.White));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.GoldOre, model9, "Gold Ore", "Can be made into gold", "This is a raw material that must be found", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Gold, Color.White));
			Model model10 = content.Load<Model>("Gems");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Diamond, model10, "Diamond", "Very hard substance", "Used to make diamond tools", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Diamond, Color.White));
			Model model11 = content.Load<Model>("Bars");
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Iron, model11, "Iron", "Used to craft items", "Made from Iron ore", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Iron, Color.White));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Copper, model11, "Copper", "Used to craft items", "Made from Copper ore", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Copper, Color.White));
			RegisterItemClass(new ModelInventoryItemClass(InventoryItemIDs.Gold, model11, "Gold", "Used to craft items", "Made from Gold ore", 64, TimeSpan.FromSeconds(0.30000001192092896), CMZColors.Gold, Color.White));
			Model model12 = content.Load<Model>("Compass");
			RegisterItemClass(new CompassInventoryItemClass(InventoryItemIDs.Compass, model12));
			Model model13 = content.Load<Model>("Locator");
			Model model14 = content.Load<Model>("Teleporter");
			RegisterItemClass(new GPSItemClass(InventoryItemIDs.GPS, model13, "Locator", "Show the direction to a chosen location and GPS coordinates", ""));
			RegisterItemClass(new GPSItemClass(InventoryItemIDs.TeleportGPS, model14, "Teleporter", "Show the direction to a chosen location and GPS coordinates", "Use the item by pressing the left trigger to teleport to the chosen location"));
			Model model15 = content.Load<Model>("Clock");
			RegisterItemClass(new ClockInventoryItemClass(InventoryItemIDs.Clock, model15));
			RegisterItemClass(new BareHandInventoryItemClass());
			Model model16 = content.Load<Model>("Knife");
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.Knife, model16, ToolMaterialTypes.Iron, "Knife", "Basic Melee Defense", "", 0.5f, 0.02f, TimeSpan.FromSeconds(0.5)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.GoldKnife, model16, ToolMaterialTypes.Gold, "Gold Knife", "Basic Melee Defense", "", 1f, 0.01f, TimeSpan.FromSeconds(0.4)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.DiamondKnife, model16, ToolMaterialTypes.Diamond, "Diamond Knife", "Basic Melee Defense", "", 2f, 0.005f, TimeSpan.FromSeconds(0.3)));
			RegisterItemClass(new KnifeInventoryItemClass(InventoryItemIDs.BloodStoneKnife, model16, ToolMaterialTypes.BloodStone, "BloodStone Knife", "Basic Melee Defense", "", 4f, 0.0033333334f, TimeSpan.FromSeconds(0.25)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.IronSpaceAssultRifle, ToolMaterialTypes.Iron, "Laser Assault Rifle", "High power full auto", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.IronSpaceSMGGun, ToolMaterialTypes.Iron, "Laser Sub Machine Gun", "High rate of fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.IronSpacePistol, ToolMaterialTypes.Iron, "Laser Pistol", "Basic semi automatic gun", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.IronSpaceBoltActionRifle, ToolMaterialTypes.Iron, "Laser Rifle", "High power very accurate", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.IronSpacePumpShotgun, ToolMaterialTypes.Iron, "Laser Shotgun", "Short range burst fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.CopperSpaceAssultRifle, ToolMaterialTypes.Copper, "Laser Assault Rifle", "High power full auto", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.CopperSpaceSMGGun, ToolMaterialTypes.Copper, "Laser Sub Machine Gun", "High rate of fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.CopperSpacePistol, ToolMaterialTypes.Copper, "Laser Pistol", "Basic semi automatic gun", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.CopperSpaceBoltActionRifle, ToolMaterialTypes.Copper, "Laser Rifle", "High power very accurate", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.CopperSpacePumpShotgun, ToolMaterialTypes.Copper, "Laser Shotgun", "Short range burst fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.GoldSpaceAssultRifle, ToolMaterialTypes.Gold, "Laser Assault Rifle", "High power full auto", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.GoldSpaceSMGGun, ToolMaterialTypes.Gold, "Laser Sub Machine Gun", "High rate of fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.GoldSpacePistol, ToolMaterialTypes.Gold, "Laser Pistol", "Basic semi automatic gun", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.GoldSpaceBoltActionRifle, ToolMaterialTypes.Gold, "Laser Rifle", "High power very accurate", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.GoldSpacePumpShotgun, ToolMaterialTypes.Gold, "Laser Shotgun", "Short range burst fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserARInventoryItemClass(InventoryItemIDs.DiamondSpaceAssultRifle, ToolMaterialTypes.Diamond, "Laser Assault Rifle", "High power full auto", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserSMGClass(InventoryItemIDs.DiamondSpaceSMGGun, ToolMaterialTypes.Diamond, "Laser Sub Machine Gun", "High rate of fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserPistolClass(InventoryItemIDs.DiamondSpacePistol, ToolMaterialTypes.Diamond, "Laser Pistol", "Basic semi automatic gun", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserRifleClass(InventoryItemIDs.DiamondSpaceBoltActionRifle, ToolMaterialTypes.Diamond, "Laser Rifle", "High power very accurate", "Uses Laser Bullets", 15f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new LaserShotgunClass(InventoryItemIDs.DiamondSpacePumpShotgun, ToolMaterialTypes.Diamond, "Laser Shotgun", "Short range burst fire", "Uses Laser Bullets", 10f, 0.0005f, GetClass(InventoryItemIDs.LaserBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.AssultRifle, ToolMaterialTypes.Iron, "Assault Rifle", "High power full auto", "Uses Bullets", 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.PumpShotgun, ToolMaterialTypes.Iron, "Shotgun", "Short range burst fire", "Uses Bullets", 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.SMGGun, ToolMaterialTypes.Iron, "Sub Machine Gun", "High rate of fire", "Uses Bullets", 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.LMGGun, ToolMaterialTypes.Iron, "Light Machine Gun", "Powerful with a large clip capacity", "Uses Bullets", 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.BoltActionRifle, ToolMaterialTypes.Iron, "Rifle", "High power very accurate", "Uses Bullets", 0.5f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.Pistol, ToolMaterialTypes.Iron, "Pistol", "Basic semi automatic gun", "Uses Bullets", 0.3f, 0.001f, GetClass(InventoryItemIDs.Bullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.GoldAssultRifle, ToolMaterialTypes.Gold, "Gold Assault Rifle", "High power full auto", "Uses Iron Bullets", 2.5f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.GoldPumpShotgun, ToolMaterialTypes.Gold, "Gold Shotgun", "Short range burst fire", "Uses Iron Bullets", 1f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.GoldSMGGun, ToolMaterialTypes.Gold, "Gold Sub Machine Gun", "High rate of fire", "Uses Iron Bullets", 1f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.GoldLMGGun, ToolMaterialTypes.Gold, "Gold Light Machine Gun", "Powerful with a large clip capacity", "Uses Iron Bullets", 2.5f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.GoldBoltActionRifle, ToolMaterialTypes.Gold, "Gold Rifle", "High power very accurate", "Uses Iron Bullets", 2.5f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.GoldPistol, ToolMaterialTypes.Gold, "Gold Pistol", "Basic semi automatic gun", "Uses Iron Bullets", 1f, 0.00045454546f, GetClass(InventoryItemIDs.IronBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.DiamondAssultRifle, ToolMaterialTypes.Diamond, "Diamond Assault Rifle", "High power full auto", "Uses Gold Bullets", 6f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.DiamondPumpShotgun, ToolMaterialTypes.Diamond, "Diamond Shotgun", "Short range burst fire", "Uses Gold Bullets", 4f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.DiamondSMGGun, ToolMaterialTypes.Diamond, "Diamond Sub Machine Gun", "High rate of fire", "Uses Gold Bullets", 4f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.DiamondLMGGun, ToolMaterialTypes.Diamond, "Diamond Light Machine Gun", "Powerful with a large clip capacity", "Uses Gold Bullets", 6f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.DiamondBoltActionRifle, ToolMaterialTypes.Diamond, "Diamond Rifle", "High power very accurate", "Uses Gold Bullets", 6f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.DiamondPistol, ToolMaterialTypes.Diamond, "Diamond Pistol", "Basic semi automatic gun", "Uses Gold Bullets", 4f, 0.00023923445f, GetClass(InventoryItemIDs.GoldBullets)));
			RegisterItemClass(new AssultRifleInventoryItemClass(InventoryItemIDs.BloodStoneAssultRifle, ToolMaterialTypes.BloodStone, "BloodStone Assault Rifle", "High power full auto", "Uses Diamond Bullets", 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new PumpShotgunInventoryItemClass(InventoryItemIDs.BloodStonePumpShotgun, ToolMaterialTypes.BloodStone, "BloodStone Shotgun", "Short range burst fire", "Uses Diamond Bullets", 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new SMGInventoryItemClass(InventoryItemIDs.BloodStoneSMGGun, ToolMaterialTypes.BloodStone, "BloodStone Sub Machine Gun", "High rate of fire", "Uses Diamond Bullets", 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new LMGInventoryItemClass(InventoryItemIDs.BloodStoneLMGGun, ToolMaterialTypes.BloodStone, "BloodStone Light Machine Gun", "Powerful with a large clip capacity", "Uses Diamond Bullets", 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new BoltRifleInventoryItemClass(InventoryItemIDs.BloodStoneBoltActionRifle, ToolMaterialTypes.BloodStone, "BloodStone Rifle", "High power very accurate", "Uses Diamond Bullets", 12f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
			RegisterItemClass(new PistolInventoryItemClass(InventoryItemIDs.BloodStonePistol, ToolMaterialTypes.BloodStone, "BloodStone Pistol", "Basic semi automatic gun", "Uses Diamond Bullets", 8f, 0.0001f, GetClass(InventoryItemIDs.DiamondBullets)));
		}

		private static void RegisterItemClass(InventoryItemClass itemClass)
		{
			AllItems[itemClass.ID] = itemClass;
		}

		public static void FinishInitialization(GraphicsDevice device)
		{
			if (_2DImages != null)
			{
				return;
			}
			_2DImages = new RenderTarget2D(CastleMinerZGame.Instance.GraphicsDevice, 512, 1024, false, SurfaceFormat.Color, DepthFormat.Depth16);
			Viewport viewport = device.Viewport;
			RasterizerState rasterizerState = device.RasterizerState;
			DepthStencilState depthStencilState = device.DepthStencilState;
			device.SetRenderTarget(_2DImages);
			Color color = new Color(0f, 0f, 0f, 0f);
			device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, color, 1f, 0);
			device.Viewport = new Viewport(0, 0, 512, 1024);
			device.RasterizerState = RasterizerState.CullCounterClockwise;
			device.DepthStencilState = DepthStencilState.Default;
			Matrix projection = Matrix.CreateOrthographic(512f, 1024f, 0.1f, 500f);
			GameTime gameTime = new GameTime();
			BlockEntity.InitUIRendering(projection);
			foreach (InventoryItemClass value in AllItems.Values)
			{
				int iD = (int)value.ID;
				Entity entity = value.CreateEntity(ItemUse.UI, false);
				Vector3 vector = new Vector3(-256 + (iD & 7) * 64 + 32, -512 + iD / 8 * 64 + 32, -200f);
				vector.Y = 0f - vector.Y;
				entity.LocalPosition += vector;
				entity.Update(CastleMinerZGame.Instance, gameTime);
				entity.Draw(device, gameTime, Matrix.Identity, projection);
			}

			// Render mod items into a separate atlas
			int modItemCount = ItemRegistry.ModItemCount;
			if (modItemCount > 0)
			{
				ItemRegistry.EnsureAllClassesCreated();
				List<InventoryItemClass> modClasses = ItemRegistry.GetAllClasses();

				int modAtlasHeight = ((modClasses.Count + 7) / 8) * 64;
				if (modAtlasHeight < 64)
					modAtlasHeight = 64;

				_2DModImages = new RenderTarget2D(CastleMinerZGame.Instance.GraphicsDevice, 512, modAtlasHeight, false, SurfaceFormat.Color, DepthFormat.Depth16);
				device.SetRenderTarget(_2DModImages);
				device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, color, 1f, 0);
				device.Viewport = new Viewport(0, 0, 512, modAtlasHeight);
				device.RasterizerState = RasterizerState.CullCounterClockwise;
				device.DepthStencilState = DepthStencilState.Default;
				Matrix modProjection = Matrix.CreateOrthographic(512f, modAtlasHeight, 0.1f, 500f);
				BlockEntity.InitUIRendering(modProjection);

				for (int i = 0; i < modClasses.Count; i++)
				{
					modClasses[i].ModIconIndex = i;
					Entity entity = modClasses[i].CreateEntity(ItemUse.UI, false);
					Vector3 vector = new Vector3(-256 + (i & 7) * 64 + 32, -modAtlasHeight / 2 + i / 8 * 64 + 32, -200f);
					vector.Y = 0f - vector.Y;
					entity.LocalPosition += vector;
					entity.Update(CastleMinerZGame.Instance, gameTime);
					entity.Draw(device, gameTime, Matrix.Identity, modProjection);
				}
			}

			device.SetRenderTarget(null);
			device.Viewport = viewport;
			device.RasterizerState = rasterizerState;
			device.DepthStencilState = depthStencilState;
		}

		public static InventoryItem Create(BinaryReader reader)
		{
			return CreateV2(reader);
		}

		public static InventoryItem CreateV2(BinaryReader reader)
		{
			short marker = reader.ReadInt16();
			if (marker == -1)
			{
				string modItemId = reader.ReadString();
				InventoryItemClass cls = ItemRegistry.GetClass(modItemId);
				if (cls == null || cls == PlaceholderItemClass.Instance)
				{
					ModLog.Warn("Save references mod item '" + modItemId + "' but mod is not loaded. Skipping slot.");
					reader.ReadInt16();
					reader.ReadSingle();
					return null;
				}
				InventoryItem item = cls.CreateItem(0);
				item.Read(reader);
				return item;
			}
			InventoryItemIDs id = (InventoryItemIDs)marker;
			InventoryItem inventoryItem2 = CreateItem(id, 0);
			inventoryItem2.Read(reader);
			return inventoryItem2;
		}

		public virtual bool IsValid()
		{
			if (_stackCount <= MaxStackCount && _stackCount > 0 && ItemClass.ID != InventoryItemIDs.BloodStoneBullets)
			{
				return ItemClass.ID != InventoryItemIDs.SpaceRock;
			}
			return false;
		}

		public virtual void GetDisplayText(StringBuilder builder)
		{
			builder.Append(_class.Name);
		}

		public bool CanStack(InventoryItem item)
		{
			if (item != this && _class == item._class)
			{
				return StackCount < MaxStackCount;
			}
			return false;
		}

		public void Stack(InventoryItem item)
		{
			if (_class == item._class && item != this && StackCount < MaxStackCount)
			{
				StackCount += item.StackCount;
				item.StackCount = 0;
				if (StackCount > MaxStackCount)
				{
					item.StackCount += StackCount - MaxStackCount;
					StackCount = MaxStackCount;
				}
			}
		}

		public InventoryItem Split()
		{
			InventoryItem inventoryItem = CreateItem(ItemClass.ID, StackCount / 2);
			StackCount -= inventoryItem.StackCount;
			return inventoryItem;
		}

		public InventoryItem PopOneItem()
		{
			InventoryItem result = CreateItem(ItemClass.ID, 1);
			StackCount--;
			return result;
		}

		protected InventoryItem(InventoryItemClass cls, int stackCount)
		{
			_class = cls;
			_coolDownTimer = new OneShotTimer(_class.CoolDownTime);
			StackCount = stackCount;
		}

		public bool CanConsume(InventoryItemClass itemType, int amount)
		{
			if (_class != itemType || StackCount < amount)
			{
				return false;
			}
			return true;
		}

		public virtual InventoryItem CreatesWhenDug(BlockTypeEnum block)
		{
			switch (block)
			{
			case BlockTypeEnum.Grass:
				return CreateItem(InventoryItemIDs.DirtBlock, 1);
			case BlockTypeEnum.SurfaceLava:
				return CreateItem(InventoryItemIDs.RockBlock, 1);
			case BlockTypeEnum.SpaceRock:
				return CreateItem(InventoryItemIDs.SpaceRockInventory, 1);
			default:
				return BlockInventoryItemClass.CreateBlockItem(block, 1);
			}
		}

		public virtual bool InflictDamage()
		{
			ItemHealthLevel -= ItemClass.ItemSelfDamagePerUse;
			if (CastleMinerZGame.Instance.InfiniteResourceMode)
			{
				ItemHealthLevel -= ItemClass.ItemSelfDamagePerUse;
			}
			if (ItemHealthLevel <= 0f)
			{
				return true;
			}
			return false;
		}

		public virtual TimeSpan TimeToDig(BlockTypeEnum blockType)
		{
			switch (blockType)
			{
			case BlockTypeEnum.SurfaceLava:
				return TimeSpan.FromSeconds(0.0);
			case BlockTypeEnum.Rock:
				return TimeSpan.FromSeconds(10.0);
			case BlockTypeEnum.Ice:
				return TimeSpan.FromSeconds(5.0);
			case BlockTypeEnum.Log:
				return TimeSpan.FromSeconds(4.0);
			case BlockTypeEnum.Wood:
				return TimeSpan.FromSeconds(3.0);
			case BlockTypeEnum.Leaves:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Sand:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Snow:
				return TimeSpan.FromSeconds(1.0);
			case BlockTypeEnum.Dirt:
				return TimeSpan.FromSeconds(1.5);
			case BlockTypeEnum.Grass:
				return TimeSpan.FromSeconds(1.5);
			case BlockTypeEnum.Torch:
				return TimeSpan.FromSeconds(0.0);
			case BlockTypeEnum.LowerDoor:
				return TimeSpan.FromSeconds(2.0);
			case BlockTypeEnum.Lantern:
				return TimeSpan.FromSeconds(2.0);
			case BlockTypeEnum.Crate:
				return TimeSpan.FromSeconds(2.0);
			default:
				{
					// Mod blocks (slots 200-255): derive dig time from Hardness
					// instead of returning MaxValue (which would make them
					// unbreakable like Bedrock). Hardness 1 -> 1s, 5+ -> unbreakable.
					int slot = (int)blockType;
					if (slot >= 200 && slot <= 255)
					{
						BlockType bt = BlockType.GetType(blockType);
						if (bt != null && bt.Hardness < 5)
							return TimeSpan.FromSeconds(Math.Max(1, bt.Hardness));
					}
					return TimeSpan.MaxValue;
				}
			}
		}

		public virtual void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (hud.ConstructionProbe._worldIndex != DigLocation)
			{
				DigLocation = hud.ConstructionProbe._worldIndex;
				DigTime = TimeSpan.Zero;
			}
			if (controller.Use.Held || controller.Shoulder.Held)
			{
				BlockTypeEnum blockWithChanges = BlockTerrain.Instance.GetBlockWithChanges(hud.ConstructionProbe._worldIndex);
				BlockType type = BlockType.GetType(blockWithChanges);
				TimeSpan timeSpan = TimeToDig(type.ParentBlockType);
				float crackAmount = (float)(DigTime.TotalSeconds / timeSpan.TotalSeconds);
				CastleMinerZGame.Instance.GameScreen.CrackBox.CrackAmount = crackAmount;
				if ((type._type == BlockTypeEnum.TNT || type._type == BlockTypeEnum.C4) && !(hud.ActiveInventoryItem.ItemClass is SpadeInventoryClass))
				{
					if (controller.Use.Pressed || controller.Shoulder.Pressed)
					{
						hud.SetFuseForExplosive(hud.ConstructionProbe._worldIndex, (type._type != BlockTypeEnum.TNT) ? ExplosiveTypes.C4 : ExplosiveTypes.TNT);
					}
				}
				else if (type.IsItemEntity)
				{
					CastleMinerZGame.Instance.GameScreen.CrackBox.CrackAmount = 0f;
				}
				if (CoolDownTimer.Expired)
				{
					CoolDownTimer.Reset();
					hud.LocalPlayer.UsingTool = true;
					CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(ItemClass.ID);
					itemStats.Used++;
					if (hud.ConstructionProbe.HitPlayer)
					{
						hud.MeleePlayer(this, hud.ConstructionProbe.PlayerHit);
					}
					else if (hud.ConstructionProbe.AbleToBuild)
					{
						if (DigTime >= timeSpan)
						{
							hud.Dig(this, true);
							DigTime = TimeSpan.Zero;
						}
						else
						{
							hud.Dig(this, false);
						}
					}
					else if (hud.ConstructionProbe.HitZombie)
					{
						hud.Melee(this);
					}
					return;
				}
			}
			else
			{
				DigTime = TimeSpan.Zero;
			}
			hud.LocalPlayer.UsingTool = false;
		}

		public void Update(GameTime gameTime)
		{
			if (InGameHUD.Instance != null && InGameHUD.Instance.ConstructionProbe.AbleToBuild)
			{
				DigTime += gameTime.ElapsedGameTime;
			}
			else
			{
				DigTime = TimeSpan.Zero;
			}
			_coolDownTimer.Update(gameTime.ElapsedGameTime);
		}

		public void Draw2D(SpriteBatch spriteBatch, Rectangle dest, Color color)
		{
			_class.Draw2D(spriteBatch, dest, color);
			if (StackCount > 1)
			{
				sbuilder.Length = 0;
				sbuilder.Concat(StackCount);
				SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
				spriteBatch.DrawOutlinedText(smallFont, sbuilder, new Vector2(dest.X + 8, dest.Y + dest.Height - smallFont.LineSpacing), Color.White, Color.Black, 1);
			}
		}

		public void Draw2D(SpriteBatch spriteBatch, Rectangle dest)
		{
			if (ItemClass.ItemSelfDamagePerUse > 0f)
			{
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(dest.X + 9, dest.Bottom - 16, dest.Width - 18, 7), Color.Black);
				spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle(dest.X + 10, dest.Bottom - 15, (int)((float)(dest.Width - 20) * ItemHealthLevel), 5), new Color(67, 188, 0));
			}
			Draw2D(spriteBatch, dest, Color.White);
		}

		public Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
		{
			return _class.CreateEntity(use, attachedToLocalPlayer);
		}

		public virtual void Write(BinaryWriter writer)
		{
			if (_class.ModItemId != null)
			{
				writer.Write((short)(-1));
				writer.Write(_class.ModItemId);
			}
			else
			{
				writer.Write((short)_class.ID);
			}
			writer.Write((short)StackCount);
			writer.Write(ItemHealthLevel);
		}

		protected virtual void Read(BinaryReader reader)
		{
			StackCount = reader.ReadInt16();
			ItemHealthLevel = reader.ReadSingle();
		}
	}
}
