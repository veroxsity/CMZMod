using DNA.CastleMinerZ.ModAPI;

namespace ExampleKillCounter
{
    [Mod(Id = "example.kill-counter", Name = "Kill Counter", Version = "1.0.0")]
    public static class KillCounterMod
    {
        private const string ModId = "example.kill-counter";
        private static int _totalKills;

        public static void OnLoad()
        {
            // Restore previous kill count from world save data
            _totalKills = Data.GetWorldInt(ModId, "totalKills", 0);
            ModLog.Info(string.Format("Kill Counter active — {0} total kills (loaded from save)", _totalKills));

            Events.EnemyKilled += OnEnemyKilled;
        }

        private static void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            _totalKills++;
            Data.SetWorldInt(ModId, "totalKills", _totalKills);

            string enemyName = args.EnemyTypeName ?? "Unknown";
            string weaponName = args.GetKillingWeaponName();
            ModLog.Info(string.Format("Kill #{0}: {1} killed with {2}",
                _totalKills, enemyName, weaponName));
        }
    }
}
