using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;

namespace ExampleDiamondRush
{
    [Mod(Id = "example.diamond-rush", Name = "Diamond Rush", Version = "1.0.0")]
    public static class DiamondRushMod
    {
        public static void OnLoad()
        {
            Worldgen.SetOreFrequency(BlockTypeEnum.DiamondOre, 10f);
            ModLog.Info("Diamond Rush active — 10x diamond ore frequency (new worlds only)");
        }
    }
}
