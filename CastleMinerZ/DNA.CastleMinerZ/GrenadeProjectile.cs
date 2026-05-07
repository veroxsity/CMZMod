using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GrenadeProjectile : Entity
	{
		private class GrenadeTraceProbe : AABBTraceProbe
		{
			public IShootableEnemy _lastEnemy;

			public override bool TestThisType(BlockTypeEnum e)
			{
				BlockType type = BlockType.GetType(e);
				if (type.CanBeTouched)
				{
					return type.BlockPlayer;
				}
				return false;
			}

			public override bool TestThisEnemy(IShootableEnemy enemy)
			{
				if (_lastEnemy != null)
				{
					return _lastEnemy != enemy;
				}
				return true;
			}
		}

		public const string cGrenadeModelName = "Grenade";

		private const float cMinSpeedSquared = 1f;

		private readonly BoundingBox cGrenadeAABB = new BoundingBox(new Vector3(-0.1f, -0.1f, -0.1f), new Vector3(0.1f, 0.1f, 0.1f));

		protected static ParticleEffect _spashEffect;

		protected static ParticleEffect _sparkEffect;

		protected static ParticleEffect _smokeEffect;

		protected static Model _grenadeModel;

		private static GrenadeTraceProbe tp = new GrenadeTraceProbe();

		protected ModelEntity _grenadeEntity;

		protected Vector3 _linearVelocity;

		protected Vector3 _lastPosition;

		protected Vector3 _rotationAxis;

		protected float _rotationSpeed;

		protected bool _isLocal;

		protected float _timeLeft;

		protected bool _stopped;

		protected bool _exploded;

		private AudioEmitter _audioEmitter = new AudioEmitter();

		public static void Init()
		{
			_grenadeModel = CastleMinerZGame.Instance.Content.Load<Model>("Grenade");
			_smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("SmokeEffect");
			_sparkEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("SparksEffect");
			_spashEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("BlasterFlash");
		}

		public static void HandleDetonateGrenadeMessage(DetonateGrenadeMessage msg)
		{
			switch (msg.GrenadeType)
			{
			case GrenadeTypeEnum.HE:
				HEGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				break;
			case GrenadeTypeEnum.Smoke:
				SmokeGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				break;
			case GrenadeTypeEnum.Flash:
				FlashGrenadeProjectile.InternalHandleDetonateGrenadeMessage(msg);
				break;
			}
		}

		public static GrenadeProjectile Create(Vector3 position, Vector3 velocity, float timeLeft, GrenadeTypeEnum grenadeType, bool isLocal)
		{
			GrenadeProjectile grenadeProjectile = null;
			switch (grenadeType)
			{
			case GrenadeTypeEnum.HE:
				grenadeProjectile = new HEGrenadeProjectile();
				break;
			case GrenadeTypeEnum.Smoke:
				grenadeProjectile = new SmokeGrenadeProjectile();
				break;
			case GrenadeTypeEnum.Flash:
				grenadeProjectile = new FlashGrenadeProjectile();
				break;
			}
			grenadeProjectile.LocalToParent = MathTools.CreateWorld(position, velocity);
			grenadeProjectile._rotationAxis = grenadeProjectile.LocalToWorld.Right;
			grenadeProjectile._rotationSpeed = 6f;
			grenadeProjectile._linearVelocity = velocity;
			grenadeProjectile._timeLeft = timeLeft;
			grenadeProjectile._isLocal = isLocal;
			return grenadeProjectile;
		}

		protected GrenadeProjectile()
		{
			_grenadeEntity = new ModelEntity(_grenadeModel);
			float num = 0.1f / _grenadeEntity.GetLocalBoundingSphere().Radius;
			_grenadeEntity.LocalPosition = new Vector3(0f, -0.08f, 0f) * num;
			_grenadeEntity.LocalScale = new Vector3(num);
			base.Children.Add(_grenadeEntity);
			_stopped = false;
			_audioEmitter.Position = base.WorldPosition;
			_audioEmitter.Up = Vector3.Up;
			_audioEmitter.Velocity = _linearVelocity;
		}

		protected virtual bool ReadyToBeRemoved()
		{
			return _exploded;
		}

		protected virtual void Explode()
		{
			_exploded = true;
		}

		protected void MoveToNewPosition(Vector3 newPosition)
		{
			IShootableEnemy shootableEnemy = null;
			tp._lastEnemy = null;
			int num = 10;
			do
			{
				tp.Init(_lastPosition, newPosition, cGrenadeAABB);
				shootableEnemy = EnemyManager.Instance.Trace(tp, false);
				tp._lastEnemy = shootableEnemy;
				if (tp._collides)
				{
					Vector3 intersection = tp.GetIntersection();
					num--;
					if (num == 0)
					{
						newPosition = intersection;
						_stopped = true;
						_linearVelocity = Vector3.Zero;
						_rotationSpeed = 0f;
						continue;
					}
					float restitution = 0.1f;
					if (shootableEnemy == null)
					{
						_audioEmitter.Position = base.WorldPosition;
						_audioEmitter.Velocity = _linearVelocity;
						if ((double)_linearVelocity.LengthSquared() > 0.01)
						{
							SoundManager.Instance.PlayInstance("BulletHitDirt", _audioEmitter);
						}
						BlockType type = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(tp._worldIndex));
						restitution = type.BounceRestitution;
					}
					HandleCollision(ref newPosition, tp._inNormal, intersection, restitution);
				}
				else
				{
					num = 0;
				}
			}
			while (!_stopped && num > 0);
			base.LocalPosition = newPosition;
		}

		protected Vector3 ReflectVectorWithRestitution(Vector3 inVec, Vector3 normal, float restitution)
		{
			Vector3 vector = normal * (0f - Vector3.Dot(inVec, normal));
			Vector3 vector2 = inVec + vector;
			return vector * restitution + vector2 * MathHelper.Lerp(1f, restitution, Math.Max(normal.Y, 0f));
		}

		protected virtual void HandleCollision(ref Vector3 newPosition, Vector3 collisionNormal, Vector3 collisionPoint, float restitution)
		{
			_linearVelocity = ReflectVectorWithRestitution(_linearVelocity, collisionNormal, restitution);
			if (collisionNormal.Y >= 0.75f && _linearVelocity.LengthSquared() < 1f)
			{
				newPosition = collisionPoint;
				_stopped = true;
				_linearVelocity = Vector3.Zero;
				_rotationSpeed = 0f;
				return;
			}
			_lastPosition = collisionPoint;
			Vector3 vector = ReflectVectorWithRestitution(newPosition - collisionPoint, collisionNormal, restitution);
			newPosition = vector + collisionPoint;
			Vector3 vector2 = _linearVelocity - collisionNormal * Vector3.Dot(_linearVelocity, collisionNormal);
			Vector3 value = Vector3.Cross(collisionNormal, vector2);
			if (value.LengthSquared() > 0f)
			{
				_rotationAxis = Vector3.Normalize(value);
				_rotationSpeed = vector2.Length() / cGrenadeAABB.Max.X;
			}
			else
			{
				_rotationSpeed = 0f;
			}
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			if (_exploded && ReadyToBeRemoved())
			{
				RemoveFromParent();
			}
			if (_stopped)
			{
				Vector3 worldPosition = base.WorldPosition;
				worldPosition.Y -= 0.5f;
				BlockType type = BlockType.GetType(BlockTerrain.Instance.GetBlockWithChanges(worldPosition));
				if (!type.BlockPlayer)
				{
					_stopped = false;
				}
			}
			if (!_stopped)
			{
				if (_rotationSpeed != 0f)
				{
					Quaternion value = Quaternion.CreateFromAxisAngle(_rotationAxis, _rotationSpeed * num);
					base.LocalRotation = Quaternion.Concatenate(base.LocalRotation, value);
				}
				_linearVelocity += Vector3.Down * (9.8f * num);
				_lastPosition = base.WorldPosition;
				Vector3 newPosition = _lastPosition + _linearVelocity * num;
				MoveToNewPosition(newPosition);
			}
			if (!_exploded)
			{
				_timeLeft -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (_timeLeft < 0f)
				{
					Explode();
				}
			}
			base.OnUpdate(gameTime);
		}
	}
}
