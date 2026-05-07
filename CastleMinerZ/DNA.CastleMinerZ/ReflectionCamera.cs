using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class ReflectionCamera : PerspectiveCamera
	{
		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime time, FilterCallback<Entity> entityFilter)
		{
			if (BlockTerrain.Instance.IsWaterWorld)
			{
				CastleMinerZGame.Instance.DrawingReflection = true;
				PerspectiveCamera fPSCamera = CastleMinerZGame.Instance.LocalPlayer.FPSCamera;
				FieldOfView = fPSCamera.FieldOfView;
				NearPlane = fPSCamera.NearPlane;
				FarPlane = fPSCamera.FarPlane;
				Matrix localToWorld = fPSCamera.LocalToWorld;
				Matrix localToParent = Matrix.Multiply(localToWorld, BlockTerrain.Instance.GetReflectionMatrix());
				base.LocalToParent = localToParent;
				base.Draw(device, spriteBatch, time, entityFilter);
				CastleMinerZGame.Instance.DrawingReflection = false;
			}
		}
	}
}
