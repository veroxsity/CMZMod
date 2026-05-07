using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class TextElement : UIElement
	{
		private bool _dirtyText = true;

		private StringBuilder _textToDraw = new StringBuilder();

		private bool _pulseDir;

		private TimeSpan _currenPulseTime = TimeSpan.FromSeconds(0.0);

		private TimeSpan _pulseTime = TimeSpan.FromSeconds(0.0);

		private float _pulseSize = 0.1f;

		private string _text = "<Text>";

		public SpriteFont Font;

		private Color _outLineColor = Color.Black;

		private int _outLineWidth = 2;

		public TimeSpan PulseTime
		{
			get
			{
				return _pulseTime;
			}
			set
			{
				_pulseTime = value;
			}
		}

		public float PulseSize
		{
			get
			{
				return _pulseSize;
			}
			set
			{
				_pulseSize = value;
			}
		}

		public string Text
		{
			get
			{
				return _text;
			}
			set
			{
				if (_text != value)
				{
					_text = value;
					_dirtyText = true;
				}
			}
		}

		public Color OutlineColor
		{
			get
			{
				return _outLineColor;
			}
			set
			{
				_outLineColor = value;
			}
		}

		public int OutlineWidth
		{
			get
			{
				return _outLineWidth;
			}
			set
			{
				_outLineWidth = value;
			}
		}

		public override Vector2 Size
		{
			get
			{
				return Font.MeasureString(Text);
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public TextElement(SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, int outlineWidth)
		{
			Text = text;
			Font = font;
			base.Location = position;
			base.Color = color;
			OutlineColor = Color.Black;
			OutlineWidth = outlineWidth;
		}

		public TextElement(SpriteFont font, string text, Vector2 position, Color color)
		{
			Text = text;
			Font = font;
			base.Location = position;
			base.Color = color;
		}

		public TextElement(string text, SpriteFont font)
		{
			Text = text;
			Font = font;
		}

		public TextElement(SpriteFont font)
		{
			Font = font;
		}

		protected virtual Color GetForColor(bool selected)
		{
			return base.Color;
		}

		protected virtual void ProcessText(string text, StringBuilder builder)
		{
			builder.Append(text);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime, bool selected)
		{
			if (_dirtyText)
			{
				_textToDraw.Length = 0;
				ProcessText(_text, _textToDraw);
				_dirtyText = false;
			}
			if (_pulseTime > TimeSpan.Zero)
			{
				if (_pulseDir)
				{
					_currenPulseTime += gameTime.ElapsedGameTime;
					if (_currenPulseTime > _pulseTime)
					{
						_currenPulseTime = _pulseTime;
						_pulseDir = !_pulseDir;
					}
				}
				else
				{
					_currenPulseTime -= gameTime.ElapsedGameTime;
					if (_currenPulseTime < TimeSpan.Zero)
					{
						_currenPulseTime = TimeSpan.Zero;
						_pulseDir = !_pulseDir;
					}
				}
				float scale = (float)(1.0 + (double)PulseSize * _currenPulseTime.TotalSeconds / _pulseTime.TotalSeconds);
				Vector2 vector = new Vector2(Size.X / 2f, Size.Y / 2f);
				if (OutlineWidth > 0)
				{
					spriteBatch.DrawOutlinedText(Font, _textToDraw, base.Location + vector, GetForColor(selected), OutlineColor, OutlineWidth, scale, 0f, vector);
				}
				else
				{
					spriteBatch.DrawString(Font, _textToDraw, base.Location + vector, GetForColor(selected), 0f, vector, scale, SpriteEffects.None, 1f);
				}
			}
			else if (OutlineWidth > 0)
			{
				spriteBatch.DrawOutlinedText(Font, _textToDraw, base.Location, GetForColor(selected), OutlineColor, OutlineWidth);
			}
			else
			{
				spriteBatch.DrawString(Font, _textToDraw, base.Location, GetForColor(selected));
			}
		}
	}
}
