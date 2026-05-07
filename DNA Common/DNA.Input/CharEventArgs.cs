using System;
using Microsoft.Xna.Framework;

namespace DNA.Input
{
	public class CharEventArgs : EventArgs
	{
		public char PressedChar;

		public GameTime GameTime;

		public bool ContiuneProcessing;

		public CharEventArgs(char c, GameTime time, bool continueProcessing)
		{
			PressedChar = c;
			GameTime = time;
			ContiuneProcessing = continueProcessing;
		}
	}
}
