using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;

namespace ExampleMarbleBlock
{
    [Mod(Id = "example.marble-block", Name = "Marble Block", Version = "1.0.0")]
    public static class MarbleBlockMod
    {
        public static void OnLoad()
        {
            // Register a custom block type (allocates a slot in the 200-255 range)
            Blocks.Register("example.marble-block", new BlockDef
            {
                DisplayName = "Marble",
                Hardness = 4,
                LightTransmission = 0.2f,
                SelfIllumination = 0.1f,
                TileIndices = new int[6] { 0, 0, 0, 0, 0, 0 },
                BlockPlayer = true,
                CanBeDug = true,
                CanBeTouched = true,
                CanBuildOn = true,
                HasAlpha = false,
                DrawFullBright = false,
                InteriorFaces = false,
                AllowSlopes = true,
                // ParentBlockType omitted -> defaults to this block's own slot
                // so mining marble drops a marble item (not rock).
            });

            // Register the inventory item that places this block
            Items.Register("example.marble-block", new ItemDef
            {
                DisplayName = "Marble Block",
                Description1 = "A polished block of marble",
                Description2 = "Slightly luminous. Crafted from stone.",
                MaxStackSize = 100,
                BehaviorClass = ItemBehaviors.Block,
            });

            // Craft recipe: 4 stone -> 4 marble blocks
            Recipes.Add("example.marble-block", 4,
                InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock,
                InventoryItemIDs.RockBlock, InventoryItemIDs.RockBlock);
        }
    }
}
