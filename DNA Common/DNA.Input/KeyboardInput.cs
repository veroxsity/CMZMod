using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class KeyboardInput
	{
		private PlayerIndex? _playerIndex;

		private KeyboardState _lastState;

		private KeyboardState _currentState;

		public PlayerIndex? PlayerIndex
		{
			get
			{
				return _playerIndex;
			}
		}

		public KeyboardState CurrentState
		{
			get
			{
				return _currentState;
			}
		}

		public KeyboardState LastState
		{
			get
			{
				return _lastState;
			}
		}

		public KeyboardInput()
		{
		}

		public KeyboardInput(PlayerIndex index)
		{
			_playerIndex = index;
		}

		public bool IsKeyDown(Keys key)
		{
			return _currentState.IsKeyDown(key);
		}

		public bool WasKeyPressed(Keys key)
		{
			if (_currentState.IsKeyDown(key))
			{
				return _lastState.IsKeyUp(key);
			}
			return false;
		}

		public bool WasKeyReleased(Keys key)
		{
			if (_currentState.IsKeyUp(key))
			{
				return _lastState.IsKeyDown(key);
			}
			return false;
		}

		public void Update()
		{
			_lastState = _currentState;
			if (_playerIndex.HasValue)
			{
				_currentState = Keyboard.GetState(_playerIndex.Value);
			}
			else
			{
				_currentState = Keyboard.GetState();
			}
		}
	}
}
