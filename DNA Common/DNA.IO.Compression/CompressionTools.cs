using System.IO;
using DNA.IO.Compression.Zip.Compression;
using DNA.IO.Compression.Zip.Compression.Streams;

namespace DNA.IO.Compression
{
	public class CompressionTools
	{
		private bool UseHeaders;

		private Inflater inflater;

		private Deflater deflater;

		private MemoryStream outStream = new MemoryStream();

		public CompressionTools()
		{
			deflater = new Deflater(Deflater.DefaultCompression, !UseHeaders);
			inflater = new Inflater(!UseHeaders);
		}

		public CompressionTools(bool useHeaders)
		{
			UseHeaders = useHeaders;
		}

		public byte[] Compress(byte[] data)
		{
			lock (deflater)
			{
				deflater.Reset();
				outStream.Position = 0L;
				outStream.SetLength(0L);
				DeflaterOutputStream deflaterOutputStream = new DeflaterOutputStream(outStream, deflater);
				BinaryWriter binaryWriter = new BinaryWriter(deflaterOutputStream);
				binaryWriter.Write(data.Length);
				binaryWriter.Write(data, 0, data.Length);
				binaryWriter.Flush();
				deflaterOutputStream.Finish();
				return outStream.ToArray();
			}
		}

		public byte[] Decompress(byte[] data)
		{
			lock (inflater)
			{
				MemoryStream baseInputStream = new MemoryStream(data);
				inflater.Reset();
				InflaterInputStream input = new InflaterInputStream(baseInputStream, inflater);
				BinaryReader binaryReader = new BinaryReader(input);
				int count = binaryReader.ReadInt32();
				return binaryReader.ReadBytes(count);
			}
		}
	}
}
