using Microsoft.Xna.Framework;

namespace DNA.Drawing
{
	public class LookAtCamera : PerspectiveCamera
	{
		public Entity LookAtEntity;

		public Angle Roll = Angle.Zero;

		public override Matrix View
		{
			get
			{
				if (LookAtEntity != null)
				{
					Matrix matrix = Matrix.CreateFromAxisAngle(Vector3.Forward, Roll.Radians);
					Matrix matrix2 = matrix * base.LocalToWorld;
					Vector3 cameraUpVector = Vector3.TransformNormal(Vector3.Up, matrix2);
					Vector3 worldPosition = base.WorldPosition;
					Vector3 worldPosition2 = LookAtEntity.WorldPosition;
					return Matrix.CreateLookAt(worldPosition, worldPosition2, cameraUpVector);
				}
				return base.View;
			}
		}
	}
}
