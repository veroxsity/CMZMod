using System;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Blocks
    {
        public static void Register(string id, BlockDef def)
        {
            Internal.BlockRegistry.Register(id, def);
        }

        public static void Modify(BlockTypeEnum block, Action<BlockDef> change)
        {
            BlockType type = BlockType.GetType(block);
            if (type == null)
                return;
            var def = new BlockDef
            {
                Id = type.Name,
                DisplayName = type.Name,
                Hardness = type.Hardness,
                LightTransmission = type.LightTransmission / 16f,
                SelfIllumination = type.SelfIllumination / 15f,
                TileIndices = (int[])type.TileIndices.Clone(),
                BlockPlayer = type.BlockPlayer,
                CanBeDug = type.CanBeDug,
                CanBeTouched = type.CanBeTouched,
                CanBuildOn = type.CanBuildOn,
                HasAlpha = type.HasAlpha,
                DrawFullBright = type.DrawFullBright,
                BouncesLasers = type.BouncesLasers,
                BounceRestitution = type.BounceRestitution,
                SpawnEntity = type.SpawnEntity,
                DamageTransmission = type.DamageTransmision,
                IsItemEntity = type.IsItemEntity,
                LightAsTranslucent = type.LightAsTranslucent,
                NeedsFancyLighting = type.NeedsFancyLighting,
                InteriorFaces = type.InteriorFaces,
                AllowSlopes = type.AllowSlopes,
                Facing = type.Facing,
                ParentBlockType = type.ParentBlockType,
            };
            change(def);
            BlockType.ApplyFromDef(block, def);
        }

        public static void Modify(string modBlockId, Action<BlockDef> change)
        {
            BlockDef def = Internal.BlockRegistry.Resolve(modBlockId);
            if (def == null)
                return;
            change(def);
            Internal.BlockRegistry.ApplyDef(modBlockId);
        }

        public static BlockDef Get(string id)
        {
            return Internal.BlockRegistry.Resolve(id);
        }
    }
}
