using Microsoft.Xna.Framework;

namespace DNA.Avatars
{
	public abstract class AvatarLightingManager
	{
		private Vector3 _lightDirection = new Vector3(-0.5f, -0.6123f, -0.6123f);

		private Color _lightColor = new Color(0.4f, 0.4f, 0.4f);

		private Color _ambientLightColor = new Color(0.55f, 0.55f, 0.55f);

		public Vector3 LightDirection
		{
			get
			{
				return _lightDirection;
			}
			set
			{
				_lightDirection = value;
			}
		}

		public Color LightColor
		{
			get
			{
				return _lightColor;
			}
			set
			{
				_lightColor = value;
			}
		}

		public Color AmbientLightColor
		{
			get
			{
				return _ambientLightColor;
			}
			set
			{
				_ambientLightColor = value;
			}
		}

		protected virtual void SetAvatarLighting(Avatar avatar, Vector3 ambientLightColor, Vector3 LightColor, Vector3 LightDirection)
		{
			avatar.AvatarRenderer.LightDirection = LightDirection;
			avatar.AvatarRenderer.LightColor = LightColor;
			avatar.AvatarRenderer.AmbientLightColor = ambientLightColor;
		}

		public virtual void SetAvatarLighting(Avatar avatar)
		{
			SetAvatarLighting(avatar, AmbientLightColor.ToVector3(), LightColor.ToVector3(), LightDirection);
		}
	}
}
