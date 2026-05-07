using System;
using DNA.CastleMinerZ.UI;

namespace DNA.CastleMinerZ.Inventory
{
	public class GrenadeItem : InventoryItem
	{
		private Player _localPlayer
		{
			get
			{
				return CastleMinerZGame.Instance.LocalPlayer;
			}
		}

		public GrenadeItem(InventoryItemClass cls, int stackCount)
			: base(cls, stackCount)
		{
		}

		public override void ProcessInput(InGameHUD hud, CastleMinerZControllerMapping controller)
		{
			hud.LocalPlayer.UsingTool = false;
			if (controller.Use.Pressed && !_localPlayer.GrenadeAnimPlaying)
			{
				_localPlayer.ReadyToThrowGrenade = false;
				_localPlayer.grenadeCookTime = TimeSpan.Zero;
				_localPlayer.PlayGrenadeAnim = true;
			}
			else if (controller.Use.Released && _localPlayer.PlayGrenadeAnim && !_localPlayer.ReadyToThrowGrenade)
			{
				_localPlayer.ReadyToThrowGrenade = true;
			}
		}
	}
}
