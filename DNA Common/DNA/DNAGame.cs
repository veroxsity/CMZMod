using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using DNA.Audio;
using DNA.Diagnostics.IssueReporting;
using DNA.Distribution;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Input;
using DNA.Multimedia.Broadcasting;
using DNA.Net;
using DNA.Profiling;
using DNA.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Net;

namespace DNA
{
	public class DNAGame : Game
	{
		private enum LoadStatus
		{
			NotStarted,
			InProcess,
			Complete
		}

		public enum ScreenModes
		{
			Mode1080,
			Mode720
		}

		private enum CodeVal
		{
			None = 0,
			Up = 1,
			Down = 2,
			Left = 4,
			Right = 8,
			A = 0x10,
			B = 0x20,
			X = 0x40,
			Y = 0x80
		}

		private bool processMessages;

		public OnlineServices LicenseServices;

		public static Random Random = new Random();

		private Version _version;

		private static DateTime _gameStartTime = DateTime.UtcNow;

		public BroadcastStream CurrentBroadcastStream;

		public Texture2D MousePointer;

		public bool PauseDuringGuide = true;

		public bool Stop;

		public DialogManager DialogManager;

		protected GraphicsDeviceManager Graphics;

		protected ScreenGroup _screenManager = new ScreenGroup(false);

		protected SpriteBatch SpriteBatch;

		protected InputManager InputManager;

		private bool _firstFrame = true;

		private NetworkSession _networkSession;

		private VoiceChat _voiceChat;

		public Texture2D DummyTexture;

		private GameTime _currentGameTime = new GameTime();

		public bool LimitElapsedGameTime = true;

		private RenderTarget2D _offscreenBuffer;

		public TaskScheduler TaskScheduler = new TaskScheduler();

		private LoadStatus _loadStatus;

		public bool ShowTitleSafeArea = true;

		public SpriteFont DebugFont;

		public bool CheatsEnabled;

		private CodeVal[] konamiCode = new CodeVal[10]
		{
			CodeVal.Up,
			CodeVal.Up,
			CodeVal.Down,
			CodeVal.Down,
			CodeVal.Left,
			CodeVal.Right,
			CodeVal.Left,
			CodeVal.Right,
			CodeVal.B,
			CodeVal.A
		};

		private int CodeLimit = 10;

		private Queue<CodeVal> recentCodes = new Queue<CodeVal>();

		protected bool HasGamerServices;

		protected GVerifier _gVerifier = new GVerifier();

		private Dictionary<byte, NetworkGamer> _currentPlayers = new Dictionary<byte, NetworkGamer>();

		private Queue<char> _inboundKeys = new Queue<char>();

		private bool _doAfterLoad;

		private Exception LastException;

		private bool _doSystemUpdates = true;

		public float Brightness;

		private StringBuilder frsb = new StringBuilder();

		private Rectangle _bufferDestRect;

		public Version Version
		{
			get
			{
				return _version;
			}
		}

		public RenderTarget2D OffScreenBuffer
		{
			get
			{
				return _offscreenBuffer;
			}
		}

		public virtual string ServerMessage
		{
			get
			{
				return "";
			}
			set
			{
			}
		}

		public ScreenGroup ScreenManager
		{
			get
			{
				return _screenManager;
			}
		}

		public bool Loading
		{
			get
			{
				return _loadStatus != LoadStatus.Complete;
			}
		}

		public GameTime CurrentGameTime
		{
			get
			{
				return _currentGameTime;
			}
			set
			{
				Interlocked.Exchange(value: (LimitElapsedGameTime && !(value.ElapsedGameTime <= TimeSpan.FromSeconds(0.1))) ? new GameTime(value.TotalGameTime, TimeSpan.FromSeconds(0.1), true) : value, location1: ref _currentGameTime);
			}
		}

		public NetworkSession CurrentNetworkSession
		{
			get
			{
				return _networkSession;
			}
		}

		public void CreateOffscreenBuffer(int width, int height)
		{
			PresentationParameters presentationParameters = base.GraphicsDevice.PresentationParameters;
			_offscreenBuffer = new RenderTarget2D(base.GraphicsDevice, width, height, false, presentationParameters.BackBufferFormat, presentationParameters.DepthStencilFormat, 1, RenderTargetUsage.DiscardContents);
		}

		public void WaitforSave()
		{
		}

		protected void StartVoiceChat(LocalNetworkGamer gamer)
		{
			_voiceChat = new VoiceChat(gamer);
		}

		protected void ShowSignIn()
		{
			DialogManager.ShowSignIn(false);
		}

		public void ShowMarketPlace()
		{
			DialogManager.ShowMarketPlace(Screen.SelectedPlayerIndex.Value);
		}

		public void ShowMarketPlace(PlayerIndex player)
		{
			DialogManager.ShowMarketPlace(player);
		}

		private string GetLocalizedAssetName(string assetName)
		{
			string[] array = new string[2]
			{
				CultureInfo.CurrentCulture.Name,
				CultureInfo.CurrentCulture.TwoLetterISOLanguageName
			};
			string[] array2 = array;
			foreach (string text in array2)
			{
				string text2 = assetName + '.' + text;
				string path = Path.Combine(base.Content.RootDirectory, text2 + ".xnb");
				if (File.Exists(path))
				{
					return text2;
				}
			}
			return assetName;
		}

		public Texture2D LoadLocalizedImage(string name)
		{
			string localizedAssetName = GetLocalizedAssetName(name);
			return base.Content.Load<Texture2D>(localizedAssetName);
		}

		public virtual void StartGamerServices()
		{
			base.Components.Add(new GamerServicesComponent(this));
			HasGamerServices = true;
		}

		public DNAGame(bool PreferMultiSampling, Version version)
		{
			InputManager = new InputManager(this);
			_version = version;
			TaskScheduler.ThreadException += TaskScheduler_ThreadException;
			DialogManager = new DialogManager(this);
			Graphics = new GraphicsDeviceManager(this);
			GraphicsDeviceLocker.Create(Graphics);
			base.Content.RootDirectory = "Content";
			Screen.PlayerSignedIn += Screen_PlayerSignedIn;
			Screen.PlayerSignedOut += Screen_PlayerSignedOut;
			Graphics.PreferredBackBufferWidth = 1280;
			Graphics.PreferredBackBufferHeight = 720;
			Graphics.PreferMultiSampling = PreferMultiSampling;
		}

		protected override void OnExiting(object sender, EventArgs args)
		{
			if (CurrentBroadcastStream != null)
			{
				CurrentBroadcastStream.Broadcasting = false;
				CurrentBroadcastStream.Dispose();
			}
			if (TaskScheduler != null)
			{
				TaskScheduler.Exit();
			}
			base.OnExiting(sender, args);
		}

		private void TaskScheduler_ThreadException(object sender, TaskScheduler.ExceptionEventArgs e)
		{
			CrashGame(e.InnerException);
		}

		protected void WantProfiling(bool fixTimeStep, bool syncRetrace)
		{
			Profiler.CreateComponent(this);
			base.IsFixedTimeStep = fixTimeStep;
			Graphics.SynchronizeWithVerticalRetrace = syncRetrace;
			Profiler.Profiling = true;
			Profiler.SetColor("Update", Color.DarkBlue);
			Profiler.SetColor("Physics", Color.DarkRed);
			Profiler.SetColor("Collision", Color.Chocolate);
			Profiler.SetColor("Drawing", Color.DarkGreen);
			Profiler.SetColor("UpdateTransform", Color.DarkGoldenrod);
			Profiler.SetColor("SetDefPose", Color.DarkGray);
			Profiler.SetColor("AnimPlrUpdate", Color.DarkOrange);
			Profiler.SetColor("CopyTforms", Color.DarkSlateBlue);
		}

		private void Screen_PlayerSignedOut(object sender, SignedOutEventArgs e)
		{
			OnPlayerSignedOut(e.Gamer);
		}

		private void Screen_PlayerSignedIn(object sender, SignedInEventArgs e)
		{
			OnPlayerSignedIn(e.Gamer);
		}

		protected virtual void OnPlayerSignedIn(SignedInGamer gamer)
		{
		}

		protected virtual void OnPlayerSignedOut(SignedInGamer gamer)
		{
		}

		public void LeaveGame()
		{
			bool flag = false;
			if (_networkSession != null)
			{
				if (_networkSession.AllowHostMigration)
				{
					flag = true;
				}
				_networkSession.Dispose();
			}
			_networkSession = null;
			if (flag)
			{
				OnSessionEnded(NetworkSessionEndReason.Disconnected);
			}
		}

		private void RegisterNetworkCallbacks(NetworkSession session)
		{
			session.GamerJoined += _networkSession_GamerJoined;
			session.GamerLeft += _networkSession_GamerLeft;
			session.GameEnded += _networkSession_GameEnded;
			session.GameStarted += _networkSession_GameStarted;
			session.HostChanged += _networkSession_HostChanged;
			session.SessionEnded += _networkSession_SessionEnded;
		}

		public void HostGame(NetworkSessionType sessionType, NetworkSessionProperties properties, IList<SignedInGamer> gamers, int maxPlayers, bool hostMigration, bool joinInprogress, SuccessCallback callback)
		{
			HostGame(sessionType, properties, gamers, maxPlayers, hostMigration, joinInprogress, callback, "XNAGame", 0, null);
		}

		public void HostGame(NetworkSessionType sessionType, NetworkSessionProperties properties, IList<SignedInGamer> gamers, int maxPlayers, bool hostMigration, bool joinInprogress, SuccessCallback callback, string gameName, int networkVersion, string password)
		{
			processMessages = false;
			NetworkSession.BeginCreate(sessionType, gamers, maxPlayers, 0, properties, delegate(IAsyncResult result)
			{
				SuccessCallback successCallback = (SuccessCallback)result.AsyncState;
				try
				{
					_networkSession = NetworkSession.EndCreate(result);
					_networkSession.AllowHostMigration = hostMigration;
					_networkSession.AllowJoinInProgress = joinInprogress;
					RegisterNetworkCallbacks(_networkSession);
				}
				catch (Exception)
				{
					if (successCallback != null)
					{
						successCallback(false);
					}
					processMessages = true;
					return;
				}
				if (successCallback != null)
				{
					successCallback(true);
				}
				processMessages = true;
			}, callback);
		}

		public void JoinInvitedGame(IList<SignedInGamer> gamers, SuccessCallback callback)
		{
			processMessages = false;
			try
			{
				NetworkSession.BeginJoinInvited(gamers, delegate(IAsyncResult result)
				{
					SuccessCallback successCallback = (SuccessCallback)result.AsyncState;
					bool success = true;
					try
					{
						_networkSession = NetworkSession.EndJoinInvited(result);
						RegisterNetworkCallbacks(_networkSession);
					}
					catch
					{
						_networkSession = null;
						success = false;
					}
					if (successCallback != null)
					{
						successCallback(success);
					}
					processMessages = true;
				}, callback);
			}
			catch
			{
				if (callback != null)
				{
					callback(false);
				}
			}
		}

		public void JoinGame(AvailableNetworkSession session, SuccessCallback callback)
		{
			JoinGame(session, null, callback, "XNAGame", 0, null);
		}

		public void JoinGame(AvailableNetworkSession session, IList<SignedInGamer> gamers, SuccessCallbackWithMessage callback, string gameName, int version, string password)
		{
			processMessages = false;
			string failureMessage = null;
			NetworkSession.BeginJoin(session, delegate(IAsyncResult result)
			{
				bool success = true;
				SuccessCallbackWithMessage successCallbackWithMessage = (SuccessCallbackWithMessage)result.AsyncState;
				try
				{
					_networkSession = NetworkSession.EndJoin(result);
					RegisterNetworkCallbacks(_networkSession);
				}
				catch (Exception ex)
				{
					failureMessage = ex.Message;
					LeaveGame();
					success = false;
				}
				if (successCallbackWithMessage != null)
				{
					successCallbackWithMessage(success, failureMessage);
				}
				processMessages = true;
			}, callback);
		}

		public void JoinGame(AvailableNetworkSession session, IList<SignedInGamer> gamers, SuccessCallback callback, string gameName, int version, string password)
		{
			processMessages = false;
			NetworkSession.BeginJoin(session, delegate(IAsyncResult result)
			{
				bool success = true;
				SuccessCallback successCallback = (SuccessCallback)result.AsyncState;
				try
				{
					_networkSession = NetworkSession.EndJoin(result);
					RegisterNetworkCallbacks(_networkSession);
				}
				catch (Exception)
				{
					LeaveGame();
					success = false;
				}
				if (successCallback != null)
				{
					successCallback(success);
				}
				processMessages = true;
			}, callback);
		}

		private void _networkSession_SessionEnded(object sender, NetworkSessionEndedEventArgs e)
		{
			OnSessionEnded(e.EndReason);
		}

		public virtual void OnSessionEnded(NetworkSessionEndReason reason)
		{
		}

		private void _networkSession_HostChanged(object sender, HostChangedEventArgs e)
		{
			OnHostChanged(e.OldHost, e.NewHost);
		}

		public virtual void OnHostChanged(NetworkGamer oldHost, NetworkGamer newHost)
		{
		}

		private void _networkSession_GameStarted(object sender, GameStartedEventArgs e)
		{
			OnGameStarted();
		}

		public virtual void OnGameStarted()
		{
		}

		private void _networkSession_GameEnded(object sender, GameEndedEventArgs e)
		{
			OnGameEnded();
		}

		public virtual void OnGameEnded()
		{
		}

		private void _networkSession_GamerJoined(object sender, GamerJoinedEventArgs e)
		{
			_currentPlayers[e.Gamer.Id] = e.Gamer;
			OnGamerJoined(e.Gamer);
		}

		protected virtual void OnGamerJoined(NetworkGamer gamer)
		{
		}

		private void _networkSession_GamerLeft(object sender, GamerLeftEventArgs e)
		{
			_currentPlayers.Remove(e.Gamer.Id);
			OnGamerLeft(e.Gamer);
		}

		protected virtual void OnGamerLeft(NetworkGamer gamer)
		{
		}

		protected override void Initialize()
		{
			DummyTexture = new Texture2D(base.GraphicsDevice, 1, 1);
			DummyTexture.SetData(new Color[1] { Color.White });
			SpriteBatch = new SpriteBatch(base.GraphicsDevice);
			base.Initialize();
		}

		private void KeyGrabber_InboundCharEvent(char obj)
		{
			_inboundKeys.Enqueue(obj);
		}

		protected override void LoadContent()
		{
			DebugFont = base.Content.Load<SpriteFont>("Debug");
			MousePointer = base.Content.Load<Texture2D>("MousePointer");
			_loadStatus = LoadStatus.InProcess;
			TaskScheduler.QueueUserWorkItem(LoadThreadRoutine);
			base.LoadContent();
		}

		public void CrashGame(Exception e)
		{
			if (LastException == null)
			{
				LastException = e;
			}
		}

		private void LoadThreadRoutine()
		{
			SecondaryLoad();
			_loadStatus = LoadStatus.Complete;
			_doAfterLoad = true;
		}

		protected virtual void SecondaryLoad()
		{
		}

		protected virtual void SendNetworkUpdates(NetworkSession session, GameTime gameTime)
		{
		}

		protected virtual void LoadingUpdate(GameTime gameTime)
		{
		}

		public void SuspendSystemUpdates()
		{
			_doSystemUpdates = false;
		}

		public void ResumeSystemUpdates()
		{
			_doSystemUpdates = true;
		}

		private void FixNetworkBug()
		{
			if (_networkSession == null)
			{
				return;
			}
			List<KeyValuePair<byte, NetworkGamer>> list = new List<KeyValuePair<byte, NetworkGamer>>();
			foreach (KeyValuePair<byte, NetworkGamer> currentPlayer in _currentPlayers)
			{
				if (CurrentNetworkSession.FindGamerById(currentPlayer.Key) == null)
				{
					list.Add(currentPlayer);
				}
			}
			foreach (KeyValuePair<byte, NetworkGamer> item in list)
			{
				_currentPlayers.Remove(item.Key);
				OnGamerLeft(item.Value);
			}
		}

		protected override void Update(GameTime gameTime)
		{
			if (LastException != null)
			{
				throw LastException;
			}
			if (_doAfterLoad)
			{
				AfterLoad();
				_doAfterLoad = false;
			}
			Profiler.MarkFrame();
			CurrentGameTime = gameTime;
			if (CurrentBroadcastStream != null)
			{
				CurrentBroadcastStream.Update(gameTime);
			}
			SoundManager.Instance.Update();
			if (Stop)
			{
				return;
			}
			bool flag = false;
			try
			{
				if (HasGamerServices && Guide.IsVisible)
				{
					flag = true;
				}
			}
			catch
			{
			}
			if (!flag || !PauseDuringGuide)
			{
				if (_firstFrame)
				{
					OnFirstFrame();
					_firstFrame = false;
				}
				if (CurrentNetworkSession != null)
				{
					try
					{
						CurrentNetworkSession.Update();
					}
					catch
					{
						FixNetworkBug();
					}
					if (CurrentNetworkSession != null && processMessages)
					{
						ProcessNetworkMessages(CurrentGameTime);
						if (_networkSession != null)
						{
							SendNetworkUpdates(_networkSession, CurrentGameTime);
						}
					}
				}
				InputManager.Update();
				while (_inboundKeys.Count != 0)
				{
					ScreenManager.ProcessChar(gameTime, _inboundKeys.Dequeue());
				}
				ScreenManager.ProcessInput(InputManager, CurrentGameTime);
				ScreenManager.Update(this, CurrentGameTime);
				EvalCodes();
				if (LastException != null)
				{
					throw LastException;
				}
				if (ScreenManager.Exiting && !Loading)
				{
					Exit();
				}
			}
			if (HasGamerServices)
			{
				DialogManager.Update(CurrentGameTime);
			}
			if (_doSystemUpdates)
			{
				base.Update(CurrentGameTime);
			}
			if (LastException == null)
			{
				return;
			}
			throw LastException;
		}

		public static PlayerID GetLocalID()
		{
			return PlayerID.Null;
		}

		protected void ShowTrialWarning(PlayerIndex player)
		{
			DialogManager.ShowMessageBox(player, "Full Mode Only", "This feature is only availible in the full version of the game.\n\nWould you like to purchase the game now?", new string[2] { "Yes", "No" }, 0, MessageBoxIcon.Warning, delegate(int? index)
			{
				int valueOrDefault = index.GetValueOrDefault();
				if (index.HasValue && valueOrDefault == 0)
				{
					ShowMarketPlace(player);
				}
			}, player);
		}

		private void EvalCodes()
		{
			if (Guide.IsTrialMode)
			{
				return;
			}
			CodeVal codeVal = CodeVal.None;
			GameController gameController = InputManager.Controllers[1];
			if (gameController.PressedButtons.A)
			{
				codeVal |= CodeVal.A;
			}
			if (gameController.PressedButtons.B)
			{
				codeVal |= CodeVal.B;
			}
			if (gameController.PressedButtons.X)
			{
				codeVal |= CodeVal.X;
			}
			if (gameController.PressedButtons.Y)
			{
				codeVal |= CodeVal.Y;
			}
			if (gameController.PressedDPad.Up)
			{
				codeVal |= CodeVal.Up;
			}
			if (gameController.PressedDPad.Down)
			{
				codeVal |= CodeVal.Down;
			}
			if (gameController.PressedDPad.Left)
			{
				codeVal |= CodeVal.Left;
			}
			if (gameController.PressedDPad.Right)
			{
				codeVal |= CodeVal.Right;
			}
			if (codeVal != CodeVal.None)
			{
				recentCodes.Enqueue(codeVal);
				while (recentCodes.Count > CodeLimit)
				{
					recentCodes.Dequeue();
				}
			}
			if (recentCodes.Count < konamiCode.Length)
			{
				return;
			}
			CodeVal[] array = recentCodes.ToArray();
			for (int i = 0; i < konamiCode.Length; i++)
			{
				if (konamiCode[i] != array[i])
				{
					return;
				}
			}
			recentCodes.Clear();
			CheatsEnabled = !CheatsEnabled;
		}

		protected virtual void OnFirstFrame()
		{
		}

		protected virtual void OnMessage(Message message)
		{
		}

		private void ProcessNetworkMessages(GameTime gameTime)
		{
			if (_networkSession == null)
			{
				return;
			}
			int num = 0;
			while (_networkSession != null && num < _networkSession.LocalGamers.Count)
			{
				LocalNetworkGamer localNetworkGamer = _networkSession.LocalGamers[num];
				while (_networkSession != null && localNetworkGamer.IsDataAvailable)
				{
					try
					{
						Message message = Message.GetMessage(localNetworkGamer);
						if (message is VoiceChatMessage)
						{
							if (_voiceChat != null)
							{
								_voiceChat.ProcessMessage((VoiceChatMessage)message);
							}
						}
						else if (message.Echo || !message.Sender.IsLocal)
						{
							OnMessage(message);
						}
					}
					catch (InvalidMessageException ex)
					{
						if (_networkSession.IsHost)
						{
							try
							{
								ex.Sender.Machine.RemoveFromSession();
							}
							catch
							{
							}
						}
						if (ex.Sender.IsHost)
						{
							LeaveGame();
						}
					}
				}
				num++;
			}
		}

		private void DrawBrightness()
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			SpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive);
			SpriteBatch.Draw(DummyTexture, viewport.Bounds, new Color(Brightness, Brightness, Brightness));
			SpriteBatch.End();
		}

		private void DrawTitleSafeArea(GameTime gameTime)
		{
			Viewport viewport = base.GraphicsDevice.Viewport;
			Rectangle titleSafeArea = viewport.TitleSafeArea;
			int num = viewport.X + viewport.Width;
			int num2 = viewport.Y + viewport.Height;
			Rectangle destinationRectangle = new Rectangle(viewport.X, viewport.Y, titleSafeArea.X - viewport.X, viewport.Height);
			Rectangle destinationRectangle2 = new Rectangle(titleSafeArea.Right, viewport.Y, num - titleSafeArea.Right, viewport.Height);
			Rectangle destinationRectangle3 = new Rectangle(titleSafeArea.Left, viewport.Y, titleSafeArea.Width, titleSafeArea.Top - viewport.Y);
			Rectangle destinationRectangle4 = new Rectangle(titleSafeArea.Left, titleSafeArea.Bottom, titleSafeArea.Width, num2 - titleSafeArea.Bottom);
			Color color = new Color(1f, 0f, 0f, 0.5f);
			SpriteBatch.Begin();
			SpriteBatch.Draw(DummyTexture, destinationRectangle, color);
			SpriteBatch.Draw(DummyTexture, destinationRectangle2, color);
			SpriteBatch.Draw(DummyTexture, destinationRectangle3, color);
			SpriteBatch.Draw(DummyTexture, destinationRectangle4, color);
			frsb.Length = 0;
			int value = (int)(1.0 / gameTime.ElapsedGameTime.TotalSeconds);
			frsb.Append(value);
			SpriteBatch.DrawOutlinedText(DebugFont, frsb, new Vector2(10f, 10f), Color.White, Color.Black, 1);
			SpriteBatch.End();
		}

		protected override bool ShowMissingRequirementMessage(Exception exception)
		{
			throw exception;
		}

		public static void Run<T>(string errorUrl, string name) where T : DNAGame, new()
		{
			Version version = new Version(0, 0);
			DateTime utcNow = DateTime.UtcNow;
			if (Debugger.IsAttached)
			{
				T val = new T();
				try
				{
					version = val.Version;
					val.Run();
					return;
				}
				finally
				{
					if (val != null)
					{
						((IDisposable)val/*cast due to .constrained prefix*/).Dispose();
					}
				}
			}
			try
			{
				T val2 = new T();
				try
				{
					version = val2.Version;
					val2.Run();
				}
				finally
				{
					if (val2 != null)
					{
						((IDisposable)val2/*cast due to .constrained prefix*/).Dispose();
					}
				}
			}
			catch (Exception e)
			{
				BlackScreenIssueReporter blackScreenIssueReporter = new BlackScreenIssueReporter(errorUrl, name, version, utcNow);
				blackScreenIssueReporter.ReportCrash(e);
			}
		}

		public static void Run<T>(IssueReporter issueReporter, OnlineServices onlineServices) where T : DNAGame, new()
		{
			new Version(0, 0);
			if (Debugger.IsAttached)
			{
				T val = new T();
				try
				{
					val.LicenseServices = onlineServices;
					Version version = val.Version;
					val.Run();
					return;
				}
				finally
				{
					if (val != null)
					{
						((IDisposable)val/*cast due to .constrained prefix*/).Dispose();
					}
				}
			}
			try
			{
				T val2 = new T();
				try
				{
					val2.LicenseServices = onlineServices;
					Version version2 = val2.Version;
					val2.Run();
				}
				finally
				{
					if (val2 != null)
					{
						((IDisposable)val2/*cast due to .constrained prefix*/).Dispose();
					}
				}
			}
			catch (Exception e)
			{
				issueReporter.ReportCrash(e);
			}
		}

		protected virtual void AfterLoad()
		{
		}

		public Vector2 ScreenToBuffer(Vector2 screenPoint)
		{
			if (_offscreenBuffer == null)
			{
				return screenPoint;
			}
			return new Vector2((screenPoint.X - (float)_bufferDestRect.Left) * (float)_offscreenBuffer.Width / (float)_bufferDestRect.Width, (screenPoint.Y - (float)_bufferDestRect.Top) * (float)_offscreenBuffer.Height / (float)_bufferDestRect.Height);
		}

		public Vector2 BufferToScreen(Vector2 bufferPoint)
		{
			if (_offscreenBuffer == null)
			{
				return bufferPoint;
			}
			return new Vector2(bufferPoint.X * (float)_bufferDestRect.Width / (float)_offscreenBuffer.Width + (float)_bufferDestRect.X, bufferPoint.Y * (float)_bufferDestRect.Height / (float)_offscreenBuffer.Height + (float)_bufferDestRect.Top);
		}

		public Point ScreenToBuffer(Point screenPoint)
		{
			if (_offscreenBuffer == null)
			{
				return screenPoint;
			}
			if (_bufferDestRect.Width == 0 || _bufferDestRect.Height == 0)
			{
				return Point.Zero;
			}
			return new Point((screenPoint.X - _bufferDestRect.Left) * _offscreenBuffer.Width / _bufferDestRect.Width, (screenPoint.Y - _bufferDestRect.Top) * _offscreenBuffer.Height / _bufferDestRect.Height);
		}

		public Point BufferToScreen(Point bufferPoint)
		{
			if (_offscreenBuffer == null)
			{
				return bufferPoint;
			}
			return new Point(bufferPoint.X * _bufferDestRect.Width / _offscreenBuffer.Width + _bufferDestRect.X, bufferPoint.Y * _bufferDestRect.Height / _offscreenBuffer.Height + _bufferDestRect.Top);
		}

		protected override void Draw(GameTime gameTime)
		{
			base.GraphicsDevice.SetRenderTarget(_offscreenBuffer);
			if (Stop)
			{
				base.GraphicsDevice.Clear(Color.Black);
				return;
			}
			if (Loading)
			{
				base.GraphicsDevice.Clear(Color.Black);
			}
			ScreenManager.Draw(base.GraphicsDevice, SpriteBatch, gameTime);
			DrawBrightness();
			if (CurrentBroadcastStream != null)
			{
				if (_offscreenBuffer == null)
				{
					throw new Exception("You must create an offscreen buffer, to use a Video Stream");
				}
				CurrentBroadcastStream.SubmitFrame(_offscreenBuffer);
			}
			base.GraphicsDevice.SetRenderTarget(null);
			if (_offscreenBuffer != null)
			{
				base.GraphicsDevice.Clear(Color.Black);
				SpriteBatch.Begin();
				float aspectRatio = base.GraphicsDevice.Viewport.AspectRatio;
				float num = (float)_offscreenBuffer.Width / (float)_offscreenBuffer.Height;
				if (aspectRatio > num)
				{
					int height = base.GraphicsDevice.Viewport.Height;
					int num2 = base.GraphicsDevice.Viewport.Height * _offscreenBuffer.Width / _offscreenBuffer.Height;
					_bufferDestRect = new Rectangle((base.GraphicsDevice.Viewport.Width - num2) / 2, 0, num2, height);
				}
				else
				{
					int num3 = base.GraphicsDevice.Viewport.Width * _offscreenBuffer.Height / _offscreenBuffer.Width;
					int width = base.GraphicsDevice.Viewport.Width;
					_bufferDestRect = new Rectangle(0, (base.GraphicsDevice.Viewport.Height - num3) / 2, width, num3);
				}
				SpriteBatch.Draw(_offscreenBuffer, _bufferDestRect, Color.White);
				SpriteBatch.End();
			}
			base.Draw(gameTime);
		}
	}
}
