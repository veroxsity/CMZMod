using System;

namespace DNA.CastleMinerZ.AI
{
	public class AlienGiveUp : ZombieGiveUp
	{
		public override void Enter(BaseZombie entity)
		{
			ZeroVelocity(entity);
			entity.CurrentPlayer = entity.PlayClip("Jump", false, TimeSpan.FromSeconds(0.25));
		}
	}
}
