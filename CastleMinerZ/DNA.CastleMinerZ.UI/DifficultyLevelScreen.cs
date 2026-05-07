using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class DifficultyLevelScreen : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement HardcoreControl;

		private TextRegionElement _descriptionText;

		public DifficultyLevelScreen(CastleMinerZGame game)
			: base(game._largeFont, Color.White, Color.Red, false)
		{
			_game = game;
			SpriteFont largeFont = _game._largeFont;
			SelectSound = "Click";
			ClickSound = "Click";
			Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
			DrawArea = new Rectangle(titleSafeArea.Left, 175, titleSafeArea.Width / 2 - 125, titleSafeArea.Bottom - 175);
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			_descriptionText.Location = new Vector2(titleSafeArea.Center.X + 75, 200f);
			_descriptionText.Size = new Vector2((float)titleSafeArea.Right - _descriptionText.Location.X, (float)titleSafeArea.Bottom - _descriptionText.Location.Y);
			AddMenuItem("No Enemies", "Build freely without enemy attacks.", GameDifficultyTypes.NOENEMIES);
			AddMenuItem("Easy", "Enemies do less damage. Zombies only appear at night. Dragons will not damage your structures.", GameDifficultyTypes.EASY);
			AddMenuItem("Normal", "Enemies appear when you cover fresh ground and swarm at night. Dragons will damage your structures.", GameDifficultyTypes.HARD);
			HardcoreControl = AddMenuItem("Hardcore", "Start with nothing and drop all items when you die.", GameDifficultyTypes.HARDCORE);
			base.SelectedIndex = 2;
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			string text = "Choose a Difficulty Level";
			spriteBatch.Begin();
			spriteBatch.DrawOutlinedText(_game._largeFont, text, new Vector2((float)titleSafeArea.Center.X - _game._largeFont.MeasureString(text).X / 2f, titleSafeArea.Y), Color.White, Color.Black, 2);
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			HardcoreControl.Visible = !_game.InfiniteResourceMode;
			base.OnUpdate(game, gameTime);
		}
	}
}
