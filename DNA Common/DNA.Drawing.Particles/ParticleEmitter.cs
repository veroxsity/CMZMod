using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Particles
{
	public class ParticleEmitter : Entity
	{
		internal class ParticleEmitterCore
		{
			private bool _emitting = true;

			private int MaxParticles;

			private bool _firstUpdate = true;

			private Vector3 _previousPosition;

			private float _timeLeftOver;

			private TimeSpan _timeToEmit;

			private Random _rand = new Random();

			private Effect particleEffect;

			private EffectParameter effectWorldParameter;

			private EffectParameter effectViewParameter;

			private EffectParameter effectProjectionParameter;

			private EffectParameter effectViewProjectionParameter;

			private EffectParameter effectViewportScaleParameter;

			private EffectParameter effectTimeParameter;

			private EffectParameter effectTotalTimeParameter;

			private EffectParameter effectMinColorParameter;

			private EffectParameter effectMaxColorParameter;

			private EffectParameter effectTileSizeParameter;

			private DynamicVertexBuffer vertexBuffer;

			private IndexBuffer indexBuffer;

			private ParticleVertex[] particles;

			private int firstActiveParticle;

			private int firstNewParticle;

			private int firstFreeParticle;

			private int firstRetiredParticle;

			private float currentTime;

			private int drawCounter;

			private static Random random = new Random();

			public static Effect _effect;

			public ParticleEmitter ParticleEmitter;

			private static Texture2D _heatShimmerTexture;

			private VertexBuffer _instanceVertexBuffer;

			private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1), new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2), new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3));

			public bool Emitting
			{
				get
				{
					return _emitting;
				}
				set
				{
					if (!_emitting && value)
					{
						_timeToEmit = _particleEffect.EmmissionTime;
						_firstUpdate = true;
					}
					_emitting = value;
				}
			}

			public bool HasActiveParticles
			{
				get
				{
					return firstActiveParticle != firstFreeParticle;
				}
			}

			private ParticleEffect _particleEffect
			{
				get
				{
					return ParticleEmitter._particleEffect;
				}
			}

			public bool Loaded
			{
				get
				{
					return particleEffect != null;
				}
			}

			public DNAGame Game
			{
				get
				{
					return ParticleEmitter._game;
				}
			}

			internal ParticleEmitterCore(ParticleEmitter emitter)
			{
				ParticleEmitter = emitter;
			}

			private void LoadParticleEffect(GraphicsDevice device)
			{
				if (_effect == null)
				{
					_effect = Game.Content.Load<Effect>("ParticleEffect");
				}
				particleEffect = _effect.Clone();
			}

			private void SetParams()
			{
				if (!Loaded)
				{
					Initialize();
				}
				EffectParameterCollection parameters = particleEffect.Parameters;
				effectWorldParameter = parameters["World"];
				effectViewParameter = parameters["View"];
				effectProjectionParameter = parameters["Projection"];
				effectViewProjectionParameter = parameters["ViewProjection"];
				effectViewportScaleParameter = parameters["ViewportScale"];
				effectTimeParameter = parameters["CurrentTime"];
				effectTotalTimeParameter = parameters["TotalTime"];
				effectMinColorParameter = parameters["MinColor"];
				effectMaxColorParameter = parameters["MaxColor"];
				effectTileSizeParameter = parameters["TileSize"];
				parameters["Duration"].SetValue((float)_particleEffect.ParticleLifeTime.TotalSeconds);
				parameters["DurationRandomness"].SetValue(_particleEffect.LifetimeVariation);
				parameters["Gravity"].SetValue(_particleEffect.Gravity);
				parameters["EndVelocity"].SetValue(_particleEffect.VelocityEnd);
				parameters["FadeOut"].SetValue(_particleEffect.FadeOut);
				parameters["StartRotation"].SetValue(_particleEffect.RandomizeRotations ? 1 : 0);
				parameters["DistortionScale"].SetValue(_particleEffect.DistortionScale);
				parameters["DistortionAmplitude"].SetValue(_particleEffect.DistortionAmplitude);
				effectMinColorParameter.SetValue(_particleEffect.ColorMin.ToVector4());
				effectMaxColorParameter.SetValue(_particleEffect.ColorMax.ToVector4());
				parameters["RotateSpeed"].SetValue(new Vector2(_particleEffect.RotateSpeedMin, _particleEffect.RotateSpeedMax));
				parameters["StartSize"].SetValue(new Vector2(_particleEffect.StartSizeMin, _particleEffect.StartSizeMax));
				parameters["EndSize"].SetValue(new Vector2(_particleEffect.EndSizeMin, _particleEffect.EndSizeMax));
				Texture2D texture = _particleEffect.Texture;
				parameters["Texture"].SetValue(texture);
				switch (_particleEffect.Technique)
				{
				case ParticleTechnique.Overlay:
					particleEffect.CurrentTechnique = particleEffect.Techniques["GlowParticles"];
					break;
				case ParticleTechnique.Normal:
					particleEffect.CurrentTechnique = particleEffect.Techniques["Particles"];
					break;
				case ParticleTechnique.HeatShimmer:
					particleEffect.CurrentTechnique = particleEffect.Techniques["HeatHazeParticles"];
					if (_heatShimmerTexture == null)
					{
						_heatShimmerTexture = Game.Content.Load<Texture2D>("HeatNormal");
					}
					if (ParticleEmitter.ScreenBackground == null)
					{
						throw new Exception("Screen Background image must be set to use Heat haze effect");
					}
					parameters["ScreenMap"].SetValue(ParticleEmitter.ScreenBackground);
					parameters["DisplacementMap"].SetValue(_heatShimmerTexture);
					break;
				}
			}

			public void Reset()
			{
				firstRetiredParticle = (firstFreeParticle = (firstNewParticle = (firstActiveParticle = 0)));
				_firstUpdate = true;
				_timeLeftOver = 0f;
				currentTime = 0f;
				drawCounter = 0;
				SetParams();
			}

			private int CalcMaxParticles()
			{
				return (int)Math.Ceiling((double)_particleEffect.ParticlesPerSecond * _particleEffect.ParticleLifeTime.TotalSeconds) + 2;
			}

			public void Initialize()
			{
				switch (_particleEffect.BlendMode)
				{
				case ParticleBlendMode.Inherit:
					ParticleEmitter.BlendState = null;
					break;
				case ParticleBlendMode.Additive:
					ParticleEmitter.BlendState = BlendState.Additive;
					break;
				case ParticleBlendMode.NonPreMult:
					ParticleEmitter.BlendState = BlendState.NonPremultiplied;
					break;
				}
				_timeToEmit = _particleEffect.EmmissionTime;
				MaxParticles = CalcMaxParticles();
				particles = new ParticleVertex[MaxParticles * 4];
				for (int i = 0; i < MaxParticles; i++)
				{
					particles[i * 4].Corner = new Vector2(-1f, -1f);
					particles[i * 4 + 1].Corner = new Vector2(1f, -1f);
					particles[i * 4 + 2].Corner = new Vector2(1f, 1f);
					particles[i * 4 + 3].Corner = new Vector2(-1f, 1f);
				}
				LoadParticleEffect(Game.GraphicsDevice);
				Reset();
				vertexBuffer = new DynamicVertexBuffer(Game.GraphicsDevice, ParticleVertex.VertexDeclaration, MaxParticles * 4, BufferUsage.WriteOnly);
				short[] array = new short[MaxParticles * 6];
				for (int j = 0; j < MaxParticles; j++)
				{
					array[j * 6] = (short)(j * 4);
					array[j * 6 + 1] = (short)(j * 4 + 1);
					array[j * 6 + 2] = (short)(j * 4 + 2);
					array[j * 6 + 3] = (short)(j * 4);
					array[j * 6 + 4] = (short)(j * 4 + 2);
					array[j * 6 + 5] = (short)(j * 4 + 3);
				}
				indexBuffer = new IndexBuffer(Game.GraphicsDevice, typeof(short), array.Length, BufferUsage.WriteOnly);
				indexBuffer.SetData(array);
			}

			public void SetInitalPosition(Vector3 position)
			{
				_previousPosition = position;
				_firstUpdate = false;
				if (!Loaded)
				{
					Initialize();
				}
			}

			public void OnUpdate(GameTime gameTime, Vector3 emitterSize)
			{
				if (MaxParticles != CalcMaxParticles())
				{
					Initialize();
				}
				Vector3 worldPosition = ParticleEmitter.WorldPosition;
				float num = 1f / _particleEffect.ParticlesPerSecond;
				if (_firstUpdate)
				{
					_timeLeftOver = num;
					SetInitalPosition(worldPosition);
				}
				bool flag = _particleEffect.EmmissionTime > TimeSpan.Zero;
				currentTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
				RetireActiveParticles();
				FreeRetiredParticles();
				if (!HasActiveParticles)
				{
					currentTime = 0f;
				}
				if (firstRetiredParticle == firstActiveParticle)
				{
					drawCounter = 0;
				}
				float num2 = (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (flag)
				{
					_timeToEmit -= gameTime.ElapsedGameTime;
					if (_timeToEmit <= TimeSpan.Zero)
					{
						num2 += (float)_timeToEmit.TotalSeconds;
						_timeToEmit = TimeSpan.Zero;
						_emitting = false;
					}
				}
				if (num2 > 0f && Emitting)
				{
					Vector3 velocity = (worldPosition - _previousPosition) / num2;
					float num3 = _timeLeftOver + num2;
					float num4 = 0f - _timeLeftOver;
					if (emitterSize.LengthSquared() == 0f)
					{
						while (num3 > num)
						{
							num4 += num;
							num3 -= num;
							float amount = num4 / num2;
							Vector3 position = Vector3.Lerp(_previousPosition, worldPosition, amount);
							AddParticle(position, velocity);
						}
					}
					else
					{
						while (num3 > num)
						{
							num4 += num;
							num3 -= num;
							Vector3 position2 = new Vector3((float)((double)worldPosition.X + (_rand.NextDouble() * 2.0 - 1.0) * (double)emitterSize.X), (float)((double)worldPosition.Y + (_rand.NextDouble() * 2.0 - 1.0) * (double)emitterSize.Y), (float)((double)worldPosition.Z + (_rand.NextDouble() * 2.0 - 1.0) * (double)emitterSize.Z));
							AddParticle(position2, velocity);
						}
					}
					_timeLeftOver = num3;
				}
				_previousPosition = worldPosition;
			}

			public void OnAfterFrame()
			{
				drawCounter++;
			}

			private void RetireActiveParticles()
			{
				float num = (float)_particleEffect.ParticleLifeTime.TotalSeconds;
				while (firstActiveParticle != firstNewParticle)
				{
					float num2 = currentTime - particles[firstActiveParticle * 4].Time;
					if (num2 < num)
					{
						break;
					}
					particles[firstActiveParticle * 4].Time = drawCounter;
					firstActiveParticle++;
					if (firstActiveParticle >= MaxParticles)
					{
						firstActiveParticle = 0;
					}
				}
			}

			private void FreeRetiredParticles()
			{
				while (firstRetiredParticle != firstActiveParticle)
				{
					int num = drawCounter - (int)particles[firstRetiredParticle * 4].Time;
					if (num < 3)
					{
						break;
					}
					firstRetiredParticle++;
					if (firstRetiredParticle >= MaxParticles)
					{
						firstRetiredParticle = 0;
					}
				}
			}

			public void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection, Vector4 lightColor)
			{
				if (!_firstUpdate)
				{
					particleEffect.Parameters["Light"].SetValue(lightColor);
					Draw(device, gameTime, view, projection);
				}
			}

			public void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
			{
				if (_firstUpdate)
				{
					return;
				}
				EffectParameterCollection parameter = particleEffect.Parameters;
				if (ParticleEmitter.EntityColor.HasValue)
				{
					Color color = DrawingTools.ModulateColors(ParticleEmitter.EntityColor.Value, _particleEffect.ColorMin);
					Color color2 = DrawingTools.ModulateColors(ParticleEmitter.EntityColor.Value, _particleEffect.ColorMax);
					effectMinColorParameter.SetValue(color.ToVector4());
					effectMaxColorParameter.SetValue(color2.ToVector4());
				}
				if (ParticleEmitter.IsDistortionEffect && ParticleEmitter.ScreenBackground != null)
				{
					particleEffect.Parameters["ScreenMap"].SetValue(ParticleEmitter.ScreenBackground);
				}
				effectTotalTimeParameter.SetValue((float)gameTime.TotalGameTime.TotalSeconds);
				if (_particleEffect.LocalSpace)
				{
					effectWorldParameter.SetValue(ParticleEmitter.LocalToWorld);
				}
				else
				{
					effectWorldParameter.SetValue(Matrix.Identity);
				}
				effectViewParameter.SetValue(view);
				effectProjectionParameter.SetValue(projection);
				effectViewProjectionParameter.SetValue(view * projection);
				effectTileSizeParameter.SetValue(_particleEffect.TileSize);
				if (vertexBuffer.IsContentLost)
				{
					vertexBuffer.SetData(particles);
				}
				if (firstNewParticle != firstFreeParticle)
				{
					AddNewParticlesToVertexBuffer();
				}
				if (firstActiveParticle == firstFreeParticle)
				{
					return;
				}
				effectViewportScaleParameter.SetValue(new Vector2(0.5f / device.Viewport.AspectRatio, -0.5f));
				effectTimeParameter.SetValue(currentTime);
				if (ParticleEmitter.Instances != null)
				{
					EffectTechnique currentTechnique = particleEffect.Techniques["ParticlesInstanced"];
					particleEffect.CurrentTechnique = currentTechnique;
					int num = ParticleEmitter.Instances.Length;
					if (num <= 0)
					{
						return;
					}
					if (ParticleEmitter._instanceListDirty)
					{
						if (_instanceVertexBuffer == null || ParticleEmitter.Instances.Length > _instanceVertexBuffer.VertexCount)
						{
							if (_instanceVertexBuffer != null)
							{
								_instanceVertexBuffer.Dispose();
							}
							_instanceVertexBuffer = new VertexBuffer(device, instanceVertexDeclaration, num, BufferUsage.WriteOnly);
						}
						_instanceVertexBuffer.SetData(ParticleEmitter.Instances, 0, ParticleEmitter.Instances.Length);
						ParticleEmitter._instanceListDirty = false;
					}
					device.SetVertexBuffers(new VertexBufferBinding(vertexBuffer, 0, 0), new VertexBufferBinding(_instanceVertexBuffer, 0, 1));
					device.Indices = indexBuffer;
					for (int i = 0; i < particleEffect.CurrentTechnique.Passes.Count; i++)
					{
						EffectPass effectPass = particleEffect.CurrentTechnique.Passes[i];
						effectPass.Apply();
						if (firstActiveParticle < firstFreeParticle)
						{
							device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4, firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2, num);
						}
						else
						{
							device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, MaxParticles * 4, 0, MaxParticles * 2, num);
						}
					}
					return;
				}
				device.SetVertexBuffer(vertexBuffer);
				device.Indices = indexBuffer;
				for (int j = 0; j < particleEffect.CurrentTechnique.Passes.Count; j++)
				{
					EffectPass effectPass2 = particleEffect.CurrentTechnique.Passes[j];
					effectPass2.Apply();
					if (firstActiveParticle < firstFreeParticle)
					{
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, firstActiveParticle * 4, (firstFreeParticle - firstActiveParticle) * 4, firstActiveParticle * 6, (firstFreeParticle - firstActiveParticle) * 2);
						continue;
					}
					device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, firstActiveParticle * 4, (MaxParticles - firstActiveParticle) * 4, firstActiveParticle * 6, (MaxParticles - firstActiveParticle) * 2);
					if (firstFreeParticle > 0)
					{
						device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, firstFreeParticle * 4, 0, firstFreeParticle * 2);
					}
				}
			}

			private void AddNewParticlesToVertexBuffer()
			{
				int num = 36;
				if (firstNewParticle < firstFreeParticle)
				{
					vertexBuffer.SetData(firstNewParticle * num * 4, particles, firstNewParticle * 4, (firstFreeParticle - firstNewParticle) * 4, num, SetDataOptions.NoOverwrite);
				}
				else
				{
					vertexBuffer.SetData(firstNewParticle * num * 4, particles, firstNewParticle * 4, (MaxParticles - firstNewParticle) * 4, num, SetDataOptions.NoOverwrite);
					if (firstFreeParticle > 0)
					{
						vertexBuffer.SetData(0, particles, 0, firstFreeParticle * 4, num, SetDataOptions.NoOverwrite);
					}
				}
				firstNewParticle = firstFreeParticle;
			}

			public void AddParticle(Vector3 position, Vector3 velocity)
			{
				if (_particleEffect.LocalSpace)
				{
					position = Vector3.Zero;
				}
				int num = firstFreeParticle + 1;
				if (num >= MaxParticles)
				{
					num = 0;
				}
				if (num != firstRetiredParticle)
				{
					velocity *= _particleEffect.EmitterVelocitySensitivity;
					float num2 = MathHelper.Lerp(_particleEffect.HorizontalVelocityMin, _particleEffect.HorizontalVelocityMax, (float)random.NextDouble());
					double num3 = random.NextDouble() * 6.2831854820251465;
					velocity.X += num2 * (float)Math.Cos(num3);
					velocity.Y += num2 * (float)Math.Sin(num3);
					velocity.Z += MathHelper.Lerp(_particleEffect.VerticalVelocityMin, _particleEffect.VerticalVelocityMax, (float)random.NextDouble());
					velocity = Vector3.TransformNormal(velocity, ParticleEmitter.LocalToWorld);
					Color color = new Color((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
					int num4 = MathTools.RandomInt(_particleEffect.FirstTileToInclude, _particleEffect.LastTileToInclude + 1);
					int x = num4 % _particleEffect.NumTilesWide;
					int y = num4 / _particleEffect.NumTilesWide;
					for (int i = 0; i < 4; i++)
					{
						particles[firstFreeParticle * 4 + i].Position = position;
						particles[firstFreeParticle * 4 + i].SetTileXY(x, y);
						particles[firstFreeParticle * 4 + i].Velocity = velocity;
						particles[firstFreeParticle * 4 + i].Random = color;
						particles[firstFreeParticle * 4 + i].Time = currentTime;
					}
					firstFreeParticle = num;
				}
			}
		}

		public Texture2D ScreenBackground;

		private bool _instanceListDirty = true;

		private Matrix[] _instances;

		private ParticleEmitterCore _core;

		private ParticleEffect _particleEffect;

		private DNAGame _game;

		public Vector4 _lightColor = Vector4.One;

		public Vector3 EmitterSize = Vector3.Zero;

		private bool _emitting;

		private static Queue<ParticleEmitter> _graveYard = new Queue<ParticleEmitter>();

		public Matrix[] Instances
		{
			get
			{
				return _instances;
			}
			set
			{
				_instances = value;
				_instanceListDirty = true;
			}
		}

		public Vector3 LightColor
		{
			get
			{
				return new Vector3(_lightColor.X, _lightColor.Y, _lightColor.Z);
			}
			set
			{
				_lightColor = new Vector4(value, 1f);
			}
		}

		public bool HasActiveParticles
		{
			get
			{
				if (_core == null)
				{
					return false;
				}
				return _core.HasActiveParticles;
			}
		}

		public bool IsDistortionEffect
		{
			get
			{
				return _particleEffect.Technique == ParticleTechnique.HeatShimmer;
			}
		}

		public bool Emitting
		{
			get
			{
				return _emitting;
			}
			set
			{
				_emitting = value;
				if (_core != null)
				{
					_core.Emitting = value;
				}
			}
		}

		public void Reset()
		{
			if (_core == null)
			{
				_core = _particleEffect.CreateParticleCore(this);
				_instanceListDirty = true;
			}
			else
			{
				_core.Reset();
				_instanceListDirty = true;
			}
		}

		public void SetInitalPosition(Vector3 position)
		{
			if (_core == null)
			{
				_core = _particleEffect.CreateParticleCore(this);
			}
			_core.SetInitalPosition(position);
		}

		internal void ReleaseCore()
		{
			_instanceListDirty = true;
			_core = null;
		}

		internal void SetCore(ParticleEmitterCore core)
		{
			_core = core;
			_core.ParticleEmitter = this;
			_core.Reset();
			_instanceListDirty = true;
		}

		internal static ParticleEmitter Create(DNAGame game, ParticleEffect particleEffect, Texture2D screenMap)
		{
			if (particleEffect.DieAfterEmmision)
			{
				if (_graveYard.Count == 0)
				{
					return new ParticleEmitter(game, particleEffect, screenMap);
				}
				ParticleEmitter particleEmitter = _graveYard.Dequeue();
				particleEmitter.Setup(game, particleEffect, screenMap);
				return particleEmitter;
			}
			return new ParticleEmitter(game, particleEffect, screenMap);
		}

		private void Setup(DNAGame game, ParticleEffect particleEffect, Texture2D screenMap)
		{
			ScreenBackground = screenMap;
			_game = game;
			_particleEffect = particleEffect;
			AlphaSort = true;
			EntityColor = null;
			base.LocalRotation = Quaternion.Identity;
			base.LocalPosition = Vector3.Zero;
			base.LocalScale = Vector3.One;
			_lightColor = Vector4.One;
			Visible = true;
			switch (_particleEffect.BlendMode)
			{
			case ParticleBlendMode.Additive:
				base.BlendState = BlendState.Additive;
				break;
			case ParticleBlendMode.NonPreMult:
				base.BlendState = BlendState.NonPremultiplied;
				break;
			}
			base.DepthStencilState = DepthStencilState.DepthRead;
			base.SamplerState = SamplerState.LinearWrap;
			if (_core != null)
			{
				ReleaseCore();
			}
			_core = _particleEffect.CreateParticleCore(this);
		}

		internal ParticleEmitter(DNAGame game, ParticleEffect particleEffect)
		{
			Setup(game, particleEffect, null);
		}

		internal ParticleEmitter(DNAGame game, ParticleEffect particleEffect, Texture2D screenMap)
		{
			Setup(game, particleEffect, screenMap);
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (_emitting && _core == null)
			{
				_core = _particleEffect.CreateParticleCore(this);
				_core.Emitting = true;
			}
			if (_core != null)
			{
				_core.OnUpdate(gameTime, EmitterSize);
				_emitting = _core.Emitting;
			}
			if (_particleEffect.DieAfterEmmision && (_core == null || (!_core.Emitting && !_core.HasActiveParticles)))
			{
				ReleaseCore();
				RemoveFromParent();
				_graveYard.Enqueue(this);
			}
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (_core != null)
			{
				_core.Draw(device, gameTime, view, projection, _lightColor);
			}
			base.Draw(device, gameTime, view, projection);
		}

		public override void OnAfterFrame()
		{
			if (_core != null)
			{
				_core.OnAfterFrame();
			}
			base.OnAfterFrame();
		}

		public void AdvanceEffect(TimeSpan time)
		{
			int num = (int)(time.TotalSeconds * 60.0);
			TimeSpan zero = TimeSpan.Zero;
			TimeSpan timeSpan = TimeSpan.FromSeconds(1.0 / 60.0);
			for (int i = 0; i < num; i++)
			{
				zero += timeSpan;
				Update(_game, new GameTime(zero, timeSpan));
			}
		}
	}
}
