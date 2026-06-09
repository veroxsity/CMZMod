using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI;

namespace ExampleFastZombies
{
    public class FastZombieType : ZombieEnemyType
    {
        public FastZombieType(EnemyTypeEnum slot)
            : base(slot, ModelNameEnum.ZOMBIE, TextureNameEnum.ZOMBIE_2, FoundInEnum.ABOVEGROUND, 2, 0.1f)
        {
            StartingHealth = 12f;
            BaseSlowSpeed = 5f;
            RandomSlowSpeed = 1f;
            BaseFastSpeed = 12f;
            BaseRunActivationTime = 2f;
            RandomRunActivationTime = 0.5f;
        }
    }

    [Mod(Id = "example.fast-zombies", Name = "Fast Zombies", Version = "1.0.0")]
    public static class FastZombiesMod
    {
        public static void OnLoad()
        {
            Entities.RegisterCustom<FastZombieType>("example.fast-zombie", new EnemyDef
            {
                FoundIn = EnemyType.FoundInEnum.ABOVEGROUND,
                SpawnWeight = 1f,
            });
            ModLog.Info("Fast Zombies active — fast custom zombies mixed into surface spawns");
        }
    }
}
