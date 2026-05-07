using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using DNA.IO.Compression;
using DNA.Security;
using DNA.Security.Cryptography;

namespace DNA.IO.Storage
{
	public abstract class SaveDevice : IDisposable
	{
		[Flags]
		private enum FileOptions : uint
		{
			None = 0u,
			Compressesd = 1u,
			Encrypted = 2u
		}

		private class FileOperationState
		{
			public string File;

			public string Pattern;

			public bool TamperProof;

			public bool Compressed;

			public FileAction Action;

			public object UserState;

			public void Reset()
			{
				TamperProof = false;
				Compressed = false;
				File = null;
				Pattern = null;
				Action = null;
				UserState = null;
			}
		}

		private const int FileIdent = 1146311762;

		private const int FileVersion = 5;

		private static byte[] CommonKey = new byte[16]
		{
			236, 34, 252, 119, 2, 225, 246, 242, 214, 172,
			157, 191, 175, 246, 57, 246
		};

		private static byte[] LocalKey;

		public static readonly int[] ProcessorAffinity = new int[1] { 5 };

		private CompressionTools compressionTools = new CompressionTools();

		private bool disposed;

		private Queue<FileOperationState> pendingStates = new Queue<FileOperationState>(100);

		private readonly object pendingOperationCountLock = new object();

		private int pendingOperations;

		public bool IsBusy
		{
			get
			{
				lock (pendingOperationCountLock)
				{
					return pendingOperations > 0;
				}
			}
		}

		public event SaveCompletedEventHandler SaveCompleted;

		public event LoadCompletedEventHandler LoadCompleted;

		public event DeleteDirectoryCompletedEventHandler DeleteDirectoryCompleted;

		public event DeleteCompletedEventHandler DeleteCompleted;

		public event FileExistsCompletedEventHandler FileExistsCompleted;

		public event GetFilesCompletedEventHandler GetFilesCompleted;

		public SaveDevice(byte[] key)
		{
			LocalKey = key;
		}

		protected abstract Stream DeviceOpenFile(string fileName, FileMode mode, FileAccess access, FileShare share);

		protected abstract void DeviceDeleteFile(string fileName);

		protected abstract bool DeviceFileExists(string fileName);

		protected abstract bool DeviceDirectoryExists(string dirName);

		protected abstract string[] DeviceGetDirectoryNames();

		protected abstract string[] DeviceGetDirectoryNames(string pattern);

		protected abstract string[] DeviceGetFileNames();

		protected abstract string[] DeviceGetFileNames(string pattern);

		protected abstract void DeviceCreateDirectory(string path);

		protected abstract void DeviceDeleteDirectory(string path);

		public abstract void Flush();

		public abstract void DeleteStorage();

		public abstract void DeviceDispose();

		protected virtual void VerifyIsReady()
		{
		}

		private void Save(string fileName, byte[] dataToSave, bool tamperProof, bool compressed)
		{
			FileOptions fileOptions = FileOptions.None;
			if (compressed)
			{
				fileOptions |= FileOptions.Compressesd;
				dataToSave = compressionTools.Compress(dataToSave);
			}
			if (tamperProof)
			{
				fileOptions |= FileOptions.Encrypted;
				dataToSave = ((LocalKey != null) ? SecurityTools.EncryptData(LocalKey, dataToSave) : SecurityTools.EncryptData(CommonKey, dataToSave));
				MD5HashProvider mD5HashProvider = new MD5HashProvider();
				Hash hash = mD5HashProvider.Compute(dataToSave);
				MemoryStream memoryStream = new MemoryStream(dataToSave.Length + hash.Data.Length + 8);
				BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
				binaryWriter.Write(hash.Data.Length);
				binaryWriter.Write(hash.Data);
				binaryWriter.Write(dataToSave.Length);
				binaryWriter.Write(dataToSave);
				binaryWriter.Flush();
				dataToSave = memoryStream.ToArray();
			}
			using (Stream output = DeviceOpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				BinaryWriter binaryWriter2 = new BinaryWriter(output);
				binaryWriter2.Write(1146311762);
				binaryWriter2.Write(5);
				binaryWriter2.Write((uint)fileOptions);
				binaryWriter2.Write(dataToSave.Length);
				binaryWriter2.Write(dataToSave);
				binaryWriter2.Flush();
			}
		}

		public virtual void Save(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			VerifyIsReady();
			lock (this)
			{
				MemoryStream memoryStream = new MemoryStream(1024);
				saveAction(memoryStream);
				byte[] dataToSave = memoryStream.ToArray();
				Save(fileName, dataToSave, tamperProof, compressed);
			}
		}

		public void Load(string fileName, FileAction loadAction)
		{
			VerifyIsReady();
			lock (this)
			{
				int num2;
				FileOptions fileOptions;
				byte[] array;
				using (Stream input = DeviceOpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					BinaryReader binaryReader = new BinaryReader(input);
					int num = binaryReader.ReadInt32();
					if (num != 1146311762)
					{
						throw new Exception();
					}
					num2 = binaryReader.ReadInt32();
					if (num2 < 3 || num2 > 5)
					{
						throw new Exception();
					}
					fileOptions = (FileOptions)binaryReader.ReadUInt32();
					int count = binaryReader.ReadInt32();
					array = binaryReader.ReadBytes(count);
				}
				if ((FileOptions.Encrypted & fileOptions) != FileOptions.None)
				{
					MemoryStream input2 = new MemoryStream(array);
					BinaryReader binaryReader2 = new BinaryReader(input2);
					MD5HashProvider mD5HashProvider = new MD5HashProvider();
					int count2 = binaryReader2.ReadInt32();
					Hash other = mD5HashProvider.CreateHash(binaryReader2.ReadBytes(count2));
					int count3 = binaryReader2.ReadInt32();
					array = binaryReader2.ReadBytes(count3);
					Hash hash = mD5HashProvider.Compute(array);
					if (!hash.Equals(other))
					{
						throw new Exception();
					}
					array = ((LocalKey != null) ? SecurityTools.DecryptData(LocalKey, array) : SecurityTools.DecryptData(CommonKey, array));
				}
				if ((FileOptions.Compressesd & fileOptions) != FileOptions.None)
				{
					array = compressionTools.Decompress(array);
				}
				if (num2 < 5)
				{
					Save(fileName, array, (FileOptions.Compressesd & fileOptions) != 0, (FileOptions.Encrypted & fileOptions) != 0);
				}
				MemoryStream stream = new MemoryStream(array);
				loadAction(stream);
			}
		}

		public void Delete(string fileName)
		{
			VerifyIsReady();
			lock (this)
			{
				if (DeviceFileExists(fileName))
				{
					DeviceDeleteFile(fileName);
				}
			}
		}

		public bool FileExists(string fileName)
		{
			VerifyIsReady();
			lock (this)
			{
				return DeviceFileExists(fileName);
			}
		}

		public string[] GetFiles()
		{
			return GetFiles(null);
		}

		public string[] GetFiles(string pattern)
		{
			VerifyIsReady();
			lock (this)
			{
				return string.IsNullOrEmpty(pattern) ? DeviceGetFileNames() : DeviceGetFileNames(pattern);
			}
		}

		public void GetDirectoriesAsync(string path)
		{
			throw new NotImplementedException();
		}

		public void GetDirectoriesAsync(string path, string pattern)
		{
			throw new NotImplementedException();
		}

		public void CreateDirectoryAsync(string path)
		{
			throw new NotImplementedException();
		}

		public void DeleteDirectoryAsync(string path)
		{
			DeleteDirectoryAsync(path, null);
		}

		public void DeleteDirectoryAsync(string path, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.File = path;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoDeleteDirectoryAsync, fileOperationState);
		}

		public string[] GetDirectories(string path)
		{
			return GetDirectories(path, null);
		}

		public string[] GetDirectories(string path, string pattern)
		{
			VerifyIsReady();
			lock (this)
			{
				if (string.IsNullOrEmpty(pattern))
				{
					pattern = "*";
				}
				path = ((!string.IsNullOrEmpty(path)) ? Path.Combine(path, pattern) : pattern);
				return DeviceGetDirectoryNames(path);
			}
		}

		public void CreateDirectory(string path)
		{
			VerifyIsReady();
			lock (this)
			{
				DeviceCreateDirectory(path);
			}
		}

		public void DeleteDirectory(string path)
		{
			VerifyIsReady();
			lock (this)
			{
				DeleteDirectoryInternal(path);
			}
		}

		private void DeleteDirectoryInternal(string path)
		{
			string[] array = DeviceGetDirectoryNames(Path.Combine(path, "*"));
			string[] array2 = array;
			foreach (string path2 in array2)
			{
				DeleteDirectoryInternal(path2);
			}
			string[] array3 = DeviceGetFileNames(Path.Combine(path, "*"));
			string[] array4 = array3;
			foreach (string fileName in array4)
			{
				DeviceDeleteFile(fileName);
			}
			DeviceDeleteDirectory(path);
		}

		public void DirectoryExistsAsync(string path)
		{
			throw new NotImplementedException();
		}

		public bool DirectoryExists(string path)
		{
			VerifyIsReady();
			lock (this)
			{
				return DeviceDirectoryExists(path);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DeviceDispose();
				}
				disposed = true;
			}
		}

		public void SaveRaw(string fileName, FileAction saveAction)
		{
			VerifyIsReady();
			lock (this)
			{
				using (Stream stream = DeviceOpenFile(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
				{
					saveAction(stream);
				}
			}
		}

		public void LoadRaw(string fileName, FileAction loadAction)
		{
			VerifyIsReady();
			lock (this)
			{
				using (Stream stream = DeviceOpenFile(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					loadAction(stream);
				}
			}
		}

		public void SaveAsync(string fileName, bool tamperProof, bool compressed, FileAction saveAction)
		{
			SaveAsync(fileName, tamperProof, compressed, saveAction, null);
		}

		public void SaveAsync(string fileName, bool tamperProof, bool compressed, FileAction saveAction, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.TamperProof = tamperProof;
			fileOperationState.Compressed = compressed;
			fileOperationState.Action = saveAction;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoSaveAsync, fileOperationState);
		}

		public void LoadAsync(string fileName, FileAction loadAction)
		{
			LoadAsync(fileName, loadAction, null);
		}

		public void LoadAsync(string fileName, FileAction loadAction, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.Action = loadAction;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoLoadAsync, fileOperationState);
		}

		public void DeleteAsync(string fileName)
		{
			DeleteAsync(fileName, null);
		}

		public void DeleteAsync(string fileName, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoDeleteAsync, fileOperationState);
		}

		public void FileExistsAsync(string fileName)
		{
			FileExistsAsync(fileName, null);
		}

		public void FileExistsAsync(string fileName, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.File = fileName;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoFileExistsAsync, fileOperationState);
		}

		public void GetFilesAsync()
		{
			GetFilesAsync(null);
		}

		public void GetFilesAsync(object userState)
		{
			GetFilesAsync("*", userState);
		}

		public void GetFilesAsync(string pattern)
		{
			GetFilesAsync(pattern, null);
		}

		public void GetFilesAsync(string pattern, object userState)
		{
			PendingOperationsIncrement();
			FileOperationState fileOperationState = GetFileOperationState();
			fileOperationState.Pattern = pattern;
			fileOperationState.UserState = userState;
			ThreadPool.QueueUserWorkItem(DoGetFilesAsync, fileOperationState);
		}

		private void SetProcessorAffinity()
		{
			Thread.CurrentThread.SetProcessorAffinity(ProcessorAffinity);
		}

		private void DoSaveAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			try
			{
				Save(fileOperationState.File, fileOperationState.TamperProof, fileOperationState.Compressed, fileOperationState.Action);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, fileOperationState.UserState);
			if (this.SaveCompleted != null)
			{
				this.SaveCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void DoLoadAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			try
			{
				Load(fileOperationState.File, fileOperationState.Action);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, fileOperationState.UserState);
			if (this.LoadCompleted != null)
			{
				this.LoadCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void DoDeleteDirectoryAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			try
			{
				DeleteDirectory(fileOperationState.File);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, fileOperationState.UserState);
			if (this.DeleteDirectoryCompleted != null)
			{
				this.DeleteDirectoryCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void DoDeleteAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			try
			{
				Delete(fileOperationState.File);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			FileActionCompletedEventArgs args = new FileActionCompletedEventArgs(error, fileOperationState.UserState);
			if (this.DeleteCompleted != null)
			{
				this.DeleteCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void DoFileExistsAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			bool result = false;
			try
			{
				result = FileExists(fileOperationState.File);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			FileExistsCompletedEventArgs args = new FileExistsCompletedEventArgs(error, result, fileOperationState.UserState);
			if (this.FileExistsCompleted != null)
			{
				this.FileExistsCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void DoGetFilesAsync(object asyncState)
		{
			SetProcessorAffinity();
			FileOperationState fileOperationState = asyncState as FileOperationState;
			Exception error = null;
			string[] result = null;
			try
			{
				result = GetFiles(fileOperationState.Pattern);
			}
			catch (Exception ex)
			{
				error = ex;
			}
			GetFilesCompletedEventArgs args = new GetFilesCompletedEventArgs(error, result, fileOperationState.UserState);
			if (this.GetFilesCompleted != null)
			{
				this.GetFilesCompleted(this, args);
			}
			ReturnFileOperationState(fileOperationState);
			PendingOperationsDecrement();
		}

		private void PendingOperationsIncrement()
		{
			lock (pendingOperationCountLock)
			{
				pendingOperations++;
			}
		}

		private void PendingOperationsDecrement()
		{
			lock (pendingOperationCountLock)
			{
				pendingOperations--;
			}
		}

		private FileOperationState GetFileOperationState()
		{
			lock (pendingStates)
			{
				if (pendingStates.Count > 0)
				{
					FileOperationState fileOperationState = pendingStates.Dequeue();
					fileOperationState.Reset();
					return fileOperationState;
				}
				return new FileOperationState();
			}
		}

		private void ReturnFileOperationState(FileOperationState state)
		{
			lock (pendingStates)
			{
				pendingStates.Enqueue(state);
			}
		}

		~SaveDevice()
		{
			Dispose(false);
		}
	}
}
