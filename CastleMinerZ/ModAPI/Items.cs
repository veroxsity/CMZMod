using System;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Items
    {
        public static void Register(string id, ItemDef def)
        {
            Internal.ItemRegistry.Register(id, def);
        }

        public static void Modify(string id, Action<ItemDef> change)
        {
            ItemDef def = Internal.ItemRegistry.Resolve(id);
            if (def != null)
                change(def);
        }

        public static ItemDef Get(string id)
        {
            return Internal.ItemRegistry.Resolve(id);
        }

        public static void SetEnemyDamage(InventoryItemIDs item, float damage)
        {
            InventoryItem.GetClass(item).EnemyDamage = damage;
        }

        public static void SetMaxStack(InventoryItemIDs item, int max)
        {
            InventoryItem.GetClass(item).MaxStackCount = max;
        }

        public static void SetCooldown(InventoryItemIDs item, TimeSpan cooldown)
        {
            InventoryItem.GetClass(item).SetCoolDown(cooldown);
        }

        public static void SetSelfDamagePerUse(InventoryItemIDs item, float damage)
        {
            InventoryItem.GetClass(item).ItemSelfDamagePerUse = damage;
        }

        public static void SetDisplayName(InventoryItemIDs item, string name)
        {
            InventoryItem.GetClass(item).SetName(name);
        }

        public static void SetDescription(InventoryItemIDs item, string desc1, string desc2)
        {
            InventoryItem.GetClass(item).SetDescription(desc1, desc2 ?? "");
        }
    }

    public static class ItemBehaviors
    {
        public static Type Sword { get { return typeof(Internal.Behaviors.ModSwordBehavior); } }
        public static Type PickAxe { get { return typeof(Internal.Behaviors.ModPickAxeBehavior); } }
        public static Type Spade { get { return typeof(Internal.Behaviors.ModSpadeBehavior); } }
        public static Type Axe { get { return typeof(Internal.Behaviors.ModAxeBehavior); } }
        public static Type Block { get { return typeof(Internal.Behaviors.ModBlockBehavior); } }
        public static Type Consumable { get { return typeof(Internal.Behaviors.ModConsumableBehavior); } }
    }
}
