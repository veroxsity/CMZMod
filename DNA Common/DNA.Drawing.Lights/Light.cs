using System;
using Microsoft.Xna.Framework;

namespace DNA.Drawing.Lights
{
	public abstract class Light : Entity
	{
		public float InnerRadius = 1f;

		private float _outerRadius = 2f;

		private float _outerRadiusSquared = 4f;

		public FallOffType FallOff;

		public Color LightColor;

		private Vector3 _lastLocation;

		public float OuterRadius
		{
			get
			{
				return _outerRadius;
			}
			set
			{
				_outerRadius = value;
				_outerRadiusSquared = _outerRadius * _outerRadius;
			}
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			_lastLocation = base.WorldPosition;
			base.Update(game, gameTime);
		}

		public virtual float GetInfluence(Vector3 worldLocation)
		{
			if (!Visible)
			{
				return 0f;
			}
			switch (FallOff)
			{
			case FallOffType.None:
				return 1f;
			case FallOffType.Linear:
			{
				float num6 = Vector3.DistanceSquared(worldLocation, _lastLocation);
				if (num6 > _outerRadiusSquared)
				{
					return 0f;
				}
				float num7 = (float)Math.Sqrt(num6);
				if (num7 < InnerRadius)
				{
					return 1f;
				}
				float num8 = OuterRadius - InnerRadius;
				float num9 = num7 - InnerRadius;
				float val = 1f - num9 / num8;
				return Math.Max(val, 0f);
			}
			case FallOffType.Squared:
			{
				float num = Vector3.DistanceSquared(worldLocation, _lastLocation);
				if (num > _outerRadiusSquared)
				{
					return 0f;
				}
				float num2 = (float)Math.Sqrt(num);
				if (num2 < InnerRadius)
				{
					return 1f;
				}
				float num3 = OuterRadius - InnerRadius;
				float num4 = num2 - InnerRadius;
				float num5 = 1f - num4 / num3;
				num5 *= num5;
				return Math.Max(num5, 0f);
			}
			default:
				return 1f;
			}
		}
	}
}
