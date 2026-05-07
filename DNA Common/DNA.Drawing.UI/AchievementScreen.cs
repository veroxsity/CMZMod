using System;
using System.Text;
using DNA.Audio;
using DNA.Input;
using DNA.Text;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class AchievementScreen<T> : Screen where T : PlayerStats
	{
		public const float deadZone = 0.25f;

		private AchievementManager<T> _achievementManager;

		private int MaxAchievementsToDisplay = 6;

		private int TopDisplayIndex;

		private SpriteFont _largeFont;

		private SpriteFont _smallFont;

		private Color _mainTextColor = new Color(225, 229, 220);

		private Color _otherTextColor = new Color(115, 131, 136);

		private Color _backColor = new Color(26, 27, 26);

		private Color _progressBackColor = new Color(38, 38, 38);

		private Color _progressColor = new Color(68, 68, 67);

		private Color _progressOutlineColor = new Color(60, 57, 52);

		private Texture2D _dummyTexture;

		private StringBuilder sbuilder = new StringBuilder();

		private OneShotTimer holdTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer scrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private bool lastselectup;

		private bool lastselectdown;

		public string ClickSound;

		private bool _mouseMovement;

		public AchievementScreen(AchievementManager<T> achievementManager, SpriteFont largeFont, SpriteFont smallFont, Texture2D dummyTexture)
			: base(true, false)
		{
			_achievementManager = achievementManager;
			_largeFont = largeFont;
			_smallFont = smallFont;
			_dummyTexture = dummyTexture;
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle rectangle = new Rectangle(128, 72, 1024, 576);
			Vector2 location = new Vector2(rectangle.X + 110, rectangle.Y + 15);
			spriteBatch.Begin();
			spriteBatch.Draw(_dummyTexture, new Rectangle(rectangle.X + 100, rectangle.Top, rectangle.Width - 200, rectangle.Height), Color.Black);
			spriteBatch.Draw(_dummyTexture, new Rectangle(rectangle.X + 105, rectangle.Top + 5, rectangle.Width - 210, rectangle.Height - 10), _backColor);
			spriteBatch.DrawOutlinedText(_largeFont, CommonResources.Awards, location, _mainTextColor, _progressOutlineColor, 2);
			int num = 0;
			for (int i = 0; i < _achievementManager.Count; i++)
			{
				if (_achievementManager[i].Acheived)
				{
					num++;
				}
			}
			sbuilder.Length = 0;
			sbuilder.Concat(num);
			sbuilder.Append("/");
			sbuilder.Concat(_achievementManager.Count);
			spriteBatch.DrawOutlinedText(_largeFont, sbuilder, new Vector2((float)(rectangle.X + rectangle.Width - 110) - _largeFont.MeasureString(sbuilder).X, location.Y), _mainTextColor, _progressOutlineColor, 2);
			location.X += 65f;
			location.Y += 75f;
			float num2 = _smallFont.MeasureString("OK").Y - 5f;
			for (int j = 0; j < MaxAchievementsToDisplay; j++)
			{
				spriteBatch.Draw(_dummyTexture, new Rectangle((int)location.X, (int)location.Y, 700, 70), _progressOutlineColor);
				spriteBatch.Draw(_dummyTexture, new Rectangle((int)location.X + 2, (int)location.Y + 2, 696, 66), _progressBackColor);
				spriteBatch.Draw(_dummyTexture, new Rectangle((int)location.X + 2, (int)location.Y + 2, (int)(696f * _achievementManager[j + TopDisplayIndex].ProgressTowardsUnlock), 66), _progressColor);
				spriteBatch.DrawOutlinedText(_smallFont, _achievementManager[j + TopDisplayIndex].Name, new Vector2(location.X + 10f, location.Y + 35f - num2), _mainTextColor, Color.Black, 1);
				spriteBatch.DrawOutlinedText(_smallFont, _achievementManager[j + TopDisplayIndex].HowToUnlock, new Vector2(location.X + 10f, location.Y + 35f), _otherTextColor, Color.Black, 1);
				if (_achievementManager[j + TopDisplayIndex].Reward == null)
				{
					spriteBatch.DrawOutlinedText(_smallFont, _achievementManager[j + TopDisplayIndex].ProgressTowardsUnlockMessage, new Vector2(location.X + 690f - _smallFont.MeasureString(_achievementManager[j + TopDisplayIndex].ProgressTowardsUnlockMessage).X, location.Y + 35f - num2 / 2f), _mainTextColor, Color.Black, 1);
				}
				else
				{
					spriteBatch.DrawOutlinedText(_smallFont, _achievementManager[j + TopDisplayIndex].ProgressTowardsUnlockMessage, new Vector2(location.X + 690f - _smallFont.MeasureString(_achievementManager[j + TopDisplayIndex].ProgressTowardsUnlockMessage).X, location.Y + 35f - num2), _mainTextColor, Color.Black, 1);
					spriteBatch.DrawOutlinedText(_smallFont, _achievementManager[j + TopDisplayIndex].Reward, new Vector2(location.X + 690f - _smallFont.MeasureString(_achievementManager[j + TopDisplayIndex].Reward).X, location.Y + 35f), _otherTextColor, Color.Black, 1);
				}
				location.Y += 80f;
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		private void PlayClickSound()
		{
			if (ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(ClickSound);
			}
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			bool flag = false;
			bool flag2 = false;
			if (input.Mouse.DeltaPosition.X != 0f || input.Mouse.DeltaPosition.Y != 0f)
			{
				_mouseMovement = true;
			}
			else if (input.Mouse.DeltaWheel != 0 || input.Keyboard.CurrentState.IsKeyDown(Keys.Down) || input.Keyboard.IsKeyDown(Keys.Up))
			{
				_mouseMovement = false;
			}
			if (controller.CurrentState.ThumbSticks.Left.Y < -0.25f || controller.CurrentState.DPad.Down == ButtonState.Pressed || controller.CurrentState.Triggers.Right > 0.25f || input.Keyboard.IsKeyDown(Keys.Down) || (input.Mouse.Position.Y > 480 && _mouseMovement))
			{
				flag2 = true;
			}
			bool flag3 = false;
			if (controller.CurrentState.ThumbSticks.Left.Y > 0.25f || controller.CurrentState.DPad.Up == ButtonState.Pressed || controller.CurrentState.Triggers.Left > 0.25f || input.Keyboard.IsKeyDown(Keys.Up) || flag3)
			{
				flag = true;
			}
			if (input.Mouse.DeltaWheel < 0)
			{
				flag2 = true;
				if (TopDisplayIndex < _achievementManager.Count - MaxAchievementsToDisplay)
				{
					PlayClickSound();
				}
				TopDisplayIndex++;
				flag2 = false;
			}
			else if (input.Mouse.DeltaWheel > 0)
			{
				flag = true;
				if (TopDisplayIndex > 0)
				{
					PlayClickSound();
				}
				TopDisplayIndex--;
				flag = false;
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y < -0.25f && controller.LastState.ThumbSticks.Left.Y > -0.25f) || controller.PressedDPad.Down || (controller.CurrentState.Triggers.Right > 0.25f && controller.LastState.Triggers.Right < 0.25f) || input.Keyboard.WasKeyPressed(Keys.Down) || (input.Mouse.Position.Y > 480 && input.Mouse.LastPosition.Y <= 480))
			{
				if (TopDisplayIndex < _achievementManager.Count - MaxAchievementsToDisplay)
				{
					PlayClickSound();
				}
				TopDisplayIndex++;
				flag2 = false;
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y > 0.25f && controller.LastState.ThumbSticks.Left.Y < 0.25f) || controller.PressedDPad.Up || (controller.CurrentState.Triggers.Left > 0.25f && controller.LastState.Triggers.Left < 0.25f) || input.Keyboard.WasKeyPressed(Keys.Up) || (input.Mouse.Position.Y < 240 && input.Mouse.Position.Y >= 240))
			{
				if (TopDisplayIndex > 0)
				{
					PlayClickSound();
				}
				TopDisplayIndex--;
				flag = false;
			}
			if (flag2)
			{
				holdTimer.Update(gameTime.ElapsedGameTime);
				if (holdTimer.Expired)
				{
					scrollTimer.Update(gameTime.ElapsedGameTime);
					if (scrollTimer.Expired)
					{
						if (TopDisplayIndex < _achievementManager.Count - MaxAchievementsToDisplay)
						{
							PlayClickSound();
						}
						scrollTimer.Reset();
						TopDisplayIndex++;
					}
				}
			}
			else if (flag)
			{
				holdTimer.Update(gameTime.ElapsedGameTime);
				if (holdTimer.Expired)
				{
					scrollTimer.Update(gameTime.ElapsedGameTime);
					if (scrollTimer.Expired)
					{
						if (TopDisplayIndex > 0)
						{
							PlayClickSound();
						}
						scrollTimer.Reset();
						TopDisplayIndex--;
					}
				}
			}
			if (TopDisplayIndex < 0)
			{
				TopDisplayIndex = 0;
			}
			if (TopDisplayIndex > _achievementManager.Count - MaxAchievementsToDisplay)
			{
				TopDisplayIndex = _achievementManager.Count - MaxAchievementsToDisplay;
			}
			if (controller.PressedButtons.A || controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PlayClickSound();
				PopMe();
			}
			base.OnPlayerInput(input, controller, chatpad, gameTime);
			if ((!flag2 && lastselectdown) || (!flag && lastselectup))
			{
				holdTimer.Reset();
			}
			lastselectdown = flag2;
			lastselectup = flag;
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}
	}
}
