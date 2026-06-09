using System;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI.Internal;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Entities
    {
        public static event Action<EnemySpawnEventArgs> EnemySpawning;

        public static void SetMaxHealth(EnemyTypeEnum type, float health)
        {
            EntityRegistry.SetMaxHealth(type, health);
        }

        public static void SetMoveSpeed(EnemyTypeEnum type, float slowSpeed, float fastSpeed)
        {
            EntityRegistry.SetMoveSpeed(type, slowSpeed, fastSpeed);
        }

        public static void SetMoveSpeed(EnemyTypeEnum type, float speed)
        {
            EntityRegistry.SetMoveSpeed(type, speed, speed * 2f);
        }

        public static void SetEmergeSpeed(EnemyTypeEnum type, float speed)
        {
            EntityRegistry.SetEmergeSpeed(type, speed);
        }

        public static void SetDamage(EnemyTypeEnum type, float damage)
        {
            EntityRegistry.SetDamage(type, damage);
        }

        public static void SetSpawnWeight(EnemyTypeEnum type, float weight)
        {
            EntityRegistry.SetSpawnWeight(type, weight);
        }

        public static void SetSpawnEnabled(EnemyTypeEnum type, bool enabled)
        {
            EntityRegistry.SetSpawnEnabled(type, enabled);
        }

        /// <summary>
        /// Register a custom enemy type in mod enum slots 200-255.
        /// T must expose a constructor taking EnemyTypeEnum, or BasicCustomEnemyType is used.
        /// </summary>
        public static EnemyTypeEnum RegisterCustom<T>(string id, EnemyDef def) where T : EnemyType
        {
            return EntityRegistry.RegisterCustom<T>(id, def);
        }

        internal static bool FireSpawnHook(ref EnemyTypeEnum type, Microsoft.Xna.Framework.Vector3 spawnPosition, ref EnemyDef overrides)
        {
            var handler = EnemySpawning;
            if (handler == null)
                return true;

            var args = new EnemySpawnEventArgs
            {
                Type = type,
                SpawnPosition = spawnPosition,
                Overrides = overrides,
            };

            foreach (Action<EnemySpawnEventArgs> cb in handler.GetInvocationList())
            {
                try
                {
                    cb(args);
                }
                catch (Exception ex)
                {
                    ModLog.Error("EnemySpawning handler failed: " + ex.Message);
                }
            }

            type = args.Type;
            overrides = args.Overrides;
            return !args.Cancel;
        }
    }
}
