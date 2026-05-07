using System;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class GPSMarkerEntity : ModelEntity
	{
		private static Model MarkerModel;

		public Color color = Color.White;

		public GPSMarkerEntity()
			: base(MarkerModel)
		{
			EnableDefaultLighting();
			EnablePerPixelLighting();
		}

		static GPSMarkerEntity()
		{
			MarkerModel = CastleMinerZGame.Instance.Content.Load<Model>("Marker");
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			base.LocalRotation = Quaternion.CreateFromYawPitchRoll((float)gameTime.TotalGameTime.TotalSeconds * 2f % ((float)Math.PI * 2f), 0f, 0f);
			base.Update(game, gameTime);
		}

		protected override bool SetEffectParams(ModelMesh mesh, Effect oeffect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (base.SetEffectParams(mesh, oeffect, gameTime, world, view, projection))
			{
				BasicEffect basicEffect = mesh.Effects[0] as BasicEffect;
				if (basicEffect != null)
				{
					if (mesh.Name.Contains("recolor_"))
					{
						if (color.A == 0)
						{
							return false;
						}
						basicEffect.DiffuseColor = color.ToVector3();
					}
					else
					{
						basicEffect.DiffuseColor = Color.White.ToVector3();
					}
				}
				return true;
			}
			return false;
		}
	}
}
