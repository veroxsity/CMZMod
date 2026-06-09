using System;
using DNA;
using DNA.CastleMinerZ.ModAPI.Internal;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Worldgen
    {
        public static event Action<ChunkGeneratedArgs> ChunkGenerated;

        public static void SetOreFrequency(BlockTypeEnum ore, float multiplier)
        {
            WorldgenRegistry.SetOreFrequency(ore, multiplier);
        }

        public static void RegisterOre(string blockId, OreSpawnDef def)
        {
            WorldgenRegistry.RegisterOre(blockId, def);
        }

        public static void RegisterBiome(string id, IModBiome biome)
        {
            ModLog.Warn("RegisterBiome is not implemented yet — use ChunkGenerated for custom terrain overlays");
        }

        public static void PlaceStructure(BlockTerrain terrain, IntVector3 origin, IStructure structure)
        {
            WorldgenRegistry.PlaceStructure(terrain, origin, structure);
        }

        /// <summary>Sets a block if the world position is inside the active terrain buffer.</summary>
        public static bool TrySetBlock(BlockTerrain terrain, IntVector3 worldPos, BlockTypeEnum type)
        {
            if (terrain == null)
                return false;
            int idx = terrain.MakeIndexFromWorldIndexVector(worldPos);
            if (idx < 0)
                return false;

            BlockTypeEnum oldType = Block.GetTypeIndex(terrain._blocks[idx]);
            BlockType oldBlock = BlockType.GetType(oldType);
            if (oldBlock != null && oldBlock.SpawnEntity)
                terrain.RemoveItemBlockEntity(oldType, worldPos);

            terrain._blocks[idx] = Block.SetType(0, type);

            BlockType newBlock = BlockType.GetType(type);
            if (newBlock != null && newBlock.SpawnEntity)
                terrain.CreateItemBlockEntity(type, worldPos);

            return true;
        }

        internal static void FireChunkGenerated(ChunkGeneratedArgs args)
        {
            var handler = ChunkGenerated;
            if (handler == null || args == null)
                return;

            foreach (Action<ChunkGeneratedArgs> cb in handler.GetInvocationList())
            {
                try
                {
                    cb(args);
                }
                catch (Exception ex)
                {
                    ModLog.Error("ChunkGenerated handler failed: " + ex.Message);
                }
            }
        }
    }
}
