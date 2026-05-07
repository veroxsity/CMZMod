using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class View
	{
		public bool Enabled = true;

		private RenderTarget2D _target;

		public Color? BackgroundColor = null;

		public Texture2D BackgroundImage;

		private DrawEventArgs args = new DrawEventArgs();

		public RenderTarget2D Target
		{
			get
			{
				return _target;
			}
			protected set
			{
				_target = value;
			}
		}

		public event EventHandler<DrawEventArgs> BeforeDraw;

		public event EventHandler<DrawEventArgs> AfterDraw;

		public View()
		{
		}

		public View(RenderTarget2D target)
		{
			_target = target;
		}

		protected void SetRenderTargetToDevice(GraphicsDevice device)
		{
			device.SetRenderTarget(_target);
		}

		protected virtual void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
		}

		public void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			args.Device = device;
			args.GameTime = gameTime;
			device.SetRenderTarget(_target);
			if (BackgroundImage != null)
			{
				if (_target == null || _target.DepthStencilFormat != DepthFormat.None)
				{
					device.Clear(ClearOptions.DepthBuffer, Color.Red, 1f, 0);
				}
				int width = device.Viewport.Width;
				int height = device.Viewport.Height;
				int num = BackgroundImage.Width * height / BackgroundImage.Height;
				int num2 = num - width;
				int num3 = num2 / 2;
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone);
				spriteBatch.Draw(BackgroundImage, new Rectangle(-num3, 0, num, height), new Rectangle(0, 0, BackgroundImage.Width, BackgroundImage.Height), Color.White);
				spriteBatch.End();
			}
			else if (BackgroundColor.HasValue)
			{
				if (_target == null || _target.DepthStencilFormat != DepthFormat.None)
				{
					device.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, BackgroundColor.Value, 1f, 0);
				}
				else
				{
					device.Clear(ClearOptions.Target, BackgroundColor.Value, 1f, 0);
				}
			}
			else if (_target == null || _target.DepthStencilFormat != DepthFormat.None)
			{
				device.Clear(ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);
			}
			if (this.BeforeDraw != null)
			{
				this.BeforeDraw(this, args);
			}
			OnDraw(device, spriteBatch, gameTime);
			if (this.AfterDraw != null)
			{
				this.AfterDraw(this, args);
			}
		}
	}
}
