using System.Collections.Generic;
using System.IO;
using DNA.IO;
using DNA.Security.Cryptography.Crypto;
using DNA.Security.Cryptography.Crypto.Generators;
using DNA.Security.Cryptography.Crypto.Parameters;
using DNA.Security.Cryptography.Math;
using DNA.Security.Cryptography.Security;

namespace DNA.Security.Cryptography
{
	public class RSAKey
	{
		private RsaPrivateCrtKeyParameters _privateKeyParams;

		private RsaKeyParameters _publicKeyParams;

		internal RsaKeyParameters Key
		{
			get
			{
				if (_privateKeyParams != null)
				{
					return _privateKeyParams;
				}
				return _publicKeyParams;
			}
		}

		public bool IsPrivate
		{
			get
			{
				return _privateKeyParams != null;
			}
		}

		public RSAKey PublicKey
		{
			get
			{
				if (_privateKeyParams == null)
				{
					return this;
				}
				return new RSAKey(_publicKeyParams);
			}
		}

		private RSAKey(RsaKeyParameters pubKey)
		{
			_publicKeyParams = pubKey;
		}

		private RSAKey()
		{
		}

		public static RSAKey Generate(int size)
		{
			RSAKey rSAKey = new RSAKey();
			RsaKeyPairGenerator rsaKeyPairGenerator = new RsaKeyPairGenerator();
			rsaKeyPairGenerator.Init(new KeyGenerationParameters(new SecureRandom(), size));
			AsymmetricCipherKeyPair asymmetricCipherKeyPair = rsaKeyPairGenerator.GenerateKeyPair();
			rSAKey._privateKeyParams = (RsaPrivateCrtKeyParameters)asymmetricCipherKeyPair.Private;
			rSAKey._publicKeyParams = (RsaKeyParameters)asymmetricCipherKeyPair.Public;
			return rSAKey;
		}

		public override string ToString()
		{
			HTFDocument hTFDocument = new HTFDocument();
			if (_privateKeyParams != null)
			{
				hTFDocument.Children.Add(new HTFElement("Modulus", _privateKeyParams.Modulus.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("PublicExponent", _privateKeyParams.PublicExponent.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("PrivateExponent", _privateKeyParams.Exponent.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("P", _privateKeyParams.P.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("Q", _privateKeyParams.Q.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("DP", _privateKeyParams.DP.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("dQ", _privateKeyParams.DQ.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("qInv", _privateKeyParams.QInv.ToString(16)));
			}
			else
			{
				hTFDocument.Children.Add(new HTFElement("Modulus", _publicKeyParams.Modulus.ToString(16)));
				hTFDocument.Children.Add(new HTFElement("PublicExponent", _publicKeyParams.Exponent.ToString(16)));
			}
			return hTFDocument.ToString();
		}

		public static RSAKey Parse(string str)
		{
			RSAKey rSAKey = new RSAKey();
			HTFDocument hTFDocument = new HTFDocument();
			hTFDocument.LoadFromString(str);
			Dictionary<string, BigInteger> dictionary = new Dictionary<string, BigInteger>();
			foreach (HTFElement child in hTFDocument.Children)
			{
				dictionary[child.ID] = new BigInteger(child.Value, 16);
			}
			if (dictionary.ContainsKey("PrivateExponent"))
			{
				rSAKey._privateKeyParams = new RsaPrivateCrtKeyParameters(dictionary["Modulus"], dictionary["PublicExponent"], dictionary["PrivateExponent"], dictionary["P"], dictionary["Q"], dictionary["DP"], dictionary["dQ"], dictionary["qInv"]);
				rSAKey._publicKeyParams = new RsaKeyParameters(false, rSAKey._privateKeyParams.Modulus, rSAKey._privateKeyParams.PublicExponent);
			}
			else
			{
				rSAKey._publicKeyParams = new RsaKeyParameters(false, dictionary["Modulus"], dictionary["PublicExponent"]);
			}
			return rSAKey;
		}

		private static void WriteBigInt(BinaryWriter writer, BigInteger bigInt)
		{
			byte[] array = bigInt.ToByteArray();
			writer.Write(array.Length);
			writer.Write(array);
		}

		private static BigInteger ReadBigInt(BinaryReader reader)
		{
			int count = reader.ReadInt32();
			return new BigInteger(reader.ReadBytes(count));
		}

		public void Write(BinaryWriter writer)
		{
			if (_privateKeyParams != null)
			{
				writer.Write(true);
				WriteBigInt(writer, _privateKeyParams.Modulus);
				WriteBigInt(writer, _privateKeyParams.PublicExponent);
				WriteBigInt(writer, _privateKeyParams.Exponent);
				WriteBigInt(writer, _privateKeyParams.P);
				WriteBigInt(writer, _privateKeyParams.Q);
				WriteBigInt(writer, _privateKeyParams.DP);
				WriteBigInt(writer, _privateKeyParams.DQ);
				WriteBigInt(writer, _privateKeyParams.QInv);
			}
			else
			{
				writer.Write(false);
				WriteBigInt(writer, _publicKeyParams.Modulus);
				WriteBigInt(writer, _publicKeyParams.Exponent);
			}
		}

		public static RSAKey Read(BinaryReader reader)
		{
			RSAKey rSAKey = new RSAKey();
			if (reader.ReadBoolean())
			{
				rSAKey._privateKeyParams = new RsaPrivateCrtKeyParameters(ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader), ReadBigInt(reader));
				rSAKey._publicKeyParams = new RsaKeyParameters(false, rSAKey._privateKeyParams.Modulus, rSAKey._privateKeyParams.PublicExponent);
			}
			else
			{
				rSAKey._publicKeyParams = new RsaKeyParameters(false, ReadBigInt(reader), ReadBigInt(reader));
			}
			return rSAKey;
		}

		public byte[] ToByteArray()
		{
			MemoryStream memoryStream = new MemoryStream();
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			Write(binaryWriter);
			binaryWriter.Flush();
			return memoryStream.ToArray();
		}

		public static RSAKey FromByteArray(byte[] data)
		{
			MemoryStream input = new MemoryStream(data);
			BinaryReader reader = new BinaryReader(input);
			return Read(reader);
		}
	}
}
