namespace DNA.CastleMinerZ.Achievements
{
	public class KillDragonGuidedMissileAchievement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				return base.PlayerStats.DragonsKilledWithGuidedMissile > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				if (base.PlayerStats.DragonsKilledWithGuidedMissile > 0)
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
				int dragonsKilledWithGuidedMissile = base.PlayerStats.DragonsKilledWithGuidedMissile;
				if (_lastAmount != dragonsKilledWithGuidedMissile)
				{
					_lastAmount = dragonsKilledWithGuidedMissile;
					lastString = dragonsKilledWithGuidedMissile + ((dragonsKilledWithGuidedMissile == 1) ? " Dragon Killed" : " Dragons Killed");
				}
				return lastString;
			}
		}

		public KillDragonGuidedMissileAchievement(CastleMinerZAchievementManager manager, string name)
			: base((AchievementManager<CastleMinerZPlayerStats>)manager, name, "Kill A Dragon With A Guided Missile")
		{
		}
	}
}
