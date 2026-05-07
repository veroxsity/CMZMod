namespace DNA.CastleMinerZ.Achievements
{
	public class KillEnemyGrenadeAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithGrenade > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithGrenade > 0)
				{
					return 1f;
				}
				return 0f;
			}
		}

		public override string ProgressTowardsUnlockMessage
		{
			get
			{
				int enemiesKilledWithGrenade = base.PlayerStats.EnemiesKilledWithGrenade;
				if (_lastAmount != enemiesKilledWithGrenade)
				{
					_lastAmount = enemiesKilledWithGrenade;
					lastString = enemiesKilledWithGrenade + ((enemiesKilledWithGrenade == 1) ? " Enemy Killed" : " Enemies Killed");
				}
				return lastString;
			}
		}

		public KillEnemyGrenadeAchievement(CastleMinerZAchievementManager manager, string name)
			: base((AchievementManager<CastleMinerZPlayerStats>)manager, name, "Kill An Enemy With A Grenade")
		{
		}
	}
}
