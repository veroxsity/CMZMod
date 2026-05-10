using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI.Internal.Behaviors
{
    public class ModBlockBehavior : BlockInventoryItemClass
    {
        public ModBlockBehavior(ItemDef def)
            : base(InventoryItemIDs.BareHands, BlockTypeEnum.Wood,
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
    }
}
