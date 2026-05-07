using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ
{
	public class ConstructionProbeClass : TraceProbe
	{
		public bool HitZombie;

		public bool HitPlayer;

		public bool CheckEnemies;

		public BaseZombie EnemyHit;

		public Player PlayerHit;

		public override bool AbleToBuild
		{
			get
			{
				if (base.AbleToBuild && !HitZombie)
				{
					return !HitPlayer;
				}
				return false;
			}
		}

		public void Init(Vector3 start, Vector3 end, bool checkEnemies)
		{
			base.Init(start, end);
			HitZombie = false;
			HitPlayer = false;
			PlayerHit = null;
			EnemyHit = null;
			CheckEnemies = checkEnemies;
		}

		public void Trace()
		{
			HitZombie = false;
			EnemyHit = null;
			HitPlayer = false;
			PlayerHit = null;
			if (CheckEnemies)
			{
				if (CastleMinerZGame.Instance.CurrentNetworkSession != null && CastleMinerZGame.Instance.PVPState != CastleMinerZGame.PVPEnum.Off)
				{
					for (int i = 0; i < CastleMinerZGame.Instance.CurrentNetworkSession.RemoteGamers.Count; i++)
					{
						NetworkGamer networkGamer = CastleMinerZGame.Instance.CurrentNetworkSession.RemoteGamers[i];
						if (networkGamer.Tag == null)
						{
							continue;
						}
						Player player = (Player)networkGamer.Tag;
						if (player.ValidLivingGamer)
						{
							Vector3 worldPosition = player.WorldPosition;
							BoundingBox playerAABB = player.PlayerAABB;
							playerAABB.Min += worldPosition;
							playerAABB.Max += worldPosition;
							TestBoundBox(playerAABB);
							if (_collides && _inT < 0.5f)
							{
								PlayerHit = player;
								HitPlayer = true;
								break;
							}
						}
					}
				}
				IShootableEnemy shootableEnemy = EnemyManager.Instance.Trace(this, true);
				if (shootableEnemy is BaseZombie)
				{
					EnemyHit = (BaseZombie)shootableEnemy;
					HitZombie = true;
				}
			}
			else
			{
				BlockTerrain.Instance.Trace(this);
			}
		}
	}
}
