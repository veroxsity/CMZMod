using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DNA.Text
{
	public static class TextTools
	{
		private static Regex SplitWordsRE = new Regex("[a-zA-Z]+", RegexOptions.Compiled);

		public static int IndexOf(this StringBuilder text, char c)
		{
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == c)
				{
					return i;
				}
			}
			return -1;
		}

		public static int CountSame(this string a, int starta, string b, int startb)
		{
			int i;
			for (i = 0; i + starta < a.Length && i + startb < b.Length && a[starta + i] == b[startb + i]; i++)
			{
			}
			return i;
		}

		public static string RemovePatternWhiteSpace(this string source)
		{
			throw new NotImplementedException();
		}

		public static string ReplaceAny(this string source, char[] chars, string newValue)
		{
			foreach (char c in chars)
			{
				source = source.Replace(c.ToString(), newValue);
			}
			return source;
		}

		public static string ReplaceAny(this string source, string[] strings, string newValue)
		{
			foreach (string oldValue in strings)
			{
				source = source.Replace(oldValue, newValue);
			}
			return source;
		}

		public static string Capitalize(this string word)
		{
			StringBuilder stringBuilder = new StringBuilder(word.ToLower());
			stringBuilder[0] = char.ToUpper(stringBuilder[0]);
			return stringBuilder.ToString();
		}

		public static string[] SplitWords(this string text)
		{
			MatchCollection matchCollection = SplitWordsRE.Matches(text);
			string[] array = new string[matchCollection.Count];
			int num = 0;
			foreach (Match item in matchCollection)
			{
				array[num++] = item.Value;
			}
			return array;
		}

		public static string IntsToString(int[] ints)
		{
			byte[] array = IntsToBytes(ints);
			string text = Encoding.UTF8.GetString(array, 0, array.Length);
			return text.Replace("\0", "");
		}

		public static byte[] IntsToBytes(int[] ints)
		{
			byte[] array = new byte[ints.Length * 4];
			int num = 0;
			for (int i = 0; i < ints.Length; i++)
			{
				array[num++] = (byte)ints[i];
				array[num++] = (byte)(ints[i] >> 8);
				array[num++] = (byte)(ints[i] >> 16);
				array[num++] = (byte)(ints[i] >> 24);
			}
			return array;
		}

		public static int[] BytesToInts(byte[] strBytes)
		{
			List<int> list = new List<int>();
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			while (num2 < strBytes.Length)
			{
				if (num >= 32)
				{
					list.Add(num3);
					num3 = 0;
					num = 0;
				}
				num3 |= strBytes[num2] << num;
				num2++;
				num += 8;
			}
			if (num > 0)
			{
				list.Add(num3);
			}
			return list.ToArray();
		}

		public static int[] StringToInts(string str, int maxInts)
		{
			Encoding uTF = Encoding.UTF8;
			char[] array = str.ToCharArray();
			new List<int>();
			int num = maxInts * 4;
			int i;
			for (i = 0; uTF.GetByteCount(array, 0, i) < num && i < array.Length; i++)
			{
			}
			if (uTF.GetByteCount(array, 0, i) > num)
			{
				i--;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(array, 0, i);
			return BytesToInts(bytes);
		}

		public static int[] StringToInts(string str)
		{
			new List<int>();
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return BytesToInts(bytes);
		}
	}
}
