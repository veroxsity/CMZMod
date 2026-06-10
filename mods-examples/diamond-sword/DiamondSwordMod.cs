using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.ModAPI;

namespace YouDiamondSword
{
    [Mod(Id = "you.diamond-sword", Name = "Diamond Sword", Version = "1.0.0")]
    public static class DiamondSwordMod
    {
        public static void OnLoad()
        {
            Items.Register("you.diamond-sword", new ItemDef {
                DisplayName = "Diamond Sword",
                Description1 = "A blade of pure diamond",
                Description2 = "Devastates undead in melee combat",
                IconTextureName = "diamond-sword",
                MaxStackSize = 1,
                BehaviorClass = ItemBehaviors.Sword,
                EnemyDamage = 25f,
                EnemyDamageType = DamageType.BLADE,
                CoolDownTime = TimeSpan.FromMilliseconds(400),
                IsMeleeWeapon = true,
            });

            Recipes.Add("you.diamond-sword", 1,
                InventoryItemIDs.Diamond,
                InventoryItemIDs.Stick);
        }
    }
}
