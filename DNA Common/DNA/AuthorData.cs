using System;
using System.IO;
using System.Reflection;
using DNA.Security.Cryptography;

namespace DNA
{
	public class AuthorData
	{
		private const int FileIdent = 1935893365;

		private const int FileVersion = 1;

		private Signature _signature;

		private int _dataVersion;

		private byte[] _rawData;

		private RSAKey _key;

		public int Version
		{
			get
			{
				return _dataVersion;
			}
		}

		public byte[] Data
		{
			get
			{
				return _rawData;
			}
		}

		public AuthorData()
		{
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			Stream manifestResourceStream = executingAssembly.GetManifestResourceStream("DNA.ADT.key");
			_key = RSAKey.Read(new BinaryReader(manifestResourceStream));
		}

		public AuthorData(RSAKey publicKey)
		{
			_key = publicKey;
		}

		public AuthorData(RSAKey privateKey, int dataVersion, byte[] data)
		{
			_dataVersion = dataVersion;
			_key = privateKey.PublicKey;
			_rawData = data;
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(dataVersion);
			binaryWriter.Write(data.Length);
			binaryWriter.Write(data);
			binaryWriter.Flush();
			byte[] data2 = memoryStream.ToArray();
			RSASignatureProvider rSASignatureProvider = new RSASignatureProvider(new SHA256HashProvider(), privateKey);
			_signature = rSASignatureProvider.Sign(data2);
		}

		public void Read(BinaryReader reader)
		{
			RSASignatureProvider rSASignatureProvider = new RSASignatureProvider(new SHA256HashProvider(), _key);
			if (reader.ReadInt32() != 1935893365 || reader.ReadInt32() != 1)
			{
				throw new Exception("Bad Data Format");
			}
			int count = reader.ReadInt32();
			byte[] array = reader.ReadBytes(count);
			int count2 = reader.ReadInt32();
			byte[] data = reader.ReadBytes(count2);
			_signature = rSASignatureProvider.FromByteArray(data);
			if (!_signature.Verify(rSASignatureProvider, array))
			{
				throw new Exception("Data Corrupt");
			}
			MemoryStream input = new MemoryStream(array);
			BinaryReader binaryReader = new BinaryReader(input);
			_dataVersion = binaryReader.ReadInt32();
			int count3 = binaryReader.ReadInt32();
			_rawData = binaryReader.ReadBytes(count3);
		}

		public void Write(BinaryWriter writer)
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			binaryWriter.Write(_dataVersion);
			binaryWriter.Write(_rawData.Length);
			binaryWriter.Write(_rawData);
			binaryWriter.Flush();
			byte[] array = memoryStream.ToArray();
			writer.Write(1935893365);
			writer.Write(1);
			writer.Write(array.Length);
			writer.Write(array);
			writer.Write(_signature.Data.Length);
			writer.Write(_signature.Data);
		}
	}
}
