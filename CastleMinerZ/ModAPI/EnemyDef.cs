using DNA.CastleMinerZ.AI;

namespace DNA.CastleMinerZ.ModAPI
{
    /// <summary>Definition data for custom enemies or per-spawn stat overrides.</summary>
    public class EnemyDef
    {
        /// <summary>Vanilla type to clone stats/AI from when registering a custom enemy.</summary>
        public EnemyTypeEnum CloneFrom = EnemyTypeEnum.ZOMBIE_0_0;

        public string DisplayName;

        /// <summary>Mod asset name (mod-id/texture) or vanilla Content texture name.</summary>
        public string TextureAssetName;

        public float? MaxHealth;
        public float? SlowSpeed;
        public float? FastSpeed;
        public float? EmergeSpeed;
        public float? MeleeDamage;

        public EnemyType.FoundInEnum FoundIn = EnemyType.FoundInEnum.ABOVEGROUND;
        public float SpawnWeight = 1f;
    }

    public class EnemySpawnEventArgs
    {
        public EnemyTypeEnum Type;
        public Microsoft.Xna.Framework.Vector3 SpawnPosition;
        public bool Cancel;
        public EnemyDef Overrides;
    }

    public enum EnemyFoundIn
    {
        Aboveground = EnemyType.FoundInEnum.ABOVEGROUND,
        Caves = EnemyType.FoundInEnum.CAVES,
        Hell = EnemyType.FoundInEnum.HELL,
        CrashSite = EnemyType.FoundInEnum.CRASHSITE,
    }
}
