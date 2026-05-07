using System;
using DNA.Audio;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class SinglePlayerStartScreen : Screen
	{
		public string ClickSound;

		public event EventHandler OnStartPressed;

		public event EventHandler OnBackPressed;

		public SinglePlayerStartScreen(bool drawBehind)
			: base(true, drawBehind)
		{
		}

		protected override bool OnInput(InputManager inputManager, GameTime gameTime)
		{
			if (inputManager.Mouse.LeftButtonPressed || inputManager.Keyboard.WasKeyPressed(Keys.Enter) || inputManager.Keyboard.WasKeyPressed(Keys.Space))
			{
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				Screen.SelectedPlayerIndex = PlayerIndex.One;
				if (this.OnStartPressed != null)
				{
					this.OnStartPressed(this, new EventArgs());
				}
			}
			if (inputManager.Keyboard.WasKeyPressed(Keys.Escape))
			{
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
				Screen.SelectedPlayerIndex = PlayerIndex.One;
				if (this.OnBackPressed != null)
				{
					this.OnBackPressed(this, new EventArgs());
				}
			}
			for (int i = 0; i < inputManager.Controllers.Length; i++)
			{
				if (inputManager.Controllers[i].PressedButtons.Start || inputManager.Controllers[i].PressedButtons.A)
				{
					if (ClickSound != null)
					{
						SoundManager.Instance.PlayInstance(ClickSound);
					}
					Screen.SelectedPlayerIndex = (PlayerIndex)i;
					if (this.OnStartPressed != null)
					{
						this.OnStartPressed(this, new EventArgs());
					}
				}
				if (inputManager.Controllers[i].PressedButtons.Back)
				{
					if (ClickSound != null)
					{
						SoundManager.Instance.PlayInstance(ClickSound);
					}
					PopMe();
					Screen.SelectedPlayerIndex = (PlayerIndex)i;
					if (this.OnBackPressed != null)
					{
						this.OnBackPressed(this, new EventArgs());
					}
				}
			}
			return base.OnInput(inputManager, gameTime);
		}
	}
}
