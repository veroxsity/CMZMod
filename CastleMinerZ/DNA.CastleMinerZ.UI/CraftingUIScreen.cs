using System;
using System.Collections.Generic;
using System.Text;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ.UI
{
	public class CraftingUIScreen : Screen
	{
		private const int Columns = 8;

		private const int Rows = 4;

		private const int ItemSize = 59;

		private Sprite _background;

		private Sprite _gridSelector;

		private Sprite _gridSquare;

		private CastleMinerZGame _game;

		private SpriteFont _bigFont;

		private SpriteFont _smallFont;

		private InGameHUD _hud;

		private Receipe _selectedRecipe;

		private int _selectedIngredientIndex;

		private DialogScreen _buyToCraftDialog;

		private Rectangle[] _itemLocations = new Rectangle[7];

		private Rectangle[] _recipeIngredientsItemLocations = new Rectangle[0];

		private int _recipeToScrollTo = -1;

		private float _scrollOffsetAmt;

		private StringBuilder stringBuilder = new StringBuilder();

		private int _drawSelectorIndex;

		private OneShotTimer waitScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.5));

		private OneShotTimer autoScrollTimer = new OneShotTimer(TimeSpan.FromSeconds(0.10000000149011612));

		private bool mouseMovementDuringScroll;

		public int SelectedRecipeIndex
		{
			get
			{
				return _hud.PlayerInventory.DiscoveredRecipies.IndexOf(_selectedRecipe);
			}
			set
			{
				if (_hud.PlayerInventory.DiscoveredRecipies.Count > 0 && value >= 0 && value < _hud.PlayerInventory.DiscoveredRecipies.Count)
				{
					_selectedRecipe = _hud.PlayerInventory.DiscoveredRecipies[value];
				}
				else
				{
					_selectedRecipe = null;
				}
			}
		}

		private PlayerInventory Inventory
		{
			get
			{
				return _hud.PlayerInventory;
			}
		}

		private List<Receipe> DiscoveredReceipes
		{
			get
			{
				return _hud.PlayerInventory.DiscoveredRecipies;
			}
		}

		public CraftingUIScreen(CastleMinerZGame game, InGameHUD hud)
			: base(true, false)
		{
			_game = game;
			_hud = hud;
			_bigFont = _game._medFont;
			_smallFont = _game._smallFont;
			_background = _game._uiSprites["BlockUIBack"];
			_gridSelector = _game._uiSprites["Selector"];
			_gridSquare = _game._uiSprites["SingleGrid"];
			_buyToCraftDialog = new DialogScreen("Purchase Game", "You must purchase the game to craft this item", null, false, _game.DialogScreenImage, _game._medFont, true);
			_buyToCraftDialog.TitlePadding = new Vector2(55f, 15f);
			_buyToCraftDialog.DescriptionPadding = new Vector2(25f, 35f);
			_buyToCraftDialog.ButtonsPadding = new Vector2(25f, 20f);
			_buyToCraftDialog.ClickSound = "Click";
			_buyToCraftDialog.OpenSound = "Popup";
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			spriteBatch.Begin();
			Rectangle destinationRectangle = new Rectangle(titleSafeArea.Center.X - _background.Width / 2, titleSafeArea.Center.Y - _background.Height / 2, _background.Width, _background.Height);
			SpriteFont smallFont = CastleMinerZGame.Instance._smallFont;
			_background.Draw(spriteBatch, destinationRectangle, Color.White);
			Vector2 vector = new Vector2(destinationRectangle.Left, destinationRectangle.Top);
			PlayerInventory playerInventory = _hud.PlayerInventory;
			Vector2 vector2 = new Vector2(404f, 334f);
			for (int i = 0; i < 4; i++)
			{
				for (int j = 0; j < 8; j++)
				{
					InventoryItem inventoryItem = playerInventory.Inventory[i * 8 + j];
					if (inventoryItem != null)
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
				if (inventoryItem2 != null)
				{
					Vector2 vector5 = vector4 + 59f * new Vector2(k, 0f);
					inventoryItem2.Draw2D(spriteBatch, new Rectangle((int)vector5.X, (int)vector5.Y, 59, 59));
				}
			}
			Color color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
			if (DiscoveredReceipes.Count > 0)
			{
				if (_selectedRecipe == null)
				{
					_selectedRecipe = DiscoveredReceipes[0];
				}
				int num = 184;
				int num2 = 39;
				int num3 = 131;
				int num4 = 77;
				int selectedRecipeIndex = SelectedRecipeIndex;
				for (int l = -2; l < 5; l++)
				{
					int num5 = selectedRecipeIndex + l;
					if (num5 < 0)
					{
						_itemLocations[l + 2] = Rectangle.Empty;
						continue;
					}
					if (num5 >= DiscoveredReceipes.Count)
					{
						_itemLocations[l + 2] = Rectangle.Empty;
						continue;
					}
					Receipe receipe = DiscoveredReceipes[num5];
					Vector2 position = vector + new Vector2(num2, num + l * num4);
					bool flag = Inventory.CanCraft(receipe);
					_gridSquare.Draw(spriteBatch, position, flag ? Color.White : color);
					receipe.Result.Draw2D(spriteBatch, new Rectangle((int)position.X + 4, (int)(position.Y + 4f + _scrollOffsetAmt), 59, 59), Inventory.CanCraft(receipe) ? Color.White : color);
					_itemLocations[l + 2] = new Rectangle((int)position.X + 4, (int)(position.Y + 4f - 9f + _scrollOffsetAmt), 59, 77);
				}
				if (_recipeIngredientsItemLocations.Length != _selectedRecipe.Ingredients.Count)
				{
					_recipeIngredientsItemLocations = new Rectangle[_selectedRecipe.Ingredients.Count];
				}
				for (int m = 0; m < _selectedRecipe.Ingredients.Count; m++)
				{
					Vector2 position2 = vector + new Vector2(num3 + m * num4, num);
					int num6 = Inventory.CountItems(_selectedRecipe.Ingredients[m].ItemClass);
					bool flag2 = num6 >= _selectedRecipe.Ingredients[m].StackCount;
					_gridSquare.Draw(spriteBatch, position2, flag2 ? Color.White : color);
					_selectedRecipe.Ingredients[m].Draw2D(spriteBatch, new Rectangle((int)position2.X + 4, (int)position2.Y + 4, 59, 59), flag2 ? Color.White : color);
					_recipeIngredientsItemLocations[m] = new Rectangle((int)position2.X + 4, (int)position2.Y + 4, 59, 59);
				}
				Vector2 location = new Vector2(404f, vector.Y + 20f);
				Vector2 position3 = vector + new Vector2(num2, num);
				InventoryItem inventoryItem3 = _selectedRecipe.Result;
				if (_selectedIngredientIndex > 0)
				{
					position3 = vector + new Vector2(num3 + (_selectedIngredientIndex - 1) * num4, num);
					inventoryItem3 = _selectedRecipe.Ingredients[_selectedIngredientIndex - 1];
				}
				else if (_drawSelectorIndex != SelectedRecipeIndex)
				{
					position3 = vector + new Vector2(num2, num + (_drawSelectorIndex - SelectedRecipeIndex) * num4);
				}
				spriteBatch.DrawOutlinedText(_bigFont, inventoryItem3.Name, location, Color.White, Color.Black, 2);
				Vector2 location2 = new Vector2(location.X, location.Y + _bigFont.MeasureString(inventoryItem3.Name).Y);
				spriteBatch.DrawOutlinedText(_smallFont, inventoryItem3.Description1, location2, Color.White, Color.Black, 1);
				location2.Y += _smallFont.MeasureString(inventoryItem3.Description1).Y;
				spriteBatch.DrawOutlinedText(_smallFont, inventoryItem3.Description2, location2, Color.White, Color.Black, 1);
				Vector2 location3 = new Vector2(366f, 210f);
				spriteBatch.DrawOutlinedText(_bigFont, "Components: ", location3, Color.White, Color.Black, 2);
				Vector2 vector6 = _bigFont.MeasureString("Press ");
				float y = vector6.Y;
				float num7 = y / (float)ControllerImages.A.Height * (float)ControllerImages.A.Width;
				spriteBatch.DrawOutlinedText(_bigFont, "Press ", new Vector2(location3.X, location3.Y - vector6.Y), Color.White, Color.Black, 2);
				spriteBatch.Draw(ControllerImages.A, new Rectangle((int)(location3.X + vector6.X), (int)(location3.Y - vector6.Y), (int)num7, (int)y), Color.White);
				spriteBatch.DrawOutlinedText(_bigFont, " To Create Item", new Vector2(location3.X + vector6.X + num7, location3.Y - vector6.Y), Color.White, Color.Black, 2);
				position3 -= new Vector2(2f, 2f);
				_gridSelector.Draw(spriteBatch, position3, Color.White);
			}
			else
			{
				_selectedRecipe = null;
				_selectedIngredientIndex = 0;
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}

		public int OtherIngredientHitTest(Point p)
		{
			for (int i = 0; i < _itemLocations.Length; i++)
			{
				if (i != 2 && _itemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public int RecipeIngredientHitTest(Point p)
		{
			for (int i = 0; i < _recipeIngredientsItemLocations.Length; i++)
			{
				if (_recipeIngredientsItemLocations[i].Contains(p))
				{
					return i;
				}
			}
			return -1;
		}

		public bool SelectedIngredientHitTest(Point p)
		{
			if (_itemLocations[2].Contains(p))
			{
				return true;
			}
			return false;
		}

		protected override bool OnPlayerInput(InputManager inputManager, GameController controller, KeyboardInput chatPad, GameTime gameTime)
		{
			int num = RecipeIngredientHitTest(inputManager.Mouse.Position);
			if (num >= 0)
			{
				_drawSelectorIndex = SelectedRecipeIndex;
				if (_selectedIngredientIndex != num + 1)
				{
					SoundManager.Instance.PlayInstance("Click");
					_selectedIngredientIndex = num + 1;
				}
			}
			if (controller.PressedButtons.A && _drawSelectorIndex == SelectedRecipeIndex)
			{
				if (_selectedIngredientIndex > 0)
				{
					if (_selectedRecipe != null)
					{
						for (int i = 0; i < DiscoveredReceipes.Count; i++)
						{
							if (DiscoveredReceipes[i].Result.ItemClass == _selectedRecipe.Ingredients[_selectedIngredientIndex - 1].ItemClass)
							{
								SelectedRecipeIndex = i;
								_selectedIngredientIndex = 0;
								SoundManager.Instance.PlayInstance("Click");
								break;
							}
							if (i == DiscoveredReceipes.Count - 1)
							{
								SoundManager.Instance.PlayInstance("Error");
							}
						}
					}
					else
					{
						SoundManager.Instance.PlayInstance("Error");
					}
				}
				else if (_selectedRecipe == null || !Inventory.CanCraft(_selectedRecipe))
				{
					SoundManager.Instance.PlayInstance("Error");
				}
				else
				{
					InventoryItem inventoryItem = _selectedRecipe.Result.ItemClass.CreateItem(_selectedRecipe.Result.StackCount);
					if (inventoryItem is GunInventoryItem && false /* Guide.IsTrialMode patched out for RGH */)
					{
						_game.GameScreen._uiGroup.ShowDialogScreen(_buyToCraftDialog, delegate
						{
							_game.ShowMarketPlace();
						});
					}
					else
					{
						Inventory.Craft(_selectedRecipe);
						_selectedIngredientIndex = 0;
						SoundManager.Instance.PlayInstance("craft");
						CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(_selectedRecipe.Result.ItemClass.ID);
						itemStats.Crafted++;
						if (_game.GameMode == GameModeTypes.Endurance)
						{
							CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
						}
					}
				}
			}
			if (inputManager.Mouse.LeftButtonPressed || inputManager.Keyboard.WasKeyPressed(Keys.Enter))
			{
				if (_selectedIngredientIndex > 0)
				{
					if (_selectedRecipe != null && RecipeIngredientHitTest(inputManager.Mouse.Position) >= 0)
					{
						for (int num2 = 0; num2 < DiscoveredReceipes.Count; num2++)
						{
							if (DiscoveredReceipes[num2].Result.ItemClass == _selectedRecipe.Ingredients[_selectedIngredientIndex - 1].ItemClass)
							{
								_recipeToScrollTo = num2;
								_selectedIngredientIndex = 0;
								SoundManager.Instance.PlayInstance("Click");
								break;
							}
							if (num2 == DiscoveredReceipes.Count - 1)
							{
								SoundManager.Instance.PlayInstance("Error");
							}
						}
					}
					else
					{
						SoundManager.Instance.PlayInstance("Error");
					}
				}
				else if (SelectedIngredientHitTest(inputManager.Mouse.Position))
				{
					if (_selectedRecipe == null || !Inventory.CanCraft(_selectedRecipe))
					{
						SoundManager.Instance.PlayInstance("Error");
					}
					else
					{
						InventoryItem inventoryItem2 = _selectedRecipe.Result.ItemClass.CreateItem(_selectedRecipe.Result.StackCount);
						if (inventoryItem2 is GunInventoryItem && false /* Guide.IsTrialMode patched out for RGH */)
						{
							_game.GameScreen._uiGroup.ShowDialogScreen(_buyToCraftDialog, delegate
							{
								_game.ShowMarketPlace();
							});
						}
						else
						{
							if (inputManager.Keyboard.IsKeyDown(Keys.LeftShift) || inputManager.Keyboard.IsKeyDown(Keys.RightShift))
							{
								while (_selectedRecipe != null && Inventory.CanCraft(_selectedRecipe))
								{
									Inventory.Craft(_selectedRecipe);
								}
							}
							else
							{
								Inventory.Craft(_selectedRecipe);
							}
							_selectedIngredientIndex = 0;
							SoundManager.Instance.PlayInstance("craft");
							CastleMinerZPlayerStats.ItemStats itemStats2 = CastleMinerZGame.Instance.PlayerStats.GetItemStats(_selectedRecipe.Result.ItemClass.ID);
							itemStats2.Crafted++;
							if (_game.GameMode == GameModeTypes.Endurance)
							{
								CastleMinerZGame.Instance.PlayerStats.TotalItemsCrafted++;
							}
						}
					}
				}
				else
				{
					int num3 = OtherIngredientHitTest(inputManager.Mouse.Position);
					if (num3 >= 0)
					{
						_recipeToScrollTo = num3 - 2 + SelectedRecipeIndex;
						_selectedIngredientIndex = 0;
					}
				}
			}
			if (controller.PressedButtons.Start)
			{
				_game.GameScreen.ShowInGameMenu();
				SoundManager.Instance.PlayInstance("Click");
			}
			if (controller.PressedButtons.Back || inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				SoundManager.Instance.PlayInstance("Click");
				PopMe();
			}
			if (controller.PressedButtons.B)
			{
				SoundManager.Instance.PlayInstance("Click");
				PopMe();
			}
			bool x = controller.PressedButtons.X;
			if (controller.PressedButtons.Y || inputManager.Keyboard.WasKeyPressed(Keys.E))
			{
				SoundManager.Instance.PlayInstance("Click");
				PopMe();
			}
			if (controller.PressedDPad.Down || (controller.CurrentState.ThumbSticks.Left.Y < -0.2f && controller.LastState.ThumbSticks.Left.Y >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Down) || inputManager.Mouse.DeltaWheel < 0)
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectDown())
				{
					SoundManager.Instance.PlayInstance("Click");
					_selectedIngredientIndex = 0;
				}
			}
			if (controller.PressedDPad.Up || (controller.CurrentState.ThumbSticks.Left.Y > 0.2f && controller.LastState.ThumbSticks.Left.Y <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Up) || inputManager.Mouse.DeltaWheel > 0)
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectUp())
				{
					SoundManager.Instance.PlayInstance("Click");
					_selectedIngredientIndex = 0;
				}
			}
			if (controller.PressedButtons.LeftShoulder || controller.PressedDPad.Left || (controller.CurrentState.ThumbSticks.Left.X < -0.2f && controller.LastState.ThumbSticks.Left.X >= -0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Left))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectLeft())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			if (controller.PressedButtons.RightShoulder || controller.PressedDPad.Right || (controller.CurrentState.ThumbSticks.Left.X > 0.2f && controller.LastState.ThumbSticks.Left.X <= 0.2f) || inputManager.Keyboard.WasKeyPressed(Keys.Right))
			{
				waitScrollTimer.Reset();
				autoScrollTimer.Reset();
				if (SelectRight())
				{
					SoundManager.Instance.PlayInstance("Click");
				}
			}
			waitScrollTimer.Update(gameTime.ElapsedGameTime);
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
				else if (controller.CurrentState.ThumbSticks.Left.X < -0.2f)
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
				else if (controller.CurrentState.ThumbSticks.Left.X > 0.2f)
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
			if (_recipeToScrollTo == -1 || inputManager.Mouse.Position != inputManager.Mouse.LastPosition)
			{
				mouseMovementDuringScroll = true;
				if (num < 0)
				{
					num = OtherIngredientHitTest(inputManager.Mouse.Position);
					if (_selectedIngredientIndex != 0 && SelectedIngredientHitTest(inputManager.Mouse.Position))
					{
						_drawSelectorIndex = SelectedRecipeIndex;
						SoundManager.Instance.PlayInstance("Click");
						_selectedIngredientIndex = 0;
					}
					else if (num >= 0)
					{
						if (_drawSelectorIndex != num - 2 + SelectedRecipeIndex)
						{
							SoundManager.Instance.PlayInstance("Click");
							_selectedIngredientIndex = 0;
							_drawSelectorIndex = num - 2 + SelectedRecipeIndex;
						}
					}
					else if (_drawSelectorIndex != SelectedRecipeIndex)
					{
						SoundManager.Instance.PlayInstance("Click");
						_drawSelectorIndex = SelectedRecipeIndex;
					}
				}
			}
			else
			{
				mouseMovementDuringScroll = false;
			}
			return base.OnPlayerInput(inputManager, controller, chatPad, gameTime);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			if (_hud.LocalPlayer.Dead)
			{
				PopMe();
			}
			if (_recipeToScrollTo >= 0)
			{
				if (_recipeToScrollTo < SelectedRecipeIndex)
				{
					_scrollOffsetAmt += 462f * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (_scrollOffsetAmt >= 77f)
					{
						_scrollOffsetAmt = 0f;
						SelectedRecipeIndex--;
						_selectedIngredientIndex = 0;
						if (!mouseMovementDuringScroll)
						{
							if (_drawSelectorIndex > 0)
							{
								_drawSelectorIndex--;
							}
							else
							{
								_drawSelectorIndex = SelectedRecipeIndex;
							}
						}
					}
				}
				else if (_recipeToScrollTo > SelectedRecipeIndex)
				{
					_scrollOffsetAmt -= 462f * (float)gameTime.ElapsedGameTime.TotalSeconds;
					if (_scrollOffsetAmt <= -77f)
					{
						_scrollOffsetAmt = 0f;
						SelectedRecipeIndex++;
						_selectedIngredientIndex = 0;
						if (!mouseMovementDuringScroll)
						{
							if (_drawSelectorIndex < DiscoveredReceipes.Count - 1)
							{
								_drawSelectorIndex++;
							}
							else
							{
								_drawSelectorIndex = SelectedRecipeIndex;
							}
						}
					}
				}
				else
				{
					_recipeToScrollTo = -1;
					_scrollOffsetAmt = 0f;
				}
			}
			base.Update(game, gameTime);
		}

		public void Reset()
		{
			SelectedRecipeIndex = 0;
			_selectedIngredientIndex = 0;
		}

		public bool SelectUp()
		{
			if (SelectedRecipeIndex > 0)
			{
				SelectedRecipeIndex--;
				_selectedIngredientIndex = 0;
				return true;
			}
			return false;
		}

		public bool SelectDown()
		{
			if (SelectedRecipeIndex < DiscoveredReceipes.Count - 1)
			{
				SelectedRecipeIndex++;
				_selectedIngredientIndex = 0;
				return true;
			}
			return false;
		}

		public bool SelectLeft()
		{
			if (_selectedIngredientIndex > 0)
			{
				_selectedIngredientIndex--;
				return true;
			}
			return false;
		}

		public bool SelectRight()
		{
			if (_selectedIngredientIndex < _selectedRecipe.Ingredients.Count)
			{
				_selectedIngredientIndex++;
				return true;
			}
			return false;
		}
	}
}
