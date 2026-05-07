using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public class SpotLight : DirectionalLight
	{
		public Angle InnerSpotAngle = Angle.FromDegrees(10f);

		public Angle OuterSpotAngle = Angle.FromDegrees(30f);

		public FallOffType ConeFalloff = FallOffType.Linear;

		public override float GetInfluence(Vector3 worldLocation)
		{
			float num = base.GetInfluence(worldLocation);
			if (num > 0f)
			{
				switch (FallOff)
				{
				case FallOffType.Linear:
				{
					Vector3 v2 = worldLocation - base.WorldPosition;
					Angle angle2 = v2.AngleBetween(base.LightDirection);
					if (angle2 > InnerSpotAngle)
					{
						float val = 1f - angle2 / OuterSpotAngle;
						val = Math.Max(val, 0f);
						num *= val;
					}
					break;
				}
				case FallOffType.Squared:
				{
					Vector3 v = worldLocation - base.WorldPosition;
					Angle angle = v.AngleBetween(base.LightDirection);
					if (angle > InnerSpotAngle)
					{
						float num2 = 1f - angle / OuterSpotAngle;
						num2 *= num2;
						num2 = Math.Max(num2, 0f);
						num *= num2;
					}
					break;
				}
				}
			}
			return num;
		}
	}
}
