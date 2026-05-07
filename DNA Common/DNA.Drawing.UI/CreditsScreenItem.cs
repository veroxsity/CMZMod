using Microsoft.Xna.Framework;

namespace DNA.Drawing.UI
{
	public class CreditsScreenItem
	{
		public string Name;

		public ItemTypes ItemType = ItemTypes.Normal;

		public Color? TextColor = null;

		public CreditsScreenItem(string name, ItemTypes itemType)
		{
			Name = name;
			ItemType = itemType;
		}

		public CreditsScreenItem(string name)
		{
			Name = name;
		}
	}
}
