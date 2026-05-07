using DNA.CastleMinerZ.Utils;

namespace DNA.CastleMinerZ.Terrain
{
	public class ChunkCacheCommand : IReleaseable, ILinkedListNode
	{
		public ChunkCacheCommandEnum _command;

		public IntVector3 _worldPosition = IntVector3.Zero;

		public BlockTypeEnum _blockType;

		public ChunkCacheCommandDelegate _callback;

		public object _context;

		public byte[] _data1;

		public byte[] _data2;

		public string _trackingString;

		public long _submittedTime;

		public int _priority = 1;

		public byte _requesterID;

		public bool _consolidate;

		public byte[] _requesterIDs = new byte[16];

		public int _numRequesters;

		public volatile ChunkCacheCommandStatus _status;

		private static int _nextObjID = 0;

		public int _objID;

		public int[] _delta;

		private static ObjectCache<ChunkCacheCommand> _cache = new ObjectCache<ChunkCacheCommand>();

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

		public ChunkCacheCommand()
		{
			_objID = _nextObjID++;
		}

		public static ChunkCacheCommand Alloc()
		{
			ChunkCacheCommand chunkCacheCommand = _cache.Get();
			chunkCacheCommand._status = ChunkCacheCommandStatus.NEW;
			chunkCacheCommand._trackingString = null;
			return chunkCacheCommand;
		}

		public bool CopyRequestersToMe(ChunkCacheCommand src)
		{
			bool result = false;
			for (int i = 0; i < src._numRequesters; i++)
			{
				bool flag = false;
				for (int j = 0; j < _numRequesters; j++)
				{
					if (_requesterIDs[j] == src._requesterIDs[i])
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					result = true;
					_requesterIDs[_numRequesters++] = src._requesterIDs[i];
				}
			}
			return result;
		}

		public void Release()
		{
			_delta = null;
			_callback = null;
			_context = null;
			_data1 = null;
			_data2 = null;
			_priority = 1;
			_consolidate = false;
			_status = ChunkCacheCommandStatus.DONE;
			_numRequesters = 0;
			_cache.Put(this);
		}
	}
}
