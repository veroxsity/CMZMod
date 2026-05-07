using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class TextRegionElement : TextElement
	{
		private Vector2 _size;

		public override Vector2 Size
		{
			get
			{
				return _size;
			}
			set
			{
				_size = value;
			}
		}

		public TextRegionElement(SpriteFont font)
			: base(font)
		{
		}

		public TextRegionElement(string text, SpriteFont font)
			: base(text, font)
		{
		}

		protected override void ProcessText(string text, StringBuilder builder)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			float x = _size.X;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				char next = '\0';
				if (i != text.Length - 1)
				{
					next = text[i + 1];
				}
				stringBuilder.Append(c);
				if (_isSeparator(c, next))
				{
					if (Font.MeasureString(stringBuilder).X > x)
					{
						builder.Append(stringBuilder2);
						builder.Append('\n');
						stringBuilder.Remove(0, stringBuilder2.Length);
					}
					stringBuilder2.Length = 0;
					stringBuilder2.Append(stringBuilder);
				}
			}
			if (Font.MeasureString(stringBuilder).X > x)
			{
				builder.Append(stringBuilder2);
				builder.Append('\n');
				stringBuilder.Remove(0, stringBuilder2.Length);
			}
			builder.Append(stringBuilder);
		}

		private bool _isSeparator(char c, char next)
		{
			if (char.IsSeparator(c))
			{
				return true;
			}
			if ((c >= '\u3040' && c <= 'ゕ') || (c >= '゠' && c <= 'ヿ') || (c >= '一' && c <= '龿'))
			{
				switch (c)
				{
				case (char)36:
				case (char)40:
				case (char)91:
				case (char)92:
				case (char)123:
				case (char)162:
				case (char)33125:
				case (char)33127:
				case (char)33129:
				case (char)33131:
				case (char)33133:
				case (char)33135:
				case (char)33137:
				case (char)33139:
				case (char)33141:
				case (char)33143:
				case (char)33145:
				case (char)33167:
				case (char)33168:
				case (char)33170:
					return false;
				default:
					switch (next)
					{
					case (char)33:
					case (char)37:
					case (char)41:
					case (char)44:
					case (char)46:
					case (char)63:
					case (char)93:
					case (char)125:
					case (char)161:
					case (char)163:
					case (char)164:
					case (char)165:
					case (char)167:
					case (char)168:
					case (char)169:
					case (char)170:
					case (char)171:
					case (char)172:
					case (char)173:
					case (char)174:
					case (char)175:
					case (char)176:
					case (char)222:
					case (char)223:
					case (char)33089:
					case (char)33090:
					case (char)33091:
					case (char)33092:
					case (char)33093:
					case (char)33094:
					case (char)33095:
					case (char)33096:
					case (char)33097:
					case (char)33098:
					case (char)33099:
					case (char)33106:
					case (char)33107:
					case (char)33108:
					case (char)33109:
					case (char)33112:
					case (char)33115:
					case (char)33126:
					case (char)33128:
					case (char)33130:
					case (char)33132:
					case (char)33134:
					case (char)33136:
					case (char)33138:
					case (char)33140:
					case (char)33142:
					case (char)33144:
					case (char)33146:
					case (char)33163:
					case (char)33164:
					case (char)33165:
					case (char)33166:
					case (char)33169:
					case (char)33171:
					case (char)33265:
					case (char)33439:
					case (char)33441:
					case (char)33443:
					case (char)33445:
					case (char)33447:
					case (char)33473:
					case (char)33505:
					case (char)33507:
					case (char)33509:
					case (char)33516:
					case (char)33600:
					case (char)33602:
					case (char)33604:
					case (char)33606:
					case (char)33608:
					case (char)33634:
					case (char)33667:
					case (char)33669:
					case (char)33671:
					case (char)33678:
					case (char)33685:
					case (char)33686:
						return false;
					default:
						return true;
					}
				}
			}
			return false;
		}
	}
}
