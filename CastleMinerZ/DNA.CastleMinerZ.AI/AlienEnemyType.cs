namespace DNA.CastleMinerZ.AI
{
	public class AlienEnemyType : EnemyType
	{
		public AlienEnemyType()
			: base(EnemyTypeEnum.ALIEN, ModelNameEnum.ALIEN, TextureNameEnum.ALIEN, FoundInEnum.CRASHSITE)
		{
			ChanceOfBulletStrike = 0.6f;
			SpawnRadius = 10;
		}

		public override float GetMaxSpeed()
		{
			return MathTools.RandomFloat(2f, 5.5f);
		}

		public override IFSMState<BaseZombie> GetEmergeState(BaseZombie entity)
		{
			return EnemyStates.AlienEmerge;
		}

		public override IFSMState<BaseZombie> GetAttackState(BaseZombie entity)
		{
			return EnemyStates.AlienAttack;
		}

		public override IFSMState<BaseZombie> GetChaseState(BaseZombie entity)
		{
			return EnemyStates.AlienChase;
		}

		public override IFSMState<BaseZombie> GetGiveUpState(BaseZombie entity)
		{
			return EnemyStates.AlienGiveUp;
		}

		public override IFSMState<BaseZombie> GetHitState(BaseZombie entity)
		{
			return EnemyStates.AlienHit;
		}

		public override IFSMState<BaseZombie> GetDieState(BaseZombie entity)
		{
			return EnemyStates.AlienDie;
		}

		public override IFSMState<BaseZombie> GetDigState(BaseZombie entity)
		{
			return GetGiveUpState(entity);
		}

		public override float GetDamageTypeMultiplier(DamageType damageType, bool headShot)
		{
			float num = 1f;
			if ((damageType & DamageType.PIERCING) != 0)
			{
				num *= 0.25f;
			}
			else if ((damageType & DamageType.SHOTGUN) != 0)
			{
				num *= 1.25f;
			}
			else if ((damageType & DamageType.BLADE) != 0)
			{
				num *= 0.75f;
			}
			if (headShot)
			{
				num *= 2f;
			}
			return num;
		}
	}
}
