using System;
using System.Collections.Generic;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    internal class PendingMainMenuItem
    {
        public string Id;
        public string Label;
        public Action OnSelect;
        public bool ShowOnTitle;
    }

    internal class HUDOverlayEntry
    {
        public string Id;
        public Action<HUDDrawArgs> Draw;
    }

    internal class ToastEntry
    {
        public string Text;
        public Color Color;
        public OneShotTimer Timer;
    }

    public static class UIRegistry
    {
        private static List<PendingMainMenuItem> _pendingMenuItems = new List<PendingMainMenuItem>();
        private static List<HUDOverlayEntry> _hudOverlays = new List<HUDOverlayEntry>();
        private static List<ToastEntry> _toasts = new List<ToastEntry>();

        public static void AddMainMenuItem(string label, Action onSelect, MainMenuItemOptions opts)
        {
            if (label == null)
                throw new ArgumentNullException("label");
            if (onSelect == null)
                throw new ArgumentNullException("onSelect");
            string modId = ModRegistry.CurrentLoadingModId ?? "unknown";
            _pendingMenuItems.Add(new PendingMainMenuItem
            {
                Id = modId + "." + label,
                Label = label,
                OnSelect = onSelect,
                ShowOnTitle = opts != null && opts.ShowOnTitle,
            });
        }

        public static void RegisterHUDOverlay(string id, Action<HUDDrawArgs> draw)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (draw == null)
                throw new ArgumentNullException("draw");
            for (int i = 0; i < _hudOverlays.Count; i++)
            {
                if (_hudOverlays[i].Id == id)
                {
                    _hudOverlays[i].Draw = draw;
                    return;
                }
            }
            _hudOverlays.Add(new HUDOverlayEntry { Id = id, Draw = draw });
        }

        public static void ShowMessage(string text, TimeSpan duration, Color color)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (duration <= TimeSpan.Zero)
                duration = TimeSpan.FromSeconds(2);
            _toasts.Add(new ToastEntry
            {
                Text = text,
                Color = color,
                Timer = new OneShotTimer(duration),
            });
        }

        public static void ApplyMainMenuItems(MainMenu menu)
        {
            if (menu == null || _pendingMenuItems.Count == 0)
                return;
            for (int i = 0; i < _pendingMenuItems.Count; i++)
            {
                PendingMainMenuItem item = _pendingMenuItems[i];
                var tag = new ModMenuItemTag(item.Id, item.OnSelect);
                menu.AddMenuItem(item.Label, tag);
            }
        }

        public static bool TryHandleMainMenuSelection(MenuItemElement item)
        {
            ModMenuItemTag tag = item.Tag as ModMenuItemTag;
            if (tag == null)
                return false;
            try
            {
                tag.OnSelect();
            }
            catch (Exception ex)
            {
                ModLog.Error("Main menu item '" + tag.Id + "' failed: " + ex.Message);
            }
            return true;
        }

        public static void DrawHUDOverlays(GraphicsDevice device, SpriteBatch spriteBatch,
            GameTime gameTime, InGameHUD hud)
        {
            if (_hudOverlays.Count == 0 || hud == null || hud.LocalPlayer == null)
                return;

            var args = new HUDDrawArgs
            {
                Device = device,
                SpriteBatch = spriteBatch,
                GameTime = gameTime,
                Bounds = device.Viewport.TitleSafeArea,
                Player = hud.LocalPlayer,
                BelowDistanceReadoutY = ComputeBelowDistanceReadoutY(device.Viewport.TitleSafeArea),
            };

            for (int i = 0; i < _hudOverlays.Count; i++)
            {
                try
                {
                    _hudOverlays[i].Draw(args);
                }
                catch (Exception ex)
                {
                    ModLog.Error("HUD overlay '" + _hudOverlays[i].Id + "' failed: " + ex.Message);
                }
            }
        }

        public static void DrawToasts(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_toasts.Count == 0)
                return;

            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null || game._medFont == null)
                return;

            TimeSpan elapsed = gameTime.ElapsedGameTime;
            for (int i = _toasts.Count - 1; i >= 0; i--)
            {
                _toasts[i].Timer.Update(elapsed);
                if (_toasts[i].Timer.Expired)
                    _toasts.RemoveAt(i);
            }
            if (_toasts.Count == 0)
                return;

            Rectangle bounds = device.Viewport.TitleSafeArea;
            spriteBatch.Begin();
            float y = bounds.Top + 40f;
            for (int i = 0; i < _toasts.Count; i++)
            {
                ToastEntry toast = _toasts[i];
                float alpha = 1f;
                if (toast.Timer.PercentComplete > 0.75f)
                    alpha = (1f - toast.Timer.PercentComplete) * 4f;
                Color textColor = new Color(toast.Color.R, toast.Color.G, toast.Color.B,
                    (byte)(toast.Color.A * alpha));
                Color shadow = new Color(0, 0, 0, (byte)(255f * alpha));
                Vector2 size = game._medFont.MeasureString(toast.Text);
                Vector2 pos = new Vector2(bounds.Center.X - size.X / 2f, y);
                spriteBatch.DrawString(game._medFont, toast.Text, pos + new Vector2(1f, 1f), shadow);
                spriteBatch.DrawString(game._medFont, toast.Text, pos, textColor);
                y += size.Y + 4f;
            }
            spriteBatch.End();
        }

        public static ScreenGroup GetActiveUIGroup()
        {
            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null)
                return null;
            if (game.GameScreen != null)
                return game.GameScreen._uiGroup;
            if (game.FrontEnd != null)
                return game.FrontEnd._uiGroup;
            return null;
        }

        private static float ComputeBelowDistanceReadoutY(Rectangle titleSafeArea)
        {
            CastleMinerZGame game = CastleMinerZGame.Instance;
            if (game == null || game._medFont == null)
                return titleSafeArea.Top + 10f;
            // Match InGameHUD layout: label row + value row, then a small gap.
            Vector2 labelSize = game._medFont.MeasureString("Distance - Max");
            return titleSafeArea.Top + labelSize.Y * 2f + 6f;
        }
    }
}
