using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI
{
    /// <summary>Spawn rules for a mod ore block during world generation.</summary>
    public class OreSpawnDef
    {
        /// <summary>Minimum world Y (inclusive) where this ore can appear.</summary>
        public int MinY = 0;

        /// <summary>Maximum world Y (exclusive) where this ore can appear.</summary>
        public int MaxY = 40;

        /// <summary>Relative spawn rarity vs vanilla ores (1.0 = similar density).</summary>
        public float Frequency = 1f;

        /// <summary>Offsets the noise field so each ore veins independently.</summary>
        public int NoiseOffset = 500;
    }
}
