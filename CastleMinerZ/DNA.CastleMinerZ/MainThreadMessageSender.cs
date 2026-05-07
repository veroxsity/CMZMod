using System;
using System.Diagnostics;
using System.Threading;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Utils;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ
{
	public class MainThreadMessageSender
	{
		private enum MessageType
		{
			SENDCHUNK,
			BROADCASTREADY,
			GETCHUNK,
			SENDDELTALIST
		}

		private class MessageCommand : IReleaseable, ILinkedListNode
		{
			private static int retryFrame = 0;

			public MessageType _type;

			public IntVector3 _position;

			public int[] _delta;

			public int _retryCount;

			public int _retryFrame;

			public int _priority;

			public byte[] _recipients = new byte[16];

			public byte[] _priorities = new byte[16];

			public int _numRecipients;

			public bool _sendToAll;

			private static ObjectCache<MessageCommand> _cache = new ObjectCache<MessageCommand>();

			private ILinkedListNode _nextNode;

			public ILinkedListNode NextNode
			{
				get
				{
					return _nextNode;
				}
				set
				{
					_nextNode = value;
				}
			}

			private bool Contains(byte recipient)
			{
				if (_sendToAll)
				{
					return true;
				}
				for (int i = 0; i < _numRecipients; i++)
				{
					if (_recipients[i] == recipient)
					{
						return true;
					}
				}
				return false;
			}

			public bool CopyReceiversToMe(byte[] recipients, int numRecipients, byte priority)
			{
				if (_sendToAll)
				{
					return false;
				}
				bool result = false;
				for (int i = 0; i < numRecipients; i++)
				{
					bool flag = false;
					for (int j = 0; j < _numRecipients; j++)
					{
						if (_recipients[j] == recipients[i])
						{
							flag = true;
							if (_priority > _priorities[j])
							{
								_priorities[j] = priority;
							}
							break;
						}
					}
					if (!flag)
					{
						result = true;
						_recipients[_numRecipients] = recipients[i];
						_priorities[_numRecipients++] = priority;
					}
				}
				return result;
			}

			public bool Execute()
			{
				bool result = false;
				try
				{
					if (CastleMinerZGame.Instance != null)
					{
						switch (_type)
						{
						case MessageType.SENDCHUNK:
						{
							if (_sendToAll)
							{
								DNA.CastleMinerZ.Net.ProvideChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, null, _position, _delta, _priority);
								break;
							}
							for (int j = 0; j < _numRecipients; j++)
							{
								NetworkGamer gamerFromID2 = CastleMinerZGame.Instance.GetGamerFromID(_recipients[j]);
								if (gamerFromID2 != null && !gamerFromID2.HasLeftSession)
								{
									DNA.CastleMinerZ.Net.ProvideChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, gamerFromID2, _position, _delta, _priorities[j]);
								}
							}
							break;
						}
						case MessageType.BROADCASTREADY:
							ClientReadyForChunksMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer);
							break;
						case MessageType.GETCHUNK:
							DNA.CastleMinerZ.Net.RequestChunkMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, _position, _priority);
							break;
						case MessageType.SENDDELTALIST:
						{
							if (_sendToAll)
							{
								ProvideDeltaListMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, null, _delta);
								break;
							}
							for (int i = 0; i < _numRecipients; i++)
							{
								NetworkGamer gamerFromID = CastleMinerZGame.Instance.GetGamerFromID(_recipients[i]);
								if (gamerFromID != null && !gamerFromID.HasLeftSession)
								{
									ProvideDeltaListMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, gamerFromID, _delta);
								}
							}
							break;
						}
						}
					}
					Release();
					result = true;
				}
				catch (Exception)
				{
				}
				return result;
			}

			public static void GameOver()
			{
				Interlocked.Increment(ref retryFrame);
			}

			public bool CanRetry()
			{
				if (_retryCount < 10)
				{
					return !WrongFrame();
				}
				return false;
			}

			public bool WrongFrame()
			{
				return _retryFrame != retryFrame;
			}

			public static MessageCommand Alloc()
			{
				MessageCommand messageCommand = _cache.Get();
				messageCommand._retryCount = 0;
				messageCommand._retryFrame = retryFrame;
				messageCommand._priority = 0;
				messageCommand._numRecipients = 0;
				messageCommand._sendToAll = false;
				return messageCommand;
			}

			public void Release()
			{
				_delta = null;
				_nextNode = null;
				_cache.Put(this);
			}
		}

		public static MainThreadMessageSender Instance;

		private SynchronizedQueue<MessageCommand> _queue = new SynchronizedQueue<MessageCommand>();

		private SimpleQueue<MessageCommand> _commandsToSend = new SimpleQueue<MessageCommand>();

		public static void Init()
		{
			if (Instance == null)
			{
				Instance = new MainThreadMessageSender();
			}
		}

		public void DrainQueue()
		{
			int num = 32768;
			int num2 = 0;
			lock (_queue)
			{
				while (!_queue.Empty && num2 < num)
				{
					MessageCommand messageCommand = _queue.Dequeue();
					if (messageCommand._type == MessageType.SENDCHUNK && messageCommand._delta != null)
					{
						num2 += messageCommand._delta.Length * 4;
					}
					_commandsToSend.Queue(messageCommand);
				}
			}
			while (!_commandsToSend.Empty)
			{
				MessageCommand messageCommand2 = _commandsToSend.Dequeue();
				if (messageCommand2.Execute())
				{
					continue;
				}
				if (messageCommand2.CanRetry())
				{
					messageCommand2._retryCount++;
					_queue.Queue(messageCommand2);
					continue;
				}
				if (Debugger.IsAttached)
				{
					messageCommand2.WrongFrame();
				}
				messageCommand2.Release();
			}
		}

		public void GameOver()
		{
			MessageCommand.GameOver();
		}

		public void RequestChunkMessage(IntVector3 pos, int priority)
		{
			lock (_queue)
			{
				for (MessageCommand messageCommand = _queue.Front; messageCommand != null; messageCommand = (MessageCommand)messageCommand.NextNode)
				{
					if (messageCommand._type == MessageType.GETCHUNK && pos.Equals(messageCommand._position))
					{
						if (messageCommand._priority < priority)
						{
							messageCommand._priority = priority;
							_queue.Remove(messageCommand);
							InsertInQueue(messageCommand);
						}
						return;
					}
				}
			}
			MessageCommand messageCommand2 = MessageCommand.Alloc();
			messageCommand2._type = MessageType.GETCHUNK;
			messageCommand2._position = pos;
			messageCommand2._priority = priority;
			InsertInQueue(messageCommand2);
		}

		public void ClientReadyForChunks()
		{
			MessageCommand messageCommand = MessageCommand.Alloc();
			messageCommand._type = MessageType.BROADCASTREADY;
			messageCommand._priority = 1;
			InsertInQueue(messageCommand);
		}

		private void InsertInQueue(MessageCommand c)
		{
			lock (_queue)
			{
				if (c._priority == 0 || _queue.Empty || _queue.Back._priority == 1)
				{
					_queue.Queue(c);
					return;
				}
				MessageCommand messageCommand = null;
				MessageCommand messageCommand2 = _queue.Front;
				do
				{
					if (messageCommand2._priority == 0)
					{
						if (messageCommand != null)
						{
							messageCommand.NextNode = c;
							c.NextNode = messageCommand2;
							_queue.IncrementCountAfterInsertion();
						}
						else
						{
							_queue.Undequeue(c);
						}
						return;
					}
					messageCommand = messageCommand2;
					messageCommand2 = (MessageCommand)messageCommand2.NextNode;
				}
				while (messageCommand2 != null);
				_queue.Queue(c);
			}
		}

		public void ProvideChunkMessage(IntVector3 pos, int[] delta, int priority, int numReceivers, byte[] receiverid)
		{
			lock (_queue)
			{
				for (MessageCommand messageCommand = _queue.Front; messageCommand != null; messageCommand = (MessageCommand)messageCommand.NextNode)
				{
					if (messageCommand._type == MessageType.SENDCHUNK && pos.Equals(messageCommand._position))
					{
						messageCommand._delta = delta;
						if (messageCommand._priority < priority)
						{
							messageCommand._priority = priority;
							_queue.Remove(messageCommand);
							InsertInQueue(messageCommand);
						}
						if (!messageCommand._sendToAll)
						{
							messageCommand.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
						}
						return;
					}
				}
			}
			MessageCommand messageCommand2 = MessageCommand.Alloc();
			messageCommand2._type = MessageType.SENDCHUNK;
			messageCommand2._position = pos;
			messageCommand2._delta = delta;
			messageCommand2._priority = priority;
			messageCommand2._numRecipients = 0;
			messageCommand2.CopyReceiversToMe(receiverid, numReceivers, (byte)priority);
			InsertInQueue(messageCommand2);
		}

		public void SendDeltaListMessage(int[] deltaList, byte receiverid, bool toall)
		{
			lock (_queue)
			{
				for (MessageCommand messageCommand = _queue.Front; messageCommand != null; messageCommand = (MessageCommand)messageCommand.NextNode)
				{
					if (messageCommand._type == MessageType.SENDDELTALIST)
					{
						messageCommand._delta = deltaList;
						if (messageCommand._sendToAll)
						{
							return;
						}
						if (toall)
						{
							messageCommand._sendToAll = true;
							messageCommand._numRecipients = 0;
							return;
						}
						for (int i = 0; i < messageCommand._numRecipients; i++)
						{
							if (messageCommand._recipients[i] == receiverid)
							{
								return;
							}
						}
						messageCommand._recipients[messageCommand._numRecipients++] = receiverid;
						return;
					}
				}
			}
			MessageCommand messageCommand2 = MessageCommand.Alloc();
			messageCommand2._type = MessageType.SENDDELTALIST;
			messageCommand2._delta = deltaList;
			messageCommand2._priority = 1;
			if (toall)
			{
				messageCommand2._sendToAll = true;
			}
			else
			{
				messageCommand2._numRecipients = 1;
				messageCommand2._recipients[0] = receiverid;
			}
			InsertInQueue(messageCommand2);
		}
	}
}
