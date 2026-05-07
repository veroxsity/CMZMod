using DNA.Drawing.UI;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.UI
{
	public class SettingsMenu : SettingScreen
	{
		private CastleMinerZGame _game;

		private BarSettingItem BrightnessBar;

		private BarSettingItem MusicVolumeBar;

		private BoolSettingItem InvertYaxis;

		private BarSettingItem ControllerSensitivityBar;

		private BoolSettingItem AutoClimb;

		public SettingsMenu(CastleMinerZGame game)
			: base(game, game._largeFont, Color.White, Color.Red, false)
		{
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			BrightnessBar = new BarSettingItem("Brightness", _game.Brightness);
			base.MenuItems.Add(BrightnessBar);
			ControllerSensitivityBar = new BarSettingItem("Controller Sensitivity", _game.PlayerStats.controllerSensitivity);
			base.MenuItems.Add(ControllerSensitivityBar);
			MusicVolumeBar = new BarSettingItem("Music Volume", _game.PlayerStats.musicVolume);
			base.MenuItems.Add(MusicVolumeBar);
			InvertYaxis = new BoolSettingItem("Invert Y Axis", _game.PlayerStats.InvertYAxis, "Inverted", "Regular");
			base.MenuItems.Add(InvertYaxis);
			AutoClimb = new BoolSettingItem("Auto Climb", _game.PlayerStats.AutoClimb, "On", "Off");
			base.MenuItems.Add(AutoClimb);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			_game.Brightness = BrightnessBar.Value / 2f;
			_game.PlayerStats.brightness = BrightnessBar.Value / 2f;
			_game.PlayerStats.InvertYAxis = InvertYaxis.On;
			_game.PlayerStats.musicVolume = MusicVolumeBar.Value;
			_game.MusicSounds.SetVolume(MusicVolumeBar.Value);
			_game.PlayerStats.AutoClimb = AutoClimb.On;
			if ((double)ControllerSensitivityBar.Value < 0.5)
			{
				_game.PlayerStats.controllerSensitivity = ControllerSensitivityBar.Value + 0.5f;
			}
			else
			{
				_game.PlayerStats.controllerSensitivity = ControllerSensitivityBar.Value * 2f;
			}
			base.OnUpdate(game, gameTime);
		}

		public override void OnPushed()
		{
			BrightnessBar.Value = _game.Brightness * 2f;
			InvertYaxis.On = _game.PlayerStats.InvertYAxis;
			MusicVolumeBar.Value = _game.PlayerStats.musicVolume;
			AutoClimb.On = _game.PlayerStats.AutoClimb;
			if (_game.PlayerStats.controllerSensitivity < 1f)
			{
				ControllerSensitivityBar.Value = _game.PlayerStats.controllerSensitivity - 0.5f;
			}
			else
			{
				ControllerSensitivityBar.Value = _game.PlayerStats.controllerSensitivity / 2f;
			}
			base.OnPushed();
		}

		public override void OnPoped()
		{
			try
			{
				_game.SavePlayerStats(_game.PlayerStats);
			}
			catch
			{
			}
			base.OnPoped();
		}
	}
}
