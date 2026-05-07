using System;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.UI
{
	public class CrateScreen : Screen
	{
		private const int Columns = 8;

		private const int Rows = 4;

		private const int ItemSize = 59;

		public Crate CurrentCrate;

		private bool _selectorInCrateGrid;

		private Sprite _grid;

		private Sprite _background;

		private Sprite _gridSelector;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Point _selectedLocation = new Point(0, 0);

		private InventoryItem _holdingItem;

		private Rectangle[] _crateItemLocations = new Rectangle[32];

		private Rectangle[] _inventoryItemLocations = new Rectangle[40];

		private Rectangle _backgroundRect;

		private bool _mousePointerActive;

		private Point _mousePointerLocation = default(Point);

		private bool _hitTestTrue;

		private StringBuilder stringBuilder = new StringBuilder();

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private OneShotTimer itemCountWaitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer itemCountAutoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private int SelectorIndex
		{
			get
			{
				return _selectedLocation.X + _selectedLocation.Y * 8;
			}
		}

		public InventoryItem SelectedItem
		{
			get
			{
				if (_selectedLocation.Y < 4)
				{
					if (_selectorInCrateGrid)
					{
						return CurrentCrate.Inventory[_selectedLocation.X + _selectedLocation.Y * 8];
					}
					return _hud.PlayerInventory.Inventory[_selectedLocation.X + _selectedLocation.Y * 8];
				}
				if (_selectedLocation.Y == 4)
				{
					return _hud.PlayerInventory.InventoryTray[_selectedLocation.X];
				}
				return null;
			}
			set
			{
				if (_selectedLocation.Y < 4)
				{
					if (_selectorInCrateGrid)
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, value, CurrentCrate, _selectedLocation.X + _selectedLocation.Y * 8);
					}
					else
					{
						_hud.PlayerInventory.Inventory[_selectedLocation.X + _selectedLocation.Y * 8] = value;
					}
				}
				if (_selectedLocation.Y == 4)
				{
					_hud.PlayerInventory.InventoryTray[_selectedLocation.X] = value;
				}
			}
		}

		public bool IsSelectedSlotLocked()
		{
			if (_selectorInCrateGrid && _game.CurrentNetworkSession != null)
			{
				foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
				{
					if (remoteGamer.Tag != null)
					{
						Player player = (Player)remoteGamer.Tag;
						if (player.FocusCrate == CurrentCrate.Location && player.FocusCrateItem == _selectedLocation)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		public bool IsSlotLocked(int index)
		{
			foreach (NetworkGamer remoteGamer in _game.CurrentNetworkSession.RemoteGamers)
			{
				if (remoteGamer.Tag != null)
				{
					Player player = (Player)remoteGamer.Tag;
					int num = player.FocusCrateItem.X + player.FocusCrateItem.Y * 8;
					if (player.FocusCrate == CurrentCrate.Location && num == index)
					{
						return true;
					}
				}
			}
			return false;
		}

		public override void OnPushed()
		{
			_selectedLocation = new Point(_game.GameScreen.HUD.PlayerInventory.SelectedInventoryIndex, 4);
			_selectorInCrateGrid = false;
			CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			base.OnPushed();
		}

		public int InventoryHitTest(Point p)
		{
			for (int i = 0; i < _inventoryItemLocations.Length; i++)
			{
				if (_inventoryItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public int CrateHitTest(Point p)
		{
			for (int i = 0; i < _crateItemLocations.Length; i++)
			{
				if (_crateItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public void ForceClose()
		{
			if (_holdingItem != null)
			{
				_hud.PlayerInventory.AddInventoryItem(_holdingItem);
				_holdingItem = null;
			}
			if (SelectedItem != null && _selectorInCrateGrid && !IsSelectedSlotLocked())
			{
				Vector3 localPosition = _game.LocalPlayer.LocalPosition;
				localPosition.Y += 1f;
				PickupManager.Instance.CreatePickup(SelectedItem, localPosition, true);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			PopMe();
			CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			_game.GameScreen.ShowBlockPicker();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (_hud.LocalPlayer.Dead)
			{
				PopMe();
			}
			else if (CurrentCrate == null)
			{
				PopMe();
				_game.GameScreen.ShowBlockPicker();
			}
			else if (SelectedItem != null && SelectedItem.StackCount < 1)
			{
				SelectedItem = null;
			}
			base.Update(game, gameTime);
		}

		public CrateScreen(CastleMinerZGame game, InGameHUD hud)
			: base(true, false)
		{
			_hud = hud;
			_game = game;
			_bigFont = _game._medFont;
			_smallFont = _game._smallFont;
			_background = _game._uiSprites["BlockUIBack"];
			_gridSelector = _game._uiSprites["Selector"];
			_grid = _game._uiSprites["InventoryGrid"];
			Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
			_backgroundRect = new Rectangle(titleSafeArea.Center.X - _background.Width / 2, titleSafeArea.Center.Y - _background.Height / 2, _background.Width, _background.Height);
			Vector2 vector = new Vector2(404f, 334f);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					Vector2 vector2 = vector + 59f * new Vector2(j, i);
					_inventoryItemLocations[j + i * 8] = new Rectangle((int)vector2.X, (int)vector2.Y, 59, 59);
				}
			}
			Vector2 vector3 = new Vector2(404f, 584f);
			for (int k = 0; k < 8; k++)
			{
				Vector2 vector4 = vector3 + 59f * new Vector2(k, 0f);
				_inventoryItemLocations[k + 32] = new Rectangle((int)vector4.X, (int)vector4.Y, 59, 59);
			}
			Vector2 vector5 = new Vector2(_backgroundRect.Left, _backgroundRect.Top);
			Vector2 vector6 = new Vector2((int)vector5.X + 169, (int)vector5.Y + 24);
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 8; m++)
				{
					Vector2 vector7 = vector6 + 59f * new Vector2(m, l);
					_crateItemLocations[m + l * 8] = new Rectangle((int)vector7.X, (int)vector7.Y, 59, 59);
				}
			}
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			spriteBatch.Begin();
			SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
			_background.Draw(spriteBatch, _backgroundRect, Color.White);
			Vector2 vector = new Vector2(_backgroundRect.Left, _backgroundRect.Top);
			_grid.Draw(spriteBatch, new Rectangle((int)vector.X + 165, (int)vector.Y + 20, _grid.Width, _grid.Height), Color.White);
			PlayerInventory playerInventory = _hud.PlayerInventory;
			Vector2 vector2 = new Vector2(404f, 334f);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = playerInventory.Inventory[i * 8 + j];
					if (inventoryItem != null && inventoryItem != _holdingItem && (_holdingItem == null || SelectedItem != inventoryItem || _mousePointerActive))
					{
						Vector2 vector3 = vector2 + 59f * new Vector2(j, i);
						inventoryItem.Draw2D(spriteBatch, new Rectangle((int)vector3.X, (int)vector3.Y, 59, 59));
					}
				}
			}
			Vector2 vector4 = new Vector2(404f, 584f);
			for (int k = 0; k < 8; k++)
			{
				InventoryItem inventoryItem2 = playerInventory.InventoryTray[k];
				if (inventoryItem2 != null && inventoryItem2 != _holdingItem && (_holdingItem == null || SelectedItem != inventoryItem2 || _mousePointerActive))
				{
					Vector2 vector5 = vector4 + 59f * new Vector2(k, 0f);
					inventoryItem2.Draw2D(spriteBatch, new Rectangle((int)vector5.X, (int)vector5.Y, 59, 59));
				}
			}
			InventoryItem[] inventory = CurrentCrate.Inventory;
			Vector2 vector6 = new Vector2((int)vector.X + 169, (int)vector.Y + 24);
			for (int l = 0; l < 4; l++)
			{
				for (int m = 0; m < 8; m++)
				{
					InventoryItem inventoryItem3 = inventory[l * 8 + m];
					if (inventoryItem3 != null && inventoryItem3 != _holdingItem && (_holdingItem == null || SelectedItem != inventoryItem3 || _mousePointerActive))
					{
						Vector2 vector7 = vector6 + 59f * new Vector2(m, l);
						inventoryItem3.Draw2D(spriteBatch, new Rectangle((int)vector7.X, (int)vector7.Y, 59, 59));
					}
				}
			}
			Vector2 vector8 = ((!_selectorInCrateGrid) ? vector2 : vector6);
			Vector2 position = ((_selectedLocation.Y >= 4) ? (vector4 + 59f * new Vector2(_selectedLocation.X, 0f)) : (vector8 + 59f * new Vector2(_selectedLocation.X, _selectedLocation.Y)));
			position -= new Vector2(4f, 4f);
			if (!_mousePointerActive || _hitTestTrue)
			{
				_gridSelector.Draw(spriteBatch, position, (_holdingItem == null) ? Color.White : Color.Red);
			}
			if (_holdingItem != null)
			{
				if (_mousePointerActive)
				{
					_holdingItem.Draw2D(spriteBatch, new Rectangle(_mousePointerLocation.X - 29, _mousePointerLocation.Y - 29, 59, 59));
				}
				else
				{
					_holdingItem.Draw2D(spriteBatch, new Rectangle((int)position.X, (int)position.Y, 59, 59));
				}
			}
			InventoryItem selectedItem = SelectedItem;
			InventoryItem holdingItem = _holdingItem;
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		private bool SwapSelectedItemLocation()
		{
			if (!_selectorInCrateGrid)
			{
				for (int i = 0; i < CurrentCrate.Inventory.Length; i++)
				{
					if (CurrentCrate.Inventory[i] != null && !IsSlotLocked(i))
					{
						int stackCount = SelectedItem.StackCount;
						InventoryItem selectedItem = SelectedItem;
						CurrentCrate.Inventory[i].Stack(selectedItem);
						SelectedItem = selectedItem;
						if (selectedItem.StackCount != stackCount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, CurrentCrate.Inventory[i], CurrentCrate, i);
						}
					}
				}
				if (SelectedItem.StackCount <= 0)
				{
					SelectedItem = null;
					return true;
				}
				for (int j = 0; j < CurrentCrate.Inventory.Length; j++)
				{
					if (CurrentCrate.Inventory[j] == null && !IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, SelectedItem, CurrentCrate, j);
						SelectedItem = null;
						return true;
					}
				}
			}
			else
			{
				for (int k = 0; k < _hud.PlayerInventory.InventoryTray.Length; k++)
				{
					if (_hud.PlayerInventory.InventoryTray[k] != null)
					{
						InventoryItem selectedItem2 = SelectedItem;
						_hud.PlayerInventory.InventoryTray[k].Stack(selectedItem2);
						SelectedItem = selectedItem2;
					}
				}
				for (int l = 0; l < _hud.PlayerInventory.Inventory.Length; l++)
				{
					if (_hud.PlayerInventory.Inventory[l] != null)
					{
						InventoryItem selectedItem3 = SelectedItem;
						_hud.PlayerInventory.Inventory[l].Stack(selectedItem3);
						SelectedItem = selectedItem3;
					}
				}
				if (SelectedItem.StackCount <= 0)
				{
					SelectedItem = null;
					_hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				for (int m = 0; m < _hud.PlayerInventory.InventoryTray.Length; m++)
				{
					if (_hud.PlayerInventory.InventoryTray[m] == null)
					{
						_hud.PlayerInventory.InventoryTray[m] = SelectedItem;
						_hud.PlayerInventory.DiscoverRecipies();
						SelectedItem = null;
						return true;
					}
				}
				for (int n = 0; n < _hud.PlayerInventory.Inventory.Length; n++)
				{
					if (_hud.PlayerInventory.Inventory[n] == null)
					{
						_hud.PlayerInventory.Inventory[n] = SelectedItem;
						_hud.PlayerInventory.DiscoverRecipies();
						SelectedItem = null;
						return true;
					}
				}
			}
			return false;
		}

		private bool SwapHoldingItemLocation()
		{
			if (!_selectorInCrateGrid)
			{
				for (int i = 0; i < CurrentCrate.Inventory.Length; i++)
				{
					if (CurrentCrate.Inventory[i] != null && !IsSlotLocked(i))
					{
						int stackCount = _holdingItem.StackCount;
						CurrentCrate.Inventory[i].Stack(_holdingItem);
						if (_holdingItem.StackCount != stackCount)
						{
							ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, CurrentCrate.Inventory[i], CurrentCrate, i);
						}
					}
				}
				if (_holdingItem.StackCount <= 0)
				{
					_holdingItem = null;
					return true;
				}
				for (int j = 0; j < CurrentCrate.Inventory.Length; j++)
				{
					if (CurrentCrate.Inventory[j] == null && !IsSlotLocked(j))
					{
						ItemCrateMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _holdingItem, CurrentCrate, j);
						_holdingItem = null;
						return true;
					}
				}
			}
			else
			{
				for (int k = 0; k < _hud.PlayerInventory.InventoryTray.Length; k++)
				{
					if (_hud.PlayerInventory.InventoryTray[k] != null)
					{
						_hud.PlayerInventory.InventoryTray[k].Stack(_holdingItem);
					}
				}
				for (int l = 0; l < _hud.PlayerInventory.Inventory.Length; l++)
				{
					if (_hud.PlayerInventory.Inventory[l] != null)
					{
						_hud.PlayerInventory.Inventory[l].Stack(_holdingItem);
					}
				}
				if (_holdingItem.StackCount <= 0)
				{
					_holdingItem = null;
					_hud.PlayerInventory.DiscoverRecipies();
					return true;
				}
				for (int m = 0; m < _hud.PlayerInventory.InventoryTray.Length; m++)
				{
					if (_hud.PlayerInventory.InventoryTray[m] == null)
					{
						_hud.PlayerInventory.InventoryTray[m] = _holdingItem;
						_hud.PlayerInventory.DiscoverRecipies();
						_holdingItem = null;
						return true;
					}
				}
				for (int n = 0; n < _hud.PlayerInventory.Inventory.Length; n++)
				{
					if (_hud.PlayerInventory.Inventory[n] == null)
					{
						_hud.PlayerInventory.Inventory[n] = _holdingItem;
						_hud.PlayerInventory.DiscoverRecipies();
						_holdingItem = null;
						return true;
					}
				}
			}
			return false;
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			if (CurrentCrate.Destroyed)
			{
				ForceClose();
				return false;
			}
			if (controller.PressedButtons.Y || ((inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift)) && inputManager.Mouse.LeftButtonPressed && _hitTestTrue))
			{
				if (_holdingItem != null)
				{
					if (SwapHoldingItemLocation())
					{
						SoundManager.Instance.PlayInstance("Click");
					}
				}
				else if (SelectedItem != null && !IsSelectedSlotLocked() && SwapSelectedItemLocation())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.PressedButtons.A || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || (inputManager.Mouse.LeftButtonPressed && _hitTestTrue))
			{
				if (!IsSelectedSlotLocked())
				{
					if (_holdingItem == null)
					{
						if (SelectedItem != null)
						{
							_holdingItem = SelectedItem;
							if (_selectorInCrateGrid)
							{
								SelectedItem = null;
							}
							else
							{
								_hud.PlayerInventory.Remove(_holdingItem);
							}
						}
						SoundManager.Instance.PlayInstance("Click");
					}
					else
					{
						InventoryItem selectedItem = SelectedItem;
						if (selectedItem != null && selectedItem.CanStack(_holdingItem))
						{
							selectedItem.Stack(_holdingItem);
							SelectedItem = selectedItem;
							if (_holdingItem.StackCount == 0)
							{
								_holdingItem = null;
							}
						}
						else
						{
							SelectedItem = _holdingItem;
							_holdingItem = selectedItem;
						}
						SoundManager.Instance.PlayInstance("Click");
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.RightStick || (inputManager.Mouse.RightButtonPressed && _hitTestTrue))
			{
				if (!IsSelectedSlotLocked())
				{
					if (_holdingItem == null)
					{
						if (SelectedItem != null)
						{
							SoundManager.Instance.PlayInstance("Click");
							if (SelectedItem.StackCount == 1)
							{
								_holdingItem = SelectedItem;
								if (_selectorInCrateGrid)
								{
									SelectedItem = null;
								}
								else
								{
									_hud.PlayerInventory.Remove(_holdingItem);
								}
							}
							else
							{
								InventoryItem selectedItem2 = SelectedItem;
								_holdingItem = selectedItem2.Split();
								SelectedItem = selectedItem2;
							}
						}
					}
					else if (_holdingItem != null)
					{
						SoundManager.Instance.PlayInstance("Click");
						if (SelectedItem != null)
						{
							if (_holdingItem.ItemClass == SelectedItem.ItemClass)
							{
								if (inputManager.Mouse.RightButtonPressed)
								{
									if (SelectedItem.StackCount < SelectedItem.MaxStackCount)
									{
										if (_holdingItem.StackCount > 1)
										{
											InventoryItem selectedItem3 = SelectedItem;
											selectedItem3.Stack(_holdingItem.PopOneItem());
											SelectedItem = selectedItem3;
										}
										else
										{
											SelectedItem.Stack(_holdingItem);
											_holdingItem = null;
										}
									}
								}
								else if (SelectedItem.StackCount > 1)
								{
									InventoryItem selectedItem4 = SelectedItem;
									InventoryItem item = selectedItem4.Split();
									_holdingItem.Stack(item);
									selectedItem4.Stack(item);
									SelectedItem = selectedItem4;
								}
								else if (_holdingItem.StackCount < _holdingItem.MaxStackCount)
								{
									_holdingItem.Stack(SelectedItem);
									SelectedItem = null;
								}
							}
							else
							{
								InventoryItem selectedItem5 = SelectedItem;
								SelectedItem = _holdingItem;
								_holdingItem = selectedItem5;
							}
						}
						else if (inputManager.Mouse.RightButtonPressed)
						{
							if (_holdingItem.StackCount > 1)
							{
								SelectedItem = _holdingItem.PopOneItem();
							}
							else
							{
								SelectedItem = _holdingItem;
								_holdingItem = null;
							}
						}
						else if (_holdingItem.StackCount > 1)
						{
							SelectedItem = _holdingItem.Split();
						}
						else
						{
							SelectedItem = _holdingItem;
							_holdingItem = null;
						}
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.PressedButtons.Start)
			{
				_game.GameScreen.ShowInGameMenu();
				SoundManager.Instance.PlayInstance("Click");
			}
			else if (controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				_holdingItem = null;
				PopMe();
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
			}
			else if (controller.PressedButtons.B)
			{
				SoundManager.Instance.PlayInstance("Click");
				if (_holdingItem != null)
				{
					_hud.PlayerInventory.AddInventoryItem(_holdingItem);
					_holdingItem = null;
				}
				else
				{
					PopMe();
					CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, IntVector3.Zero, Point.Zero);
				}
			}
			else if (inputManager.Mouse.LeftButtonPressed && !_backgroundRect.Contains(inputManager.Mouse.Position) && _holdingItem != null)
			{
				_mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition = _game.LocalPlayer.LocalPosition;
				localPosition.Y += 1f;
				PickupManager.Instance.CreatePickup(_holdingItem, localPosition, true);
				SoundManager.Instance.PlayInstance("dropitem");
				_holdingItem = null;
			}
			else if (inputManager.Mouse.RightButtonPressed && !_backgroundRect.Contains(inputManager.Mouse.Position) && _holdingItem != null)
			{
				InventoryItem item2;
				if (_holdingItem.StackCount > 1)
				{
					item2 = _holdingItem.PopOneItem();
				}
				else
				{
					item2 = _holdingItem;
					_holdingItem = null;
				}
				_mousePointerActive = true;
				SoundManager.Instance.PlayInstance("Click");
				Vector3 localPosition2 = _game.LocalPlayer.LocalPosition;
				localPosition2.Y += 1f;
				PickupManager.Instance.CreatePickup(item2, localPosition2, true);
				SoundManager.Instance.PlayInstance("dropitem");
			}
			else if (controller.PressedButtons.X || inputManager.Keyboard.WasKeyPressed(Keys.Q))
			{
				if (!IsSelectedSlotLocked())
				{
					SoundManager.Instance.PlayInstance("Click");
					if (_holdingItem != null)
					{
						Vector3 localPosition3 = _game.LocalPlayer.LocalPosition;
						localPosition3.Y += 1f;
						PickupManager.Instance.CreatePickup(_holdingItem, localPosition3, true);
						SoundManager.Instance.PlayInstance("dropitem");
						_holdingItem = null;
					}
					else if (SelectedItem != null)
					{
						if (_selectorInCrateGrid)
						{
							Vector3 localPosition4 = _game.LocalPlayer.LocalPosition;
							localPosition4.Y += 1f;
							PickupManager.Instance.CreatePickup(SelectedItem, localPosition4, true);
							SoundManager.Instance.PlayInstance("dropitem");
							SelectedItem = null;
						}
						else
						{
							_hud.PlayerInventory.DropItem(SelectedItem);
						}
					}
				}
				else
				{
					SoundManager.Instance.PlayInstance("Error");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f && controller.LastState.ThumbSticks.Right.Y >= -0.2f)
			{
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel < 0 && _hitTestTrue)
			{
				_mousePointerActive = true;
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (inputManager.Mouse.DeltaWheel > 0 && _hitTestTrue)
			{
				_mousePointerActive = true;
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f && controller.LastState.ThumbSticks.Right.Y <= 0.2f)
			{
				itemCountWaitScrollTimer.Reset();
				itemCountAutoScrollTimer.Reset();
				if (ItemCountUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedButtons.LeftShoulder || controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < -0.2f && controller.LastState.ThumbSticks.Left.X >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Left))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectLeft())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			if (controller.PressedButtons.RightShoulder || controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > 0.2f && controller.LastState.ThumbSticks.Left.X <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Right))
			{
				_mousePointerActive = false;
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectRight())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
				CrateFocusMessage.Send((LocalNetworkGamer)_game.LocalPlayer.Gamer, _selectorInCrateGrid ? CurrentCrate.Location : IntVector3.Zero, _selectedLocation);
			}
			itemCountWaitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (itemCountWaitScrollTimer.Expired && !controller.PressedButtons.A)
			{
				if (controller.CurrentState.ThumbSticks.Right.Y < -0.2f)
				{
					itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (itemCountAutoScrollTimer.Expired)
					{
						itemCountAutoScrollTimer.Reset();
						if (ItemCountDown())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Right.Y > 0.2f)
				{
					itemCountAutoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (itemCountAutoScrollTimer.Expired)
					{
						itemCountAutoScrollTimer.Reset();
						if (ItemCountUp())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			waitScrollTimer.Update(gameTime.ElapsedGameTime);
			if (waitScrollTimer.Expired)
			{
				if (controller.CurrentState.ThumbSticks.Left.Y < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Up))
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
				else if (controller.CurrentState.ThumbSticks.Left.Y > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Down))
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
				else if (controller.CurrentState.ThumbSticks.Left.X < -0.2f || inputManager.Keyboard.IsKeyDown(Keys.Left))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectLeft())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
				else if (controller.CurrentState.ThumbSticks.Left.X > 0.2f || inputManager.Keyboard.IsKeyDown(Keys.Right))
				{
					autoScrollTimer.Update(gameTime.ElapsedGameTime);
					if (autoScrollTimer.Expired)
					{
						autoScrollTimer.Reset();
						if (SelectRight())
						{
							SoundManager.Instance.PlayInstance("Click");
						}
					}
				}
			}
			if (inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				_mousePointerActive = true;
				_mousePointerLocation = inputManager.Mouse.Position;
				int num = InventoryHitTest(inputManager.Mouse.Position);
				if (num >= 0)
				{
					_hitTestTrue = true;
					_selectedLocation.Y = num / 8;
					_selectedLocation.X = num % 8;
					_selectorInCrateGrid = false;
				}
				else
				{
					num = CrateHitTest(inputManager.Mouse.Position);
					if (num >= 0)
					{
						_hitTestTrue = true;
						_selectedLocation.Y = num / 8;
						_selectedLocation.X = num % 8;
						_selectorInCrateGrid = true;
					}
					else
					{
						_hitTestTrue = false;
					}
				}
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public bool ItemCountDown()
		{
			if (IsSelectedSlotLocked())
			{
				return false;
			}
			if (_holdingItem == null)
			{
				return false;
			}
			if (SelectedItem == null)
			{
				if (_holdingItem.StackCount == 1)
				{
					SelectedItem = _holdingItem;
					_holdingItem = null;
				}
				else if (_holdingItem.StackCount > 1)
				{
					SelectedItem = _holdingItem.PopOneItem();
				}
			}
			else
			{
				if (_holdingItem.ItemClass != SelectedItem.ItemClass || SelectedItem.StackCount >= SelectedItem.MaxStackCount)
				{
					return false;
				}
				InventoryItem selectedItem = SelectedItem;
				if (_holdingItem.StackCount == 1)
				{
					selectedItem.Stack(_holdingItem);
					_holdingItem = null;
				}
				else if (_holdingItem.StackCount > 1)
				{
					selectedItem.Stack(_holdingItem.PopOneItem());
				}
				SelectedItem = selectedItem;
			}
			return true;
		}

		public bool ItemCountUp()
		{
			if (IsSelectedSlotLocked())
			{
				return false;
			}
			if (_holdingItem == null)
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem = SelectedItem;
					if (_selectorInCrateGrid)
					{
						SelectedItem = null;
					}
					else
					{
						_hud.PlayerInventory.Remove(_holdingItem);
					}
				}
				else if (SelectedItem.StackCount > 1)
				{
					InventoryItem selectedItem = SelectedItem;
					_holdingItem = selectedItem.PopOneItem();
					SelectedItem = selectedItem;
				}
			}
			else
			{
				if (SelectedItem == null)
				{
					return false;
				}
				if (_holdingItem.ItemClass != SelectedItem.ItemClass || _holdingItem.StackCount >= _holdingItem.MaxStackCount)
				{
					return false;
				}
				InventoryItem selectedItem2 = SelectedItem;
				if (SelectedItem.StackCount == 1)
				{
					_holdingItem.Stack(selectedItem2);
					SelectedItem = null;
				}
				else if (SelectedItem.StackCount > 1)
				{
					_holdingItem.Stack(selectedItem2.PopOneItem());
					SelectedItem = selectedItem2;
				}
			}
			return true;
		}

		public bool SelectDown()
		{
			_selectedLocation.Y++;
			if (_selectorInCrateGrid)
			{
				if (_selectedLocation.Y > 3)
				{
					_selectedLocation.Y = 0;
					_selectorInCrateGrid = false;
				}
			}
			else if (_selectedLocation.Y > 4)
			{
				_selectedLocation.Y = 0;
				_selectorInCrateGrid = true;
			}
			return true;
		}

		public bool SelectUp()
		{
			_selectedLocation.Y--;
			if (_selectorInCrateGrid)
			{
				if (_selectedLocation.Y < 0)
				{
					_selectorInCrateGrid = false;
					_selectedLocation.Y = 4;
				}
			}
			else if (_selectedLocation.Y < 0)
			{
				_selectorInCrateGrid = true;
				_selectedLocation.Y = 3;
			}
			return true;
		}

		public bool SelectLeft()
		{
			_selectedLocation.X--;
			if (_selectedLocation.X < 0)
			{
				_selectedLocation.X = 7;
			}
			return true;
		}

		public bool SelectRight()
		{
			_selectedLocation.X++;
			if (_selectedLocation.X > 7)
			{
				_selectedLocation.X = 0;
			}
			return true;
		}
	}
}
