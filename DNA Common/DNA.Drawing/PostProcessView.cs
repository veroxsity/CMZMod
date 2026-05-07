using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class PostProcessView : BltTargetView
	{
		private RenderTarget2D _renderTarget1;

		private RenderTarget2D _renderTarget2;

		private RenderTarget2D _lastFrame;

		private Effect bloomExtractEffect;

		private Effect bloomCombineEffect;

		private Effect gaussianBlurEffect;

		public BloomSettings BloomSettings = BloomSettings.Default;

		private float[] sampleWeights = new float[0];

		private Vector2[] sampleOffsets = new Vector2[0];

		public RenderTarget2D Target1
		{
			get
			{
				return _renderTarget1;
			}
		}

		public RenderTarget2D Target2
		{
			get
			{
				return _renderTarget2;
			}
		}

		public Texture2D LastFrame
		{
			get
			{
				return _lastFrame;
			}
		}

		public PostProcessView(Game game, RenderTarget2D destinationTarget)
			: base(game, destinationTarget, 1, false)
		{
			bloomExtractEffect = game.Content.Load<Effect>("PostProcess\\BloomExtract");
			bloomCombineEffect = game.Content.Load<Effect>("PostProcess\\BloomCombine");
			gaussianBlurEffect = game.Content.Load<Effect>("PostProcess\\GaussianBlur");
			SurfaceFormat preferredFormat;
			DepthFormat depthStencilFormat;
			int num;
			int num2;
			if (destinationTarget == null)
			{
				PresentationParameters presentationParameters = game.GraphicsDevice.PresentationParameters;
				num = presentationParameters.BackBufferWidth;
				num2 = presentationParameters.BackBufferHeight;
				preferredFormat = presentationParameters.BackBufferFormat;
				depthStencilFormat = presentationParameters.DepthStencilFormat;
				int multiSampleCount = presentationParameters.MultiSampleCount;
			}
			else
			{
				num = destinationTarget.Width;
				num2 = destinationTarget.Height;
				preferredFormat = destinationTarget.Format;
				depthStencilFormat = destinationTarget.DepthStencilFormat;
				int multiSampleCount2 = destinationTarget.MultiSampleCount;
			}
			num /= 2;
			num2 /= 2;
			_renderTarget1 = new RenderTarget2D(game.GraphicsDevice, num, num2, false, preferredFormat, depthStencilFormat, 1, RenderTargetUsage.DiscardContents);
			_renderTarget2 = new RenderTarget2D(game.GraphicsDevice, num, num2, false, preferredFormat, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
			_lastFrame = new RenderTarget2D(game.GraphicsDevice, num, num2, true, preferredFormat, DepthFormat.None, 1, RenderTargetUsage.DiscardContents);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			device.SamplerStates[1] = SamplerState.LinearClamp;
			bloomExtractEffect.Parameters["BloomThreshold"].SetValue(BloomSettings.BloomThreshold);
			DrawFullscreenQuad(spriteBatch, base.OffScreenTarget, _renderTarget1, bloomExtractEffect);
			SetBlurEffectParameters(1f / (float)_renderTarget1.Width, 0f);
			DrawFullscreenQuad(spriteBatch, _renderTarget1, _renderTarget2, gaussianBlurEffect);
			SetBlurEffectParameters(0f, 1f / (float)_renderTarget1.Height);
			DrawFullscreenQuad(spriteBatch, _renderTarget2, _renderTarget1, gaussianBlurEffect);
			EffectParameterCollection parameters = bloomCombineEffect.Parameters;
			parameters["BloomIntensity"].SetValue(BloomSettings.BloomIntensity);
			parameters["BaseIntensity"].SetValue(BloomSettings.BaseIntensity);
			parameters["BloomSaturation"].SetValue(BloomSettings.BloomSaturation);
			parameters["BaseSaturation"].SetValue(BloomSettings.BaseSaturation);
			device.Textures[1] = base.OffScreenTarget;
			DrawFullscreenQuad(spriteBatch, _renderTarget1, _renderTarget2, bloomCombineEffect);
			device.SetRenderTarget(_lastFrame);
			Viewport viewport = device.Viewport;
			DrawFullscreenQuad(spriteBatch, _renderTarget1, viewport.Width, viewport.Height, bloomCombineEffect);
			SetRenderTargetToDevice(device);
			viewport = device.Viewport;
			DrawFullscreenQuad(spriteBatch, _renderTarget1, viewport.Width, viewport.Height, bloomCombineEffect);
		}

		private void SetBlurEffectParameters(float dx, float dy)
		{
			EffectParameter effectParameter = gaussianBlurEffect.Parameters["SampleWeights"];
			EffectParameter effectParameter2 = gaussianBlurEffect.Parameters["SampleOffsets"];
			int count = effectParameter.Elements.Count;
			if (sampleWeights.Length != count)
			{
				sampleWeights = new float[count];
			}
			if (sampleOffsets.Length != count)
			{
				sampleOffsets = new Vector2[count];
			}
			sampleWeights[0] = ComputeGaussian(0f);
			sampleOffsets[0] = new Vector2(0f);
			float num = sampleWeights[0];
			for (int i = 0; i < count / 2; i++)
			{
				float num2 = ComputeGaussian(i + 1);
				sampleWeights[i * 2 + 1] = num2;
				sampleWeights[i * 2 + 2] = num2;
				num += num2 * 2f;
				float num3 = (float)(i * 2) + 1.5f;
				Vector2 vector = new Vector2(dx, dy) * num3;
				sampleOffsets[i * 2 + 1] = vector;
				sampleOffsets[i * 2 + 2] = -vector;
			}
			for (int j = 0; j < sampleWeights.Length; j++)
			{
				sampleWeights[j] /= num;
			}
			effectParameter.SetValue(sampleWeights);
			effectParameter2.SetValue(sampleOffsets);
		}

		private float ComputeGaussian(float n)
		{
			float blurAmount = BloomSettings.BlurAmount;
			return (float)(1.0 / Math.Sqrt(Math.PI * 2.0 * (double)blurAmount) * Math.Exp((0f - n * n) / (2f * blurAmount * blurAmount)));
		}
	}
}
