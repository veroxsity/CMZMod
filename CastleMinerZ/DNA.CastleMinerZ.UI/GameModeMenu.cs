using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class GameModeMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		private TextRegionElement _descriptionText;

		private MenuItemElement SurvivalControl;

		private MenuItemElement DragonEnduranceControl;

		private MenuItemElement CreativeControl;

		public GameModeMenu(CastleMinerZGame game)
			: base(game._largeFont, Color.White, Color.Red, false)
		{
			_game = game;
			SpriteFont largeFont = _game._largeFont;
			SelectSound = "Click";
			ClickSound = "Click";
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
			DrawArea = new Rectangle(titleSafeArea.Left, 175, titleSafeArea.Width / 2 - 125, titleSafeArea.Bottom - 175);
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			_descriptionText.Location = new Vector2(titleSafeArea.Center.X + 75, 200f);
			_descriptionText.Size = new Vector2((float)titleSafeArea.Right - _descriptionText.Location.X, (float)titleSafeArea.Bottom - _descriptionText.Location.Y);
			AddMenuItem("Endurance", "Earn awards by seeing how far you can travel from the start point. Changes to the world will not be saved in this mode.", GameModeTypes.Endurance);
			SurvivalControl = AddMenuItem("Survival", "Mine resources and build your fortress while defending yourself from the undead horde. Your creations will be saved in this mode. You can play with or without enemies.", GameModeTypes.Survival);
			DragonEnduranceControl = AddMenuItem("Dragon Endurance", "Fend off wave after wave of dragons. Unlock this mode by defeating the undead dragon in Endurance Mode. Your creations will be saved in this mode.", GameModeTypes.DragonEndurance);
			CreativeControl = AddMenuItem("Creative", "Build structures with unlimited resources. You can play with or without enemies. Unlock this mode with a promotional code from the original CastleMiner game.", GameModeTypes.Creative);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			string text = "Choose a Game Mode";
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(_game._largeFont, text, new Vector2((float)titleSafeArea.Center.X - _game._largeFont.MeasureString(text).X / 2f, titleSafeArea.Y), Color.White, Color.Black, 2);
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			SignedInGamer currentGamer = Screen.CurrentGamer;
			SurvivalControl.TextColor = ((!Guide.IsTrialMode) ? Color.White : Color.Gray);
			CreativeControl.TextColor = ((!Guide.IsTrialMode && CastleMinerZGame.Instance.FrontEnd.PromoCodes[5].Redeemed) ? Color.White : Color.Gray);
			DragonEnduranceControl.TextColor = ((!Guide.IsTrialMode && (CastleMinerZGame.Instance.PlayerStats.UndeadDragonKills > 0 || _game.PlayerStats.v1Player || CastleMinerZGame.Instance.FrontEnd.PromoCodes[4].Redeemed)) ? Color.White : Color.Gray);
			base.OnUpdate(game, gameTime);
		}
	}
}
