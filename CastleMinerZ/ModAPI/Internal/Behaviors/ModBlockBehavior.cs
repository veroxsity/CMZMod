using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI.Internal.Behaviors
{
    public class ModBlockBehavior : BlockInventoryItemClass
    {
        public ModBlockBehavior(ItemDef def)
            : base(InventoryItemIDs.BareHands, ResolveBlockType(def),
                   def.Description1, def.Description2, def.EnemyDamage)
        {
            ModItemId = def.Id;
            SetName(def.DisplayName);
            MaxStackCount = def.MaxStackSize;
            EnemyDamageType = def.EnemyDamageType;
            ItemSelfDamagePerUse = def.ItemSelfDamagePerUse;
            if (!string.IsNullOrEmpty(def.UseSoundCue))
                SetUseSound(def.UseSoundCue);
            IconTextureName = def.IconTextureName;
        }

        private static BlockTypeEnum ResolveBlockType(ItemDef def)
        {
            // Prefer an explicitly set BlockId; otherwise fall back to the item's
            // own Id (matches the common pattern where a mod registers a block
            // and an item with the same string id).
            string id = !string.IsNullOrEmpty(def.BlockId) ? def.BlockId : def.Id;
            if (!string.IsNullOrEmpty(id))
            {
                BlockTypeEnum slot = BlockRegistry.GetSlot(id);
                if (slot != BlockTypeEnum.Empty)
                    return slot;
            }
            // Unregistered: fall back to Wood so the item is still placeable.
            return BlockTypeEnum.Wood;
        }
    }
}
