using System.Text;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.ModAPI;
using DNA.CastleMinerZ.ModAPI.Internal;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;

namespace ExampleModInfo
{
    [Mod(Id = "example.mod-info", Name = "Mod Info", Version = "1.0.0")]
    public static class ModInfoMod
    {
        public static void OnLoad()
        {
            UI.AddMainMenuItem("Loaded Mods", OpenModInfoScreen,
                new MainMenuItemOptions { ShowOnTitle = true });
            ModLog.Info("Mod Info active — see Loaded Mods on title menu");
        }

        private static void OpenModInfoScreen()
        {
            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null)
                return;
            UI.PushScreen(new ModInfoScreen(game));
        }
    }

    public class ModInfoScreen : MenuScreen
    {
        public ModInfoScreen(CastleMinerZGame game)
            : base(game._largeFont, Color.White, Color.Red, false)
        {
            ClickSound = "Click";
            SelectSound = "Click";
            Rectangle titleSafeArea = game.GraphicsDevice.Viewport.TitleSafeArea;
            DrawArea = new Rectangle(titleSafeArea.Left, 175,
                titleSafeArea.Width / 2 - 125, titleSafeArea.Bottom - 175);
            HorizontalAlignment = HorizontalAlignmentTypes.Right;
            VerticalAlignment = VerticalAlignmentTypes.Top;
            LineSpacing = -10;

            var loaded = ModRegistry.LoadedModIds;
            if (loaded.Count == 0)
            {
                AddMenuItem("(no mods loaded)", "noop");
            }
            else
            {
                for (int i = 0; i < loaded.Count; i++)
                    AddMenuItem(loaded[i], "noop");
            }
            AddMenuItem("Back", "back");
        }

        protected override void OnMenuItemSelected(MenuItemElement selectedControl)
        {
            if (selectedControl.Tag as string == "back")
                UI.PopScreen();
        }
    }
}
