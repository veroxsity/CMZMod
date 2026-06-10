using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class EntityRegistry
    {
        public const int ModEnemySlotStart = 200;
        public const int ModEnemySlotEnd = 255;

        private const float NoMeleeDamageOverride = -1f;

        private static bool[] _spawnEnabled;
        private static float[] _spawnWeights;
        private static float[] _meleeDamageOverrides;
        private static Dictionary<string, EnemyTypeEnum> _customIds;
        private static List<SpawnPoolEntry> _spawnPool;
        private static int _nextModSlot = ModEnemySlotStart;
        private static Random _rnd = new Random();

        private struct SpawnPoolEntry
        {
            public EnemyTypeEnum Type;
            public EnemyType.FoundInEnum FoundIn;
            public float Weight;
        }

        static EntityRegistry()
        {
            EnsureArrays();
        }

        private static void EnsureArrays()
        {
            int size = ModEnemySlotEnd + 1;
            if (_spawnEnabled != null && _spawnEnabled.Length >= size)
                return;
            bool[] enabled = new bool[size];
            float[] weights = new float[size];
            float[] damages = new float[size];
            for (int i = 0; i < size; i++)
            {
                enabled[i] = true;
                weights[i] = 1f;
                damages[i] = NoMeleeDamageOverride;
            }
            if (_spawnEnabled != null)
            {
                for (int i = 0; i < _spawnEnabled.Length && i < size; i++)
                {
                    enabled[i] = _spawnEnabled[i];
                    weights[i] = _spawnWeights[i];
                    damages[i] = _meleeDamageOverrides[i];
                }
            }
            _spawnEnabled = enabled;
            _spawnWeights = weights;
            _meleeDamageOverrides = damages;
            if (_customIds == null)
                _customIds = new Dictionary<string, EnemyTypeEnum>();
            if (_spawnPool == null)
                _spawnPool = new List<SpawnPoolEntry>();
        }

        public static void SetMaxHealth(EnemyTypeEnum type, float health)
        {
            EnemyType enemy = GetMutableType(type);
            if (enemy != null)
                enemy.StartingHealth = health;
        }

        public static void SetMoveSpeed(EnemyTypeEnum type, float slowSpeed, float fastSpeed)
        {
            EnemyType enemy = GetMutableType(type);
            if (enemy == null)
                return;
            enemy.BaseSlowSpeed = slowSpeed;
            enemy.BaseFastSpeed = fastSpeed;
        }

        public static void SetEmergeSpeed(EnemyTypeEnum type, float speed)
        {
            EnemyType enemy = GetMutableType(type);
            if (enemy != null)
                enemy.SpawnAnimationSpeed = speed;
        }

        public static void SetDamage(EnemyTypeEnum type, float damage)
        {
            EnsureArrays();
            int idx = (int)type;
            if (idx >= 0 && idx < _meleeDamageOverrides.Length)
                _meleeDamageOverrides[idx] = damage;
        }

        public static void SetSpawnWeight(EnemyTypeEnum type, float weight)
        {
            EnsureArrays();
            int idx = (int)type;
            if (idx >= 0 && idx < _spawnWeights.Length)
                _spawnWeights[idx] = Math.Max(0f, weight);
        }

        public static void SetSpawnEnabled(EnemyTypeEnum type, bool enabled)
        {
            EnsureArrays();
            int idx = (int)type;
            if (idx >= 0 && idx < _spawnEnabled.Length)
                _spawnEnabled[idx] = enabled;
        }

        public static bool IsSpawnEnabled(EnemyTypeEnum type)
        {
            EnsureArrays();
            int idx = (int)type;
            if (idx < 0 || idx >= _spawnEnabled.Length)
                return true;
            return _spawnEnabled[idx];
        }

        public static float GetMeleeDamageOverride(EnemyTypeEnum type)
        {
            EnsureArrays();
            int idx = (int)type;
            if (idx < 0 || idx >= _meleeDamageOverrides.Length)
                return NoMeleeDamageOverride;
            return _meleeDamageOverrides[idx];
        }

        public static EnemyTypeEnum RegisterCustom<T>(string id, EnemyDef def) where T : EnemyType
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (def == null)
                throw new ArgumentNullException("def");
            if (_customIds.ContainsKey(id))
                throw new ArgumentException("Custom enemy ID already registered: " + id);
            if (_nextModSlot > ModEnemySlotEnd)
                throw new InvalidOperationException("Mod enemy limit reached (56 slots max)");

            EnemyTypeEnum slot = (EnemyTypeEnum)_nextModSlot;
            _nextModSlot++;

            EnemyType instance = CreateCustomInstance<T>(slot, def);
            ApplyDefToType(instance, def);
            EnemyType.RegisterModEnemy(instance);

            _customIds[id] = slot;
            if (def.SpawnWeight > 0f)
            {
                _spawnPool.Add(new SpawnPoolEntry
                {
                    Type = slot,
                    FoundIn = def.FoundIn,
                    Weight = def.SpawnWeight,
                });
            }

            ModLog.Info(string.Format("Registered custom enemy '{0}' -> slot {1}", id, (int)slot));
            return slot;
        }

        private static EnemyType CreateCustomInstance<T>(EnemyTypeEnum slot, EnemyDef def) where T : EnemyType
        {
            try
            {
                return (EnemyType)Activator.CreateInstance(typeof(T), slot);
            }
            catch (MissingMethodException)
            {
                BasicCustomEnemyType basic = new BasicCustomEnemyType(slot, def);
                return basic;
            }
        }

        public static void ApplyDefToType(EnemyType type, EnemyDef def)
        {
            if (type == null || def == null)
                return;
            if (def.MaxHealth.HasValue)
                type.StartingHealth = def.MaxHealth.Value;
            if (def.SlowSpeed.HasValue)
                type.BaseSlowSpeed = def.SlowSpeed.Value;
            if (def.FastSpeed.HasValue)
                type.BaseFastSpeed = def.FastSpeed.Value;
            if (def.EmergeSpeed.HasValue)
                type.SpawnAnimationSpeed = def.EmergeSpeed.Value;
            if (def.MeleeDamage.HasValue)
                SetDamage(type.EType, def.MeleeDamage.Value);
            if (!string.IsNullOrEmpty(def.TextureAssetName))
            {
                Microsoft.Xna.Framework.Graphics.Texture2D tex = Assets.LoadTexture(def.TextureAssetName);
                if (tex != null)
                    type.EnemyTexture = tex;
            }
        }

        public static EnemyTypeEnum ResolveSpawnType(EnemyTypeEnum vanilla, EnemyType.FoundInEnum foundIn)
        {
            EnemyTypeEnum custom = TryPickCustom(foundIn);
            if (custom != EnemyTypeEnum.COUNT)
                return custom;
            return FilterEnabled(vanilla, foundIn);
        }

        private static EnemyTypeEnum TryPickCustom(EnemyType.FoundInEnum foundIn)
        {
            float poolWeight = 0f;
            for (int i = 0; i < _spawnPool.Count; i++)
            {
                SpawnPoolEntry entry = _spawnPool[i];
                if (entry.FoundIn == foundIn && IsSpawnEnabled(entry.Type))
                    poolWeight += entry.Weight;
            }
            if (poolWeight <= 0f)
                return EnemyTypeEnum.COUNT;

            // Vanilla pool weight is fixed at 1.0 — modder SpawnWeight is relative to that.
            float total = 1f + poolWeight;
            float roll = (float)_rnd.NextDouble() * total;
            if (roll < 1f)
                return EnemyTypeEnum.COUNT;

            roll -= 1f;
            for (int i = 0; i < _spawnPool.Count; i++)
            {
                SpawnPoolEntry entry = _spawnPool[i];
                if (entry.FoundIn != foundIn || !IsSpawnEnabled(entry.Type))
                    continue;
                roll -= entry.Weight;
                if (roll <= 0f)
                    return entry.Type;
            }
            return EnemyTypeEnum.COUNT;
        }

        private static EnemyTypeEnum FilterEnabled(EnemyTypeEnum chosen, EnemyType.FoundInEnum foundIn)
        {
            if (IsSpawnEnabled(chosen))
                return chosen;

            for (int i = 0; i < _spawnPool.Count; i++)
            {
                SpawnPoolEntry entry = _spawnPool[i];
                if (entry.FoundIn == foundIn && IsSpawnEnabled(entry.Type))
                    return entry.Type;
            }

            int start;
            int end;
            GetVanillaRange(foundIn, out start, out end);
            for (int i = start; i <= end; i++)
            {
                EnemyTypeEnum candidate = (EnemyTypeEnum)i;
                if (IsSpawnEnabled(candidate))
                    return candidate;
            }
            return chosen;
        }

        private static void GetVanillaRange(EnemyType.FoundInEnum foundIn, out int start, out int end)
        {
            switch (foundIn)
            {
                case EnemyType.FoundInEnum.CAVES:
                    start = (int)EnemyTypeEnum.SKEL_0_0;
                    end = (int)EnemyTypeEnum.SKEL_AXES_1_2;
                    break;
                case EnemyType.FoundInEnum.CRASHSITE:
                    start = (int)EnemyTypeEnum.ALIEN;
                    end = (int)EnemyTypeEnum.ALIEN;
                    break;
                default:
                    start = (int)EnemyTypeEnum.ZOMBIE_0_0;
                    end = (int)EnemyTypeEnum.ARCHER_1_2;
                    break;
            }
        }

        public static bool FireSpawnHook(ref EnemyTypeEnum type, Vector3 spawnPosition, ref EnemyDef overrides)
        {
            return Entities.FireSpawnHook(ref type, spawnPosition, ref overrides);
        }

        public static void ApplySpawnOverrides(BaseZombie zombie, EnemyDef overrides)
        {
            if (zombie == null || overrides == null)
                return;
            if (overrides.MaxHealth.HasValue)
                zombie.Health = overrides.MaxHealth.Value;
            if (overrides.SlowSpeed.HasValue)
            {
                zombie.InitPkg.SlowSpeed = overrides.SlowSpeed.Value;
                zombie.CurrentSpeed = overrides.SlowSpeed.Value;
            }
            if (overrides.FastSpeed.HasValue)
                zombie.InitPkg.FastSpeed = overrides.FastSpeed.Value;
            if (overrides.MeleeDamage.HasValue)
                SetDamage(zombie.EType.EType, overrides.MeleeDamage.Value);
        }

        private static EnemyType GetMutableType(EnemyTypeEnum type)
        {
            if (EnemyType.Types == null)
                return null;
            int idx = (int)type;
            if (idx < 0 || idx >= EnemyType.Types.Length)
                return null;
            return EnemyType.Types[idx];
        }
    }
}
