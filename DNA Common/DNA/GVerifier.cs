using DNA.Security;

namespace DNA
{
	public class GVerifier
	{
		private static byte[] Key = new byte[32]
		{
			223, 11, 100, 213, 74, 199, 64, 73, 109, 173,
			136, 21, 16, 234, 243, 33, 234, 239, 126, 140,
			232, 186, 72, 153, 134, 6, 91, 196, 117, 38,
			142, 13
		};

		private byte[][] checkStr = new byte[2][]
		{
			new byte[16]
			{
				80, 48, 100, 35, 217, 166, 5, 255, 156, 43,
				183, 49, 83, 201, 226, 121
			},
			new byte[16]
			{
				190, 20, 122, 60, 196, 121, 161, 167, 21, 243,
				138, 80, 240, 76, 169, 142
			}
		};

		public bool Verify(string data)
		{
			for (int i = 0; i < checkStr.Length; i++)
			{
				string text = SecurityTools.DecryptString(Key, checkStr[i]);
				if (data == text)
				{
					return false;
				}
			}
			return true;
		}
	}
}
