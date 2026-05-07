namespace DNA.CastleMinerZ.AI
{
	public class AlienHit : ZombieHit
	{
		public override string GetAnimName(BaseZombie entity)
		{
			return "Damage" + (entity.Rnd.Next(2) + 1);
		}
	}
}
