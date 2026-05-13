using System;
using System.IO;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.ModAPI
{
    public class ItemDef
    {
        public string Id { get; internal set; }
        public string DisplayName { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string IconTextureName { get; set; }
        public int MaxStackSize { get; set; }
        public Type BehaviorClass { get; set; }
        public float EnemyDamage { get; set; }
        public DamageType EnemyDamageType { get; set; }
        public TimeSpan CoolDownTime { get; set; }
        public float ItemSelfDamagePerUse { get; set; }
        public string UseSoundCue { get; set; }
        public PlayerMode PlayerMode { get; set; }
        public bool IsMeleeWeapon { get; set; }
        public float PickupTimeoutLength { get; set; }
        public Color LaserColor { get; set; }
        public Func<BinaryWriter, object, byte[]> SerializeCustomData { get; set; }
        public Func<BinaryReader, object> DeserializeCustomData { get; set; }

        /// <summary>
        /// For items using the Block behavior: ID of the registered BlockDef that
        /// this item places. If null or unregistered, falls back to Wood.
        /// </summary>
        public string BlockId { get; set; }

        private InventoryItem.InventoryItemClass _cachedClass;

        public ItemDef()
        {
            DisplayName = "";
            Description1 = "";
            Description2 = "";
            IconTextureName = null;
            MaxStackSize = 100;
            BehaviorClass = null;
            EnemyDamage = 0f;
            EnemyDamageType = DamageType.BLUNT;
            CoolDownTime = TimeSpan.Zero;
            ItemSelfDamagePerUse = 0f;
            UseSoundCue = null;
            PlayerMode = PlayerMode.Generic;
            IsMeleeWeapon = false;
            PickupTimeoutLength = 30f;
            LaserColor = Color.Blue; // default to diamond-blue if mod is a sword
        }

        public InventoryItem.InventoryItemClass CreateClass()
        {
            if (_cachedClass != null)
                return _cachedClass;

            if (BehaviorClass == null)
                throw new InvalidOperationException("BehaviorClass must be set on ItemDef");

            _cachedClass = (InventoryItem.InventoryItemClass)Activator.CreateInstance(
                BehaviorClass, new object[] { this });

            return _cachedClass;
        }
    }
}
