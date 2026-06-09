using System;
using System.Collections.Generic;
using DNA;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Terrain.WorldBuilders;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class WorldgenRegistry
    {
        private const int OreTableSize = 256;
        private const float DefaultMultiplier = 1f;

        private static float[] _oreMultipliers;
        private static List<ModOreEntry> _modOres;

        private struct ModOreEntry
        {
            public BlockTypeEnum Block;
            public OreSpawnDef Def;
        }

        static WorldgenRegistry()
        {
            ResetOreMultipliers();
            _modOres = new List<ModOreEntry>();
        }

        private static void ResetOreMultipliers()
        {
            _oreMultipliers = new float[OreTableSize];
            for (int i = 0; i < OreTableSize; i++)
                _oreMultipliers[i] = DefaultMultiplier;
        }

        public static void SetOreFrequency(BlockTypeEnum ore, float multiplier)
        {
            EnsureOreTable();
            int idx = (int)ore;
            if (idx < 0 || idx >= OreTableSize)
                return;
            _oreMultipliers[idx] = Math.Max(0.05f, multiplier);
        }

        public static int GetOreThresholdBonus(BlockTypeEnum ore)
        {
            EnsureOreTable();
            int idx = (int)ore;
            if (idx < 0 || idx >= OreTableSize)
                return 0;
            float mult = _oreMultipliers[idx];
            return (int)((mult - 1f) * 20f);
        }

        public static void RegisterOre(string blockId, OreSpawnDef def)
        {
            if (blockId == null)
                throw new ArgumentNullException("blockId");
            if (def == null)
                throw new ArgumentNullException("def");

            BlockDef block = BlockRegistry.Resolve(blockId);
            if (block == null)
                throw new ArgumentException("Unknown mod block ID: " + blockId);

            _modOres.Add(new ModOreEntry
            {
                Block = block.Slot,
                Def = def,
            });
            ModLog.Info(string.Format("Registered mod ore '{0}' for worldgen (Y {1}-{2})",
                blockId, def.MinY, def.MaxY));
        }

        public static void DepositModOres(BlockTerrain terrain, IntVector3 minLoc, int worldSeed)
        {
            if (_modOres.Count == 0 || terrain == null)
                return;

            IntNoise noise = new IntNoise(new Random(worldSeed ^ 0x5A17));
            int rockBlock = Block.SetType(0, BlockTypeEnum.Rock);

            for (int i = 0; i < _modOres.Count; i++)
            {
                ModOreEntry entry = _modOres[i];
                OreSpawnDef def = entry.Def;
                int oreBlock = Block.SetType(0, entry.Block);
                int threshold = (int)(255f - def.Frequency * 40f);
                if (threshold < 180)
                    threshold = 180;

                for (int z = 0; z < 16; z++)
                {
                    int worldZ = minLoc.Z + z;
                    for (int x = 0; x < 16; x++)
                    {
                        int worldX = minLoc.X + x;
                        for (int y = def.MinY; y < def.MaxY; y++)
                        {
                            IntVector3 pos = new IntVector3(worldX, y, worldZ);
                            int idx = terrain.MakeIndexFromWorldIndexVector(pos);
                            if (terrain._blocks[idx] != rockBlock)
                                continue;

                            IntVector3 noisePos = pos + new IntVector3(def.NoiseOffset, def.NoiseOffset, def.NoiseOffset);
                            int n1 = noise.ComputeNoise(noisePos / 3);
                            int n2 = noise.ComputeNoise(noisePos);
                            n1 += (n2 - 128) / 8;
                            if (n1 > threshold)
                                terrain._blocks[idx] = oreBlock;
                        }
                    }
                }
            }
        }

        public static void PlaceStructure(BlockTerrain terrain, IntVector3 origin, IStructure structure)
        {
            if (terrain == null || structure == null)
                return;
            try
            {
                structure.Place(terrain, origin);
            }
            catch (Exception ex)
            {
                ModLog.Error("PlaceStructure failed: " + ex.Message);
            }
        }

        private static void EnsureOreTable()
        {
            if (_oreMultipliers == null)
                ResetOreMultipliers();
        }
    }
}
