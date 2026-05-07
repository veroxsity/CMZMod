using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class BltTargetView : View
	{
		private RenderTarget2D _offScreenBuffer;

		public RenderTarget2D OffScreenTarget
		{
			get
			{
				return _offScreenBuffer;
			}
		}

		public BltTargetView(Game game, RenderTarget2D destinationTarget, int downsample, bool mipmap)
			: base(destinationTarget)
		{
			int num;
			int num2;
			SurfaceFormat preferredFormat;
			DepthFormat depthStencilFormat;
			int multiSampleCount;
			if (destinationTarget == null)
			{
				PresentationParameters presentationParameters = game.GraphicsDevice.PresentationParameters;
				num = presentationParameters.BackBufferWidth;
				num2 = presentationParameters.BackBufferHeight;
				preferredFormat = presentationParameters.BackBufferFormat;
				depthStencilFormat = presentationParameters.DepthStencilFormat;
				multiSampleCount = presentationParameters.MultiSampleCount;
			}
			else
			{
				num = destinationTarget.Width;
				num2 = destinationTarget.Height;
				preferredFormat = destinationTarget.Format;
				depthStencilFormat = destinationTarget.DepthStencilFormat;
				multiSampleCount = destinationTarget.MultiSampleCount;
			}
			_offScreenBuffer = new RenderTarget2D(game.GraphicsDevice, num / downsample, num2 / downsample, mipmap, preferredFormat, depthStencilFormat, multiSampleCount, RenderTargetUsage.DiscardContents);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Viewport viewport = device.Viewport;
			SetRenderTargetToDevice(device);
			DrawFullscreenQuad(spriteBatch, OffScreenTarget, viewport.Width, viewport.Height, null);
		}

		protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, RenderTarget2D renderTarget, Effect effect)
		{
			spriteBatch.GraphicsDevice.SetRenderTarget(renderTarget);
			DrawFullscreenQuad(spriteBatch, texture, renderTarget.Width, renderTarget.Height, effect);
		}

		protected void DrawFullscreenQuad(SpriteBatch spriteBatch, Texture2D texture, int width, int height, Effect effect)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, null, null, null, effect);
			spriteBatch.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			spriteBatch.End();
		}
	}
}
