using System.Collections.Generic;
using DNA.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class CreditsScreen : Screen
	{
		private SpriteFont _headerFont;

		private SpriteFont _normalFont;

		private SpriteFont _titleFont;

		private List<CreditsScreenItem> Items = new List<CreditsScreenItem>();

		private float _topItemDrawLocation = 720f;

		public Color TextColor = Color.White;

		public bool LeftAligned = true;

		private float scrollRate = 40f;

		private float totalLength;

		public CreditsScreen(SpriteFont titleFont, SpriteFont headerFont, SpriteFont normalFont, bool acceptInput, bool drawBehind)
			: base(acceptInput, drawBehind)
		{
			_headerFont = headerFont;
			_normalFont = normalFont;
			_titleFont = titleFont;
		}

		public CreditsScreenItem AddCreditsItem(string name)
		{
			CreditsScreenItem creditsScreenItem = new CreditsScreenItem(name);
			Items.Add(creditsScreenItem);
			totalLength += _normalFont.MeasureString(name).Y;
			return creditsScreenItem;
		}

		public CreditsScreenItem AddCreditsItem(string name, ItemTypes itemType)
		{
			CreditsScreenItem creditsScreenItem = new CreditsScreenItem(name, itemType);
			Items.Add(creditsScreenItem);
			switch (itemType)
			{
			case ItemTypes.Title:
				totalLength += _titleFont.MeasureString(name).Y;
				break;
			case ItemTypes.Header:
				totalLength += _headerFont.MeasureString(name).Y;
				break;
			default:
				totalLength += _normalFont.MeasureString(name).Y;
				break;
			}
			return creditsScreenItem;
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (controller.PressedButtons.B || controller.PressedButtons.Back || input.Keyboard.WasKeyPressed(Keys.Escape))
			{
				PopMe();
			}
			float num = ((input.Mouse.DeltaWheel != 0) ? ((float)input.Mouse.DeltaWheel) : controller.CurrentState.ThumbSticks.Left.Y);
			scrollRate = 40f + num * 200f;
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}

		public override void OnPushed()
		{
			_topItemDrawLocation = 720f;
			scrollRate = 40f;
			base.OnPushed();
		}

		public override void Update(DNAGame game, GameTime gameTime)
		{
			Rectangle titleSafeArea = game.GraphicsDevice.Viewport.TitleSafeArea;
			if (_topItemDrawLocation + totalLength < 0f)
			{
				_topItemDrawLocation = 0f - totalLength;
			}
			if (_topItemDrawLocation > 720f)
			{
				_topItemDrawLocation = 720f;
			}
			_topItemDrawLocation -= (float)gameTime.ElapsedGameTime.TotalSeconds * scrollRate;
			base.Update(game, gameTime);
		}

		public override void Draw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			Vector2 location = new Vector2(0f, _topItemDrawLocation);
			spriteBatch.Begin();
			for (int i = 0; i < Items.Count; i++)
			{
				SpriteFont spriteFont = ((Items[i].ItemType == ItemTypes.Title) ? _titleFont : ((Items[i].ItemType != ItemTypes.Header) ? _normalFont : _headerFont));
				Vector2 vector = spriteFont.MeasureString(Items[i].Name);
				if (location.Y > -50f && location.Y < 720f)
				{
					Color textColor = (Items[i].TextColor.HasValue ? Items[i].TextColor.Value : TextColor);
					if (LeftAligned)
					{
						location.X = titleSafeArea.Left;
					}
					else
					{
						location.X = (float)titleSafeArea.Center.X - vector.X / 2f;
					}
					spriteBatch.DrawOutlinedText(spriteFont, Items[i].Name, location, textColor, Color.Black, 1);
				}
				location.Y += vector.Y;
			}
			spriteBatch.End();
			base.Draw(device, spriteBatch, gameTime);
		}
	}
}
