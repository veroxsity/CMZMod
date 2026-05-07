using System;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class CameraView : View
	{
		private FilterCallback<Entity> _filterBase;

		private FilterCallback<Entity> _filter;

		private Camera _camera;

		public Camera Camera
		{
			get
			{
				return _camera;
			}
			set
			{
				_camera = value;
				if (_camera != null && _camera.Scene == null)
				{
					throw new Exception("Camera Must Be in Scene");
				}
			}
		}

		public static bool FilterDistortions(Entity e)
		{
			if (e is ParticleEmitter)
			{
				ParticleEmitter particleEmitter = (ParticleEmitter)e;
				if (particleEmitter.IsDistortionEffect)
				{
					return false;
				}
			}
			return !(e is IScreenDistortion);
		}

		public CameraView(RenderTarget2D target, Camera camera)
			: base(target)
		{
			_filterBase = FilterEntities;
			if (camera != null && camera.Scene == null)
			{
				throw new Exception("Camera Must Be in Scene");
			}
			_camera = camera;
		}

		public CameraView(RenderTarget2D target, Camera camera, FilterCallback<Entity> filter)
			: base(target)
		{
			_filterBase = FilterEntities;
			_filter = filter;
			if (camera != null && camera.Scene == null)
			{
				throw new Exception("Camera Must Be in Scene");
			}
			_camera = camera;
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			base.OnDraw(device, spriteBatch, gameTime);
			if (Camera != null)
			{
				Camera.Draw(device, spriteBatch, gameTime, _filterBase);
			}
		}

		private bool FilterEntities(Entity e)
		{
			bool flag = true;
			if (_filter != null)
			{
				flag = _filter(e);
			}
			if (flag)
			{
				return FilterEntity(e);
			}
			return false;
		}

		protected virtual bool FilterEntity(Entity e)
		{
			return true;
		}
	}
}
