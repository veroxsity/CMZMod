using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;

namespace ExampleCheapTorches
{
    [Mod(Id = "example.cheap-torches", Name = "Cheap Torches", Version = "1.0.0")]
    public static class CheapTorchesMod
    {
        public static void OnLoad()
        {
            // Replace the torch recipe: 1 stick -> 4 torches (no coal)
            Recipes.Modify(InventoryItemIDs.Torch, 4, InventoryItemIDs.Stick);
        }
    }
}
