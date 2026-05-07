using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using DNA.CastleMinerZ.Utils;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Terrain
{
	public class ChunkCache
	{
		private const int MAX_CHUNK_MEMORY = 524288;

		private const float ASK_HOST_FOR_CHUNKS_INTERVAL = 2f;

		private const int MAX_CHUNKS_TO_ASK_HOST_FOR = 2;

		private const float HEARTBEAT_INTERVAL = 0.5f;

		private const byte CHUNKLIST_VERSION = 0;

		private const int MAX_PR0_WAITING = 5;

		private const int MAX_PR1_WAITING = 2;

		public static ChunkCache Instance;

		private SynchronizedQueue<ChunkCacheCommand> _commandQueue = new SynchronizedQueue<ChunkCacheCommand>();

		private SimpleQueue<CachedChunk> _cachedChunks = new SimpleQueue<CachedChunk>();

		private SimpleQueue<CachedChunk> _waitingChunks = new SimpleQueue<CachedChunk>();

		public int[] _localChunks;

		public int _numLocalChunks;

		public int[] _remoteChunks;

		public int _numRemoteChunks;

		public int[] _copyOfLocalChunks;

		private AutoResetEvent _commandsWaiting = new AutoResetEvent(false);

		private Thread _thread;

		private Stopwatch _queueTimer = Stopwatch.StartNew();

		private float _timeTilHeartbeat = 0.5f;

		private float _timeTilPushChunks = 2f;

		private volatile bool _quit;

		private volatile bool _running;

		private ChunkCacheCommandDelegate _internalChunkLoadedDelegate;

		private bool _weAreHosting;

		public WorldInfo _worldInfo;

		public bool AlreadyForcedRestart;

		public float CurrentQueueDelay;

		private int _numPri1Waiting;

		private int _numPri0Waiting;

		public bool IsStorageEnabled
		{
			get
			{
				if (_worldInfo != null)
				{
					return _worldInfo.SavePath != null;
				}
				return false;
			}
		}

		public string RootPath
		{
			get
			{
				return _worldInfo.SavePath;
			}
		}

		public bool Running
		{
			get
			{
				if (_running)
				{
					return !_quit;
				}
				return false;
			}
		}

		public int NumPendingRequests
		{
			get
			{
				return _commandQueue.Count;
			}
		}

		static ChunkCache()
		{
			Instance = new ChunkCache();
		}

		private void TestCallback(ChunkCacheCommand cmd)
		{
			cmd.Release();
		}

		private ChunkCache()
		{
			_internalChunkLoadedDelegate = InternalRetrieveChunkCallback;
		}

		private void DrainCommandQueue()
		{
			while (!_quit && !_commandQueue.Empty)
			{
				ChunkCacheCommand command = _commandQueue.Dequeue();
				try
				{
					RunCommand(command);
				}
				catch (Exception e)
				{
					CastleMinerZGame.Instance.CrashGame(e);
				}
			}
		}

		private void StartThread()
		{
			_commandsWaiting.Reset();
			_running = true;
			while (!_quit)
			{
				DrainCommandQueue();
				if (!_quit)
				{
					_commandsWaiting.WaitOne();
				}
			}
			lock (this)
			{
				_running = false;
				while (!_commandQueue.Empty)
				{
					_commandQueue.Dequeue().Release();
				}
			}
		}

		public void Flush(bool wait)
		{
			ChunkCacheCommand chunkCacheCommand = null;
			lock (this)
			{
				if (Running)
				{
					chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.FLUSH;
					AddCommand(chunkCacheCommand);
				}
			}
			if (chunkCacheCommand != null)
			{
				while (wait && chunkCacheCommand._status != ChunkCacheCommandStatus.DONE)
				{
				}
			}
		}

		public void Start(bool wait)
		{
			if (!_running)
			{
				_thread = new Thread((ThreadStart)delegate
				{
					Thread.CurrentThread.SetProcessorAffinity(5);
					StartThread();
				});
				_thread.Name = "ChunkCacheThread";
				_quit = false;
				_numPri0Waiting = 0;
				_numPri1Waiting = 0;
				_timeTilHeartbeat = 0f;
				_timeTilPushChunks = 3f;
				_thread.Start();
				while (!_running)
				{
				}
			}
		}

		public void Stop(bool wait)
		{
			lock (this)
			{
				if (Running)
				{
					ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.SHUTDOWN;
					AddCommand(chunkCacheCommand);
				}
			}
			while (wait && _running)
			{
			}
		}

		public void ResetWaitingChunks()
		{
			ChunkCacheCommand chunkCacheCommand = null;
			chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.RESETWAITINGCHUNKS;
			AddCommand(chunkCacheCommand);
		}

		protected bool ConsolidateNewCommand(ChunkCacheCommand command)
		{
			for (ChunkCacheCommand chunkCacheCommand = _commandQueue.Front; chunkCacheCommand != null; chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode)
			{
				if (chunkCacheCommand._command == command._command)
				{
					if (chunkCacheCommand._priority >= command._priority)
					{
						command.Release();
						return true;
					}
					_commandQueue.Remove(chunkCacheCommand);
					chunkCacheCommand.Release();
					return false;
				}
			}
			return false;
		}

		protected bool ConsolidateNewChunkRequest(ChunkCacheCommand command)
		{
			for (ChunkCacheCommand chunkCacheCommand = _commandQueue.Front; chunkCacheCommand != null; chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode)
			{
				if (chunkCacheCommand._command == command._command && chunkCacheCommand._worldPosition.Equals(command._worldPosition))
				{
					if (chunkCacheCommand._priority >= command._priority)
					{
						chunkCacheCommand.CopyRequestersToMe(command);
						command.Release();
						return true;
					}
					command.CopyRequestersToMe(chunkCacheCommand);
					_commandQueue.Remove(chunkCacheCommand);
					chunkCacheCommand.Release();
					return false;
				}
			}
			return false;
		}

		public void AddCommand(ChunkCacheCommand command)
		{
			lock (this)
			{
				if (Running)
				{
					command._submittedTime = _queueTimer.ElapsedMilliseconds;
					lock (_commandQueue)
					{
						if (command._command == ChunkCacheCommandEnum.FETCHDELTAFORCLIENT)
						{
							if (ConsolidateNewChunkRequest(command))
							{
								return;
							}
						}
						else if (command._consolidate && ConsolidateNewCommand(command))
						{
							return;
						}
						if (command._priority == 0 || _commandQueue.Empty)
						{
							_commandQueue.Queue(command);
						}
						else if (_commandQueue.Back._priority == 1)
						{
							_commandQueue.Queue(command);
						}
						else if (_commandQueue.Front._priority == 0)
						{
							_commandQueue.Undequeue(command);
						}
						else
						{
							ChunkCacheCommand chunkCacheCommand = _commandQueue.Front;
							ChunkCacheCommand chunkCacheCommand2 = null;
							bool flag = false;
							while (chunkCacheCommand != null)
							{
								if (chunkCacheCommand._priority == 0)
								{
									chunkCacheCommand2.NextNode = command;
									command.NextNode = chunkCacheCommand;
									_commandQueue.IncrementCountAfterInsertion();
									flag = true;
									break;
								}
								chunkCacheCommand2 = chunkCacheCommand;
								chunkCacheCommand = (ChunkCacheCommand)chunkCacheCommand.NextNode;
							}
							if (!flag)
							{
								_commandQueue.Queue(command);
							}
						}
					}
					command._status = ChunkCacheCommandStatus.QUEUED;
					_commandsWaiting.Set();
				}
				else
				{
					command.Release();
				}
			}
		}

		public CachedChunk FindChunk(IntVector3 v, SimpleQueue<CachedChunk> queue)
		{
			CachedChunk result = null;
			for (CachedChunk cachedChunk = queue.Front; cachedChunk != null; cachedChunk = (CachedChunk)cachedChunk.NextNode)
			{
				if (cachedChunk._worldMin.Equals(v))
				{
					result = cachedChunk;
					queue.Remove(cachedChunk);
					queue.Queue(cachedChunk);
					break;
				}
			}
			return result;
		}

		private void ReduceMemory()
		{
			if (!Instance.IsStorageEnabled)
			{
				return;
			}
			CachedChunk cachedChunk = _cachedChunks.Front;
			if (cachedChunk == null)
			{
				return;
			}
			int num = 0;
			while (cachedChunk != null)
			{
				num += cachedChunk._numEntries * 4 + 100;
				cachedChunk = (CachedChunk)cachedChunk.NextNode;
			}
			if (num <= 524288)
			{
				return;
			}
			CachedChunk cachedChunk2 = null;
			CachedChunk cachedChunk3 = null;
			cachedChunk = _cachedChunks.Front;
			while (cachedChunk != null && num > 524288)
			{
				CachedChunk cachedChunk4 = (CachedChunk)cachedChunk.NextNode;
				cachedChunk.NextNode = null;
				if (cachedChunk.SameAsDisk)
				{
					num -= cachedChunk._numEntries * 4 + 100;
					cachedChunk.Save();
					cachedChunk.Release();
					cachedChunk = cachedChunk4;
					continue;
				}
				if (cachedChunk2 == null)
				{
					cachedChunk2 = cachedChunk;
				}
				else
				{
					cachedChunk3.NextNode = cachedChunk;
				}
				cachedChunk3 = cachedChunk;
				cachedChunk = cachedChunk4;
			}
			if (cachedChunk != null)
			{
				if (cachedChunk2 == null)
				{
					cachedChunk2 = cachedChunk;
				}
				else
				{
					cachedChunk3.NextNode = cachedChunk;
				}
			}
			_cachedChunks.ReplaceFromList(cachedChunk2);
		}

		private void MoveFromAToB(CachedChunk v, SimpleQueue<CachedChunk> a, SimpleQueue<CachedChunk> b)
		{
			for (CachedChunk cachedChunk = a.Front; cachedChunk != null; cachedChunk = (CachedChunk)cachedChunk.NextNode)
			{
				if (cachedChunk == v)
				{
					a.Remove(cachedChunk);
					b.Queue(cachedChunk);
					break;
				}
			}
		}

		private CachedChunk GetCachedChunk(IntVector3 v)
		{
			return FindChunk(v, _cachedChunks);
		}

		private CachedChunk GetWaitingChunk(IntVector3 v)
		{
			return FindChunk(v, _waitingChunks);
		}

		private CachedChunk CreateChunk(IntVector3 v, SimpleQueue<CachedChunk> queue)
		{
			CachedChunk cachedChunk = CachedChunk.Alloc();
			cachedChunk.Init(v);
			queue.Queue(cachedChunk);
			AddChunkToLocalList(v);
			return cachedChunk;
		}

		private CachedChunk CreateCachedChunk(IntVector3 v)
		{
			return CreateChunk(v, _cachedChunks);
		}

		private CachedChunk CreateWaitingChunk(IntVector3 v)
		{
			return CreateChunk(v, _waitingChunks);
		}

		private void FlushCachedChunks()
		{
			CachedChunk cachedChunk = _cachedChunks.Front;
			while (cachedChunk != null)
			{
				CachedChunk cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				cachedChunk.Save();
				cachedChunk = cachedChunk2;
			}
		}

		private void ForceReadWaitingChunks()
		{
			while (!_waitingChunks.Empty)
			{
				CachedChunk cachedChunk = _waitingChunks.Dequeue();
				cachedChunk.RetroReadFromDisk();
				if (cachedChunk._numEntries == 0)
				{
					RemoveChunkFromLocalList(cachedChunk._worldMin);
					cachedChunk.Release();
				}
				else
				{
					_cachedChunks.Queue(cachedChunk);
				}
			}
		}

		private void InternalResetWaitingChunks()
		{
			for (CachedChunk cachedChunk = _waitingChunks.Front; cachedChunk != null; cachedChunk = cachedChunk.NextNode as CachedChunk)
			{
				cachedChunk.StripFetchCommands();
			}
		}

		private void Heartbeat()
		{
			CachedChunk cachedChunk = _waitingChunks.Front;
			_numPri0Waiting = 0;
			_numPri1Waiting = 0;
			while (cachedChunk != null)
			{
				CachedChunk cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				if (cachedChunk._loadingPriority == 0)
				{
					_numPri0Waiting++;
				}
				else
				{
					_numPri1Waiting++;
				}
				cachedChunk = cachedChunk2;
			}
		}

		private void ChangeChunkHosts()
		{
			CachedChunk cachedChunk = _waitingChunks.Front;
			while (cachedChunk != null)
			{
				CachedChunk cachedChunk2 = (CachedChunk)cachedChunk.NextNode;
				cachedChunk.HostChanged();
				cachedChunk = cachedChunk2;
			}
		}

		private string MakeFilename()
		{
			return Path.Combine(Instance.RootPath, "chunklist.lst");
		}

		public bool CIDInRemoteList(int cid)
		{
			if (_numRemoteChunks == 0)
			{
				return false;
			}
			for (int i = 0; i < _numRemoteChunks; i++)
			{
				if (_remoteChunks[i] == cid)
				{
					return true;
				}
			}
			return false;
		}

		public bool ChunkInRemoteList(IntVector3 chunkCorner)
		{
			if (_numRemoteChunks == 0)
			{
				return false;
			}
			return CIDInRemoteList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		public bool CIDInLocalList(int cid)
		{
			if (_numLocalChunks == 0)
			{
				return false;
			}
			for (int i = 0; i < _numLocalChunks; i++)
			{
				if (_localChunks[i] == cid)
				{
					return true;
				}
			}
			return false;
		}

		public bool ChunkInLocalList(IntVector3 chunkCorner)
		{
			if (_numLocalChunks == 0)
			{
				return false;
			}
			return CIDInLocalList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		private void AddChunkToLocalList(int cid)
		{
			if (_localChunks == null)
			{
				_localChunks = new int[100];
			}
			else if (_numLocalChunks == _localChunks.Length)
			{
				int[] array = new int[_numLocalChunks + 100];
				Buffer.BlockCopy(_localChunks, 0, array, 0, _numLocalChunks * 4);
				_localChunks = array;
			}
			_localChunks[_numLocalChunks++] = cid;
		}

		public void AddChunkToLocalList(IntVector3 chunkCorner)
		{
			int num = CachedChunk.MakeCIDFromChunkCorner(chunkCorner);
			if (_localChunks == null)
			{
				AddChunkToLocalList(num);
				return;
			}
			for (int i = 0; i < _numLocalChunks; i++)
			{
				if (_localChunks[i] == num)
				{
					return;
				}
			}
			AddChunkToLocalList(num);
		}

		private void RemoveChunkFromLocalList(int cid)
		{
			if (_localChunks == null)
			{
				return;
			}
			for (int i = 0; i < _numLocalChunks; i++)
			{
				if (_localChunks[i] == cid)
				{
					_numLocalChunks--;
					if (_numLocalChunks != i || _numLocalChunks != 0)
					{
						_localChunks[i] = _localChunks[_numLocalChunks];
					}
					break;
				}
			}
		}

		private void RemoveChunkFromLocalList(IntVector3 chunkCorner)
		{
			RemoveChunkFromLocalList(CachedChunk.MakeCIDFromChunkCorner(chunkCorner));
		}

		private void LoadChunkList()
		{
			if (!_weAreHosting || !Instance.IsStorageEnabled || _numLocalChunks != 0)
			{
				return;
			}
			SignedInGamer currentGamer = Screen.CurrentGamer;
			MakeFilename();
			try
			{
				string[] files = CastleMinerZGame.Instance.SaveDevice.GetFiles(Path.Combine(Instance.RootPath, "*.*"));
				IntVector3 zero = IntVector3.Zero;
				zero.Y = -64;
				for (int i = 0; i < files.Length; i++)
				{
					string fileName = Path.GetFileName(files[i]);
					if (fileName[0] == 'X' && fileName.EndsWith(".dat"))
					{
						int num = fileName.IndexOf('X');
						int num2 = fileName.IndexOf('Y');
						int num3 = fileName.IndexOf('Z');
						int num4 = fileName.IndexOf('.');
						if (num4 > num3 && num3 > num2 && num2 > num)
						{
							zero.X = int.Parse(fileName.Substring(num + 1, num2 - (num + 1)));
							zero.Z = int.Parse(fileName.Substring(num3 + 1, num4 - (num3 + 1)));
							AddChunkToLocalList(zero);
						}
					}
				}
			}
			catch (Exception)
			{
			}
		}

		private void RunCommand(ChunkCacheCommand command)
		{
			bool flag = true;
			long num = 0L;
			CurrentQueueDelay = (float)(_queueTimer.ElapsedMilliseconds - command._submittedTime) / 1000f;
			command._status = ChunkCacheCommandStatus.PROCESSING;
			if (command._trackingString != null)
			{
				num = _queueTimer.ElapsedMilliseconds;
			}
			switch (command._command)
			{
			case ChunkCacheCommandEnum.RESETWAITINGCHUNKS:
				InternalResetWaitingChunks();
				break;
			case ChunkCacheCommandEnum.FLUSH:
				FlushCachedChunks();
				break;
			case ChunkCacheCommandEnum.SHUTDOWN:
				InternalResetWaitingChunks();
				FlushCachedChunks();
				_quit = true;
				while (!_cachedChunks.Empty)
				{
					_cachedChunks.Dequeue().Release();
				}
				while (!_waitingChunks.Empty)
				{
					_waitingChunks.Dequeue().Release();
				}
				_numLocalChunks = 0;
				_copyOfLocalChunks = null;
				_worldInfo = null;
				AlreadyForcedRestart = false;
				_numRemoteChunks = 0;
				_remoteChunks = null;
				_numPri0Waiting = 0;
				_numPri1Waiting = 0;
				break;
			case ChunkCacheCommandEnum.BECOMECLIENT:
				_weAreHosting = false;
				if (command._context != null)
				{
					if (_worldInfo == null)
					{
						BroadcastThatWereReady();
					}
					_worldInfo = (WorldInfo)command._context;
				}
				break;
			case ChunkCacheCommandEnum.BECOMEHOST:
				_weAreHosting = true;
				if (command._context != null)
				{
					_worldInfo = (WorldInfo)command._context;
				}
				LoadChunkList();
				_remoteChunks = null;
				_numRemoteChunks = 0;
				ForceReadWaitingChunks();
				break;
			case ChunkCacheCommandEnum.HOSTCHANGED:
				if (!_weAreHosting)
				{
					ChangeChunkHosts();
				}
				break;
			case ChunkCacheCommandEnum.ASKHOSTFORREMOTECHUNKS:
				InternalAskHostForSomeRemoteChunks();
				break;
			case ChunkCacheCommandEnum.SENDREMOTECHUNKLIST:
				InternalSendRemoteChunkList(command._requesterID, false);
				break;
			case ChunkCacheCommandEnum.SENDREMOTECHUNKLISTTOALL:
				InternalSendRemoteChunkList(0, true);
				break;
			case ChunkCacheCommandEnum.REMOTECHUNKLISTARRIVED:
				if (_worldInfo != null && _remoteChunks == null)
				{
					if (command._delta == null)
					{
						_remoteChunks = new int[1];
						_numRemoteChunks = 0;
					}
					else
					{
						_remoteChunks = command._delta;
						_numRemoteChunks = command._delta.Length;
					}
				}
				break;
			case ChunkCacheCommandEnum.HEARTBEAT:
				Heartbeat();
				ReduceMemory();
				break;
			case ChunkCacheCommandEnum.DELTAARRIVED:
			{
				IntVector3 intVector2 = CachedChunk.MakeChunkCorner(command._worldPosition);
				CachedChunk waitingChunk = GetWaitingChunk(intVector2);
				if (waitingChunk != null)
				{
					waitingChunk.SetDelta(command._delta, false);
					waitingChunk.ExecuteCommands();
					if (waitingChunk._numEntries == 0)
					{
						_waitingChunks.Remove(waitingChunk);
						RemoveChunkFromLocalList(intVector2);
						waitingChunk.Release();
					}
					else
					{
						MoveFromAToB(waitingChunk, _waitingChunks, _cachedChunks);
					}
				}
				else
				{
					flag = false;
					command.Release();
					command = null;
				}
				break;
			}
			case ChunkCacheCommandEnum.MOD:
			case ChunkCacheCommandEnum.FETCHDELTAFORTERRAIN:
			case ChunkCacheCommandEnum.FETCHDELTAFORCLIENT:
			{
				CachedChunk cachedChunk = null;
				flag = false;
				IntVector3 intVector = CachedChunk.MakeChunkCorner(command._worldPosition);
				int cid = CachedChunk.MakeCIDFromChunkCorner(intVector);
				if (CIDInLocalList(cid))
				{
					cachedChunk = GetCachedChunk(intVector);
					if (cachedChunk != null)
					{
						cachedChunk.RunCommand(command);
						break;
					}
					cachedChunk = GetWaitingChunk(intVector);
					if (cachedChunk != null)
					{
						cachedChunk.QueueCommand(command);
						break;
					}
					cachedChunk = CreateCachedChunk(intVector);
					cachedChunk.GetChunkFromDisk();
					cachedChunk.RunCommand(command);
				}
				else if (CIDInRemoteList(cid) || (!_weAreHosting && _remoteChunks == null))
				{
					cachedChunk = CreateWaitingChunk(intVector);
					cachedChunk.GetChunkFromHost(command._priority);
					cachedChunk.QueueCommand(command);
				}
				else if (command._command != ChunkCacheCommandEnum.MOD)
				{
					command._delta = null;
					command._callback(command);
				}
				else
				{
					cachedChunk = CreateCachedChunk(intVector);
					cachedChunk.RunCommand(command);
				}
				break;
			}
			}
			if (command != null && command._trackingString != null)
			{
				float num2 = (float)(_queueTimer.ElapsedMilliseconds - num) / 1000f;
			}
			if (flag)
			{
				command.Release();
			}
		}

		public void Update(GameTime time)
		{
			if (Running)
			{
				_timeTilHeartbeat -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_timeTilHeartbeat <= 0f)
				{
					ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
					chunkCacheCommand._command = ChunkCacheCommandEnum.HEARTBEAT;
					chunkCacheCommand._consolidate = true;
					AddCommand(chunkCacheCommand);
					_timeTilHeartbeat = 0.5f;
				}
				_timeTilPushChunks -= (float)time.ElapsedGameTime.TotalSeconds;
				if (_timeTilPushChunks <= 0f)
				{
					AskHostForSomeRemoteChunks();
					_timeTilPushChunks = 2f;
				}
			}
		}

		private void BroadcastThatWereReady()
		{
			MainThreadMessageSender.Instance.ClientReadyForChunks();
			_remoteChunks = null;
			_numRemoteChunks = 0;
		}

		private void InternalRetrieveChunkCallback(ChunkCacheCommand cmd)
		{
			if (CastleMinerZGame.Instance.CurrentNetworkSession != null)
			{
				MainThreadMessageSender.Instance.ProvideChunkMessage(cmd._worldPosition, cmd._delta, cmd._priority, cmd._numRequesters, cmd._requesterIDs);
			}
			cmd.Release();
		}

		public void GetChunkFromServer(IntVector3 worldmin, int priority)
		{
			MainThreadMessageSender.Instance.RequestChunkMessage(worldmin, priority);
		}

		private void InternalAskHostForSomeRemoteChunks()
		{
			if (_numRemoteChunks == 0 || _numPri0Waiting > 5 || _numPri1Waiting > 2)
			{
				return;
			}
			int num = 0;
			while (num < 2 && _numRemoteChunks > 0)
			{
				_numRemoteChunks--;
				if (!CIDInLocalList(_remoteChunks[_numRemoteChunks]))
				{
					IntVector3 v = CachedChunk.MakeChunkCornerFromCID(_remoteChunks[_numRemoteChunks]);
					CachedChunk cachedChunk = CreateWaitingChunk(v);
					cachedChunk.GetChunkFromHost(0);
					num++;
				}
			}
		}

		private void InternalSendRemoteChunkList(byte requesterId, bool toall)
		{
			if (_numLocalChunks != 0)
			{
				if (_copyOfLocalChunks == null || _copyOfLocalChunks.Length != _numLocalChunks)
				{
					_copyOfLocalChunks = new int[_numLocalChunks];
					Buffer.BlockCopy(_localChunks, 0, _copyOfLocalChunks, 0, _numLocalChunks * 4);
				}
				MainThreadMessageSender.Instance.SendDeltaListMessage(_copyOfLocalChunks, requesterId, toall);
			}
		}

		public void AskHostForSomeRemoteChunks()
		{
			if (!_weAreHosting && _numRemoteChunks != 0)
			{
				ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
				chunkCacheCommand._command = ChunkCacheCommandEnum.ASKHOSTFORREMOTECHUNKS;
				chunkCacheCommand._priority = 0;
				chunkCacheCommand._consolidate = true;
				AddCommand(chunkCacheCommand);
			}
		}

		public void SendRemoteChunkList(byte requesterId, bool toall)
		{
			if (Running && CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.CurrentNetworkSession.SessionType != NetworkSessionType.Local)
			{
				ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
				chunkCacheCommand._command = (toall ? ChunkCacheCommandEnum.SENDREMOTECHUNKLISTTOALL : ChunkCacheCommandEnum.SENDREMOTECHUNKLIST);
				chunkCacheCommand._requesterID = requesterId;
				AddCommand(chunkCacheCommand);
			}
		}

		public void RemoteChunkListArrived(int[] deltaList)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.REMOTECHUNKLISTARRIVED;
			chunkCacheCommand._delta = deltaList;
			AddCommand(chunkCacheCommand);
		}

		public void AddModifiedBlock(IntVector3 worldIndex, BlockTypeEnum blockType)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.MOD;
			chunkCacheCommand._worldPosition = worldIndex;
			chunkCacheCommand._blockType = blockType;
			chunkCacheCommand._priority = 1;
			Instance.AddCommand(chunkCacheCommand);
		}

		public void RetrieveChunkForNetwork(byte requesterID, IntVector3 worldmin, int priority, object context)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._callback = _internalChunkLoadedDelegate;
			chunkCacheCommand._worldPosition = CachedChunk.MakeChunkCorner(worldmin);
			chunkCacheCommand._command = ChunkCacheCommandEnum.FETCHDELTAFORCLIENT;
			chunkCacheCommand._context = context;
			chunkCacheCommand._priority = priority;
			chunkCacheCommand._requesterIDs[0] = requesterID;
			chunkCacheCommand._numRequesters = 1;
			AddCommand(chunkCacheCommand);
		}

		public void ChunkDeltaArrived(IntVector3 worldmin, int[] delta, byte priority)
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.DELTAARRIVED;
			chunkCacheCommand._worldPosition = worldmin;
			chunkCacheCommand._delta = delta;
			chunkCacheCommand._priority = priority;
			AddCommand(chunkCacheCommand);
		}

		public void HostChanged()
		{
			ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
			chunkCacheCommand._command = ChunkCacheCommandEnum.HOSTCHANGED;
			AddCommand(chunkCacheCommand);
		}

		public void MakeHost(WorldInfo worldinfo, bool value)
		{
			if (Running)
			{
				ChunkCacheCommand chunkCacheCommand = ChunkCacheCommand.Alloc();
				chunkCacheCommand._command = (value ? ChunkCacheCommandEnum.BECOMEHOST : ChunkCacheCommandEnum.BECOMECLIENT);
				chunkCacheCommand._context = worldinfo;
				AddCommand(chunkCacheCommand);
			}
		}
	}
}
