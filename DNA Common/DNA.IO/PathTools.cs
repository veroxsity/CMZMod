using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DNA.IO
{
	public static class PathTools
	{
		private static Regex HasQuestionMarkRegEx = new Regex("\\?", RegexOptions.Compiled);

		private static Regex IlegalCharactersRegex = new Regex("[\\/:<>|\"]", RegexOptions.Compiled);

		private static Regex CatchExtentionRegex = new Regex("^\\s*.+\\.([^\\.]+)\\s*$", RegexOptions.Compiled);

		private static string NonDotCharacters = "[^.]*";

		public static string ReplaceInvalidChars(string orginalPath)
		{
			return ReplaceInvalidChars(orginalPath, '_');
		}

		public static string ReplaceInvalidChars(string orginalPath, char replaceChar)
		{
			StringBuilder stringBuilder = new StringBuilder(orginalPath);
			for (int i = 0; i < stringBuilder.Length; i++)
			{
				char c = stringBuilder[i];
				if (c == '?' || c == '*')
				{
					stringBuilder[i] = replaceChar;
					continue;
				}
				char[] invalidPathChars = Path.GetInvalidPathChars();
				foreach (char c2 in invalidPathChars)
				{
					if (c == c2)
					{
						stringBuilder[i] = replaceChar;
					}
				}
			}
			return stringBuilder.ToString();
		}

		public static string GetTempFolderPath()
		{
			string text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(text);
			return text;
		}

		public static string GetTempFilePath()
		{
			return Path.Combine(Path.GetTempPath(), GetTempFileName());
		}

		public static string GetTempFileName()
		{
			return Guid.NewGuid().ToString() + ".tmp";
		}

		public static string[] GetFileNames(string[] paths)
		{
			string[] array = new string[paths.Length];
			for (int i = 0; i < paths.Length; i++)
			{
				array[i] = Path.GetFileName(paths[i]);
			}
			return array;
		}

		public static string RootDirectory(string path)
		{
			int num = path.IndexOf(Path.DirectorySeparatorChar);
			if (num < 0)
			{
				return "";
			}
			if (num == 0)
			{
				path = path.Substring(1);
				return RootDirectory(path);
			}
			string text = path.Substring(0, num);
			if (text == ".")
			{
				return RootDirectory(path.Substring(num));
			}
			if (text.Length > 1 && text[1] == ':')
			{
				return RootDirectory(path.Substring(num));
			}
			return text;
		}

		public static Regex FilePatternToRegex(string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException();
			}
			pattern = pattern.Trim();
			if (pattern.Length == 0)
			{
				throw new ArgumentException("Pattern is empty.");
			}
			if (IlegalCharactersRegex.IsMatch(pattern))
			{
				throw new ArgumentException("Patterns contains ilegal characters.");
			}
			bool flag = CatchExtentionRegex.IsMatch(pattern);
			bool flag2 = false;
			if (HasQuestionMarkRegEx.IsMatch(pattern))
			{
				flag2 = true;
			}
			else if (flag)
			{
				flag2 = CatchExtentionRegex.Match(pattern).Groups[1].Length != 3;
			}
			string input = Regex.Escape(pattern);
			input = "^" + Regex.Replace(input, "\\\\\\*", ".*");
			input = Regex.Replace(input, "\\\\\\?", ".");
			if (!flag2 && flag)
			{
				input += NonDotCharacters;
			}
			input += "$";
			return new Regex(input, RegexOptions.IgnoreCase | RegexOptions.Compiled);
		}
	}
}
