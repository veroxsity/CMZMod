using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace DNA.IO
{
	public static class BinaryWriterExtensions
	{
		public static void Write(this BinaryWriter writer, Angle angle)
		{
			writer.Write(angle.Radians);
		}

		public static void Write(this BinaryWriter writer, Quaternion value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Z);
			writer.Write(value.W);
		}

		public static void Write(this BinaryWriter writer, Matrix matrix)
		{
			writer.Write(matrix.M11);
			writer.Write(matrix.M12);
			writer.Write(matrix.M13);
			writer.Write(matrix.M14);
			writer.Write(matrix.M21);
			writer.Write(matrix.M22);
			writer.Write(matrix.M23);
			writer.Write(matrix.M24);
			writer.Write(matrix.M31);
			writer.Write(matrix.M32);
			writer.Write(matrix.M33);
			writer.Write(matrix.M34);
			writer.Write(matrix.M41);
			writer.Write(matrix.M42);
			writer.Write(matrix.M43);
			writer.Write(matrix.M44);
		}

		public static void Write(this BinaryWriter writer, Vector3 value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Z);
		}

		public static void Write(this BinaryWriter writer, IntVector3 value)
		{
			writer.Write(value.X);
			writer.Write(value.Y);
			writer.Write(value.Z);
		}

		public static void Write(this BinaryWriter writer, IntVector3[] value)
		{
			if (value == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(value, value.Length);
			}
		}

		public static void Write(this BinaryWriter writer, IntVector3[] value, int count)
		{
			int num = ((value != null) ? Math.Min(value.Length, count) : 0);
			writer.Write(num);
			for (int i = 0; i < num; i++)
			{
				writer.Write(value[i]);
			}
		}

		public static void Write(this BinaryWriter writer, uint[] value, int count)
		{
			int num = ((value != null) ? Math.Min(value.Length, count) : 0);
			writer.Write(num);
			for (int i = 0; i < num; i++)
			{
				writer.Write(value[i]);
			}
		}

		public static void Write(this BinaryWriter writer, uint[] value)
		{
			if (value == null)
			{
				writer.Write(0);
			}
			else
			{
				writer.Write(value, value.Length);
			}
		}

		public static Angle ReadAngle(this BinaryReader reader)
		{
			return Angle.FromRadians(reader.ReadSingle());
		}

		public static Matrix ReadMatrix(this BinaryReader reader)
		{
			return new Matrix(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static Vector3 ReadVector3(this BinaryReader reader)
		{
			return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}

		public static IntVector3 ReadIntVector3(this BinaryReader reader)
		{
			return new IntVector3(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
		}

		public static IntVector3[] ReadIntVector3Array(this BinaryReader reader)
		{
			IntVector3[] array = null;
			int num = reader.ReadInt32();
			if (num > 0)
			{
				array = new IntVector3[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = reader.ReadIntVector3();
				}
			}
			return array;
		}

		public static uint[] ReadUIntArray(this BinaryReader reader)
		{
			uint[] array = null;
			int num = reader.ReadInt32();
			if (num > 0)
			{
				array = new uint[num];
				for (int i = 0; i < num; i++)
				{
					array[i] = reader.ReadUInt32();
				}
			}
			return array;
		}

		public static Quaternion ReadQuaternion(this BinaryReader reader)
		{
			return new Quaternion(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
		}
	}
}
