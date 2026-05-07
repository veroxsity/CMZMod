using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ
{
	public class HEGrenadeProjectile : GrenadeProjectile
	{
		public static void InternalHandleDetonateGrenadeMessage(DetonateGrenadeMessage msg)
		{
			Explosive.DetonateGrenade(msg.Location, ExplosiveTypes.HEGrenade, msg.Sender.Id, msg.OnGround);
		}

		protected override void Explode()
		{
			if (_isLocal)
			{
				DetonateGrenadeMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, base.WorldPosition, GrenadeTypeEnum.HE, _stopped);
				Explosive.FindBlocksToRemove(IntVector3.FromVector3(base.WorldPosition), ExplosiveTypes.HEGrenade, false);
			}
			base.Explode();
		}
	}
}
