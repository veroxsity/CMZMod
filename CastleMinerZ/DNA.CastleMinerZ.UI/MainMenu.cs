using System.Text;
using DNA.Drawing;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class MainMenu : MenuScreen
	{
		// === MOD BUILD MARKER ===
		// Renders on the title screen so we can confirm at a glance that this
		// is our custom build, not vanilla retail. Change for each test build.
		private const string BuildTag = "CMZMOD DEV BUILD";

		private CastleMinerZGame _game;

		private MenuItemElement purchaseControl;

		private MenuItemElement achievementControl;

		private MenuItemElement reedemControl;

		private MenuItemElement hostOnlineControl;

		private MenuItemElement joinOnlineControl;

		private StringBuilder builder = new StringBuilder();

		public MainMenu(CastleMinerZGame game)
			: base(game._largeFont, Color.White, Color.Red, false)
		{
			SpriteFont largeFont = game._largeFont;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
			DrawArea = new Rectangle(titleSafeArea.Left, 175, titleSafeArea.Width / 2 - 125, titleSafeArea.Bottom - 175);
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			hostOnlineControl = AddMenuItem("Host Online", MainMenuItems.HostOnline);
			joinOnlineControl = AddMenuItem("Join Online", MainMenuItems.JoinOnline);
			AddMenuItem("Play Offline", MainMenuItems.PlayOffline);
			purchaseControl = AddMenuItem("Purchase", MainMenuItems.Purchase);
			achievementControl = AddMenuItem("Awards", MainMenuItems.Awards);
			reedemControl = AddMenuItem("Redeem Code", MainMenuItems.Redeem);
			AddMenuItem("Options", MainMenuItems.Options);
			AddMenuItem("Exit", MainMenuItems.Quit);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			spriteBatch.Begin();

			// Draw the mod build marker top-left of the title-safe area.
			SpriteFont markerFont = _game._largeFont;
			Vector2 markerPos = new Vector2(titleSafeArea.Left + 10f, titleSafeArea.Top + 10f);
			spriteBatch.DrawString(markerFont, BuildTag, markerPos + new Vector2(2f, 2f), Color.Black);
			spriteBatch.DrawString(markerFont, BuildTag, markerPos, Color.Lime);

			int num = 512;
			int height = _game.Logo.Height * num / _game.Logo.Width;
			_game.Logo.Draw(spriteBatch, new Rectangle(titleSafeArea.Center.X - num / 2, 25, num, height), Color.White);
			Rectangle rectangle = new Rectangle(titleSafeArea.Center.X, titleSafeArea.Top, titleSafeArea.Width / 2, titleSafeArea.Height);
			spriteBatch.Draw(_game._uiSprites["ALW2Box"], new Vector2(rectangle.Center.X - _game._uiSprites["ALW2Box"].Width / 2, 175f), Color.White);
			string text = "Purchase Avatar Warfare";
			Vector2 vector = _game._medLargeFont.MeasureString(text);
			float num2 = 175 + _game._uiSprites["ALW2Box"].Height + 10;
			spriteBatch.DrawOutlinedText(_game._medLargeFont, text, new Vector2((float)rectangle.Center.X - vector.X / 2f, num2), Color.White, Color.Black, 2);
			text = "For a Special Unlock Code";
			vector = _game._medLargeFont.MeasureString(text);
			num2 += vector.Y;
			spriteBatch.DrawOutlinedText(_game._medLargeFont, text, new Vector2((float)rectangle.Center.X - vector.X / 2f, num2), Color.White, Color.Black, 2);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			bool flag = Screen.CurrentGamer.Privileges.AllowOnlineSessions && !Guide.IsTrialMode;
			SignedInGamer currentGamer = Screen.CurrentGamer;
			purchaseControl.Visible = Guide.IsTrialMode;
			achievementControl.Visible = !Guide.IsTrialMode;
			reedemControl.TextColor = (Guide.IsTrialMode ? Color.Gray : Color.White);
			hostOnlineControl.TextColor = (flag ? Color.White : Color.Gray);
			joinOnlineControl.TextColor = (flag ? Color.White : Color.Gray);
			base.OnUpdate(game, gameTime);
		}
	}
}
