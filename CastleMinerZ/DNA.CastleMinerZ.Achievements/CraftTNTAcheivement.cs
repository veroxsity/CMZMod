using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.Achievements
{
	public class CraftTNTAcheivement : AchievementManager<CastleMinerZPlayerStats>.Achievement
	{
		private string lastString;

		private int _lastAmount = -1;

		protected override bool IsSastified
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				return itemStats.Crafted > 0;
			}
		}

		public override float ProgressTowardsUnlock
		{
			get
			{
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				if (itemStats.Crafted > 0)
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
				CastleMinerZPlayerStats.ItemStats itemStats = CastleMinerZGame.Instance.PlayerStats.GetItemStats(InventoryItemIDs.TNT);
				int crafted = itemStats.Crafted;
				if (_lastAmount != crafted)
				{
					_lastAmount = crafted;
					lastString = crafted + " TNT Crafted";
				}
				return lastString;
			}
		}

		public CraftTNTAcheivement(CastleMinerZAchievementManager manager, string name)
			: base((AchievementManager<CastleMinerZPlayerStats>)manager, name, "Craft TNT")
		{
		}
	}
}
