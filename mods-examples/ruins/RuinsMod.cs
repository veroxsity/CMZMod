using DNA;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;

namespace ExampleRuins
{
    public class StoneMarkerStructure : IStructure
    {
        public void Place(BlockTerrain terrain, IntVector3 origin)
        {
            // 3x3 rock pad in air above the surface column.
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dz = -1; dz <= 1; dz++)
                {
                    IntVector3 foot = new IntVector3(origin.X + dx, origin.Y, origin.Z + dz);
                    Worldgen.TrySetBlock(terrain, foot, BlockTypeEnum.Rock);
                }
            }

            IntVector3 pillar = new IntVector3(origin.X, origin.Y + 1, origin.Z);
            if (Worldgen.TrySetBlock(terrain, pillar, BlockTypeEnum.Rock))
            {
                // TorchPOSY attaches to the solid block below (the pillar).
                IntVector3 light = new IntVector3(origin.X, origin.Y + 2, origin.Z);
                Worldgen.TrySetBlock(terrain, light, BlockTypeEnum.TorchPOSY);
            }
        }
    }

    [Mod(Id = "example.ruins", Name = "Surface Ruins", Version = "1.0.0")]
    public static class RuinsMod
    {
        private static readonly StoneMarkerStructure Marker = new StoneMarkerStructure();

        public static void OnLoad()
        {
            Worldgen.ChunkGenerated += OnChunkGenerated;
            ModLog.Info("Surface Ruins active — 1% chance stone marker per chunk (new worlds only)");
        }

        private static void OnChunkGenerated(ChunkGeneratedArgs args)
        {
            if (args.Terrain == null)
                return;

            int seed = args.ChunkMin.X * 734287 ^ args.ChunkMin.Z * 912671;
            var rnd = new System.Random(seed);
            if (rnd.Next(100) != 0)
                return;

            int x = args.ChunkMin.X + rnd.Next(2, 14);
            int z = args.ChunkMin.Z + rnd.Next(2, 14);
            int surfaceY = FindTopSolidY(args.Terrain, x, z);
            if (surfaceY < 0 || surfaceY > 58)
                return;

            // Place marker one block above the top solid block (in air).
            var origin = new IntVector3(x, surfaceY + 1, z);
            Worldgen.PlaceStructure(args.Terrain, origin, Marker);
            ModLog.Info(string.Format("Surface ruin placed at X:{0} Y:{1} Z:{2}", origin.X, origin.Y, origin.Z));
        }

        private static int FindTopSolidY(BlockTerrain terrain, int worldX, int worldZ)
        {
            for (int worldY = 58; worldY >= -55; worldY--)
            {
                IntVector3 pos = new IntVector3(worldX, worldY, worldZ);
                int idx = terrain.MakeIndexFromWorldIndexVector(pos);
                if (idx < 0)
                    continue;

                BlockTypeEnum type = Block.GetType(terrain._blocks[idx])._type;
                if (type == BlockTypeEnum.Empty || type == BlockTypeEnum.NumberOfBlocks)
                    continue;
                if (BlockType.GetType(type).HasAlpha)
                    continue;
                return worldY;
            }
            return -1;
        }
    }
}
