using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Particles
{
	public class ParticleEffect : ParticleBase<Texture2D>
	{
		[NonSerialized]
		private List<ParticleEmitter.ParticleEmitterCore> _particleCores = new List<ParticleEmitter.ParticleEmitterCore>();

		public void Flush()
		{
			for (int i = 0; i < _particleCores.Count; i++)
			{
				_particleCores[i].ParticleEmitter.ReleaseCore();
			}
			_particleCores.Clear();
		}

		internal ParticleEmitter.ParticleEmitterCore CreateParticleCore(ParticleEmitter emitter)
		{
			for (int i = 0; i < _particleCores.Count; i++)
			{
				if (!_particleCores[i].HasActiveParticles || _particleCores[i].ParticleEmitter.Scene == null)
				{
					_particleCores[i].ParticleEmitter.ReleaseCore();
					emitter.SetCore(_particleCores[i]);
					return _particleCores[i];
				}
			}
			ParticleEmitter.ParticleEmitterCore particleEmitterCore = new ParticleEmitter.ParticleEmitterCore(emitter);
			_particleCores.Add(particleEmitterCore);
			return particleEmitterCore;
		}

		public ParticleEmitter CreateEmitter(DNAGame game)
		{
			return ParticleEmitter.Create(game, this, null);
		}

		public ParticleEmitter CreateEmitter(DNAGame game, Texture2D reflectionMap)
		{
			return ParticleEmitter.Create(game, this, reflectionMap);
		}
	}
}
