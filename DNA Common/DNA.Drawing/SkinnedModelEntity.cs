using DNA.Drawing.Animation;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class SkinnedModelEntity : ModelEntity
	{
		protected Matrix[] _skinTransforms;

		private SkinedAnimationData SkinningData
		{
			get
			{
				return (SkinedAnimationData)base.Model.Tag;
			}
		}

		protected override Skeleton GetSkeleton()
		{
			return SkinningData.Skeleton;
		}

		public SkinnedModelEntity(Model model)
			: base(model)
		{
			_skinTransforms = new Matrix[SkinningData.Skeleton.Count];
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			base.OnUpdate(gameTime);
			UpdateSkinTransforms();
		}

		private void UpdateSkinTransforms()
		{
			for (int i = 0; i < _skinTransforms.Length; i++)
			{
				_skinTransforms[i] = SkinningData.InverseBindPose[i] * _worldBoneTransforms[i];
			}
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is SkinnedEffect)
			{
				SkinnedEffect skinnedEffect = (SkinnedEffect)effect;
				skinnedEffect.SetBoneTransforms(_skinTransforms);
				skinnedEffect.EnableDefaultLighting();
				skinnedEffect.SpecularColor = new Vector3(0.25f);
				skinnedEffect.SpecularPower = 16f;
			}
			else if (effect.Parameters["Bones"] != null)
			{
				effect.Parameters["Bones"].SetValue(_skinTransforms);
			}
			return base.SetEffectParams(mesh, effect, gameTime, Matrix.Identity, view, projection);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			for (int i = 0; i < base.Model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = base.Model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					Effect effect = modelMesh.Effects[j];
					SetEffectParams(modelMesh, effect, gameTime, base.LocalToWorld, view, projection);
				}
				modelMesh.Draw();
			}
			if (ShowSkeleton)
			{
				DrawWireframeBones(device, view, projection);
			}
		}
	}
}
