namespace DNA
{
	public static class GameConsole
	{
		private static IConsole _control;

		public static void SetControl(IConsole control)
		{
			_control = control;
		}

		public static void Write(char value)
		{
			if (_control != null)
			{
				_control.Write(value);
			}
		}

		public static void Write(string value)
		{
			if (_control != null)
			{
				_control.Write(value);
			}
		}

		public static void WriteLine(string value)
		{
			if (_control != null)
			{
				_control.WriteLine(value);
			}
		}

		public static void WriteLine()
		{
			_control.WriteLine();
		}
	}
}
