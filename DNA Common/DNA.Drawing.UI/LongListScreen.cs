using System;
using System.Collections.Generic;
using DNA.Audio;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class LongListScreen : Screen
	{
		public abstract class MenuItem
		{
			public object Tag;

			public bool Selected;

			public MenuItem(object tag)
			{
				Tag = tag;
			}

			public abstract void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, Vector2 pos);

			public abstract Vector2 Measure();
		}

		public class TextItem : MenuItem
		{
			public string Text;

			public SpriteFont Font;

			public Color Color = Color.White;

			public Color SelectedColor = Color.Red;

			private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

			private bool selectedDirection;

			public TextItem(string text, SpriteFont font, object tag)
				: base(tag)
			{
				Text = text;
				Font = font;
			}

			public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, Vector2 pos)
			{
				Color textColor = Color;
				if (Selected)
				{
					flashTimer.Update(gameTime.ElapsedGameTime);
					if (flashTimer.Expired)
					{
						flashTimer.Reset();
						selectedDirection = !selectedDirection;
					}
					textColor = ((!selectedDirection) ? Color.Lerp(SelectedColor, Color, flashTimer.PercentComplete) : Color.Lerp(Color, SelectedColor, flashTimer.PercentComplete));
				}
				spriteBatch.DrawOutlinedText(Font, Text, pos, textColor, Color.Black, 1);
			}

			public override Vector2 Measure()
			{
				return Font.MeasureString(Text);
			}
		}

		public const float deadZone = 0.25f;

		private List<MenuItem> _menuItems = new List<MenuItem>();

		protected int _middleIndex;

		protected int _selectedIndex;

		private OneShotTimer holdTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private OneShotTimer scrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private OneShotTimer accelerateDelayTimer = new OneShotTimer(TimeSpan.FromSeconds(3.0));

		private OneShotTimer accelerateTimer = new OneShotTimer(TimeSpan.FromSeconds(5.0));

		private TimeSpan scrollRate = TimeSpan.FromSeconds(0.05000000074505806);

		private bool lastselectup;

		private bool lastselectdown;

		public string SelectSound;

		public string ClickSound;

		private Rectangle[] _itemLocations = new Rectangle[0];

		public Rectangle destRect;

		private Viewport viewPort = default(Viewport);

		private bool hitTestTrue;

		public List<MenuItem> MenuItems
		{
			get
			{
				return _menuItems;
			}
		}

		public MenuItem SelectedItem
		{
			get
			{
				if (_selectedIndex < 0 || _selectedIndex >= _menuItems.Count)
				{
					return null;
				}
				return _menuItems[_selectedIndex];
			}
		}

		public event EventHandler<SelectedEventArgs> Clicked;

		public event EventHandler<SelectedEventArgs> BackClicked;

		public LongListScreen(bool drawBehind)
			: base(true, drawBehind)
		{
		}

		protected virtual void OnClicked(MenuItem selectedItem)
		{
		}

		public void Click()
		{
			OnClicked(SelectedItem);
			if (this.Clicked != null)
			{
				this.Clicked(this, new SelectedEventArgs(SelectedItem.Tag));
			}
			if (ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(ClickSound);
			}
		}

		protected virtual void OnBack()
		{
		}

		public void Back()
		{
			OnBack();
			if (this.BackClicked != null)
			{
				this.BackClicked(this, null);
			}
			if (ClickSound != null)
			{
				SoundManager.Instance.PlayInstance(ClickSound);
			}
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

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			bool flag = false;
			flag = true;
			bool flag2 = false;
			bool flag3 = false;
			float num = viewPort.Bounds.Height / 3;
			float num2 = (float)viewPort.Bounds.Bottom - num;
			float num3 = num;
			float num4 = (float)viewPort.Bounds.Bottom - num * 0.2f;
			float num5 = num * 0.2f;
			if (controller.CurrentState.ThumbSticks.Left.Y < -0.25f || controller.CurrentState.DPad.Down == ButtonState.Pressed || controller.CurrentState.Triggers.Right > 0.25f || input.Keyboard.IsKeyDown(Keys.Down) || (!flag && (float)input.Mouse.Position.Y > num2 && !hitTestTrue && input.Mouse.Position.X < viewPort.Bounds.Center.X) || (!flag && input.Mouse.Position.Y > destRect.Bottom && input.Mouse.Position.X < viewPort.Bounds.Center.X))
			{
				flag3 = true;
				if (!lastselectdown)
				{
					accelerateDelayTimer.Reset();
					accelerateTimer.Reset();
				}
			}
			if (controller.CurrentState.ThumbSticks.Left.Y > 0.25f || controller.CurrentState.DPad.Up == ButtonState.Pressed || controller.CurrentState.Triggers.Left > 0.25f || input.Keyboard.IsKeyDown(Keys.Up) || (!flag && (float)input.Mouse.Position.Y < num3 && !hitTestTrue))
			{
				flag2 = true;
				if (!lastselectup)
				{
					accelerateDelayTimer.Reset();
					accelerateTimer.Reset();
				}
			}
			if (Math.Abs(controller.CurrentState.ThumbSticks.Left.Y) > 0.8f || (!flag && (float)input.Mouse.Position.Y < num5) || (!flag && (float)input.Mouse.Position.Y > num4))
			{
				accelerateDelayTimer.Update(gameTime.ElapsedGameTime);
			}
			if (accelerateDelayTimer.Expired)
			{
				accelerateTimer.Update(gameTime.ElapsedGameTime);
			}
			float num6 = accelerateTimer.PercentComplete * (float)scrollRate.TotalSeconds / 2f;
			float val = Math.Abs(controller.CurrentState.ThumbSticks.Left.Y);
			float val2 = ((!flag && (float)input.Mouse.Position.Y < num3 && !hitTestTrue) ? ((num3 - (float)input.Mouse.Position.Y) / num) : ((flag || !((float)input.Mouse.Position.Y > num2) || hitTestTrue) ? 0f : (((float)input.Mouse.Position.Y - num2) / num)));
			float num7 = Math.Max(val, val2);
			if (num7 > 0f)
			{
				scrollTimer.MaxTime = TimeSpan.FromSeconds(scrollRate.TotalSeconds / (double)num7 - (double)num6);
			}
			else
			{
				scrollTimer.MaxTime = TimeSpan.FromSeconds(0.1);
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y < -0.25f && controller.LastState.ThumbSticks.Left.Y > -0.25f) || controller.PressedDPad.Down || (controller.CurrentState.Triggers.Right > 0.25f && controller.LastState.Triggers.Right < 0.25f) || input.Keyboard.WasKeyPressed(Keys.Down) || (!flag && (float)input.Mouse.Position.Y > num2 && (float)input.Mouse.LastPosition.Y <= num2 && !hitTestTrue) || input.Mouse.DeltaWheel < 0)
			{
				if (_middleIndex < _menuItems.Count - 1 && SelectSound != null && input.Mouse.DeltaWheel < 0)
				{
					SoundManager.Instance.PlayInstance(SelectSound);
				}
				_middleIndex++;
			}
			if ((controller.CurrentState.ThumbSticks.Left.Y > 0.25f && controller.LastState.ThumbSticks.Left.Y < 0.25f) || controller.PressedDPad.Up || (controller.CurrentState.Triggers.Left > 0.25f && controller.LastState.Triggers.Left < 0.25f) || input.Keyboard.WasKeyPressed(Keys.Up) || (!flag && (float)input.Mouse.Position.Y < num3 && (float)input.Mouse.LastPosition.Y >= num3 && !hitTestTrue) || input.Mouse.DeltaWheel > 0)
			{
				if (_middleIndex > 0 && SelectSound != null && input.Mouse.DeltaWheel > 0)
				{
					SoundManager.Instance.PlayInstance(SelectSound);
				}
				_middleIndex--;
			}
			if (flag3)
			{
				holdTimer.Update(gameTime.ElapsedGameTime);
				if (holdTimer.Expired)
				{
					scrollTimer.Update(gameTime.ElapsedGameTime);
					if (scrollTimer.Expired)
					{
						scrollTimer.Reset();
						if (accelerateTimer.Expired)
						{
							_middleIndex += 10;
						}
						else
						{
							_middleIndex++;
						}
						if (_middleIndex < _menuItems.Count && SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(SelectSound);
						}
					}
				}
			}
			else if (flag2)
			{
				holdTimer.Update(gameTime.ElapsedGameTime);
				if (holdTimer.Expired)
				{
					scrollTimer.Update(gameTime.ElapsedGameTime);
					if (scrollTimer.Expired)
					{
						scrollTimer.Reset();
						if (accelerateTimer.Expired)
						{
							_middleIndex -= 10;
						}
						else
						{
							_middleIndex--;
						}
						if (_middleIndex >= 0 && SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(SelectSound);
						}
					}
				}
			}
			if (_middleIndex < 0)
			{
				_middleIndex = 0;
			}
			if (_middleIndex >= _menuItems.Count)
			{
				_middleIndex = _menuItems.Count - 1;
			}
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter))
			{
				if (MenuItems.Count > 0)
				{
					Click();
				}
				else
				{
					Back();
				}
			}
			else if (input.Mouse.LeftButtonPressed)
			{
				if (MenuItems.Count > 0)
				{
					if (HitTest(input.Mouse.Position) >= 0)
					{
						Click();
					}
				}
				else
				{
					Back();
				}
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				Back();
			}
			base.OnPlayerInput(input, controller, chatpad, gameTime);
			if ((!flag3 && lastselectdown) || (!flag2 && lastselectup))
			{
				holdTimer.Reset();
			}
			lastselectdown = flag3;
			lastselectup = flag2;
			int num8 = HitTest(input.Mouse.Position);
			if (num8 >= 0)
			{
				hitTestTrue = true;
				if (_selectedIndex != num8)
				{
					if (SelectSound != null && !flag3 && !flag2)
					{
						SoundManager.Instance.PlayInstance(SelectSound);
					}
					_selectedIndex = num8;
				}
			}
			else
			{
				hitTestTrue = false;
				if (_selectedIndex != _middleIndex)
				{
					if (SelectSound != null)
					{
						SoundManager.Instance.PlayInstance(SelectSound);
					}
					_selectedIndex = _middleIndex;
				}
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private void _drawPreviousItems(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int num = middleIndex - 1;
			if (num < 0 || num >= _menuItems.Count)
			{
				return;
			}
			float num2 = startYPos;
			while (num >= 0)
			{
				MenuItem menuItem = _menuItems[num];
				if (num == _selectedIndex)
				{
					menuItem.Selected = true;
				}
				else
				{
					menuItem.Selected = false;
				}
				Vector2 vector = menuItem.Measure();
				num2 -= vector.Y;
				if (num2 + vector.Y < (float)destRect.Top)
				{
					break;
				}
				_setItemLocation(num, new Rectangle(destRect.Left, (int)num2, (int)vector.X, (int)vector.Y));
				menuItem.Draw(device, spriteBatch, gameTime, new Vector2(destRect.Left, num2));
				num--;
			}
			while (num >= 0)
			{
				_setItemLocation(num, Rectangle.Empty);
				num--;
			}
		}

		private void _drawPostItems(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			int i = middleIndex + 1;
			if (i < 0 || i >= _menuItems.Count)
			{
				return;
			}
			float num = startYPos;
			while (i < _menuItems.Count)
			{
				MenuItem menuItem = _menuItems[i];
				if (i == _selectedIndex)
				{
					menuItem.Selected = true;
				}
				else
				{
					menuItem.Selected = false;
				}
				Vector2 vector = menuItem.Measure();
				if (num > (float)viewPort.Bounds.Bottom)
				{
					break;
				}
				_setItemLocation(i, new Rectangle(destRect.Left, (int)num, (int)vector.X, (int)vector.Y));
				menuItem.Draw(device, spriteBatch, gameTime, new Vector2(destRect.Left, num));
				i++;
				num += vector.Y;
			}
			for (; i < _menuItems.Count; i++)
			{
				_setItemLocation(i, Rectangle.Empty);
			}
		}

		private void _drawSelectedItem(int middleIndex, float startYPos, GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (middleIndex >= 0 && middleIndex < _menuItems.Count)
			{
				MenuItem menuItem = _menuItems[middleIndex];
				Vector2 vector = menuItem.Measure();
				menuItem.Draw(device, spriteBatch, gameTime, new Vector2(destRect.Left, startYPos));
				_setItemLocation(middleIndex, new Rectangle(destRect.Left, (int)startYPos, (int)vector.X, (int)vector.Y));
			}
		}

		private void _setItemLocation(int index, Rectangle location)
		{
			try
			{
				_itemLocations[index] = location;
			}
			catch
			{
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_itemLocations.Length != _menuItems.Count)
			{
				_itemLocations = new Rectangle[_menuItems.Count];
			}
			viewPort = device.Viewport;
			int middleIndex = _middleIndex;
			if (middleIndex >= 0 && middleIndex < _menuItems.Count)
			{
				MenuItem menuItem = _menuItems[middleIndex];
				if (middleIndex == _selectedIndex)
				{
					menuItem.Selected = true;
				}
				else
				{
					menuItem.Selected = false;
				}
				Vector2 vector = menuItem.Measure();
				float num = (float)destRect.Center.Y - vector.Y / 2f;
				spriteBatch.Begin();
				_drawSelectedItem(middleIndex, num, device, spriteBatch, gameTime);
				_drawPreviousItems(middleIndex, num, device, spriteBatch, gameTime);
				_drawPostItems(middleIndex, num + vector.Y, device, spriteBatch, gameTime);
				spriteBatch.End();
				base.OnDraw(device, spriteBatch, gameTime);
			}
		}
	}
}
