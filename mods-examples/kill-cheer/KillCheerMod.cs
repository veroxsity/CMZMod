using DNA.CastleMinerZ.ModAPI;

namespace ExampleKillCheer
{
    [Mod(Id = "example.kill-cheer", Name = "Kill Cheer", Version = "1.0.0")]
    public static class KillCheerMod
    {
        public static void OnLoad()
        {
            Events.EnemyKilled += OnEnemyKilled;
            ModLog.Info("Kill Cheer active — gold sound on each kill");
        }

        private static void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            Audio.Play("pickupitem", args.DeathPosition);
        }
    }
}
