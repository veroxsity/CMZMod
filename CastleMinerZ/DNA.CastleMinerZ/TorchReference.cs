using System.IO;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ
{
	public struct TorchReference
	{
		public Vector3 Position;

		public BlockFace Facing;

		public TorchReference(Vector3 blockCenter, BlockFace facing)
		{
			Position = blockCenter;
			Facing = facing;
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Position.X);
			writer.Write(Position.Y);
			writer.Write(Position.Z);
			writer.Write((byte)Facing);
		}

		public void Read(BinaryReader reader)
		{
			Position.X = reader.ReadSingle();
			Position.Y = reader.ReadSingle();
			Position.Z = reader.ReadSingle();
			Facing = (BlockFace)reader.ReadByte();
		}
	}
}
