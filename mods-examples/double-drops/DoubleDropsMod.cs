using DNA;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;

namespace ExampleDoubleDrops
{
    [Mod(Id = "example.double-drops", Name = "Double Drops", Version = "1.0.0")]
    public static class DoubleDropsMod
    {
        public static void OnLoad()
        {
            Events.PlayerMinedBlock += OnPlayerMinedBlock;
            ModLog.Info("Double Drops active");
        }

        private static void OnPlayerMinedBlock(PlayerMinedBlockEventArgs args)
        {
            // Diagnostic
            BlockType bt = BlockType.GetType(args.BlockType);
            string blockName = bt != null ? bt.Name : "<null>";
            string dropName  = (args.Drop != null && args.Drop.ItemClass != null)
                                ? args.Drop.ItemClass.Name : "<null>";

            ModLog.Info(string.Format(
                "Mined: {0} (slot {1}) -> {2}",
                blockName, (int)args.BlockType, dropName));

            // If the player didn't get a vanilla drop (wrong tier pickaxe on
            // an ore, special block, etc) there's nothing to double.
            if (args.Drop == null || args.Drop.ItemClass == null)
                return;

            // Spawn a duplicate of whatever vanilla just dropped.
            InventoryItem bonus = args.Drop.ItemClass.CreateItem(args.Drop.StackCount);
            if (bonus != null && PickupManager.Instance != null)
            {
                Vector3 pos = IntVector3.ToVector3(args.BlockPosition) + new Vector3(0.5f, 0.5f, 0.5f);
                PickupManager.Instance.CreatePickup(bonus, pos, false);
            }
        }
    }
}
