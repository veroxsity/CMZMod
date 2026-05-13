using System;
using DNA.CastleMinerZ.ModAPI;

namespace DNA.CastleMinerZ.Terrain
{
	public class BlockType
	{
		private const float cDeadBounce = 0.1f;

		private const float cSoftBounce = 0.4f;

		private const float cHardBounce = 0.6f;

		private static readonly BlockType[] _blockTypes;

		public const int MaxBlockTypes = 256;
		public const int ModBlockSlotStart = 200;
		public const int ModBlockSlotEnd = 255;

		public BlockTypeEnum _type;

		public string Name;

		public int[] TileIndices;

		public int LightTransmission;

		public int SelfIllumination;

		public int DamageMask;

		public BlockFace Facing;

		public BlockTypeEnum ParentBlockType;

		public bool IsItemEntity;

		public bool BlockPlayer;

		public bool NeedsFancyLighting;

		public bool HasAlpha;

		public bool CanBeDug;

		public bool CanBeTouched;

		public bool CanBuildOn;

		public bool DrawFullBright;

		public bool LightAsTranslucent;

		public bool InteriorFaces;

		public bool SpawnEntity;

		public float DamageTransmision;

		public int Hardness;

		public bool BouncesLasers;

		public float BounceRestitution;

		public bool AllowSlopes;

		public bool Opaque
		{
			get
			{
				return LightTransmission == 0;
			}
		}

		public bool Clear
		{
			get
			{
				return LightTransmission == 16;
			}
		}

		public int this[BlockFace i]
		{
			get
			{
				return TileIndices[(int)i % 6];
			}
		}

		public int this[int i]
		{
			get
			{
				return TileIndices[i % 6];
			}
		}

		static BlockType()
		{
			var vanilla = new BlockType[46]
			{
				new BlockType(BlockTypeEnum.Empty, "Air", 5, 1f, 0f, 1f, false, false, false, true, false, false, false, false, false, false, false, false, 0.6f, -1),
				new BlockType(BlockTypeEnum.Dirt, "Dirt", 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._00),
				new BlockType(BlockTypeEnum.Grass, "Grass", 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._02, Octal._01, Octal._01, Octal._00, Octal._01, Octal._01),
				new BlockType(BlockTypeEnum.Sand, "Sand", 1, 0f, 0f, 0.7f, false, false, false, false, false, true, true, true, true, false, false, false, 0.1f, Octal._03),
				new BlockType(BlockTypeEnum.Lantern, "Lantern", 2, 0f, 1f, 1f, false, false, false, false, true, true, true, true, true, false, true, false, 0.6f, Octal._04),
				new BlockType(BlockTypeEnum.FixedLantern, "Lantern", 5, 0f, 1f, 1f, false, false, false, false, true, true, true, true, false, false, false, false, 0.6f, Octal._04),
				new BlockType(BlockTypeEnum.Rock, "Rock", 3, 0f, 0f, 0.5f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._05),
				new BlockType(BlockTypeEnum.GoldOre, "Gold Ore", 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._06),
				new BlockType(BlockTypeEnum.IronOre, "Iron Ore", 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._07),
				new BlockType(BlockTypeEnum.CopperOre, "Copper Ore", 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._10),
				new BlockType(BlockTypeEnum.CoalOre, "Coal", 3, 0f, 0f, 0.5f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._11),
				new BlockType(BlockTypeEnum.DiamondOre, "Diamonds", 3, 0f, 0f, 0.4f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._12),
				new BlockType(BlockTypeEnum.SurfaceLava, "Lava", 3, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.6f, Octal._13),
				new BlockType(BlockTypeEnum.DeepLava, "Lava", 3, 0f, 1f, 1f, false, false, false, false, false, false, true, true, true, true, false, false, 0.1f, Octal._13),
				new BlockType(BlockTypeEnum.Bedrock, "Bedrock", 5, 0f, 0f, 0.3f, false, false, false, false, false, true, true, true, false, false, false, true, 0.6f, Octal._14),
				new BlockType(BlockTypeEnum.Snow, "Snow", 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._15),
				new BlockType(BlockTypeEnum.Ice, "Ice", 2, 0.9f, 0f, 0.9f, false, true, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._16),
				new BlockType(BlockTypeEnum.Log, "Log", 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._20, Octal._17, Octal._17, Octal._20, Octal._17, Octal._17),
				new BlockType(BlockTypeEnum.Leaves, "Leaves", 1, 0.4f, 0f, 1f, false, true, true, true, false, false, true, true, true, false, false, false, 0.1f, Octal._21),
				new BlockType(BlockTypeEnum.Wood, "Wood", 2, 0f, 0f, 0.8f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._22),
				new BlockType(BlockTypeEnum.BloodStone, "BloodStone", 4, 0f, 0f, 0.2f, false, false, false, false, false, true, true, true, true, false, false, true, 0.6f, Octal._23),
				new BlockType(BlockTypeEnum.SpaceRock, "Space Rock", 4, 0f, 0f, 0.1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._24),
				new BlockType(BlockTypeEnum.IronWall, "Iron Wall", 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._25),
				new BlockType(BlockTypeEnum.CopperWall, "Copper Wall", 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._26),
				new BlockType(BlockTypeEnum.GoldenWall, "Golden Wall", 4, 0f, 0f, 0.3f, false, false, false, false, true, true, true, true, true, false, false, false, 0.6f, Octal._27),
				new BlockType(BlockTypeEnum.DiamondWall, "Diamond Wall", 4, 0f, 0f, 0.2f, false, false, false, false, true, true, true, true, true, false, false, true, 0.6f, Octal._30),
				new BlockType(BlockTypeEnum.Torch, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, -1),
				new BlockType(BlockTypeEnum.TorchPOSX, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSX, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.TorchNEGZ, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.NEGZ, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.TorchNEGX, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.NEGX, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.TorchPOSZ, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSZ, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.TorchPOSY, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.6f, BlockFace.POSY, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.TorchNEGY, "Torch", 1, 1f, 1f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.NEGY, BlockTypeEnum.Torch),
				new BlockType(BlockTypeEnum.Crate, "Crate", 2, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._31),
				new BlockType(BlockTypeEnum.LowerDoorClosedZ, "Door", 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.LowerDoorClosedX, "Door", 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.LowerDoor, "Door", 1, 0f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, true, false, 0.4f, -1),
				new BlockType(BlockTypeEnum.UpperDoorClosed, "Door", 1, 0.5f, 0f, 0.8f, true, false, false, true, false, true, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.LowerDoorOpenZ, "Door", 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.LowerDoorOpenX, "Door", 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, true, false, 0.4f, BlockFace.POSY, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.UpperDoorOpen, "Door", 1, 1f, 0f, 1f, true, false, false, true, false, false, true, false, true, false, false, false, 0.4f, BlockFace.NUM_FACES, BlockTypeEnum.LowerDoor),
				new BlockType(BlockTypeEnum.TNT, "TNT", 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.4f, Octal._34, Octal._33, Octal._33, Octal._34, Octal._33, Octal._33),
				new BlockType(BlockTypeEnum.C4, "C4", 1, 0f, 0f, 1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._36, Octal._35, Octal._35, Octal._36, Octal._35, Octal._35),
				new BlockType(BlockTypeEnum.Slime, "Space Goo", 4, 0f, 1f, 1f, false, false, false, false, false, true, true, true, true, true, false, false, 0.1f, Octal._32),
				new BlockType(BlockTypeEnum.SpaceRockInventory, "Space Rock", 4, 0f, 0f, 0.1f, false, false, false, false, false, true, true, true, true, false, false, false, 0.6f, Octal._24),
				new BlockType(BlockTypeEnum.NumberOfBlocks, "Air", 5, 1f, 0f, 1f, false, false, false, true, false, true, false, false, false, false, false, false, 0.6f, -1)
			};
			_blockTypes = new BlockType[256];
			for (int i = 0; i < 46; i++)
				_blockTypes[i] = vanilla[i];
			_blockTypes[35].AllowSlopes = false;
			_blockTypes[34].AllowSlopes = false;
			_blockTypes[36].AllowSlopes = false;
			_blockTypes[37].AllowSlopes = false;
		}

		public static bool IsEmpty(BlockTypeEnum btype)
		{
			if (btype != BlockTypeEnum.NumberOfBlocks)
			{
				return btype == BlockTypeEnum.Empty;
			}
			return true;
		}

		public override string ToString()
		{
			return Name;
		}

		public static BlockType GetType(BlockTypeEnum t)
		{
			int idx = (int)t;
			if (idx < 0 || idx >= _blockTypes.Length)
			{
				return _blockTypes[1]; // Dirt fallback (out-of-range slot)
			}
			BlockType bt = _blockTypes[idx];
			if (bt == null)
			{
				return _blockTypes[1]; // Dirt fallback (unregistered slot 46-199)
			}
			return bt;
		}

		public static void RegisterModBlock(ModAPI.BlockDef def)
		{
			int slot = (int)def.Slot;
			if (slot < 200 || slot > 255)
				throw new ArgumentOutOfRangeException("def.Slot", "Mod block slot must be 200-255");
			if (_blockTypes[slot] != null)
				throw new InvalidOperationException("Block slot " + slot + " is already occupied");

			int[] t = def.TileIndices ?? new int[6];
			float lt = def.LightTransmission;
			float si = def.SelfIllumination;
			float dt = def.DamageTransmission;

			_blockTypes[slot] = new BlockType(
				def.Slot,
				def.DisplayName ?? def.Id,
				def.Hardness,
				lt,
				si,
				dt,
				def.IsItemEntity,
				def.LightAsTranslucent,
				def.InteriorFaces,
				def.HasAlpha,
				def.NeedsFancyLighting,
				def.BlockPlayer,
				def.CanBeTouched,
				def.CanBuildOn,
				def.CanBeDug,
				def.DrawFullBright,
				def.SpawnEntity,
				def.BouncesLasers,
				def.BounceRestitution,
				t[4], t[0], t[3], t[5], t[2], t[1]);
			_blockTypes[slot].AllowSlopes = def.AllowSlopes;
			_blockTypes[slot].Facing = def.Facing;
			_blockTypes[slot].ParentBlockType = def.ParentBlockType;
		}

		public int TransmitLight(int inlight)
		{
			return Math.Max(0, (inlight - 1) * LightTransmission >> 4);
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, int texIndexPosY, int texIndexPosX, int texIndexPosZ, int texIndexNegY, int texIndexNegX, int texIndexNegZ)
		{
			Name = name;
			_type = type;
			Hardness = hardness;
			LightTransmission = (int)Math.Floor(lightTransmission * 16f + 0.5f);
			SelfIllumination = (int)Math.Floor(selfIllumination * 15f + 0.5f);
			IsItemEntity = isItemEntity;
			HasAlpha = alphaInTexture;
			NeedsFancyLighting = hasSpecular;
			BlockPlayer = blockPlayer;
			CanBeDug = canBeDug;
			CanBeTouched = canBeTouched;
			DrawFullBright = fullBright;
			LightAsTranslucent = xlucent;
			InteriorFaces = interior;
			SpawnEntity = spawnEntity;
			CanBuildOn = canBuildOn;
			DamageTransmision = damageTransmision;
			BouncesLasers = bounceLasers;
			BounceRestitution = bounceRestitution;
			AllowSlopes = blockPlayer;
			Facing = BlockFace.NUM_FACES;
			ParentBlockType = type;
			TileIndices = new int[6] { texIndexPosX, texIndexNegZ, texIndexNegX, texIndexPosZ, texIndexPosY, texIndexNegY };
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, int texIndexPosY)
		{
			Name = name;
			_type = type;
			Hardness = hardness;
			LightTransmission = (int)Math.Floor(lightTransmission * 16f + 0.5f);
			SelfIllumination = (int)Math.Floor(selfIllumination * 15f + 0.5f);
			IsItemEntity = isItemEntity;
			HasAlpha = alphaInTexture;
			NeedsFancyLighting = hasSpecular;
			BlockPlayer = blockPlayer;
			CanBeDug = canBeDug;
			CanBeTouched = canBeTouched;
			DrawFullBright = fullBright;
			LightAsTranslucent = xlucent;
			InteriorFaces = interior;
			SpawnEntity = spawnEntity;
			CanBuildOn = canBuildOn;
			DamageTransmision = damageTransmision;
			BouncesLasers = bounceLasers;
			BounceRestitution = bounceRestitution;
			AllowSlopes = blockPlayer;
			Facing = BlockFace.NUM_FACES;
			ParentBlockType = type;
			TileIndices = new int[6] { texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY, texIndexPosY };
		}

		private BlockType(BlockTypeEnum type, string name, int hardness, float lightTransmission, float selfIllumination, float damageTransmision, bool isItemEntity, bool xlucent, bool interior, bool alphaInTexture, bool hasSpecular, bool blockPlayer, bool canBeTouched, bool canBuildOn, bool canBeDug, bool fullBright, bool spawnEntity, bool bounceLasers, float bounceRestitution, BlockFace facing, BlockTypeEnum parentBlock)
		{
			Name = name;
			_type = type;
			Hardness = hardness;
			LightTransmission = (int)Math.Floor(lightTransmission * 16f + 0.5f);
			SelfIllumination = (int)Math.Floor(selfIllumination * 15f + 0.5f);
			IsItemEntity = isItemEntity;
			HasAlpha = alphaInTexture;
			NeedsFancyLighting = hasSpecular;
			BlockPlayer = blockPlayer;
			CanBeDug = canBeDug;
			CanBeTouched = canBeTouched;
			DrawFullBright = fullBright;
			LightAsTranslucent = xlucent;
			InteriorFaces = interior;
			SpawnEntity = spawnEntity;
			CanBuildOn = canBuildOn;
			DamageTransmision = damageTransmision;
			BouncesLasers = bounceLasers;
			BounceRestitution = bounceRestitution;
			AllowSlopes = blockPlayer;
			Facing = facing;
			ParentBlockType = parentBlock;
			TileIndices = new int[6] { -1, -1, -1, -1, -1, -1 };
		}
	}
}
