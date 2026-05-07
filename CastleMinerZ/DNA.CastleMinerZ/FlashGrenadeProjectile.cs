using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ
{
	internal class FlashGrenadeProjectile : GrenadeProjectile
	{
		public static void InternalHandleDetonateGrenadeMessage(DetonateGrenadeMessage msg)
		{
		}

		protected override void Explode()
		{
			base.Explode();
		}
	}
}
