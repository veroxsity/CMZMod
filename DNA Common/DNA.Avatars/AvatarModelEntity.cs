using DNA.Drawing;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Avatars
{
	public class AvatarModelEntity : SkinnedModelEntity
	{
		public Vector3[] DirectLightColor = new Vector3[2];

		public Vector3[] DirectLightDirection = new Vector3[2];

		public Vector3 AmbientLight = Vector3.One;

		public AvatarModelEntity(Model model)
			: base(model)
		{
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is DNAEffect)
			{
				effect.Parameters["LightDirection1"].SetValue(DirectLightDirection[0]);
				effect.Parameters["LightColor1"].SetValue(DirectLightColor[0]);
				effect.Parameters["LightDirection2"].SetValue(DirectLightDirection[1]);
				effect.Parameters["LightColor2"].SetValue(DirectLightColor[1]);
				effect.Parameters["AmbientLight"].SetValue(AmbientLight * 0.25f);
				effect.Parameters["MetalIntensity"].SetValue(AmbientLight);
			}
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
