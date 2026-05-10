using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;

namespace DNA.CastleMinerZ.ModAPI.Internal.Behaviors
{
    public class ModConsumableBehavior : InventoryItem.InventoryItemClass
    {
        public ModConsumableBehavior(ItemDef def)
            : base(InventoryItemIDs.BareHands, def.DisplayName,
                   def.Description1, def.Description2,
                   def.MaxStackSize, def.CoolDownTime)
        {
            ModItemId = def.Id;
            EnemyDamage = def.EnemyDamage;
            EnemyDamageType = def.EnemyDamageType;
            ItemSelfDamagePerUse = def.ItemSelfDamagePerUse;
            _playerMode = def.PlayerMode;
            if (!string.IsNullOrEmpty(def.UseSoundCue))
                SetUseSound(def.UseSoundCue);
            IconTextureName = def.IconTextureName;
        }

        public override Entity CreateEntity(ItemUse use, bool attachedToLocalPlayer)
        {
            return new Entity();
        }
    }
}
