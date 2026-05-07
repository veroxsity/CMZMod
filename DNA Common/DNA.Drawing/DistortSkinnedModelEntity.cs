using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class DistortSkinnedModelEntity : SkinnedModelEntity, IScreenDistortion
	{
		private Texture2D _backgroundImage;

		private float _distortionScale = 0.1f;

		private bool _blur;

		public Texture2D ScreenBackground
		{
			get
			{
				return _backgroundImage;
			}
			set
			{
				_backgroundImage = value;
			}
		}

		public float DistortionScale
		{
			get
			{
				return _distortionScale;
			}
			set
			{
				_distortionScale = value;
			}
		}

		public bool Blur
		{
			get
			{
				return _blur;
			}
			set
			{
				_blur = value;
			}
		}

		public DistortSkinnedModelEntity(Game game, Model model, Texture2D backgroundImage)
			: base(model)
		{
			_backgroundImage = backgroundImage;
			AlphaSort = true;
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					meshPart.Effect = new DistortSkinnedEffect(game);
				}
			}
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			DistortSkinnedEffect distortSkinnedEffect = (DistortSkinnedEffect)effect;
			distortSkinnedEffect.Texture = _backgroundImage;
			distortSkinnedEffect.SetBoneTransforms(_skinTransforms);
			distortSkinnedEffect.DistortionScale = _distortionScale;
			distortSkinnedEffect.Blur = _blur;
			return base.SetEffectParams(mesh, effect, gameTime, world, view, projection);
		}
	}
}
