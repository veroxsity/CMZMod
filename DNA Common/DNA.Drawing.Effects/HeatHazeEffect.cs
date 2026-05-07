using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Effects
{
	public class HeatHazeEffect : DNAEffect
	{
		private float _waveMagnitude = 0.2f;

		private EffectParameter displaceTextureParam;

		private EffectParameter screenTextureParam;

		private EffectParameter waveMagnitudeParam;

		private Texture2D heatMap;

		public float WaveMagnitude
		{
			get
			{
				return _waveMagnitude;
			}
			set
			{
				_waveMagnitude = value;
			}
		}

		public Texture2D DisplacementTexture
		{
			get
			{
				return displaceTextureParam.GetValueTexture2D();
			}
			set
			{
				displaceTextureParam.SetValue(value);
			}
		}

		public Texture2D ScreenTexture
		{
			get
			{
				return screenTextureParam.GetValueTexture2D();
			}
			set
			{
				screenTextureParam.SetValue(value);
			}
		}

		public HeatHazeEffect(Game game)
			: base(game.Content.Load<Effect>("HeatHaze"))
		{
			heatMap = game.Content.Load<Texture2D>("HeatNormal");
			CacheEffectParameters(null);
		}

		protected HeatHazeEffect(HeatHazeEffect cloneSource)
			: base(cloneSource)
		{
			CacheEffectParameters(cloneSource);
			_waveMagnitude = cloneSource._waveMagnitude;
			heatMap = cloneSource.heatMap;
		}

		public override Effect Clone()
		{
			return new HeatHazeEffect(this);
		}

		private void CacheEffectParameters(HeatHazeEffect cloneSource)
		{
			displaceTextureParam = base.Parameters["DisplacementMap"];
			screenTextureParam = base.Parameters["ScreenMap"];
			waveMagnitudeParam = base.Parameters["WaveMagnitude"];
			displaceTextureParam.SetValue(heatMap);
		}

		protected override void OnApply()
		{
			waveMagnitudeParam.SetValue(_waveMagnitude);
			base.OnApply();
		}
	}
}
