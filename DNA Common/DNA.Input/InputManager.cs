using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class InputManager
	{
		private DNAGame _game;

		private MouseInput _mouseState;

		private KeyboardInput _keyboardState;

		private KeyboardInput[] _chatPads = new KeyboardInput[4];

		private GameController[] _controllers = new GameController[4];

		private ControllerButtons _buttonsPressed;

		private bool _previousFrameCaptureMouse;

		private bool _previousFrameWasActive;

		public MouseInput Mouse
		{
			get
			{
				return _mouseState;
			}
		}

		public KeyboardInput Keyboard
		{
			get
			{
				return _keyboardState;
			}
		}

		public KeyboardInput[] ChatPads
		{
			get
			{
				return _chatPads;
			}
		}

		public GameController[] Controllers
		{
			get
			{
				return _controllers;
			}
		}

		public ControllerButtons ButtonsPressed
		{
			get
			{
				return _buttonsPressed;
			}
		}

		public InputManager(DNAGame game)
		{
			_game = game;
			_mouseState = new MouseInput(game);
			_controllers[0] = new GameController(PlayerIndex.One);
			_controllers[1] = new GameController(PlayerIndex.Two);
			_controllers[2] = new GameController(PlayerIndex.Three);
			_controllers[3] = new GameController(PlayerIndex.Four);
			_keyboardState = new KeyboardInput();
			_chatPads[0] = new KeyboardInput(PlayerIndex.One);
			_chatPads[1] = new KeyboardInput(PlayerIndex.Two);
			_chatPads[2] = new KeyboardInput(PlayerIndex.Three);
			_chatPads[3] = new KeyboardInput(PlayerIndex.Four);
		}

		protected void HandleChangeInMouseCapture()
		{
			bool captureMouse = _game.ScreenManager.CaptureMouse;
			bool isActive = _game.IsActive;
			if (isActive)
			{
				if (isActive != _previousFrameWasActive && captureMouse)
				{
					ResetMouse(true);
				}
				else if (captureMouse != _previousFrameCaptureMouse)
				{
					ResetMouse(true);
				}
			}
			_previousFrameWasActive = isActive;
			_previousFrameCaptureMouse = captureMouse;
		}

		protected void ResetMouse(bool zeroDeltas)
		{
			Viewport viewport = _game.GraphicsDevice.Viewport;
			Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2, zeroDeltas);
		}

		public void Update()
		{
			HandleChangeInMouseCapture();
			if (_game.IsActive)
			{
				_mouseState.Update();
				_keyboardState.Update();
				for (int i = 0; i < _controllers.Length; i++)
				{
					_chatPads[i].Update();
				}
			}
			Buttons buttons = (Buttons)0;
			for (int j = 0; j < _controllers.Length; j++)
			{
				GameController gameController = _controllers[j];
				gameController.Update();
				if (gameController.PressedButtons.A)
				{
					buttons |= Buttons.A;
				}
				if (gameController.PressedButtons.B)
				{
					buttons |= Buttons.B;
				}
				if (gameController.PressedButtons.Back)
				{
					buttons |= Buttons.Back;
				}
				if (gameController.PressedButtons.BigButton)
				{
					buttons |= Buttons.BigButton;
				}
				if (gameController.PressedButtons.LeftShoulder)
				{
					buttons |= Buttons.LeftShoulder;
				}
				if (gameController.PressedButtons.LeftStick)
				{
					buttons |= Buttons.LeftStick;
				}
				if (gameController.PressedButtons.RightShoulder)
				{
					buttons |= Buttons.RightShoulder;
				}
				if (gameController.PressedButtons.RightStick)
				{
					buttons |= Buttons.RightStick;
				}
				if (gameController.PressedButtons.Start)
				{
					buttons |= Buttons.Start;
				}
				if (gameController.PressedButtons.X)
				{
					buttons |= Buttons.X;
				}
				if (gameController.PressedButtons.Y)
				{
					buttons |= Buttons.Y;
				}
			}
			_buttonsPressed = new ControllerButtons(buttons);
			if (_game.IsActive && _game.ScreenManager.CaptureMouse)
			{
				ResetMouse(false);
			}
		}
	}
}
