using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class MouseInput
	{
		private struct LocalMouseState
		{
			public MouseState MouseState;

			public ButtonState LeftButton
			{
				get
				{
					return MouseState.LeftButton;
				}
			}

			public ButtonState MiddleButton
			{
				get
				{
					return MouseState.MiddleButton;
				}
			}

			public ButtonState RightButton
			{
				get
				{
					return MouseState.RightButton;
				}
			}

			public int ScrollWheelValue
			{
				get
				{
					return MouseState.ScrollWheelValue;
				}
			}

			public int X
			{
				get
				{
					return MouseState.X;
				}
				set
				{
					MouseState = new MouseState(value, MouseState.Y, MouseState.ScrollWheelValue, MouseState.LeftButton, MouseState.MiddleButton, MouseState.RightButton, MouseState.XButton1, MouseState.XButton2);
				}
			}

			public ButtonState XButton1
			{
				get
				{
					return MouseState.XButton1;
				}
			}

			public ButtonState XButton2
			{
				get
				{
					return MouseState.XButton2;
				}
			}

			public int Y
			{
				get
				{
					return MouseState.Y;
				}
				set
				{
					MouseState = new MouseState(MouseState.X, value, MouseState.ScrollWheelValue, MouseState.LeftButton, MouseState.MiddleButton, MouseState.RightButton, MouseState.XButton1, MouseState.XButton2);
				}
			}

			public LocalMouseState(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2)
			{
				MouseState = new MouseState(x, y, scrollWheel, leftButton, middleButton, rightButton, xButton1, xButton2);
			}

			public LocalMouseState(MouseState mouseState)
			{
				MouseState = mouseState;
			}

			public void SetMouseState(MouseState mouseState)
			{
				MouseState = mouseState;
			}

			public static bool operator !=(LocalMouseState left, LocalMouseState right)
			{
				return left.MouseState != right.MouseState;
			}

			public static bool operator ==(LocalMouseState left, LocalMouseState right)
			{
				return left.MouseState == right.MouseState;
			}

			public override bool Equals(object obj)
			{
				return MouseState.Equals(obj);
			}

			public override int GetHashCode()
			{
				return MouseState.GetHashCode();
			}

			public override string ToString()
			{
				return MouseState.ToString();
			}
		}

		private DNAGame _game;

		private LocalMouseState _lastState;

		private LocalMouseState _currentState;

		public MouseState LastState
		{
			get
			{
				return _lastState.MouseState;
			}
		}

		public MouseState CurrentState
		{
			get
			{
				return _currentState.MouseState;
			}
		}

		public Point Position
		{
			get
			{
				return _game.ScreenToBuffer(new Point(_currentState.X, _currentState.Y));
			}
		}

		public Point LastPosition
		{
			get
			{
				return _game.ScreenToBuffer(new Point(_lastState.X, _lastState.Y));
			}
		}

		public Vector2 DeltaPosition
		{
			get
			{
				Vector2 vector = _game.ScreenToBuffer(new Vector2(_currentState.X, _currentState.Y));
				Vector2 vector2 = _game.ScreenToBuffer(new Vector2(_lastState.X, _lastState.Y));
				return new Vector2(vector.X - vector2.X, vector.Y - vector2.Y);
			}
		}

		public int DeltaWheel
		{
			get
			{
				return _currentState.ScrollWheelValue - _lastState.ScrollWheelValue;
			}
		}

		public bool LeftButtonDown
		{
			get
			{
				return _currentState.LeftButton == ButtonState.Pressed;
			}
		}

		public bool MiddleButtonDown
		{
			get
			{
				return _currentState.MiddleButton == ButtonState.Pressed;
			}
		}

		public bool RightButtonDown
		{
			get
			{
				return _currentState.RightButton == ButtonState.Pressed;
			}
		}

		public bool XButton1Down
		{
			get
			{
				return _currentState.XButton1 == ButtonState.Pressed;
			}
		}

		public bool XButton2Down
		{
			get
			{
				return _currentState.XButton2 == ButtonState.Pressed;
			}
		}

		public bool LeftButtonPressed
		{
			get
			{
				if (_currentState.LeftButton == ButtonState.Pressed)
				{
					return _lastState.LeftButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool MiddleButtonPressed
		{
			get
			{
				if (_currentState.MiddleButton == ButtonState.Pressed)
				{
					return _lastState.MiddleButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool RightButtonPressed
		{
			get
			{
				if (_currentState.RightButton == ButtonState.Pressed)
				{
					return _lastState.RightButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool XButton1Pressed
		{
			get
			{
				if (_currentState.XButton1 == ButtonState.Pressed)
				{
					return _lastState.XButton1 == ButtonState.Released;
				}
				return false;
			}
		}

		public bool XButton2Pressed
		{
			get
			{
				if (_currentState.XButton2 == ButtonState.Pressed)
				{
					return _lastState.XButton2 == ButtonState.Released;
				}
				return false;
			}
		}

		public bool LeftButtonReleased
		{
			get
			{
				if (_lastState.LeftButton == ButtonState.Pressed)
				{
					return _currentState.LeftButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool MiddleButtonReleased
		{
			get
			{
				if (_lastState.MiddleButton == ButtonState.Pressed)
				{
					return _currentState.MiddleButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool RightButtonReleased
		{
			get
			{
				if (_lastState.RightButton == ButtonState.Pressed)
				{
					return _currentState.RightButton == ButtonState.Released;
				}
				return false;
			}
		}

		public bool XButton1Released
		{
			get
			{
				if (_lastState.XButton1 == ButtonState.Pressed)
				{
					return _currentState.XButton1 == ButtonState.Released;
				}
				return false;
			}
		}

		public bool XButton2Released
		{
			get
			{
				if (_lastState.XButton2 == ButtonState.Pressed)
				{
					return _currentState.XButton2 == ButtonState.Released;
				}
				return false;
			}
		}

		public void SetPosition(int x, int y, bool resetDeltas)
		{
			x = 0;
			y = 0;
			Mouse.SetPosition(x, y);
			_lastState.X = x;
			_lastState.Y = y;
			if (resetDeltas)
			{
				_currentState.X = x;
				_currentState.Y = y;
			}
		}

		public void Update()
		{
			_lastState = _currentState;
			_currentState.SetMouseState(Mouse.GetState());
		}

		public MouseInput(DNAGame game)
		{
			_game = game;
		}
	}
}
