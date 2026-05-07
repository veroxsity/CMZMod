using System;
using DNA.CastleMinerZ.Net;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieGiveUp : EnemyBaseState
	{
		public override void Enter(BaseZombie entity)
		{
			ZeroVelocity(entity);
			entity.CurrentPlayer = entity.PlayClip("eat_start", false, TimeSpan.FromSeconds(0.25));
			if (entity.Target != null && entity.Target.IsLocal && entity.Target.Gamer != null)
			{
				EnemyGiveUpMessage.Send(entity.EnemyID, entity.Target.Gamer.Id);
			}
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.CurrentPlayer.Finished)
			{
				entity.Remove();
			}
		}
	}
}
