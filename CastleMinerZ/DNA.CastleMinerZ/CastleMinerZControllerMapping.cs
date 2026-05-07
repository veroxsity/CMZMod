using DNA.Input;
using Microsoft.Xna.Framework.Input;

namespace DNA.CastleMinerZ
{
	public class CastleMinerZControllerMapping : FPSControllerMapping
	{
		public Trigger Shoulder;

		public Trigger LeftTrigger;

		public Trigger Use;

		public Trigger Reload;

		public Trigger NextItem;

		public Trigger PrevoiusItem;

		public Trigger BlockUI;

		public Trigger PlayersScreen;

		public Trigger NoFallMode;

		public Trigger FlyMode;

		public Trigger Activate;

		public override void ProcessInput(KeyboardInput keyboard, MouseInput mouse, GameController controller)
		{
			PrevoiusItem.Pressed = controller.PressedButtons.LeftShoulder || controller.PressedDPad.Left || mouse.DeltaWheel > 0;
			PrevoiusItem.Released = controller.ReleasedButtons.LeftShoulder || controller.ReleasedDPad.Left || mouse.DeltaWheel == 0;
			PrevoiusItem.Held = controller.CurrentState.IsButtonDown(Buttons.LeftShoulder) || controller.CurrentState.DPad.Left == ButtonState.Pressed || mouse.DeltaWheel > 0;
			NextItem.Pressed = controller.PressedButtons.RightShoulder || controller.PressedDPad.Right || mouse.DeltaWheel < 0;
			NextItem.Released = controller.ReleasedButtons.RightShoulder || controller.ReleasedDPad.Right || mouse.DeltaWheel == 0;
			NextItem.Held = controller.CurrentState.IsButtonDown(Buttons.RightShoulder) || controller.CurrentState.DPad.Right == ButtonState.Pressed || mouse.DeltaWheel < 0;
			BlockUI.Pressed = controller.PressedButtons.Y || keyboard.WasKeyPressed(Keys.E);
			BlockUI.Released = controller.PressedButtons.Y || keyboard.WasKeyReleased(Keys.E);
			BlockUI.Held = controller.CurrentState.IsButtonDown(Buttons.Y) || keyboard.IsKeyDown(Keys.E);
			Activate.Pressed = controller.PressedButtons.B || mouse.RightButtonPressed;
			Activate.Released = controller.PressedButtons.B || mouse.RightButtonReleased;
			Activate.Held = controller.CurrentState.IsButtonDown(Buttons.B) || mouse.RightButtonDown;
			Shoulder.Pressed = controller.PressedButtons.LeftTrigger || mouse.RightButtonPressed;
			Shoulder.Released = controller.ReleasedButtons.LeftTrigger || mouse.RightButtonReleased;
			Shoulder.Held = controller.CurrentState.IsButtonDown(Buttons.LeftTrigger) || mouse.RightButtonDown;
			LeftTrigger.Pressed = controller.PressedButtons.LeftTrigger;
			LeftTrigger.Released = controller.ReleasedButtons.LeftTrigger;
			LeftTrigger.Held = controller.CurrentState.IsButtonDown(Buttons.LeftTrigger);
			Use.Pressed = controller.PressedButtons.RightTrigger || mouse.LeftButtonPressed;
			Use.Released = controller.ReleasedButtons.RightTrigger || mouse.LeftButtonReleased;
			Use.Held = controller.CurrentState.IsButtonDown(Buttons.RightTrigger) || mouse.LeftButtonDown;
			Reload.Pressed = controller.PressedButtons.X || keyboard.WasKeyPressed(Keys.R);
			Reload.Released = controller.ReleasedButtons.X || keyboard.WasKeyReleased(Keys.R);
			Reload.Held = controller.CurrentState.IsButtonDown(Buttons.X) || keyboard.IsKeyDown(Keys.R);
			PlayersScreen.Pressed = controller.PressedButtons.Back || keyboard.WasKeyPressed(Keys.Tab);
			PlayersScreen.Released = controller.ReleasedButtons.Back || keyboard.WasKeyReleased(Keys.Tab);
			PlayersScreen.Held = controller.CurrentState.IsButtonDown(Buttons.Back) || keyboard.IsKeyDown(Keys.Tab);
			NoFallMode.Pressed = controller.PressedButtons.LeftStick;
			NoFallMode.Released = controller.ReleasedButtons.LeftStick;
			NoFallMode.Held = controller.CurrentState.IsButtonDown(Buttons.LeftStick);
			FlyMode.Pressed = controller.PressedButtons.RightStick;
			FlyMode.Released = controller.ReleasedButtons.RightStick;
			FlyMode.Held = controller.CurrentState.IsButtonDown(Buttons.RightStick);
			base.ProcessInput(keyboard, mouse, controller);
		}
	}
}
