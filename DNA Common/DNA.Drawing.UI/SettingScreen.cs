using System.Collections.Generic;
using DNA.Audio;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class SettingScreen : Screen
	{
		public string SelectSound;

		public string ClickSound;

		private List<SettingItemElement> _menuItems = new List<SettingItemElement>();

		private int _selectedIndex;

		public float? MenuStart = null;

		private Rectangle[] _itemLocations = new Rectangle[0];

		public int? LineSpacing;

		public SpriteFont Font;

		public Color TextColor;

		public Color SelectedColor;

		public Color OutlineColor = Color.Black;

		public int OnlineWidth = 2;

		private DNAGame _game;

		public List<SettingItemElement> MenuItems
		{
			get
			{
				return _menuItems;
			}
		}

		public int SelectedIndex
		{
			get
			{
				return _selectedIndex;
			}
			set
			{
				_selectedIndex = value;
			}
		}

		public SettingScreen(DNAGame game, SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			Font = font;
			_game = game;
			TextColor = Color.White;
			SelectedColor = Color.Red;
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

		public SettingScreen(DNAGame game, SpriteFont font, Color textColor, Color selectedColor, bool drawBehind)
			: base(true, drawBehind)
		{
			TextColor = textColor;
			SelectedColor = selectedColor;
			Font = font;
			_game = game;
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			int num = HitTest(input.Mouse.Position);
			if (!_mouseActive)
			{
				num = -1;
			}
			if (input.Mouse.DeltaPosition != Vector2.Zero)
			{
				_mouseActive = true;
			}
			bool flag = true;
			if (num >= 0)
			{
				if (_menuItems[num].Visible)
				{
					if (_selectedIndex != num)
					{
						if (SelectSound != null)
						{
							SoundManager.Instance.PlayInstance(SelectSound);
						}
						_selectedIndex = num;
					}
					BarSettingItem barSettingItem = _menuItems[num] as BarSettingItem;
					if (barSettingItem != null)
					{
						flag = !barSettingItem.SetBarValue(input.Mouse);
					}
				}
				else
				{
					num = -1;
				}
			}
			float num2 = 0.25f;
			if (controller.PressedDPad.Down || controller.PressedButtons.RightShoulder || (controller.CurrentState.ThumbSticks.Left.Y < 0f - num2 && controller.LastState.ThumbSticks.Left.Y > 0f - num2) || (controller.CurrentState.ThumbSticks.Right.Y < 0f - num2 && controller.LastState.ThumbSticks.Right.Y > 0f - num2) || input.Keyboard.WasKeyPressed(Keys.Down))
			{
				if (SelectSound != null)
				{
					SoundManager.Instance.PlayInstance(SelectSound);
				}
				SelectNext();
			}
			if (controller.PressedDPad.Up || controller.PressedButtons.LeftShoulder || (controller.CurrentState.ThumbSticks.Left.Y > num2 && controller.LastState.ThumbSticks.Left.Y < num2) || (controller.CurrentState.ThumbSticks.Right.Y > num2 && controller.LastState.ThumbSticks.Right.Y < num2) || input.Keyboard.WasKeyPressed(Keys.Up))
			{
				if (SelectSound != null)
				{
					SoundManager.Instance.PlayInstance(SelectSound);
				}
				SelectPrevious();
			}
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
			}
			if (controller.PressedButtons.A || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && HitTest(input.Mouse.Position) >= 0))
			{
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				if (flag)
				{
					ClickedMenuItem();
				}
			}
			if (controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < 0f - num2 && controller.LastState.ThumbSticks.Left.X > 0f - num2) || (controller.CurrentState.ThumbSticks.Right.X < 0f - num2 && controller.LastState.ThumbSticks.Right.X > 0f - num2) || input.Keyboard.WasKeyPressed(Keys.Left) || (input.Mouse.DeltaWheel < 0 && HitTest(input.Mouse.Position) >= 0))
			{
				DecreasedMenuItem();
				_menuItems[_selectedIndex].ResetTimer();
			}
			else if ((controller.CurrentState.DPad.Left == ButtonState.Pressed || controller.CurrentState.ThumbSticks.Left.X < 0f - num2 || controller.CurrentState.ThumbSticks.Right.X < 0f - num2 || input.Keyboard.IsKeyDown(Keys.Left)) && _menuItems[_selectedIndex].ChangeValue(gameTime.ElapsedGameTime))
			{
				DecreasedMenuItem();
			}
			if (controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > num2 && controller.LastState.ThumbSticks.Left.X < num2) || (controller.CurrentState.ThumbSticks.Right.X > num2 && controller.LastState.ThumbSticks.Right.X < num2) || input.Keyboard.WasKeyPressed(Keys.Right) || (input.Mouse.DeltaWheel > 0 && HitTest(input.Mouse.Position) >= 0))
			{
				IncreasedMenuItem();
				_menuItems[_selectedIndex].ResetTimer();
			}
			else if ((controller.CurrentState.DPad.Right == ButtonState.Pressed || controller.CurrentState.ThumbSticks.Left.X > num2 || controller.CurrentState.ThumbSticks.Right.X > num2 || input.Keyboard.IsKeyDown(Keys.Right)) && _menuItems[_selectedIndex].ChangeValue(gameTime.ElapsedGameTime))
			{
				IncreasedMenuItem();
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		private void ClickedMenuItem()
		{
			if (_selectedIndex >= 0)
			{
				SettingItemElement settingItemElement = _menuItems[_selectedIndex];
				settingItemElement.Clicked();
			}
		}

		private void IncreasedMenuItem()
		{
			if (_selectedIndex >= 0)
			{
				SettingItemElement settingItemElement = _menuItems[_selectedIndex];
				settingItemElement.Increased();
			}
		}

		private void DecreasedMenuItem()
		{
			if (_selectedIndex >= 0)
			{
				SettingItemElement settingItemElement = _menuItems[_selectedIndex];
				settingItemElement.Decreased();
			}
		}

		private void SelectFirst()
		{
			if (_menuItems.Count == 0)
			{
				_selectedIndex = -1;
			}
			else
			{
				_selectedIndex = 0;
			}
		}

		private void SelectNext()
		{
			if (_menuItems.Count == 0)
			{
				_selectedIndex = -1;
				return;
			}
			int selectedIndex = _selectedIndex;
			do
			{
				_selectedIndex++;
				if (_selectedIndex >= _menuItems.Count)
				{
					_selectedIndex = 0;
				}
				if (_selectedIndex == selectedIndex && !_menuItems[_selectedIndex].Visible)
				{
					_selectedIndex = -1;
					break;
				}
			}
			while (!_menuItems[_selectedIndex].Visible);
		}

		private void SelectPrevious()
		{
			if (_menuItems.Count == 0)
			{
				_selectedIndex = -1;
				return;
			}
			int selectedIndex = _selectedIndex;
			do
			{
				_selectedIndex--;
				if (_selectedIndex < 0)
				{
					_selectedIndex = _menuItems.Count - 1;
				}
				if (_selectedIndex == selectedIndex && !_menuItems[_selectedIndex].Visible)
				{
					_selectedIndex = -1;
					break;
				}
			}
			while (!_menuItems[_selectedIndex].Visible);
		}

		private Vector2 MeasureItem(SettingItemElement item)
		{
			SpriteFont spriteFont = ((item.Font == null) ? Font : item.Font);
			return spriteFont.MeasureString(item.Text);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_itemLocations.Length != _menuItems.Count)
			{
				_itemLocations = new Rectangle[_menuItems.Count];
			}
			while (_selectedIndex >= _menuItems.Count || !MenuItems[_selectedIndex].Visible)
			{
				_selectedIndex++;
				_selectedIndex %= _menuItems.Count;
			}
			spriteBatch.Begin();
			float num = 0f;
			float num2 = 0f;
			if (LineSpacing.HasValue)
			{
				num2 = LineSpacing.Value;
			}
			for (int i = 0; i < _menuItems.Count; i++)
			{
				if (_menuItems[i].Visible)
				{
					num += MeasureItem(_menuItems[i]).Y + num2;
				}
			}
			num -= num2;
			float num3 = device.Viewport.Height;
			int width = device.Viewport.Width;
			float num4 = (num3 - num) / 2f;
			if (MenuStart.HasValue)
			{
				num4 = MenuStart.Value;
			}
			for (int j = 0; j < _menuItems.Count; j++)
			{
				SettingItemElement settingItemElement = _menuItems[j];
				if (settingItemElement.Visible)
				{
					SpriteFont font = ((settingItemElement.Font == null) ? Font : settingItemElement.Font);
					Color textColor = (settingItemElement.TextColor.HasValue ? settingItemElement.TextColor.Value : TextColor);
					Color outlineColor = (settingItemElement.OutlineColor.HasValue ? settingItemElement.OutlineColor.Value : OutlineColor);
					int outlineWidth = (settingItemElement.OnlineWidth.HasValue ? settingItemElement.OnlineWidth.Value : OnlineWidth);
					Vector2 vector = MeasureItem(settingItemElement);
					if (j == _selectedIndex)
					{
						textColor = (settingItemElement.SelectedColor.HasValue ? settingItemElement.SelectedColor.Value : SelectedColor);
					}
					_itemLocations[j] = new Rectangle(device.Viewport.TitleSafeArea.Left, (int)num4, device.Viewport.TitleSafeArea.Width, (int)vector.Y);
					settingItemElement.OnDraw(_game, device, spriteBatch, font, textColor, outlineColor, outlineWidth, num4);
					num4 += vector.Y + num2;
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
