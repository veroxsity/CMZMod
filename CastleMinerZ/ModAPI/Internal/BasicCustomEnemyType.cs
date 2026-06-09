using DNA.CastleMinerZ.AI;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    /// <summary>Clones a vanilla enemy type for mod registration (zombie AI by default).</summary>
    public class BasicCustomEnemyType : ZombieEnemyType
    {
        public BasicCustomEnemyType(EnemyTypeEnum slot, EnemyDef def)
            : base(
                slot,
                EnemyType.ModelNameEnum.ZOMBIE,
                EnemyType.TextureNameEnum.ZOMBIE_0,
                def != null ? def.FoundIn : EnemyType.FoundInEnum.ABOVEGROUND,
                2,
                0.1f)
        {
            if (def == null)
                return;

            EnemyType template = EnemyType.GetEnemyType(def.CloneFrom);
            if (template == null)
                return;

            StartingHealth = template.StartingHealth;
            BaseSlowSpeed = template.BaseSlowSpeed;
            RandomSlowSpeed = template.RandomSlowSpeed;
            BaseFastSpeed = template.BaseFastSpeed;
            BaseRunActivationTime = template.BaseRunActivationTime;
            RandomRunActivationTime = template.RandomRunActivationTime;
            FastJumpSpeed = template.FastJumpSpeed;
            SpawnAnimationSpeed = template.SpawnAnimationSpeed;
            AttackAnimationSpeed = template.AttackAnimationSpeed;
            DieAnimationSpeed = template.DieAnimationSpeed;
            HitAnimationSpeed = template.HitAnimationSpeed;
            HasRunFast = template.HasRunFast;
            SpawnRadius = template.SpawnRadius;
            DiggingMultiplier = template.DiggingMultiplier;
            HardestBlockThatCanBeDug = template.HardestBlockThatCanBeDug;
            ChanceOfBulletStrike = template.ChanceOfBulletStrike;
            FoundIn = def.FoundIn;
            TextureIndex = template.TextureIndex;
            EnemyTexture = template.EnemyTexture;
            ModelName = template.ModelName;
            Scale = template.Scale;
            Facing = template.Facing;
        }
    }
}
