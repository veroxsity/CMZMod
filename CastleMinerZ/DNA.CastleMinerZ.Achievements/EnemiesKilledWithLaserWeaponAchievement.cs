namespace DNA.CastleMinerZ.Achievements
{
	public class EnemiesKilledWithLaserWeaponAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.EnemiesKilledWithLaserWeapon > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.EnemiesKilledWithLaserWeapon > 0)
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
				int enemiesKilledWithLaserWeapon = base.PlayerStats.EnemiesKilledWithLaserWeapon;
				if (_lastAmount != enemiesKilledWithLaserWeapon)
				{
					_lastAmount = enemiesKilledWithLaserWeapon;
					lastString = enemiesKilledWithLaserWeapon + ((enemiesKilledWithLaserWeapon == 1) ? " Enemy Killed" : " Enemies Killed");
				}
				return lastString;
			}
		}

		public EnemiesKilledWithLaserWeaponAchievement(CastleMinerZAchievementManager manager, string name)
			: base((AchievementManager<CastleMinerZPlayerStats>)manager, name, "Kill An Enemy With A Laser Weapon")
		{
		}
	}
}
