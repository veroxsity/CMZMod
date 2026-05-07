using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class ListSettingItem : SettingItemElement
	{
		private int _index;

		private List<object> _items;

		public int Index
		{
			get
			{
				return _index;
			}
			set
			{
				_index = value;
			}
		}

		public object CurrentItem
		{
			get
			{
				return _items[_index];
			}
		}

		public ListSettingItem(string text, List<object> items, int defaultIndex)
			: base(text)
		{
			_index = defaultIndex;
			_items = items;
		}

		public override void Clicked()
		{
			_index++;
			if (_index >= _items.Count)
			{
				_index = 0;
			}
		}

		public override void Decreased()
		{
			_index--;
			if (_index < 0)
			{
				_index = 0;
			}
		}

		public override void Increased()
		{
			_index++;
			if (_index >= _items.Count)
			{
				_index = _items.Count - 1;
			}
		}

		public override void OnDraw(DNAGame _game, GraphicsDevice device, SpriteBatch spriteBatch, SpriteFont font, Color textColor, Color outlineColor, int outlineWidth, float yLoc)
		{
			Rectangle titleSafeArea = device.Viewport.TitleSafeArea;
			Vector2 vector = font.MeasureString(">");
			float num = (float)(titleSafeArea.Right - titleSafeArea.Center.X - 120) - vector.X * 2f;
			float num2 = titleSafeArea.Center.X + 50;
			string text = CurrentItem.ToString();
			float x = num2 + vector.X + 10f + num / 2f - font.MeasureString(text).X / 2f;
			spriteBatch.DrawOutlinedText(font, "<", new Vector2(num2, yLoc), textColor, outlineColor, outlineWidth);
			spriteBatch.DrawOutlinedText(font, ">", new Vector2((float)(titleSafeArea.Right - 50) - vector.X, yLoc), textColor, outlineColor, outlineWidth);
			spriteBatch.DrawOutlinedText(font, text, new Vector2(x, yLoc), textColor, outlineColor, outlineWidth);
			base.OnDraw(_game, device, spriteBatch, font, textColor, outlineColor, outlineWidth, yLoc);
		}
	}
}
