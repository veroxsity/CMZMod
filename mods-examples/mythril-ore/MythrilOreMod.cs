using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;

namespace ExampleMythrilOre
{
    [Mod(Id = "example.mythril-ore", Name = "Mythril Ore", Version = "1.0.0")]
    public static class MythrilOreMod
    {
        public static void OnLoad()
        {
            Blocks.Register("example.mythril-ore", new BlockDef
            {
                DisplayName = "Mythril Ore",
                Hardness = 3,
                LightTransmission = 0.1f,
                SelfIllumination = 0.3f,
                TileIndices = new int[6] { 0, 0, 0, 0, 0, 0 },
                BlockPlayer = true,
                CanBeDug = true,
                CanBeTouched = true,
                CanBuildOn = true,
                HasAlpha = false,
                DrawFullBright = false,
                InteriorFaces = false,
                AllowSlopes = true,
            });

            Items.Register("example.mythril-ore", new ItemDef
            {
                DisplayName = "Mythril Ore",
                Description1 = "Rare glowing ore",
                Description2 = "Mine with a high-tier pickaxe.",
                MaxStackSize = 100,
                BehaviorClass = ItemBehaviors.Block,
            });

            Worldgen.RegisterOre("example.mythril-ore", new OreSpawnDef
            {
                MinY = 5,
                MaxY = 45,
                Frequency = 1.5f,
                NoiseOffset = 1337,
            });

            ModLog.Info("Mythril Ore active — custom ore veins in new worlds (Y 5-45)");
        }
    }
}
