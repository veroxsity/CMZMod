using System;
using DNA.Audio;
using DNA.CastleMinerZ.Inventory;
using Microsoft.Xna.Framework;

namespace DNA.CastleMinerZ.AI
{
	public class ZombieDig : ZombieTryAttack
	{
		public override void Enter(BaseZombie entity)
		{
			Vector3 vector = entity.Target.WorldPosition - entity.WorldPosition;
			if (entity.Target.IsLocal && vector.LengthSquared() > 256f)
			{
				entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
				return;
			}
			if (entity.OnGround)
			{
				ZeroVelocity(entity);
			}
			entity.CurrentPlayer = entity.PlayClip(GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
			entity.HitCount = 0;
			entity.SwingCount = 0;
			entity.MissCount = 0;
			entity.CurrentPlayer.Speed = entity.EType.AttackAnimationSpeed;
		}

		public override void Update(BaseZombie entity, float dt)
		{
			if (entity.OnGround)
			{
				ZeroVelocity(entity);
			}
			else
			{
				ReduceVelocity(entity);
			}
			if (entity.IsNearAnimationEnd)
			{
				if (entity.MissCount == 4)
				{
					entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
					return;
				}
				if (entity.MissCount == 0 || (entity.MissCount & 2) != 0)
				{
					entity.StateMachine.ChangeState(entity.EType.GetChaseState(entity));
					return;
				}
				Vector3 vector = entity.Target.WorldPosition - entity.WorldPosition;
				float y = vector.Y;
				vector.Y = 0f;
				float num = vector.Length();
				if (num < 1f && Math.Abs(y) < 1.5f)
				{
					entity.StateMachine.ChangeState(entity.EType.GetAttackState(entity));
					return;
				}
				if (entity.Target.IsLocal && (num > 16f || Math.Abs(y) > 8f))
				{
					entity.StateMachine.ChangeState(entity.EType.GetGiveUpState(entity));
					return;
				}
				float desiredHeading = (float)Math.Atan2(0f - vector.Z, vector.X) + (float)Math.PI / 2f;
				entity.LocalRotation = Quaternion.CreateFromYawPitchRoll(MakeHeading(entity, desiredHeading), 0f, 0f);
				entity.CurrentPlayer = entity.PlayClip(GetRandomAttack(entity), false, TimeSpan.FromSeconds(0.25));
				entity.HitCount = 0;
				entity.MissCount = 0;
			}
			else
			{
				if (entity.AnimationIndex == -1)
				{
					return;
				}
				float[] array = HitTimes[entity.AnimationIndex];
				if (entity.CurrentPlayer.CurrentTime.TotalSeconds >= (double)(array[entity.HitCount] / entity.CurrentPlayer.Speed))
				{
					Vector3 worldPosition = entity.WorldPosition;
					Vector3 worldPosition2 = entity.Target.WorldPosition;
					IntVector3 intVector = IntVector3.FromVector3(entity.WorldPosition);
					if (worldPosition2.Y >= worldPosition.Y + 1f)
					{
						intVector.Y++;
					}
					else if (worldPosition2.Y <= worldPosition.Y - 1f)
					{
						intVector.Y--;
					}
					IntVector3 minCorner = intVector;
					IntVector3 maxCorner = intVector;
					maxCorner.Y += 2;
					if (worldPosition2.X > worldPosition.X)
					{
						maxCorner.X++;
					}
					else
					{
						minCorner.X--;
					}
					if (worldPosition2.Z >= worldPosition.Z)
					{
						maxCorner.Z++;
					}
					else
					{
						minCorner.Z--;
					}
					entity.SwingCount++;
					bool flag = true;
					int hits = (int)((float)entity.SwingCount * entity.EType.DiggingMultiplier);
					switch (Explosive.EnemyBreakBlocks(minCorner, maxCorner, hits, entity.EType.HardestBlockThatCanBeDug, entity.Target.IsLocal))
					{
					case Explosive.EnemyBreakBlocksResult.BlocksBroken:
						entity.MissCount |= 2;
						break;
					case Explosive.EnemyBreakBlocksResult.BlocksWillBreak:
						entity.MissCount |= 1;
						break;
					case Explosive.EnemyBreakBlocksResult.BlocksWillNotBreak:
						entity.MissCount |= 4;
						break;
					case Explosive.EnemyBreakBlocksResult.RegionIsEmpty:
						flag = false;
						break;
					}
					if (flag)
					{
						SoundManager.Instance.PlayInstance("ZombieDig", entity.SoundEmitter);
					}
					entity.HitCount++;
					if (entity.HitCount == array.Length)
					{
						entity.AnimationIndex = -1;
						entity.HitCount = 0;
					}
				}
			}
		}
	}
}
