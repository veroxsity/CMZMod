using System;

namespace DNA.CastleMinerZ.AI
{
	public class AlienChase : ZombieChase
	{
		protected override void StartMoveAnimation(BaseZombie entity)
		{
			entity.CurrentPlayer = entity.PlayClip("MoveLoop", true, TimeSpan.FromSeconds(0.25));
			entity.CurrentPlayer.Speed = Math.Min(entity.CurrentSpeed / _walkSpeed, 1f);
		}
	}
}
