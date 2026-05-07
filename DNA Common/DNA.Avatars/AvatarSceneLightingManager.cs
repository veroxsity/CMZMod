using System.Collections.ObjectModel;
using DNA.Drawing;
using DNA.Drawing.Lights;
using Microsoft.Xna.Framework;

namespace DNA.Avatars
{
	public class AvatarSceneLightingManager : AvatarLightingManager
	{
		public virtual bool UseLight(Avatar avatar, Light light)
		{
			return true;
		}

		protected virtual Vector3 GetAvatarWorldPosition(Avatar avatar)
		{
			return avatar.WorldPosition + new Vector3(0f, 1f, 0f);
		}

		protected virtual Scene GetScene(Avatar avatar)
		{
			return avatar.Scene;
		}

		public override void SetAvatarLighting(Avatar avatar)
		{
			Scene scene = GetScene(avatar);
			Vector3 avatarWorldPosition = GetAvatarWorldPosition(avatar);
			Vector3 zero = Vector3.Zero;
			Vector3 zero2 = Vector3.Zero;
			Vector3 zero3 = Vector3.Zero;
			float num = 0f;
			float num2 = 0f;
			ReadOnlyCollection<Light> lights = scene.Lights;
			int count = lights.Count;
			for (int i = 0; i < count; i++)
			{
				Light light = lights[i];
				float influence = light.GetInfluence(avatarWorldPosition);
				if (!(influence > 0f) || !UseLight(avatar, light))
				{
					continue;
				}
				num += influence;
				if (light is AmbientLight)
				{
					zero2 += light.LightColor.ToVector3() * influence;
					num2 += influence;
					continue;
				}
				zero3 += light.LightColor.ToVector3() * influence;
				num = influence;
				if (light is DirectionalLight)
				{
					DirectionalLight directionalLight = (DirectionalLight)light;
					zero += directionalLight.LightDirection * influence;
				}
				else
				{
					Vector3 vector = avatarWorldPosition - light.WorldPosition;
					zero += vector * influence;
				}
			}
			if (num2 < 1f)
			{
				zero2 += base.AmbientLightColor.ToVector3() * (1f - num2);
			}
			if (num < 1f)
			{
				zero3 += base.LightColor.ToVector3() * (1f - num);
				zero += base.LightDirection * (1f - num);
			}
			base.LightDirection.Normalize();
			SetAvatarLighting(avatar, zero2, zero3, zero);
		}
	}
}
