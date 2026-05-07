using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.GamerServices;

namespace DNA.IO.Storage
{
	public class IsolatedStorageSaveDevice : SaveDevice
	{
		protected IsolatedStorageFile _currentContainer;

		public PlayerISOSaveDevice GetPlayerISOSaveDevice(SignedInGamer gamer, byte[] key)
		{
			return new PlayerISOSaveDevice(gamer, key, _currentContainer);
		}

		public IsolatedStorageSaveDevice(byte[] key)
			: base(key)
		{
			_currentContainer = GetIsolatedStorage();
		}

		public bool DoesPlayerStorageExist(SignedInGamer gamer)
		{
			return _currentContainer.DirectoryExists(gamer.Gamertag);
		}

		private static IsolatedStorageFile GetIsolatedStorage()
		{
			return IsolatedStorageFile.GetUserStoreForApplication();
		}

		public override void Flush()
		{
			lock (this)
			{
				if (_currentContainer != null)
				{
					_currentContainer.Dispose();
					try
					{
						_currentContainer = GetIsolatedStorage();
						return;
					}
					catch
					{
						_currentContainer = null;
						return;
					}
				}
			}
		}

		public override void DeviceDispose()
		{
			if (_currentContainer != null)
			{
				_currentContainer.Dispose();
				_currentContainer = null;
			}
		}

		public override void DeleteStorage()
		{
			lock (this)
			{
				if (_currentContainer != null)
				{
					try
					{
						_currentContainer.Remove();
						_currentContainer.Dispose();
						_currentContainer = GetIsolatedStorage();
						return;
					}
					catch
					{
						_currentContainer = null;
						return;
					}
				}
			}
		}

		protected override Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			return _currentContainer.OpenFile(fileName, mode);
		}

		protected override void DeviceDeleteFile(string fileName)
		{
			_currentContainer.DeleteFile(fileName);
		}

		protected override bool DeviceFileExists(string fileName)
		{
			return _currentContainer.FileExists(fileName);
		}

		protected override bool DeviceDirectoryExists(string dirName)
		{
			return _currentContainer.DirectoryExists(dirName);
		}

		protected override string[] DeviceGetDirectoryNames()
		{
			return _currentContainer.GetDirectoryNames();
		}

		protected override string[] DeviceGetFileNames()
		{
			return _currentContainer.GetFileNames();
		}

		protected override string[] DeviceGetFileNames(string pattern)
		{
			string[] fileNames = _currentContainer.GetFileNames(pattern);
			return FilterAndAppend(pattern, fileNames);
		}

		private static string[] FilterAndAppend(string pattern, string[] paths)
		{
			string directoryName = Path.GetDirectoryName(pattern);
			string fileName = Path.GetFileName(pattern);
			Regex regex = null;
			if (!string.IsNullOrEmpty(fileName))
			{
				regex = PathTools.FilePatternToRegex(fileName);
			}
			List<string> list = new List<string>();
			for (int i = 0; i < paths.Length; i++)
			{
				paths[i] = Path.Combine(directoryName, paths[i]);
				string fileName2 = Path.GetFileName(paths[i]);
				if (regex == null || regex.IsMatch(fileName2))
				{
					list.Add(Path.Combine(directoryName, fileName2));
				}
			}
			return list.ToArray();
		}

		protected override string[] DeviceGetDirectoryNames(string pattern)
		{
			string[] directoryNames = _currentContainer.GetDirectoryNames(pattern);
			return FilterAndAppend(pattern, directoryNames);
		}

		protected override void DeviceCreateDirectory(string path)
		{
			_currentContainer.CreateDirectory(path);
		}

		protected override void DeviceDeleteDirectory(string path)
		{
			_currentContainer.DeleteDirectory(path);
		}
	}
}
