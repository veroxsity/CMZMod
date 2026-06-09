using DNA;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI
{
    public class ChunkGeneratedArgs
    {
        public BlockTerrain Terrain;
        public IntVector3 ChunkMin;
        public IntVector3 ChunkSize;
    }

    /// <summary>Places blocks relative to an origin during post-gen hooks.</summary>
    public interface IStructure
    {
        void Place(BlockTerrain terrain, IntVector3 origin);
    }

    /// <summary>Reserved for Phase 4 Step 5b — full biome dispatch is not implemented yet.</summary>
    public interface IModBiome
    {
        string Id { get; }
    }
}
