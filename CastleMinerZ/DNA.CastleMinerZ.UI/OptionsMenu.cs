using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	internal class OptionsMenu : MenuScreen
	{
		private CastleMinerZGame _game;

		public DialogScreen _deleteStorageDialog;

		private ControllerScreen _controllerScreen;

		private SettingsMenu _settingsMenu;

		private SpriteBatch SpriteBatch;

		private ScreenGroup _uiGroup;

		private TextRegionElement _descriptionText;

		private bool Cancel;

		public OptionsMenu(CastleMinerZGame game, ScreenGroup uiGroup, SpriteBatch spriteBatch)
			: base(game._largeFont, Color.White, Color.Red, false)
		{
			SpriteBatch = spriteBatch;
			SpriteFont largeFont = game._largeFont;
			_uiGroup = uiGroup;
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
			DrawArea = new Rectangle(titleSafeArea.Left, 175, titleSafeArea.Width / 2 - 125, titleSafeArea.Bottom - 175);
			HorizontalAlignment = HorizontalAlignmentTypes.Right;
			VerticalAlignment = VerticalAlignmentTypes.Top;
			LineSpacing = -10;
			_descriptionText = new TextRegionElement(_game._medLargeFont);
			_descriptionText.Location = new Vector2(titleSafeArea.Center.X + 75, 200f);
			_descriptionText.Size = new Vector2((float)titleSafeArea.Right - _descriptionText.Location.X, (float)titleSafeArea.Bottom - _descriptionText.Location.Y);
			AddMenuItem("Controls", "View in game controls and settings", OptionsMenuItems.Controls);
			AddMenuItem("Erase Storage", "Erase all worlds and stats.", OptionsMenuItems.EraseStorage);
			AddMenuItem("Optimize Storage", "Clean up storage to make saving and loading faster. This will not erase your worlds.", OptionsMenuItems.OptimizeStorage);
			AddMenuItem("Settings", "Change game settings such as volume and brightness.", OptionsMenuItems.Settings);
			AddMenuItem("Release Notes", "View the release notes.", OptionsMenuItems.ReleaseNotes);
			_deleteStorageDialog = new DialogScreen("Erase Storage", "Are you sure you want to delete everything?", null, true, _game.DialogScreenImage, _game._medFont, true);
			_deleteStorageDialog.TitlePadding = new Vector2(55f, 15f);
			_deleteStorageDialog.DescriptionPadding = new Vector2(25f, 35f);
			_deleteStorageDialog.ButtonsPadding = new Vector2(25f, 20f);
			_deleteStorageDialog.ClickSound = "Click";
			_deleteStorageDialog.OpenSound = "Popup";
			_controllerScreen = new ControllerScreen(_game, false);
			_settingsMenu = new SettingsMenu(_game);
			base.MenuItemSelected += OptionsMenu_MenuItemSelected;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override void OnMenuItemFocus(MenuItemElement selectedControl)
		{
			_descriptionText.Text = selectedControl.Description;
			base.OnMenuItemFocus(selectedControl);
		}

		private void OptionsMenu_MenuItemSelected(object sender, SelectedMenuItemArgs e)
		{
			switch ((OptionsMenuItems)e.MenuItem.Tag)
			{
			case OptionsMenuItems.Controls:
				_uiGroup.PushScreen(_controllerScreen);
				break;
			case OptionsMenuItems.EraseStorage:
				_uiGroup.ShowDialogScreen(_deleteStorageDialog, delegate
				{
					if (_deleteStorageDialog.OptionSelected != -1)
					{
						WaitScreen.DoWait(_uiGroup, "Deleting Storage...", delegate
						{
							_game.SaveDevice.DeleteStorage();
						}, null);
						_game.FrontEnd.PopToStartScreen();
					}
				});
				break;
			case OptionsMenuItems.OptimizeStorage:
				Cancel = false;
				_game.FrontEnd.OptimizeStorage();
				break;
			case OptionsMenuItems.Settings:
				_uiGroup.PushScreen(_settingsMenu);
				break;
			case OptionsMenuItems.ReleaseNotes:
				_game.FrontEnd.PushReleaseNotesScreen();
				break;
			}
		}
	}
}
