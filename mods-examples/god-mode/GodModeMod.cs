using DNA.CastleMinerZ.ModAPI;

namespace ExampleGodMode
{
    [Mod(Id = "example.god-mode", Name = "God Mode", Version = "1.0.0")]
    public static class GodModeMod
    {
        public static void OnLoad()
        {
            Events.PlayerTakeDamage += OnPlayerTakeDamage;
            ModLog.Info("God Mode active — all damage cancelled");
        }

        private static void OnPlayerTakeDamage(PlayerDamageEventArgs args)
        {
            args.Cancel = true;
            ModLog.Info(string.Format("God Mode: blocked {0} damage from {1}",
                args.DamageAmount, args.DamageSource));
        }
    }
}
