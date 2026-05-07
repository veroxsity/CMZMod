using System;
using System.Collections.Generic;
using System.Threading;
using DNA.Audio;
using DNA.Input;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DNA.Drawing.UI
{
	public class DialogScreen : Screen
	{
		public string Title;

		private string[] _options;

		protected Texture2D _bgImage;

		protected SpriteFont _font;

		public Vector2 TitlePadding = new Vector2(20f, 5f);

		public Vector2 DescriptionPadding = new Vector2(10f, 10f);

		public Vector2 OptionsPadding = new Vector2(10f, 10f);

		public Vector2 ButtonsPadding = new Vector2(10f, 10f);

		public Color TitleColor = Color.White;

		public Color DescriptionColor = Color.White;

		public Color OptionsColor = Color.White;

		public Color OptionsSelectedColor = Color.Red;

		public Color ButtonsColor = Color.White;

		public string ClickSound;

		public string OpenSound;

		private TextRegionElement _descriptionText;

		private List<string> optionLinesToPrint = new List<string>();

		private List<int> optionsStartLine = new List<int>();

		private bool _optionsLinesCalculated;

		protected int _optionSelected = -1;

		private int optionCurrentlySelected;

		private OneShotTimer flashTimer = new OneShotTimer(TimeSpan.FromSeconds(0.25));

		private bool selectedDirection;

		private bool JoystickMoved;

		public ThreadStart Callback;

		private Texture2D DummyTexture;

		private Rectangle _button0Loc = default(Rectangle);

		private Rectangle _button1Loc = default(Rectangle);

		private Rectangle _button2Loc = Rectangle.Empty;

		private Rectangle _button3Loc = Rectangle.Empty;

		protected string[] _buttonOptions;

		protected float _endOfDescriptionLoc;

		protected Rectangle _buttonAloc
		{
			get
			{
				return _button0Loc;
			}
		}

		protected Rectangle _buttonBloc
		{
			get
			{
				return _button1Loc;
			}
		}

		protected Rectangle _buttonYloc
		{
			get
			{
				return _button2Loc;
			}
		}

		protected Rectangle _buttonXloc
		{
			get
			{
				return _button3Loc;
			}
		}

		public int OptionSelected
		{
			get
			{
				return _optionSelected;
			}
		}

		public DialogScreen(string title, string description, string[] options, bool printCancel, Texture2D bgImage, SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			Title = title;
			_descriptionText = new TextRegionElement(description, font);
			_descriptionText.OutlineWidth = 1;
			if (options != null)
			{
				_options = new string[options.Length];
			}
			_options = options;
			_bgImage = bgImage;
			_font = font;
			if (options == null)
			{
				optionCurrentlySelected = 0;
			}
			_buttonOptions = new string[(!printCancel) ? 1 : 2];
			_buttonOptions[0] = " " + CommonResources.OK;
			if (printCancel)
			{
				_buttonOptions[1] = " " + CommonResources.Cancel;
			}
		}

		public DialogScreen(string title, string description, string[] buttonOptions, Texture2D bgImage, SpriteFont font, bool drawBehind)
			: base(true, drawBehind)
		{
			Title = title;
			_descriptionText = new TextRegionElement(description, font);
			_descriptionText.OutlineWidth = 1;
			_options = null;
			_bgImage = bgImage;
			_font = font;
			optionCurrentlySelected = 0;
			if (buttonOptions != null)
			{
				_buttonOptions = new string[buttonOptions.Length];
				_buttonOptions = buttonOptions;
			}
			else
			{
				_buttonOptions = new string[1];
				_buttonOptions[0] = " " + CommonResources.OK;
			}
		}

		public void SetButtonOptions(string[] buttonOptions)
		{
			_button0Loc = (_button1Loc = (_button2Loc = (_button3Loc = Rectangle.Empty)));
			if (buttonOptions != null)
			{
				_buttonOptions = new string[buttonOptions.Length];
				_buttonOptions = buttonOptions;
			}
			else
			{
				_buttonOptions = new string[1];
				_buttonOptions[0] = " " + CommonResources.OK;
			}
		}

		public override void OnPushed()
		{
			if (OpenSound != null)
			{
				SoundManager.Instance.PlayInstance(OpenSound);
			}
			float w = _bgImage.Width;
			GetOptionsLines(w);
			base.OnPushed();
		}

		private void GetOptionsLines(float w)
		{
			if (_optionsLinesCalculated || _options == null)
			{
				return;
			}
			_optionsLinesCalculated = true;
			float num = 0f;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < _options.Length; i++)
			{
				string text = _options[i];
				num = 0f;
				num2 = 0;
				num3 = 0;
				if (text == null)
				{
					continue;
				}
				for (int j = 0; j < text.Length; j++)
				{
					if (text[j] == '\n')
					{
						if (_font.MeasureString(text.Substring(num3, j - num3 + 1)).X > w - OptionsPadding.X * 2f)
						{
							optionLinesToPrint.Add(text.Substring(num3, num2 - num3));
							if (optionsStartLine.Count < i + 1)
							{
								optionsStartLine.Add(optionLinesToPrint.Count - 1);
							}
							optionLinesToPrint.Add(text.Substring(num2 + 1, j - num2));
						}
						else
						{
							optionLinesToPrint.Add(text.Substring(num3, j - num3));
							if (optionsStartLine.Count < i + 1)
							{
								optionsStartLine.Add(optionLinesToPrint.Count - 1);
							}
						}
						num3 = j + 1;
						num = 0f;
						num2 = j;
					}
					if (text[j] == ' ')
					{
						float x = _font.MeasureString(text.Substring(num2, j - num2)).X;
						num += x;
						if (num > w - OptionsPadding.X * 2f)
						{
							optionLinesToPrint.Add(text.Substring(num3, num2 - num3 + 1));
							if (optionsStartLine.Count < i + 1)
							{
								optionsStartLine.Add(optionLinesToPrint.Count - 1);
							}
							num3 = num2 + 1;
							num = x;
							num2 = j + 1;
						}
						else
						{
							num2 = j;
						}
					}
					if (j != text.Length - 1)
					{
						continue;
					}
					if (_font.MeasureString(text.Substring(num3, j - num3 + 1)).X > w - OptionsPadding.X * 2f)
					{
						optionLinesToPrint.Add(text.Substring(num3, num2 - num3 + 1));
						if (optionsStartLine.Count < i + 1)
						{
							optionsStartLine.Add(optionLinesToPrint.Count - 1);
						}
						optionLinesToPrint.Add(text.Substring(num2 + 1, j - num2));
					}
					else
					{
						optionLinesToPrint.Add(text.Substring(num3, j - num3 + 1));
						if (optionsStartLine.Count < i + 1)
						{
							optionsStartLine.Add(optionLinesToPrint.Count - 1);
						}
					}
				}
			}
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			Rectangle rectangle = new Rectangle(0, 0, 1280, 720);
			float num = _bgImage.Width;
			float num2 = _bgImage.Height;
			Rectangle destinationRectangle = new Rectangle((int)((float)rectangle.Center.X - num / 2f), (int)((float)rectangle.Center.Y - num2 / 2f), (int)num, (int)num2);
			if (DummyTexture == null)
			{
				DummyTexture = new Texture2D(device, 1, 1);
				DummyTexture.SetData(new Color[1] { Color.White });
			}
			spriteBatch.Begin();
			Rectangle destinationRectangle2 = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
			spriteBatch.Draw(DummyTexture, destinationRectangle2, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.Draw(_bgImage, destinationRectangle, Color.White);
			spriteBatch.DrawOutlinedText(_font, Title, new Vector2((float)destinationRectangle.X + TitlePadding.X, (float)destinationRectangle.Y + TitlePadding.Y), TitleColor, Color.Black, 1);
			float num3 = (float)destinationRectangle.Y + DescriptionPadding.Y;
			float num4 = _font.LineSpacing;
			float x = (float)destinationRectangle.X + DescriptionPadding.X;
			float num5 = (float)destinationRectangle.Y + DescriptionPadding.Y + (float)_font.LineSpacing;
			_descriptionText.Location = new Vector2(x, num5);
			_descriptionText.Size = new Vector2(num - DescriptionPadding.X * 2f, num2 - DescriptionPadding.Y - (float)_font.LineSpacing);
			_descriptionText.Draw(device, spriteBatch, gameTime, false);
			_endOfDescriptionLoc = num5 + num4;
			num3 = (float)(destinationRectangle.Y + destinationRectangle.Height) - OptionsPadding.Y - _font.MeasureString(Title).Y * (float)(optionLinesToPrint.Count + 2) - ButtonsPadding.Y;
			for (int i = 0; i < optionLinesToPrint.Count; i++)
			{
				if (i >= optionsStartLine[optionCurrentlySelected])
				{
					if (optionCurrentlySelected == _options.Length - 1 || i < optionsStartLine[optionCurrentlySelected + 1])
					{
						if (i == optionsStartLine[optionCurrentlySelected])
						{
							flashTimer.Update(gameTime.ElapsedGameTime);
							if (flashTimer.Expired)
							{
								flashTimer.Reset();
								selectedDirection = !selectedDirection;
							}
						}
						Color textColor = ((!selectedDirection) ? Color.Lerp(OptionsSelectedColor, OptionsColor, flashTimer.PercentComplete) : Color.Lerp(OptionsColor, OptionsSelectedColor, flashTimer.PercentComplete));
						num3 += _font.MeasureString(Title).Y;
						spriteBatch.DrawOutlinedText(_font, optionLinesToPrint[i], new Vector2((float)destinationRectangle.X + OptionsPadding.X, num3), textColor, Color.Black, 1);
					}
					else
					{
						num3 += _font.MeasureString(Title).Y;
						spriteBatch.DrawOutlinedText(_font, optionLinesToPrint[i], new Vector2((float)destinationRectangle.X + OptionsPadding.X, num3), OptionsColor, Color.Black, 1);
					}
				}
				else
				{
					num3 += _font.MeasureString(Title).Y;
					spriteBatch.DrawOutlinedText(_font, optionLinesToPrint[i], new Vector2((float)destinationRectangle.X + OptionsPadding.X, num3), OptionsColor, Color.Black, 1);
				}
			}
			Vector2 vector = _font.MeasureString(_buttonOptions[0]);
			float num6 = vector.Y / (float)ControllerImages.A.Height;
			int num7 = (int)((float)ControllerImages.A.Width * num6);
			num3 = (float)(destinationRectangle.Y + destinationRectangle.Height) - ButtonsPadding.Y - _font.MeasureString(Title).Y;
			spriteBatch.Draw(ControllerImages.A, new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X), (int)num3, num7, (int)vector.Y), Color.White);
			_button0Loc = new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)num7), (int)num3, (int)vector.X, (int)vector.Y);
			spriteBatch.DrawOutlinedText(_font, _buttonOptions[0], new Vector2((float)destinationRectangle.X + ButtonsPadding.X + (float)num7, num3), ButtonsColor, Color.Black, 1);
			if (_buttonOptions.Length > 1)
			{
				vector = _font.MeasureString(_buttonOptions[1]);
				num6 = vector.Y / (float)ControllerImages.B.Height;
				num7 = (int)((float)ControllerImages.B.Width * num6);
				spriteBatch.Draw(ControllerImages.B, new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)num7 + _font.MeasureString(_buttonOptions[0]).X + 10f), (int)num3, num7, (int)vector.Y), Color.White);
				_button1Loc = new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 2) + _font.MeasureString(_buttonOptions[0]).X + 10f), (int)num3, (int)vector.X, (int)vector.Y);
				spriteBatch.DrawOutlinedText(_font, _buttonOptions[1], new Vector2((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 2) + _font.MeasureString(_buttonOptions[0]).X + 10f, num3), ButtonsColor, Color.Black, 1);
				if (_buttonOptions.Length > 2)
				{
					vector = _font.MeasureString(_buttonOptions[2]);
					num6 = vector.Y / (float)ControllerImages.Y.Height;
					num7 = (int)((float)ControllerImages.Y.Width * num6);
					spriteBatch.Draw(ControllerImages.Y, new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 2) + _font.MeasureString(_buttonOptions[0] + _buttonOptions[1]).X + 20f), (int)num3, num7, (int)vector.Y), Color.White);
					_button2Loc = new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 3) + _font.MeasureString(_buttonOptions[0] + _buttonOptions[1]).X + 20f), (int)num3, (int)vector.X, (int)vector.Y);
					spriteBatch.DrawOutlinedText(_font, _buttonOptions[2], new Vector2((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 3) + _font.MeasureString(_buttonOptions[0] + _buttonOptions[1]).X + 20f, num3), ButtonsColor, Color.Black, 1);
					if (_buttonOptions.Length > 3)
					{
						vector = _font.MeasureString(_buttonOptions[3]);
						num6 = vector.Y / (float)ControllerImages.X.Height;
						num7 = (int)((float)ControllerImages.X.Width * num6);
						spriteBatch.Draw(ControllerImages.X, new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 3) + _font.MeasureString(_buttonOptions[0]).X + _font.MeasureString(_buttonOptions[1]).X + _font.MeasureString(_buttonOptions[2]).X + 30f), (int)num3, num7, (int)vector.Y), Color.White);
						_button3Loc = new Rectangle((int)((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 4) + _font.MeasureString(_buttonOptions[0]).X + _font.MeasureString(_buttonOptions[1]).X + _font.MeasureString(_buttonOptions[2]).X + 30f), (int)num3, (int)vector.X, (int)vector.Y);
						spriteBatch.DrawOutlinedText(_font, _buttonOptions[3], new Vector2((float)destinationRectangle.X + ButtonsPadding.X + (float)(num7 * 4) + _font.MeasureString(_buttonOptions[0]).X + _font.MeasureString(_buttonOptions[1]).X + _font.MeasureString(_buttonOptions[2]).X + 30f, num3), ButtonsColor, Color.Black, 1);
					}
				}
			}
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}

		protected override bool OnPlayerInput(InputManager input, GameController controller, KeyboardInput chatpad, GameTime gameTime)
		{
			if (controller.PressedButtons.A || controller.PressedButtons.Start || input.Keyboard.WasKeyPressed(Keys.Enter) || (input.Mouse.LeftButtonPressed && _button0Loc.Contains(input.Mouse.Position)))
			{
				_optionSelected = optionCurrentlySelected;
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
				if (Callback != null)
				{
					Callback();
				}
			}
			if (_buttonOptions.Length > 2 && (controller.PressedButtons.Y || (input.Mouse.LeftButtonPressed && _button2Loc.Contains(input.Mouse.Position))))
			{
				_optionSelected = 2;
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
				if (Callback != null)
				{
					Callback();
				}
			}
			if (_buttonOptions.Length > 3 && (controller.PressedButtons.X || (input.Mouse.LeftButtonPressed && _button3Loc.Contains(input.Mouse.Position))))
			{
				_optionSelected = 3;
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
				if (Callback != null)
				{
					Callback();
				}
			}
			if (controller.PressedButtons.Back || controller.PressedButtons.B || input.Keyboard.WasKeyPressed(Keys.Escape) || (input.Mouse.LeftButtonPressed && _button1Loc.Contains(input.Mouse.Position)))
			{
				_optionSelected = -1;
				if (ClickSound != null)
				{
					SoundManager.Instance.PlayInstance(ClickSound);
				}
				PopMe();
				if (Callback != null)
				{
					Callback();
				}
			}
			if (controller.CurrentState.ThumbSticks.Left.Y > 0f || controller.CurrentState.IsButtonDown(Buttons.DPadUp) || input.Keyboard.IsKeyDown(Keys.Up))
			{
				if (_options != null && !JoystickMoved)
				{
					JoystickMoved = true;
					if (optionCurrentlySelected > 0)
					{
						optionCurrentlySelected--;
						if (ClickSound != null)
						{
							SoundManager.Instance.PlayInstance(ClickSound);
						}
					}
				}
			}
			else if (controller.CurrentState.ThumbSticks.Left.Y < 0f || controller.CurrentState.IsButtonDown(Buttons.DPadDown) || input.Keyboard.IsKeyDown(Keys.Down))
			{
				if (_options != null && !JoystickMoved)
				{
					JoystickMoved = true;
					if (optionCurrentlySelected < _options.Length - 1)
					{
						optionCurrentlySelected++;
						if (ClickSound != null)
						{
							SoundManager.Instance.PlayInstance(ClickSound);
						}
					}
				}
			}
			else if (JoystickMoved)
			{
				JoystickMoved = false;
			}
			return base.OnPlayerInput(input, controller, chatpad, gameTime);
		}
	}
}
