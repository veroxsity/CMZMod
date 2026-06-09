using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI;

namespace ExampleTougherZombies
{
    [Mod(Id = "example.tougher-zombies", Name = "Tougher Zombies", Version = "1.0.0")]
    public static class TougherZombiesMod
    {
        public static void OnLoad()
        {
            for (int i = (int)EnemyTypeEnum.ZOMBIE_0_0; i <= (int)EnemyTypeEnum.ZOMBIE_2_4; i++)
                Entities.SetMaxHealth((EnemyTypeEnum)i, 50f);

            ModLog.Info("Tougher Zombies active — surface zombies have 50 HP");
        }
    }
}
