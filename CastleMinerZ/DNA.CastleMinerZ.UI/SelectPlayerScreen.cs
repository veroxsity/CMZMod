using System;
using System.Text;
using DNA.Audio;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.UI
{
	public class SelectPlayerScreen : Screen
	{
		public Player PlayerSelected;

		public SelectPlayerCallback _callback;

		private SpriteFont font;

		private int _selectedIndex;

		private bool _showMe;

		private CastleMinerZGame _game;

		private Rectangle[] _itemLocations = new Rectangle[0];

		private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool flashDir;

		private StringBuilder _builder = new StringBuilder();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private SelectPlayerScreen(CastleMinerZGame game, bool showME, bool drawBehind, SelectPlayerCallback callback)
			: base(true, drawBehind)
		{
			_showMe = showME;
			font = game._medFont;
			_game = game;
			_callback = callback;
		}

		public int HitTest(Point p)
		{
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				if (_itemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public static void SelectPlayer(CastleMinerZGame game, ScreenGroup group, bool showME, bool drawBehind, SelectPlayerCallback callback)
		{
			SelectPlayerScreen screen = new SelectPlayerScreen(game, showME, drawBehind, callback);
			group.PushScreen(screen);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_itemLocations.Length != _game.CurrentNetworkSession.AllGamers.Count)
			{
				_itemLocations = new Rectangle[_game.CurrentNetworkSession.AllGamers.Count];
			}
			flashTimer.Update(gameTime.ElapsedGameTime);
			if (flashTimer.Expired)
			{
				flashTimer.Reset();
				flashDir = !flashDir;
			}
			Viewport viewport = device.Viewport;
			Rectangle titleSafeArea = viewport.TitleSafeArea;
			spriteBatch.Begin();
			int count = _game.CurrentNetworkSession.AllGamers.Count;
			int maxGamers = _game.CurrentNetworkSession.MaxGamers;
			_builder.Length = 0;
			_builder.Append("Players ").Concat(count).Append("/")
				.Concat(maxGamers);
			Vector2 vector = font.MeasureString(_builder);
			spriteBatch.DrawOutlinedText(font, _builder, new Vector2((float)titleSafeArea.Right - vector.X, (float)titleSafeArea.Bottom - vector.Y), Color.White, Color.Black, 2);
			float[] array = new float[1];
			float num = 0f;
			num += (array[0] = font.MeasureString("XXXXXXXXXXXXXXXXXXX ").X);
			float num2 = ((float)viewport.Width - num) / 2f;
			float num3 = titleSafeArea.Top;
			spriteBatch.DrawOutlinedText(font, "Player", new Vector2(num2, num3), Color.Orange, Color.Black, 2);
			num3 += (float)font.LineSpacing;
			int num4 = 0;
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag == null)
				{
					continue;
				}
				Player player = (Player)networkGamer.Tag;
				if (!_showMe && player.IsLocal)
				{
					continue;
				}
				if (num4 == _selectedIndex)
				{
					spriteBatch.Draw(CastleMinerZGame.Instance.DummyTexture, new Rectangle((int)num2, (int)num3, (int)num, font.LineSpacing), Color.Lerp(Color.White, Color.Red, flashDir ? flashTimer.PercentComplete : (1f - flashTimer.PercentComplete)));
				}
				num4++;
				Vector2 vector2 = font.MeasureString(player.Gamer.Gamertag);
				_itemLocations[i] = new Rectangle((int)num2, (int)num3, (int)vector2.X, (int)vector2.Y);
				spriteBatch.DrawOutlinedText(font, player.Gamer.Gamertag, new Vector2(num2, num3), player.Gamer.IsLocal ? Color.Red : Color.White, Color.Black, 2);
				if (player.Profile != null)
				{
					float num5 = (float)font.LineSpacing * 0.9f;
					float num6 = (float)font.LineSpacing - num5;
					if (player.GamerPicture != null)
					{
						spriteBatch.Draw(player.GamerPicture, new Rectangle((int)(num2 - (float)font.LineSpacing), (int)(num3 + num6), (int)num5, (int)num5), Color.White);
					}
				}
				num3 += (float)font.LineSpacing;
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			int num = HitTest(inputManager.Mouse.Position);
			if (num >= 0 && _selectedIndex != num)
			{
				SoundManager.Instance.PlayInstance("Click");
				_selectedIndex = num;
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && num >= 0))
			{
				PopMe();
				if (_callback != null)
				{
					_callback(PlayerSelected);
				}
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PlayerSelected = null;
				PopMe();
				if (_callback != null)
				{
					_callback(null);
				}
			}
			if (waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f)
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f)
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			int maxGamers = GetMaxGamers();
			if (_selectedIndex <= 0)
			{
				_selectedIndex = 0;
			}
			if (_selectedIndex >= maxGamers)
			{
				_selectedIndex = maxGamers - 1;
			}
			SetSelection();
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		private void SetSelection()
		{
			PlayerSelected = null;
			int num = 0;
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (_showMe || networkGamer != _game.MyNetworkGamer))
				{
					if (num == _selectedIndex)
					{
						PlayerSelected = (Player)networkGamer.Tag;
					}
					num++;
				}
			}
		}

		private int GetMaxGamers()
		{
			int num = 0;
			for (int i = 0; i < _game.CurrentNetworkSession.AllGamers.Count; i++)
			{
				NetworkGamer networkGamer = _game.CurrentNetworkSession.AllGamers[i];
				if (networkGamer.Tag != null && (_showMe || networkGamer != _game.MyNetworkGamer))
				{
					num++;
				}
			}
			return num;
		}

		private bool SelectDown()
		{
			_selectedIndex++;
			int maxGamers = GetMaxGamers();
			if (_selectedIndex >= maxGamers)
			{
				_selectedIndex = maxGamers - 1;
				return false;
			}
			return true;
		}

		private bool SelectUp()
		{
			_selectedIndex--;
			if (_selectedIndex <= 0)
			{
				_selectedIndex = 0;
				return false;
			}
			return true;
		}
	}
}
