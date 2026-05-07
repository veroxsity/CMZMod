using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Xna.Framework.GamerServices;

namespace DNA.IO.Storage
{
	public class PlayerISOSaveDevice : IsolatedStorageSaveDevice
	{
		public SignedInGamer Gamer { get; private set; }

		internal PlayerISOSaveDevice(SignedInGamer gamer, byte[] key, IsolatedStorageFile container)
			: base(key)
		{
			Gamer = gamer;
			_currentContainer = container;
			if (!_currentContainer.DirectoryExists(Gamer.Gamertag))
			{
				_currentContainer.CreateDirectory(Gamer.Gamertag);
			}
		}

		private string MakeRootRelative(string path)
		{
			return Path.Combine(Gamer.Gamertag, path);
		}

		public override void DeleteStorage()
		{
			lock (this)
			{
				if (_currentContainer == null)
				{
					return;
				}
				string[] directoryNames = _currentContainer.GetDirectoryNames();
				if (directoryNames.Length > 1)
				{
					try
					{
						if (_currentContainer.DirectoryExists(Gamer.Gamertag))
						{
							_currentContainer.DeleteDirectory(Gamer.Gamertag);
							_currentContainer.CreateDirectory(Gamer.Gamertag);
						}
						return;
					}
					catch
					{
						return;
					}
				}
				if (directoryNames.Length == 1 && directoryNames[0] == Gamer.Gamertag)
				{
					base.DeleteStorage();
				}
			}
		}

		protected override Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share)
		{
			string fileName2 = MakeRootRelative(fileName);
			return base.DeviceOpenFile(fileName2, mode, access, share);
		}

		protected override void DeviceDeleteFile(string fileName)
		{
			base.DeviceDeleteFile(MakeRootRelative(fileName));
		}

		protected override bool DeviceFileExists(string fileName)
		{
			return base.DeviceFileExists(MakeRootRelative(fileName));
		}

		protected override bool DeviceDirectoryExists(string dirName)
		{
			return base.DeviceDirectoryExists(MakeRootRelative(dirName));
		}

		protected override string[] DeviceGetDirectoryNames()
		{
			string[] array = base.DeviceGetDirectoryNames(Gamer.Gamertag);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring((Gamer.Gamertag + Path.DirectorySeparatorChar).Length);
			}
			return array;
		}

		protected override string[] DeviceGetFileNames()
		{
			string[] array = base.DeviceGetFileNames(Gamer.Gamertag);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring((Gamer.Gamertag + Path.DirectorySeparatorChar).Length);
			}
			return array;
		}

		protected override string[] DeviceGetFileNames(string pattern)
		{
			pattern = MakeRootRelative(pattern);
			string[] array = base.DeviceGetFileNames(pattern);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring((Gamer.Gamertag + Path.DirectorySeparatorChar).Length);
			}
			return array;
		}

		protected override string[] DeviceGetDirectoryNames(string pattern)
		{
			pattern = MakeRootRelative(pattern);
			string[] array = base.DeviceGetDirectoryNames(pattern);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring((Gamer.Gamertag + Path.DirectorySeparatorChar).Length);
			}
			return array;
		}

		protected override void DeviceCreateDirectory(string path)
		{
			base.DeviceCreateDirectory(MakeRootRelative(path));
		}

		protected override void DeviceDeleteDirectory(string path)
		{
			base.DeviceDeleteDirectory(MakeRootRelative(path));
		}

		public override void Save(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			if (!_currentContainer.DirectoryExists(Gamer.Gamertag))
			{
				_currentContainer.CreateDirectory(Gamer.Gamertag);
			}
			base.Save(fileName, tamperProof, compressed, saveAction);
		}
	}
}
