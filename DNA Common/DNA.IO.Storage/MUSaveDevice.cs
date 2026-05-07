using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Storage;

namespace DNA.IO.Storage
{
	public abstract class MUSaveDevice : SaveDevice, IGameComponent, IUpdateable
	{
		private static string promptForCancelledMessage;

		private static string forceCancelledReselectionMessage;

		private static string promptForDisconnectedMessage;

		private static string forceDisconnectedReselectionMessage;

		private static string deviceRequiredTitle;

		private static string deviceOptionalTitle;

		private static readonly string[] deviceOptionalOptions;

		private static readonly string[] deviceRequiredOptions;

		public bool ForceDeviceSelection;

		public bool PromptForReselect;

		private string _containerName;

		private int updateOrder;

		private bool enabled = true;

		private bool deviceWasConnected;

		private SaveDevicePromptState state;

		private readonly SaveDevicePromptEventArgs promptEventArgs = new SaveDevicePromptEventArgs();

		private readonly SaveDeviceEventArgs eventArgs = new SaveDeviceEventArgs();

		private StorageDevice storageDevice;

		private SuccessCallback _promptCallback;

		private StorageContainer _currentContainer;

		public static string PromptForCancelledMessage
		{
			get
			{
				return promptForCancelledMessage;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					promptForCancelledMessage = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string ForceCancelledReselectionMessage
		{
			get
			{
				return forceCancelledReselectionMessage;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					forceCancelledReselectionMessage = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string PromptForDisconnectedMessage
		{
			get
			{
				return promptForDisconnectedMessage;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					promptForDisconnectedMessage = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string ForceDisconnectedReselectionMessage
		{
			get
			{
				return forceDisconnectedReselectionMessage;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					forceDisconnectedReselectionMessage = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string DeviceRequiredTitle
		{
			get
			{
				return deviceRequiredTitle;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					deviceRequiredTitle = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string DeviceOptionalTitle
		{
			get
			{
				return deviceOptionalTitle;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					deviceOptionalTitle = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string OkOption
		{
			get
			{
				return deviceRequiredOptions[0];
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					deviceRequiredOptions[0] = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string YesOption
		{
			get
			{
				return deviceOptionalOptions[0];
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					deviceOptionalOptions[0] = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public static string NoOption
		{
			get
			{
				return deviceOptionalOptions[1];
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					deviceOptionalOptions[1] = ((value.Length < 256) ? value : value.Substring(0, 256));
				}
			}
		}

		public bool IsReady
		{
			get
			{
				if (storageDevice != null)
				{
					return storageDevice.IsConnected;
				}
				return false;
			}
		}

		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if (enabled != value)
				{
					enabled = value;
					if (this.EnabledChanged != null)
					{
						this.EnabledChanged(this, null);
					}
				}
			}
		}

		public int UpdateOrder
		{
			get
			{
				return updateOrder;
			}
			set
			{
				if (updateOrder != value)
				{
					updateOrder = value;
					if (this.UpdateOrderChanged != null)
					{
						this.UpdateOrderChanged(this, null);
					}
				}
			}
		}

		public event EventHandler<SaveDeviceEventArgs> DeviceDisconnected;

		public event EventHandler<EventArgs> EnabledChanged;

		public event EventHandler<EventArgs> UpdateOrderChanged;

		static MUSaveDevice()
		{
			deviceOptionalOptions = new string[2];
			deviceRequiredOptions = new string[1];
			StorageSettings.ResetSaveDeviceStrings();
		}

		protected MUSaveDevice(string containerName, byte[] key)
			: base(key)
		{
			_containerName = containerName;
		}

		public virtual void Initialize()
		{
		}

		public void PromptForDevice(SuccessCallback callBack)
		{
			_promptCallback = callBack;
			if (state == SaveDevicePromptState.None)
			{
				state = SaveDevicePromptState.ShowSelector;
			}
		}

		public static void EnsureCreated(StorageContainer container, string path)
		{
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName))
			{
				EnsureCreated(container, directoryName);
			}
			if (!container.DirectoryExists(path))
			{
				container.CreateDirectory(path);
			}
		}

		protected abstract void GetStorageDevice(AsyncCallback callback, SuccessCallback resultCallback);

		protected virtual void PrepareEventArgs(SaveDeviceEventArgs args)
		{
			args.Response = ((!ForceDeviceSelection) ? SaveDeviceEventResponse.Prompt : SaveDeviceEventResponse.Force);
			args.PlayerToPrompt = null;
		}

		public void Update(GameTime gameTime)
		{
			if (!GamerServicesDispatcher.IsInitialized)
			{
				throw new InvalidOperationException(Strings.NeedGamerService);
			}
			bool flag = (PromptForReselect || this.DeviceDisconnected != null) && storageDevice != null;
			bool flag2 = deviceWasConnected && storageDevice != null;
			if (flag)
			{
				flag2 = storageDevice.IsConnected;
			}
			if (!flag2 && deviceWasConnected)
			{
				PrepareEventArgs(eventArgs);
				if (this.DeviceDisconnected != null)
				{
					this.DeviceDisconnected(this, eventArgs);
				}
				if (PromptForReselect)
				{
					HandleEventArgResults();
				}
				else
				{
					state = SaveDevicePromptState.None;
				}
			}
			else if (!flag2)
			{
				try
				{
					if (!Guide.IsVisible)
					{
						switch (state)
						{
						case SaveDevicePromptState.ShowSelector:
							state = SaveDevicePromptState.None;
							GetStorageDevice(StorageDeviceSelectorCallback, _promptCallback);
							break;
						case SaveDevicePromptState.PromptForCanceled:
							ShowMessageBox(eventArgs.PlayerToPrompt, deviceOptionalTitle, promptForCancelledMessage, deviceOptionalOptions, ReselectPromptCallback, _promptCallback);
							break;
						case SaveDevicePromptState.ForceCanceledReselection:
							ShowMessageBox(eventArgs.PlayerToPrompt, deviceRequiredTitle, forceCancelledReselectionMessage, deviceRequiredOptions, ForcePromptCallback, null);
							break;
						case SaveDevicePromptState.PromptForDisconnected:
							ShowMessageBox(eventArgs.PlayerToPrompt, deviceOptionalTitle, promptForDisconnectedMessage, deviceOptionalOptions, ReselectPromptCallback, null);
							break;
						case SaveDevicePromptState.ForceDisconnectedReselection:
							ShowMessageBox(eventArgs.PlayerToPrompt, deviceRequiredTitle, forceDisconnectedReselectionMessage, deviceRequiredOptions, ForcePromptCallback, null);
							break;
						}
					}
				}
				catch (GuideAlreadyVisibleException)
				{
				}
			}
			deviceWasConnected = flag2;
		}

		private void StorageDeviceSelectorCallback(IAsyncResult result)
		{
			SuccessCallback successCallback = (SuccessCallback)result.AsyncState;
			storageDevice = StorageDevice.EndShowSelector(result);
			if (storageDevice != null && storageDevice.IsConnected)
			{
				try
				{
					_currentContainer = OpenContainer(_containerName);
					if (successCallback != null)
					{
						successCallback(true);
					}
					return;
				}
				catch
				{
					if (successCallback != null)
					{
						successCallback(false);
					}
					return;
				}
			}
			PrepareEventArgs(eventArgs);
			HandleEventArgResults();
		}

		private void ForcePromptCallback(IAsyncResult result)
		{
			Guide.EndShowMessageBox(result);
			state = SaveDevicePromptState.ShowSelector;
		}

		private void ReselectPromptCallback(IAsyncResult result)
		{
			int? num = Guide.EndShowMessageBox(result);
			state = ((num.HasValue && num.Value == 0) ? SaveDevicePromptState.ShowSelector : SaveDevicePromptState.None);
			promptEventArgs.ShowDeviceSelector = state == SaveDevicePromptState.ShowSelector;
			SuccessCallback successCallback = (SuccessCallback)result.AsyncState;
			if (state == SaveDevicePromptState.None && successCallback != null)
			{
				successCallback(false);
			}
		}

		private void HandleEventArgResults()
		{
			storageDevice = null;
			switch (eventArgs.Response)
			{
			case SaveDeviceEventResponse.Prompt:
				state = (deviceWasConnected ? SaveDevicePromptState.PromptForDisconnected : SaveDevicePromptState.PromptForCanceled);
				break;
			case SaveDeviceEventResponse.Force:
				state = (deviceWasConnected ? SaveDevicePromptState.ForceDisconnectedReselection : SaveDevicePromptState.ForceCanceledReselection);
				break;
			default:
				state = SaveDevicePromptState.None;
				break;
			}
		}

		private static void ShowMessageBox(PlayerIndex? player, string title, string text, IEnumerable<string> buttons, AsyncCallback callback, object state)
		{
			if (player.HasValue)
			{
				Guide.BeginShowMessageBox(player.Value, title, text, buttons, 0, MessageBoxIcon.None, callback, state);
			}
			else
			{
				Guide.BeginShowMessageBox(title, text, buttons, 0, MessageBoxIcon.None, callback, state);
			}
		}

		private StorageContainer OpenContainer(string containerName)
		{
			IAsyncResult asyncResult = storageDevice.BeginOpenContainer(containerName, null, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			return storageDevice.EndOpenContainer(asyncResult);
		}

		protected override void VerifyIsReady()
		{
			if (!IsReady)
			{
				throw new InvalidOperationException(Strings.StorageDevice_is_not_valid);
			}
		}

		public override void Flush()
		{
			if (storageDevice == null)
			{
				return;
			}
			lock (this)
			{
				if (_currentContainer != null)
				{
					_currentContainer.Dispose();
					try
					{
						_currentContainer = OpenContainer(_containerName);
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
			if (storageDevice == null)
			{
				return;
			}
			lock (this)
			{
				if (_currentContainer != null)
				{
					_currentContainer.Dispose();
					try
					{
						storageDevice.DeleteContainer(_containerName);
						_currentContainer = OpenContainer(_containerName);
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
			return _currentContainer.OpenFile(fileName, mode, access, share);
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
			return _currentContainer.GetFileNames(pattern);
		}

		protected override string[] DeviceGetDirectoryNames(string pattern)
		{
			return _currentContainer.GetDirectoryNames(pattern);
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
