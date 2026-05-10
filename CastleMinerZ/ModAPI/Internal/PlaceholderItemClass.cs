using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public class PlaceholderItemClass : DNA.CastleMinerZ.Inventory.InventoryItem.InventoryItemClass
    {
        public static PlaceholderItemClass Instance = new PlaceholderItemClass();

        private PlaceholderItemClass()
            : base(DNA.CastleMinerZ.Inventory.InventoryItemIDs.BareHands,
                  "MISSING MOD ITEM", "This item's mod is not installed",
                  "Reinstall the mod to restore this item",
                  1, System.TimeSpan.Zero)
        {
            EnemyDamage = 0f;
            EnemyDamageType = DamageType.BLUNT;
        }

        public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
        {
            return new Entity();
        }

        public override DNA.CastleMinerZ.Inventory.InventoryItem CreateItem(int stackCount)
        {
            var item = base.CreateItem(stackCount);
            return item;
        }
    }
}
