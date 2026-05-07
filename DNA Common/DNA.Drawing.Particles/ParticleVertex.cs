using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Particles
{
	internal struct ParticleVertex
	{
		public const int SizeInBytes = 36;

		private int _cornerAndTileIndex;

		public Vector3 Position;

		public Vector3 Velocity;

		public Color Random;

		public float Time;

		public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Byte4, VertexElementUsage.Position, 0), new VertexElement(4, VertexElementFormat.Vector3, VertexElementUsage.Position, 1), new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0), new VertexElement(28, VertexElementFormat.Color, VertexElementUsage.Color, 0), new VertexElement(32, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0));

		public Vector2 Corner
		{
			get
			{
				return new Vector2((_cornerAndTileIndex & 0xFF) - 128, ((_cornerAndTileIndex >> 8) & 0xFF) - 128);
			}
			set
			{
				_cornerAndTileIndex &= -65536;
				_cornerAndTileIndex |= (int)(((uint)Math.Floor(value.Y + 128f) << 8) & 0xFF00);
				_cornerAndTileIndex |= (int)((uint)Math.Floor(value.X + 128f) & 0xFF);
			}
		}

		public void SetTileXY(int x, int y)
		{
			_cornerAndTileIndex &= 65535;
			_cornerAndTileIndex |= (x << 16) & 0xFF0000;
			_cornerAndTileIndex |= (y << 24) & -16777216;
		}
	}
}
