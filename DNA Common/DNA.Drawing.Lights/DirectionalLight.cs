using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public class DirectionalLight : Light
	{
		public Vector3 LightDirection
		{
			get
			{
				return base.LocalToWorld.Forward;
			}
		}
	}
}
