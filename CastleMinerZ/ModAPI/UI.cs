using System;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.ModAPI.Internal;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class UI
    {
        public static void AddMainMenuItem(string label, Action onSelect, MainMenuItemOptions opts = null)
        {
            UIRegistry.AddMainMenuItem(label, onSelect, opts);
        }

        public static void RegisterHUDOverlay(string id, Action<HUDDrawArgs> draw)
        {
            UIRegistry.RegisterHUDOverlay(id, draw);
        }

        public static void ShowMessage(string text, TimeSpan duration)
        {
            ShowMessage(text, duration, Color.White);
        }

        public static void ShowMessage(string text, TimeSpan duration, Color color)
        {
            UIRegistry.ShowMessage(text, duration, color);
        }

        public static void PushScreen(Screen screen)
        {
            if (screen == null)
                throw new ArgumentNullException("screen");
            ScreenGroup group = UIRegistry.GetActiveUIGroup();
            if (group == null)
            {
                ModLog.Warn("UI.PushScreen: no active UI group");
                return;
            }
            group.PushScreen(screen);
        }

        public static void PopScreen()
        {
            ScreenGroup group = UIRegistry.GetActiveUIGroup();
            if (group == null)
            {
                ModLog.Warn("UI.PopScreen: no active UI group");
                return;
            }
            group.PopScreen();
        }

        public static SpriteFont GetSmallFont()
        {
            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null)
                return null;
            return game._smallFont;
        }

        public static SpriteFont GetMedFont()
        {
            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null)
                return null;
            return game._medFont;
        }
    }
}
