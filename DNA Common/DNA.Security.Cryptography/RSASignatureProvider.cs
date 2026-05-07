using System.Globalization;
using DNA.Security.Cryptography.Crypto.Signers;

namespace DNA.Security.Cryptography
{
	public class RSASignatureProvider : GenericSignatureProvider
	{
		private class RSASignature : Signature
		{
			public RSASignature(byte[] data)
				: base(data)
			{
			}

			public override bool Verify(ISignatureProvider signer, byte[] data, long start, long length)
			{
				RSASignatureProvider rSASignatureProvider = (RSASignatureProvider)signer;
				RsaDigestSigner rsaDigestSigner = (RsaDigestSigner)rSASignatureProvider.Signer;
				rsaDigestSigner.BlockUpdate(data, (int)start, (int)length);
				return rsaDigestSigner.VerifySignature(base.Data);
			}
		}

		private static RsaDigestSigner GetSigner(GenericHashProvider hashProvider, RSAKey key)
		{
			RsaDigestSigner rsaDigestSigner = new RsaDigestSigner(hashProvider.GetHashAlgorythim());
			rsaDigestSigner.Init(key.IsPrivate, key.Key);
			return rsaDigestSigner;
		}

		public RSASignatureProvider(GenericHashProvider hashProvider, RSAKey key)
			: base(GetSigner(hashProvider, key))
		{
		}

		private RSASignatureProvider(RsaDigestSigner rsa)
			: base(rsa)
		{
		}

		public override Signature FromByteArray(byte[] data)
		{
			return new RSASignature(data);
		}

		public override Signature Parse(string s)
		{
			byte[] array = new byte[s.Length / 2];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = byte.Parse(s.Substring(i * 2, 2), NumberStyles.HexNumber);
			}
			return new RSASignature(array);
		}
	}
}
