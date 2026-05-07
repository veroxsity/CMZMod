using DNA.CastleMinerZ.Net;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherGuidedItem : RocketLauncherBaseItem
	{
		public RocketLauncherGuidedItem(RocketLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			_deleteMe = true;
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherGuidedShotFired);
			return false;
		}
	}
}
