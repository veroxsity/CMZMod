using DNA.CastleMinerZ.ModAPI;
using Microsoft.Xna.Framework;

namespace ExampleCoordsHUD
{
    [Mod(Id = "example.coords-hud", Name = "Coords HUD", Version = "1.0.0")]
    public static class CoordsHUDMod
    {
        public static void OnLoad()
        {
            UI.RegisterHUDOverlay("coords", DrawCoords);
            ModLog.Info("Coords HUD active — position shown below distance readout");
        }

        private static void DrawCoords(HUDDrawArgs args)
        {
            if (args.Player == null)
                return;
            Vector3 pos = args.Player.WorldPosition;
            string text = string.Format("X:{0:F0}  Y:{1:F0}  Z:{2:F0}", pos.X, pos.Y, pos.Z);
            var font = UI.GetSmallFont();
            if (font == null)
                return;

            Vector2 size = font.MeasureString(text);
            // Right-align with vanilla distance HUD (22px inset matches InGameHUD).
            Vector2 pos2D = new Vector2(args.Bounds.Right - size.X - 22f, args.BelowDistanceReadoutY);

            args.SpriteBatch.Begin();
            args.SpriteBatch.DrawString(font, text, pos2D + new Vector2(1f, 1f), Color.Black);
            args.SpriteBatch.DrawString(font, text, pos2D, Color.White);
            args.SpriteBatch.End();
        }
    }
}
