using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Events
    {
        public static event Action<PlayerDamageEventArgs> PlayerTakeDamage;
        public static event Action<BlockDestroyedEventArgs> BlockDestroyed;
        public static event Action<PlayerMinedBlockEventArgs> PlayerMinedBlock;
        public static event Action<GameTickEventArgs> GameTick;
        public static event Action<PlayerRespawnEventArgs> PlayerRespawn;
        public static event Action<ItemCraftedEventArgs> ItemCrafted;
        public static event Action<EnemyKilledEventArgs> EnemyKilled;

        internal static void FirePlayerTakeDamage(PlayerDamageEventArgs args)
        {
            var handler = PlayerTakeDamage;
            if (handler != null)
                FireEvent(PlayerTakeDamage, args);
        }

        internal static void FireBlockDestroyed(BlockDestroyedEventArgs args)
        {
            var handler = BlockDestroyed;
            if (handler != null)
                FireEvent(BlockDestroyed, args);
        }

        internal static void FirePlayerMinedBlock(PlayerMinedBlockEventArgs args)
        {
            var handler = PlayerMinedBlock;
            if (handler != null)
                FireEvent(PlayerMinedBlock, args);
        }

        internal static void FireGameTick(GameTickEventArgs args)
        {
            var handler = GameTick;
            if (handler != null)
                FireEvent(GameTick, args);
        }

        internal static void FirePlayerRespawn(PlayerRespawnEventArgs args)
        {
            var handler = PlayerRespawn;
            if (handler != null)
                FireEvent(PlayerRespawn, args);
        }

        internal static void FireItemCrafted(ItemCraftedEventArgs args)
        {
            var handler = ItemCrafted;
            if (handler != null)
                FireEvent(ItemCrafted, args);
        }

        internal static void FireEnemyKilled(EnemyKilledEventArgs args)
        {
            var handler = EnemyKilled;
            if (handler != null)
                FireEvent(EnemyKilled, args);
        }

        private static void FireEvent<T>(Action<T> eventHandler, T args)
        {
            foreach (Action<T> handler in eventHandler.GetInvocationList())
            {
                try
                {
                    handler(args);
                }
                catch (Exception ex)
                {
                    ModLog.Error("Event handler failed: " + ex.Message);
                }
            }
        }
    }

    public class PlayerDamageEventArgs
    {
        public float DamageAmount;
        public Vector3 DamageSource;
        public Player Player;
        public bool Cancel;
    }

    public class BlockDestroyedEventArgs
    {
        public IntVector3 BlockPosition;
        public BlockTypeEnum BlockType;
        public Player DestroyedBy;
    }

    public class PlayerMinedBlockEventArgs
    {
        /// <summary>The block that was mined.</summary>
        public BlockTypeEnum BlockType;
        /// <summary>World position of the mined block.</summary>
        public IntVector3 BlockPosition;
        /// <summary>The item vanilla mining would drop for this block+tool combination. May be null (e.g. low-tier pickaxe on ore).</summary>
        public InventoryItem Drop;
        /// <summary>The tool the player used.</summary>
        public InventoryItem Tool;
        /// <summary>The player who mined the block.</summary>
        public Player Player;
    }

    public class GameTickEventArgs
    {
        public GameTime GameTime;
    }

    public class PlayerRespawnEventArgs
    {
        public Player Player;
    }

    public class ItemCraftedEventArgs
    {
        public Receipe Recipe;
        public InventoryItem Result;
    }

    public class EnemyKilledEventArgs
    {
        public BaseZombie Enemy;
        public InventoryItemIDs KillingItemID;
        /// <summary>Mod item string ID when the killing weapon is a mod item (uses BareHands enum slot).</summary>
        public string KillingModItemId;
        public byte ShooterID;
        public Vector3 DeathPosition;
        public string EnemyTypeName;

        public string GetKillingWeaponName()
        {
            if (KillingModItemId != null)
                return KillingModItemId;
            return KillingItemID.ToString();
        }
    }
}
