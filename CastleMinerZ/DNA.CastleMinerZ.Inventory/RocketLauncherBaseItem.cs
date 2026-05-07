using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework.Net;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherBaseItem : GunInventoryItem
	{
		protected bool _deleteMe;

		public RocketLauncherBaseItem(RocketLauncherBaseInventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override bool InflictDamage()
		{
			_deleteMe = true;
			ChangeCarriedItemMessage.Send((LocalNetworkGamer)CastleMinerZGame.Instance.LocalPlayer.Gamer, InventoryItemIDs.RocketLauncherShotFired);
			return false;
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			if (_deleteMe && !hud.LocalPlayer.UsingAnimationPlaying)
			{
				hud.PlayerInventory.Remove(this);
			}
			else
			{
				base.ProcessInput(hud, controller);
			}
		}
	}
}
