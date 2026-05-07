using System;
using Microsoft.Xna.Framework;

namespace DNA.Input
{
	public class InputEventArgs : EventArgs
	{
		public InputManager InputManager;

		public GameTime GameTime;

		public bool ContiuneProcessing;

		public InputEventArgs(InputManager inputManger, GameTime time, bool continueProcessing)
		{
			InputManager = inputManger;
			GameTime = time;
			ContiuneProcessing = continueProcessing;
		}
	}
}
