using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Multimedia.Broadcasting
{
	public abstract class BroadcastStream : IDisposable
	{
		public bool StartBroadcastOnNewGame;

		private bool _disposed;

		public abstract bool Broadcasting { get; set; }

		public BroadcastStream()
		{
		}

		public abstract void SubmitFrame(RenderTarget2D frameBuffer);

		public virtual void Update(GameTime gameTime)
		{
		}

		public virtual void Dispose()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
			}
		}

		~BroadcastStream()
		{
			if (!_disposed)
			{
				Dispose();
			}
		}
	}
}
