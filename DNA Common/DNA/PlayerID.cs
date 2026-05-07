using System;
using System.IO;

namespace DNA
{
	public class PlayerID : IComparable<PlayerID>, IEquatable<PlayerID>
	{
		private byte[] _playerHash;

		public static readonly PlayerID Null = new PlayerID(null);

		public byte[] Data
		{
			get
			{
				return _playerHash;
			}
		}

		public PlayerID(byte[] hash)
		{
			_playerHash = hash;
		}

		public void Read(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			_playerHash = new byte[num];
			for (int i = 0; i < num; i++)
			{
				_playerHash[i] = reader.ReadByte();
			}
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(_playerHash.Length);
			for (int i = 0; i < _playerHash.Length; i++)
			{
				writer.Write(_playerHash[i]);
			}
		}

		public override int GetHashCode()
		{
			int num = 0;
			for (int i = 0; i < _playerHash.Length; i++)
			{
				num ^= _playerHash[i];
			}
			return num;
		}

		public static bool operator ==(PlayerID a, PlayerID b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(PlayerID a, PlayerID b)
		{
			return !a.Equals(b);
		}

		public override bool Equals(object obj)
		{
			return CompareTo((PlayerID)obj) == 0;
		}

		public override string ToString()
		{
			return _playerHash.ToString();
		}

		public int CompareTo(PlayerID other)
		{
			if (GetType() != other.GetType())
			{
				return -1;
			}
			if (_playerHash.Length != other._playerHash.Length)
			{
				return -1;
			}
			for (int i = 0; i < _playerHash.Length; i++)
			{
				int num = _playerHash[i] - other._playerHash[i];
				if (num != 0)
				{
					return num;
				}
			}
			return 0;
		}

		public bool Equals(PlayerID other)
		{
			return CompareTo(other) == 0;
		}
	}
}
