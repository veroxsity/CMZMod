using System;
using DNA.Audio;
using DNA.Drawing;
using DNA.Drawing.Particles;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class ExplosiveFlashEntity : Entity, IEquatable<ExplosiveFlashEntity>
	{
		private class FlashingModelEntity : ModelEntity
		{
			private static Model _model;

			public FlashingModelEntity()
				: base(_model)
			{
			}

			static FlashingModelEntity()
			{
				_model = CastleMinerZGame.Instance.Content.Load<Model>("WhiteBox");
			}

			protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
			{
				BasicEffect basicEffect = (BasicEffect)effect;
				basicEffect.Alpha = 0.5f;
				basicEffect.DiffuseColor = Color.Red.ToVector3();
				return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
			}
		}

		private static TimeSpan _maxLifetime;

		private static ParticleEffect _smokeEffect;

		private bool _flashOn = true;

		private OneShotTimer _timer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private TimeSpan _lifeTime = TimeSpan.Zero;

		public IntVector3 BlockPosition = IntVector3.Zero;

		private SoundCue3D _fuseCue;

		private AudioEmitter _emitter;

		private ParticleEmitter _smokeEmitter;

		private FlashingModelEntity _flashingModel;

		static ExplosiveFlashEntity()
		{
			_maxLifetime = TimeSpan.FromSeconds(8.0);
			_smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("TorchSmoke");
		}

		public ExplosiveFlashEntity(IntVector3 position)
		{
			base.LocalPosition = position + new Vector3(0.5f, -0.002f, 0.5f);
			BlockPosition = position;
			_emitter = new AudioEmitter();
			_emitter.Position = base.LocalPosition;
			_fuseCue = SoundManager.Instance.PlayInstance("Fuse", _emitter);
			base.BlendState = BlendState.Additive;
			base.DepthStencilState = DepthStencilState.DepthRead;
			DrawPriority = 300;
			_smokeEmitter = _smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
			_smokeEmitter.Emitting = true;
			_smokeEmitter.DrawPriority = 900;
			_smokeEmitter.LocalPosition += new Vector3(0f, 1f, 0f);
			_smokeEmitter.LocalRotation = Quaternion.CreateFromAxisAngle(Vector3.Left, Angle.FromDegrees(90f).Radians);
			base.Children.Add(_smokeEmitter);
			_flashingModel = new FlashingModelEntity();
			base.Children.Add(_flashingModel);
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			_timer.Update(gameTime.ElapsedGameTime);
			if (_timer.Expired)
			{
				_timer.Reset();
				_flashOn = !_flashOn;
				if (_flashOn)
				{
					_flashingModel.Visible = true;
				}
				else
				{
					_flashingModel.Visible = false;
				}
			}
			_lifeTime += gameTime.ElapsedGameTime;
			if (_lifeTime > TimeSpan.FromSeconds(3.0))
			{
				_timer.MaxTime = TimeSpan.FromSeconds(0.125);
			}
			if (_lifeTime > _maxLifetime && CastleMinerZGame.Instance.GameScreen != null)
			{
				CastleMinerZGame.Instance.GameScreen.RemoveExplosiveFlashModel(BlockPosition);
			}
			base.Update(game, gameTime);
		}

		public bool Equals(ExplosiveFlashEntity other)
		{
			if (base.LocalPosition == other.LocalPosition)
			{
				return true;
			}
			return false;
		}
	}
}
