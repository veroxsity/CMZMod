using System;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class DistortSkinnedEffect : DNAEffect
	{
		public const int MaxBones = 72;

		private const float blurAmount = 2f;

		private float _distortionScale = 0.1f;

		private EffectParameter textureParam;

		private EffectParameter bonesParam;

		private EffectParameter shaderIndexParam;

		private EffectParameter distortionScaleParam;

		private float alpha = 1f;

		public bool Blur = true;

		private EffectTechnique distortTechnique;

		private EffectTechnique distortBlurTechnique;

		private int weightsPerVertex = 4;

		public float DistortionScale
		{
			get
			{
				return _distortionScale;
			}
			set
			{
				_distortionScale = value;
			}
		}

		public float Alpha
		{
			get
			{
				return alpha;
			}
			set
			{
				alpha = value;
			}
		}

		public Texture2D Texture
		{
			get
			{
				return textureParam.GetValueTexture2D();
			}
			set
			{
				textureParam.SetValue(value);
			}
		}

		public int WeightsPerVertex
		{
			get
			{
				return weightsPerVertex;
			}
			set
			{
				if (value != 1 && value != 2 && value != 4)
				{
					throw new ArgumentOutOfRangeException("value");
				}
				weightsPerVertex = value;
			}
		}

		private void SetBlurEffectParameters(float dx, float dy)
		{
			EffectParameter effectParameter = base.Parameters["SampleWeights"];
			EffectParameter effectParameter2 = base.Parameters["SampleOffsets"];
			int count = effectParameter.Elements.Count;
			float[] array = new float[count];
			Vector2[] array2 = new Vector2[count];
			array[0] = ComputeGaussian(0f);
			array2[0] = new Vector2(0f);
			float num = array[0];
			for (int i = 0; i < count / 2; i++)
			{
				num += (array[i * 2 + 2] = (array[i * 2 + 1] = ComputeGaussian(i + 1))) * 2f;
				float num2 = (float)(i * 2) + 1.5f;
				Vector2 vector = new Vector2(dx, dy) * num2;
				array2[i * 2 + 1] = vector;
				array2[i * 2 + 2] = -vector;
			}
			for (int j = 0; j < array.Length; j++)
			{
				array[j] /= num;
			}
			effectParameter.SetValue(array);
			effectParameter2.SetValue(array2);
		}

		private static float ComputeGaussian(float n)
		{
			return (float)(1.0 / Math.Sqrt(Math.PI * 4.0) * Math.Exp((0f - n * n) / 8f));
		}

		public void SetBoneTransforms(Matrix[] boneTransforms)
		{
			if (boneTransforms == null || boneTransforms.Length == 0)
			{
				throw new ArgumentNullException("boneTransforms");
			}
			if (boneTransforms.Length > 72)
			{
				throw new ArgumentException();
			}
			bonesParam.SetValue(boneTransforms);
		}

		public Matrix[] GetBoneTransforms(int count)
		{
			if (count <= 0 || count > 72)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			Matrix[] valueMatrixArray = bonesParam.GetValueMatrixArray(count);
			for (int i = 0; i < valueMatrixArray.Length; i++)
			{
				valueMatrixArray[i].M44 = 1f;
			}
			return valueMatrixArray;
		}

		public DistortSkinnedEffect(Game game)
			: base(game.Content.Load<Effect>("DistortSkinnedEffect"))
		{
			CacheEffectParameters(null);
			Matrix[] array = new Matrix[72];
			for (int i = 0; i < 72; i++)
			{
				array[i] = Matrix.Identity;
			}
			SetBoneTransforms(array);
		}

		protected DistortSkinnedEffect(DistortSkinnedEffect cloneSource)
			: base(cloneSource)
		{
			CacheEffectParameters(cloneSource);
			Blur = cloneSource.Blur;
			alpha = cloneSource.alpha;
			_distortionScale = cloneSource._distortionScale;
			weightsPerVertex = cloneSource.weightsPerVertex;
		}

		public override Effect Clone()
		{
			return new DistortSkinnedEffect(this);
		}

		private void CacheEffectParameters(DistortSkinnedEffect cloneSource)
		{
			textureParam = base.Parameters["Texture"];
			distortionScaleParam = base.Parameters["DistortionScale"];
			bonesParam = base.Parameters["Bones"];
			shaderIndexParam = base.Parameters["ShaderIndex"];
			distortTechnique = base.Techniques["Distort"];
			distortBlurTechnique = base.Techniques["DistortBlur"];
			PresentationParameters presentationParameters = base.GraphicsDevice.PresentationParameters;
			int backBufferWidth = presentationParameters.BackBufferWidth;
			int backBufferHeight = presentationParameters.BackBufferHeight;
			SurfaceFormat backBufferFormat = presentationParameters.BackBufferFormat;
			DepthFormat depthStencilFormat = presentationParameters.DepthStencilFormat;
			SetBlurEffectParameters(1f / (float)backBufferWidth, 1f / (float)backBufferHeight);
		}

		protected override void OnApply()
		{
			int num = 0;
			if (weightsPerVertex == 2)
			{
				num++;
			}
			else if (weightsPerVertex == 4)
			{
				num += 2;
			}
			shaderIndexParam.SetValue(num);
			distortionScaleParam.SetValue(_distortionScale);
			base.CurrentTechnique = (Blur ? distortBlurTechnique : distortTechnique);
			base.OnApply();
		}
	}
}
