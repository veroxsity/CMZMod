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
	public class MenuScreen : Screen
	{
		public enum HorizontalAlignmentTypes
		{
			Left,
			Right,
			Center
		}

		public enum VerticalAlignmentTypes
		{
			Top,
			Center,
			Bottom
		}

		private bool _flashDir;

		private OneShotTimer _flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		public string SelectSound;

		public string ClickSound;

		private List<MenuItemElement> _menuItems = new List<MenuItemElement>();

		private Rectangle[] _itemLocations = new Rectangle[0];

		private int _selectedIndex;

		public HorizontalAlignmentTypes HorizontalAlignment = HorizontalAlignmentTypes.Center;

		public VerticalAlignmentTypes VerticalAlignment = VerticalAlignmentTypes.Center;

		public Rectangle? DrawArea = null;

		public SpriteFont Font;

		public Color TextColor = Color.White;

		public Color SelectedColor = Color.Red;

		public Color OutlineColor = Color.Black;

		public int OutlineWidth = 2;

		public int? LineSpacing;

		private MenuItemElement _lastSelectedItem;

		public List<MenuItemElement> MenuItems
		{
			get
			{
				return _menuItems;
			}
		}

		public TimeSpan FlashTime
		{
			get
			{
				return _flashTimer.MaxTime;
			}
			set
			{
				_flashTimer.MaxTime = value;
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

		public MenuItemElement SelectedMenuItem
		{
			get
			{
				return MenuItems[SelectedIndex];
			}
		}

		public event EventHandler<SelectedMenuItemArgs> MenuItemSelected;

		public MenuItemElement AddMenuItem(string text, object tag)
		{
			MenuItemElement menuItemElement = new MenuItemElement(text, tag);
			MenuItems.Add(menuItemElement);
			return menuItemElement;
		}

		public MenuItemElement AddMenuItem(string text, string description, object tag)
		{
			MenuItemElement menuItemElement = new MenuItemElement(text, description, tag);
			MenuItems.Add(menuItemElement);
			return menuItemElement;
		}

		public MenuScreen(SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			Font = font;
		}

		public MenuScreen(SpriteFont font, Color textColor, Color selectedColor, bool drawBehind)
			: base(true, drawBehind)
		{
			TextColor = textColor;
			SelectedColor = selectedColor;
			Font = font;
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
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && num >= 0))
			{
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				SelectMenuItem();
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		protected virtual void OnMenuItemSelected(MenuItemElement selectedControl)
		{
		}

		protected virtual void OnMenuItemFocus(MenuItemElement selectedControl)
		{
		}

		private void SelectMenuItem()
		{
			if (_selectedIndex >= 0)
			{
				MenuItemElement menuItemElement = _menuItems[_selectedIndex];
				OnMenuItemSelected(menuItemElement);
				if (this.MenuItemSelected != null)
				{
					this.MenuItemSelected(this, new SelectedMenuItemArgs(menuItemElement));
				}
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

		private Vector2 MeasureItem(MenuItemElement item)
		{
			SpriteFont spriteFont = ((item.Font == null) ? Font : item.Font);
			return spriteFont.MeasureString(item.Text);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (_lastSelectedItem != SelectedMenuItem)
			{
				_lastSelectedItem = SelectedMenuItem;
				OnMenuItemFocus(_lastSelectedItem);
			}
			base.OnUpdate(game, gameTime);
		}

		public int HitTest(Point p)
		{
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				if (_itemLocations[i].Contains(p) && _menuItems[i].Visible)
				{
					return i;
				}
			}
			return -1;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			if (_itemLocations.Length != _menuItems.Count)
			{
				_itemLocations = new Rectangle[_menuItems.Count];
			}
			_flashTimer.Update(gameTime.ElapsedGameTime);
			if (_flashTimer.Expired)
			{
				_flashTimer.Reset();
				_flashDir = !_flashDir;
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
			Rectangle rectangle = device.Viewport.TitleSafeArea;
			if (DrawArea.HasValue)
			{
				rectangle = DrawArea.Value;
			}
			float num3 = rectangle.Height;
			float num4 = rectangle.Width;
			float num5 = (num3 - num) / 2f + (float)rectangle.Y;
			if (VerticalAlignment == VerticalAlignmentTypes.Top)
			{
				num5 = rectangle.Top;
			}
			else if (VerticalAlignment == VerticalAlignmentTypes.Bottom)
			{
				num5 = (float)rectangle.Bottom - num;
			}
			for (int j = 0; j < _menuItems.Count; j++)
			{
				MenuItemElement menuItemElement = _menuItems[j];
				if (menuItemElement.Visible)
				{
					SpriteFont font = ((menuItemElement.Font == null) ? Font : menuItemElement.Font);
					Color color = (menuItemElement.TextColor.HasValue ? menuItemElement.TextColor.Value : TextColor);
					Color outlineColor = (menuItemElement.OutlineColor.HasValue ? menuItemElement.OutlineColor.Value : OutlineColor);
					int outlineWidth = (menuItemElement.OnlineWidth.HasValue ? menuItemElement.OnlineWidth.Value : OutlineWidth);
					Vector2 vector = MeasureItem(menuItemElement);
					Vector2 location = new Vector2(rectangle.Left, num5);
					if (HorizontalAlignment == HorizontalAlignmentTypes.Center)
					{
						location.X = (float)rectangle.X + (num4 - vector.X) / 2f;
					}
					else if (HorizontalAlignment == HorizontalAlignmentTypes.Right)
					{
						location.X = (float)rectangle.Right - vector.X;
					}
					num5 += vector.Y + num2;
					Color textColor = color;
					if (j == _selectedIndex)
					{
						Color value = (menuItemElement.SelectedColor.HasValue ? menuItemElement.SelectedColor.Value : SelectedColor);
						float amount = (_flashDir ? _flashTimer.PercentComplete : (1f - _flashTimer.PercentComplete));
						textColor = Color.Lerp(color, value, amount);
					}
					_itemLocations[j] = new Rectangle((int)location.X, (int)location.Y, (int)vector.X, (int)vector.Y);
					spriteBatch.DrawOutlinedText(font, menuItemElement.Text, location, textColor, outlineColor, outlineWidth);
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
