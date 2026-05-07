using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Net;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class RocketEntity : Entity
	{
		public const string RocketModelName = "weapons\\RPGGrenade";

		private const float cMaxVelocity = 50f;

		private const float cFuseTime = 0.25f;

		private const float cLaunchVelocity = 3.5f;

		private const float cLaunchBlend = 0.75f;

		private const float cRotationalSpeedMultiplier = 0.5f;

		private const float cRocketLifetime = 10f;

		private static ParticleEffect sSmokeTrailEffect;

		private static ParticleEffect sFireTrailEffect;

		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private ModelEntity _rocket;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private Vector3 _startingVelocity;

		private Vector3 _emittedDirection;

		private Vector3 _startPoint;

		private Vector3 _guidedPosition;

		private InventoryItemIDs _weaponType;

		private float _runningTime;

		private float _maxSpeed;

		private float _timeToFullGuidance;

		private float _timeToMaxSpeed;

		private bool _guided;

		private bool _doExplosion;

		private bool _active;

		private AudioEmitter _audioEmitter = new AudioEmitter();

		private SoundCue3D _whooshCue;

		public static void Init()
		{
			sFireTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("RocketFireTrail");
			sSmokeTrailEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("RocketSmokeTrail");
		}

		public RocketEntity(Vector3 position, Vector3 vector, InventoryItemIDs weaponType, bool guided, bool doExplosion)
		{
			_rocket = new ModelEntity(CastleMinerZGame.Instance.Content.Load<Model>("weapons\\RPGGrenade"));
			_rocket.EnableDefaultLighting();
			_rocket.LocalRotation = Quaternion.CreateFromYawPitchRoll(-(float)Math.PI / 2f, 0f, 0f);
			_active = true;
			base.Children.Add(_rocket);
			_emittedDirection = vector;
			_startPoint = position + vector;
			_doExplosion = doExplosion;
			_weaponType = weaponType;
			base.LocalToParent = MathTools.CreateWorld(_startPoint, _emittedDirection);
			_runningTime = -0.25f;
			_guidedPosition = _startPoint;
			_startingVelocity = Vector3.Normalize(Vector3.Lerp(base.LocalToWorld.Forward, base.LocalToWorld.Up, 0.75f)) * 3.5f;
			_audioEmitter.Forward = _emittedDirection;
			_audioEmitter.Position = base.LocalPosition;
			_audioEmitter.Up = Vector3.Up;
			_audioEmitter.Velocity = _startingVelocity;
			_whooshCue = SoundManager.Instance.PlayInstance("RocketWhoosh", _audioEmitter);
			if (weaponType == InventoryItemIDs.RocketLauncherGuided)
			{
				_guided = guided;
				_maxSpeed = 50f;
				_timeToFullGuidance = 2.5f;
				_timeToMaxSpeed = 1f;
			}
			else
			{
				_guided = false;
				_maxSpeed = 25f;
				_timeToFullGuidance = 1f;
				_timeToMaxSpeed = 1f;
			}
			_smokeEmitter = sSmokeTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
			_smokeEmitter.Emitting = false;
			_smokeEmitter.DrawPriority = 900;
			base.Children.Add(_smokeEmitter);
			_fireEmitter = sFireTrailEffect.CreateEmitter(CastleMinerZGame.Instance);
			_fireEmitter.Emitting = false;
			_fireEmitter.DrawPriority = 900;
			base.Children.Add(_fireEmitter);
			Collidee = false;
			Collider = false;
		}

		protected override void OnParentChanged(Entity oldParent, Entity newParent)
		{
			if (newParent == null && _whooshCue != null && _whooshCue.IsPlaying)
			{
				_whooshCue.Stop(AudioStopOptions.Immediate);
			}
			base.OnParentChanged(oldParent, newParent);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			if (!_active)
			{
				if (!_fireEmitter.HasActiveParticles && !_smokeEmitter.HasActiveParticles)
				{
					_fireEmitter.RemoveFromParent();
					_smokeEmitter.RemoveFromParent();
					RemoveFromParent();
				}
				return;
			}
			bool flag = false;
			float num = (float)gameTime.ElapsedGameTime.TotalSeconds;
			_runningTime += num;
			Vector3 vector = Vector3.Zero;
			float num2 = 0f;
			if (_runningTime < _timeToFullGuidance)
			{
				float num3 = _runningTime + 0.25f;
				vector = _startPoint + Vector3.Multiply(_startingVelocity, num3) + Vector3.Multiply(Vector3.Down, 4.9f * num3 * num3);
			}
			Vector3 fwd;
			Vector3 vector5;
			if (_runningTime >= 0f)
			{
				_fireEmitter.Emitting = true;
				_smokeEmitter.Emitting = true;
				Vector3 vector4;
				if (_guided)
				{
					Vector3 vector2;
					if (EnemyManager.Instance != null && EnemyManager.Instance.DragonIsAlive)
					{
						flag = true;
						vector2 = EnemyManager.Instance.DragonPosition;
					}
					else
					{
						vector2 = base.WorldPosition + base.LocalToWorld.Forward;
					}
					Vector3 vector3 = Vector3.Normalize(vector2 - base.WorldPosition);
					if (_runningTime >= _timeToFullGuidance)
					{
						vector4 = vector3;
						fwd = vector4;
					}
					else
					{
						float num4 = _runningTime / _timeToFullGuidance;
						vector4 = Vector3.Normalize(Vector3.Lerp(_emittedDirection, vector3, num4));
						fwd = Vector3.Normalize(Vector3.Lerp(_emittedDirection, vector3, (float)Math.Sqrt(num4)));
					}
				}
				else
				{
					vector4 = _emittedDirection;
					fwd = _emittedDirection;
				}
				if (_runningTime < _timeToMaxSpeed)
				{
					float num5 = (float)Math.Sqrt(_runningTime / _timeToMaxSpeed);
					num2 = num5 * _maxSpeed;
					_guidedPosition += vector4 * (num2 * num);
					vector5 = Vector3.Lerp(vector, _guidedPosition, num5);
				}
				else
				{
					num2 = _maxSpeed;
					_guidedPosition += vector4 * (num2 * num);
					vector5 = _guidedPosition;
				}
			}
			else
			{
				vector5 = vector;
				fwd = _emittedDirection;
			}
			if (num2 > 0f)
			{
				Quaternion value = Quaternion.CreateFromYawPitchRoll(0f, 0f, num * 0.5f * num2);
				_rocket.LocalRotation = Quaternion.Concatenate(_rocket.LocalRotation, value);
			}
			Vector3 worldPosition = base.WorldPosition;
			base.LocalToParent = MathTools.CreateWorld(vector5, fwd);
			bool flag2 = false;
			if (!flag && (BlockTerrain.Instance == null || !BlockTerrain.Instance.IsTracerStillInWorld(vector5)))
			{
				flag2 = true;
			}
			else
			{
				IShootableEnemy shootableEnemy = null;
				Vector3 vector6 = base.WorldPosition;
				bool flag3 = false;
				if (_runningTime > 10f)
				{
					flag3 = true;
				}
				else
				{
					tp.Init(worldPosition, base.WorldPosition);
					shootableEnemy = EnemyManager.Instance.Trace(tp, false);
					if (tp._collides)
					{
						flag3 = true;
						vector6 = tp.GetIntersection();
					}
				}
				if (flag3)
				{
					flag2 = true;
					if (_doExplosion)
					{
						bool hitDragon = shootableEnemy is DragonClientEntity;
						DetonateRocketMessage.Send(CastleMinerZGame.Instance.MyNetworkGamer, vector6, ExplosiveTypes.Rocket, _weaponType, hitDragon);
						Explosive.FindBlocksToRemove(IntVector3.FromVector3(vector6), ExplosiveTypes.Rocket, false);
					}
				}
			}
			_audioEmitter.Forward = _guidedPosition;
			_audioEmitter.Position = base.LocalPosition;
			if (flag2)
			{
				_fireEmitter.Emitting = false;
				_smokeEmitter.Emitting = false;
				_rocket.RemoveFromParent();
				_rocket = null;
				_active = false;
			}
		}
	}
}
