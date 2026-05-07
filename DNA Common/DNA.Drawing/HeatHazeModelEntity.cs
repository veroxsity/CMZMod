using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class HeatHazeModelEntity : ModelEntity, IScreenDistortion
	{
		private Texture2D _backgroundImage;

		public float WaveMagnitude = 0.2f;

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

		public HeatHazeModelEntity(Game game, Model model, Texture2D backgoundImage)
			: base(model)
		{
			_backgroundImage = backgoundImage;
			foreach (ModelMesh mesh in model.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					meshPart.Effect = new HeatHazeEffect(game);
				}
			}
			AlphaSort = true;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			for (int i = 0; i < base.Model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = base.Model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					HeatHazeEffect heatHazeEffect = (HeatHazeEffect)modelMesh.Effects[j];
					heatHazeEffect.WaveMagnitude = WaveMagnitude;
					heatHazeEffect.ScreenTexture = _backgroundImage;
				}
			}
			base.Draw(device, gameTime, view, projection);
		}
	}
}
