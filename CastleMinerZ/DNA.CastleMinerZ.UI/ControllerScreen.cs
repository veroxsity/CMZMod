using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	internal class ControllerScreen : Screen
	{
		private CastleMinerZGame _game;

		private SpriteFont _UIFont;

		private static Texture2D _controlsScreen;

		private string _invertText;

		private bool inGame;

		private Rectangle _invertTextLocation = default(Rectangle);

		static ControllerScreen()
		{
			_controlsScreen = CastleMinerZGame.Instance.Content.Load<Texture2D>("Controls");
		}

		public ControllerScreen(CastleMinerZGame game, bool InGame)
			: base(true, false)
		{
			_game = game;
			_UIFont = _game._largeFont;
			_invertText = (_game.PlayerStats.InvertYAxis ? " Invert Y Axis(Inverted)" : " Invert Y Axis(Regular)");
			inGame = InGame;
		}

		public override void OnPushed()
		{
			_invertText = (_game.PlayerStats.InvertYAxis ? " Invert Y Axis(Inverted)" : " Invert Y Axis(Regular)");
			base.OnPushed();
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (controller.PressedButtons.Y || inputManager.Keyboard.WasKeyPressed(Keys.Y) || (inputManager.Mouse.LeftButtonPressed && _invertTextLocation.Contains(inputManager.Mouse.Position)))
			{
				_game.PlayerStats.InvertYAxis = !_game.PlayerStats.InvertYAxis;
				_invertText = (_game.PlayerStats.InvertYAxis ? " Invert Y Axis(Inverted)" : " Invert Y Axis(Regular)");
			}
			else if (controller.ButtonPressed || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PopMe();
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Viewport viewport = device.Viewport;
			Rectangle titleSafeArea = viewport.TitleSafeArea;
			spriteBatch.Begin();
			if (inGame)
			{
				spriteBatch.Draw(destinationRectangle: new Rectangle(0, 0, viewport.Width, viewport.Height), texture: _game.DummyTexture, color: new Color(0f, 0f, 0f, 0.5f));
			}
			int width = _controlsScreen.Width;
			spriteBatch.Draw(_controlsScreen, new Vector2((viewport.Width - _controlsScreen.Width) / 2, titleSafeArea.Top), Color.White);
			Vector2 vector = _UIFont.MeasureString(_invertText);
			float num = vector.Y / (float)ControllerImages.Y.Height;
			int num2 = (int)((float)ControllerImages.Y.Width * num);
			int num3 = (int)((float)(titleSafeArea.Y + titleSafeArea.Height) - vector.Y);
			spriteBatch.Draw(ControllerImages.Y, new Rectangle(titleSafeArea.X, num3, num2, (int)vector.Y), Color.White);
			spriteBatch.DrawOutlinedText(_UIFont, _invertText, new Vector2(titleSafeArea.X + num2, num3), Color.White, Color.Black, 1);
			Vector2 vector2 = _UIFont.MeasureString(_invertText);
			_invertTextLocation = new Rectangle(titleSafeArea.X + num2, num3, (int)vector2.X, (int)vector2.Y);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
