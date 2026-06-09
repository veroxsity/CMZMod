using DNA.CastleMinerZ.ModAPI;
using Microsoft.Xna.Framework;

namespace ExampleKillCounter
{
    [Mod(Id = "example.kill-counter", Name = "Kill Counter", Version = "1.0.0")]
    public static class KillCounterMod
    {
        private const string ModId = "example.kill-counter";
        private static int _totalKills;
        private static string _displayText = "Kills: 0";

        public static void OnLoad()
        {
            _totalKills = Data.GetWorldInt(ModId, "totalKills", 0);
            _displayText = "Kills: " + _totalKills;
            ModLog.Info(string.Format("Kill Counter active — {0} total kills (loaded from save)", _totalKills));

            Events.EnemyKilled += OnEnemyKilled;
            UI.RegisterHUDOverlay("kill-counter", DrawKillCounter);
        }

        private static void OnEnemyKilled(EnemyKilledEventArgs args)
        {
            _totalKills++;
            Data.SetWorldInt(ModId, "totalKills", _totalKills);
            _displayText = "Kills: " + _totalKills;

            string enemyName = args.EnemyTypeName ?? "Unknown";
            string weaponName = args.GetKillingWeaponName();
            ModLog.Info(string.Format("Kill #{0}: {1} killed with {2} ({3:F0} max HP)",
                _totalKills, enemyName, weaponName, args.GetMaxHealth()));
        }

        private static void DrawKillCounter(HUDDrawArgs args)
        {
            var font = UI.GetSmallFont();
            if (font == null)
                return;
            var pos = new Vector2(args.Bounds.Left + 10, args.Bounds.Top + 200);
            args.SpriteBatch.Begin();
            args.SpriteBatch.DrawString(font, _displayText, pos + new Vector2(1, 1), Color.Black);
            args.SpriteBatch.DrawString(font, _displayText, pos, Color.Yellow);
            args.SpriteBatch.End();
        }
    }
}
