using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SpriteEffect : Effect
	{
		private EffectParameter matrixParam;

		public SpriteEffect(Effect effect)
			: base(effect)
		{
			CacheEffectParameters();
		}

		protected SpriteEffect(SpriteEffect cloneSource)
			: base(cloneSource)
		{
			CacheEffectParameters();
		}

		public override Effect Clone()
		{
			return new SpriteEffect(this);
		}

		private void CacheEffectParameters()
		{
			matrixParam = base.Parameters["MatrixTransform"];
		}

		protected override void OnApply()
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			Matrix matrix = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
			Matrix matrix2 = Matrix.CreateTranslation(-0.5f, -0.5f, 0f);
			matrixParam.SetValue(matrix2 * matrix);
		}
	}
}
