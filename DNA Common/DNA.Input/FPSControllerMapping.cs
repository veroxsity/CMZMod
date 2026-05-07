using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DNA.Input
{
	public class FPSControllerMapping : ControllerMapping
	{
		public bool InvertY;

		public float Sensitivity = 1f;

		public Vector2 Movement;

		public Vector2 Aiming;

		public Trigger Fire;

		public Trigger Jump;

		public override void ProcessInput(KeyboardInput keyboard, MouseInput mouse, GameController controller)
		{
			Movement = controller.CurrentState.ThumbSticks.Left;
			Aiming = controller.CurrentState.ThumbSticks.Right;
			Aiming.X += mouse.DeltaPosition.X / 20f;
			Aiming.Y -= mouse.DeltaPosition.Y / 20f;
			if (InvertY)
			{
				Aiming.Y = 0f - Aiming.Y;
			}
			Aiming *= Sensitivity;
			if (keyboard.IsKeyDown(Keys.W))
			{
				Movement.Y = 1f;
			}
			if (keyboard.IsKeyDown(Keys.S))
			{
				Movement.Y = -1f;
			}
			if (keyboard.IsKeyDown(Keys.D))
			{
				Movement.X = 1f;
			}
			if (keyboard.IsKeyDown(Keys.A))
			{
				Movement.X = -1f;
			}
			Jump.Pressed = controller.PressedButtons.A || keyboard.WasKeyPressed(Keys.Space);
			Jump.Released = controller.ReleasedButtons.A || keyboard.WasKeyReleased(Keys.Space);
			Jump.Held = controller.CurrentState.Buttons.A == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Space);
			Fire.Pressed = (controller.CurrentState.Triggers.Right > 0.5f && controller.LastState.Triggers.Right <= 0.5f) || mouse.LeftButtonPressed;
			Fire.Released = (controller.CurrentState.Triggers.Right < 0.5f && controller.LastState.Triggers.Right >= 0.5f) || mouse.LeftButtonReleased;
			Fire.Held = controller.CurrentState.Triggers.Right > 0.5f || mouse.LeftButtonDown;
		}
	}
}
