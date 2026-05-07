using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class GraphicsDeviceLocker
	{
		public static GraphicsDeviceLocker Instance;

		public static void Create(GraphicsDeviceManager gdm)
		{
			if (Instance == null)
			{
				Instance = new GraphicsDeviceLocker();
			}
		}

		public bool TryLockDeviceTimed(ref Stopwatch sw)
		{
			return true;
		}

		public bool TryLockDevice()
		{
			return true;
		}

		public void UnlockDevice()
		{
		}
	}
}
