using DNA.CastleMinerZ.ModAPI;

namespace ExampleCraftLogger
{
    [Mod(Id = "example.craft-logger", Name = "Craft Logger", Version = "1.0.0")]
    public static class CraftLoggerMod
    {
        public static void OnLoad()
        {
            Events.ItemCrafted += OnItemCrafted;
            ModLog.Info("Craft Logger active — all crafting will be logged");
        }

        private static void OnItemCrafted(ItemCraftedEventArgs args)
        {
            if (args.Result != null && args.Result.ItemClass != null)
            {
                string itemName = args.Result.ItemClass.Name;
                int count = args.Result.StackCount;
                ModLog.Info(string.Format("Crafted: {0}x {1}", count, itemName));
            }
        }
    }
}
