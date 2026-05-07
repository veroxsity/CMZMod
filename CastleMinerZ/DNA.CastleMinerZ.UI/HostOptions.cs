using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.UI
{
	public class HostOptions : MenuScreen
	{
		private CastleMinerZGame _game;

		private MenuItemElement pubItem;

		private MenuItemElement pvpItem;

		private MenuItemElement banListItem;

		public HostOptions(CastleMinerZGame game)
			: base(game._largeFont, false)
		{
			_game = game;
			ClickSound = "Click";
			SelectSound = "Click";
			AddMenuItem("Return To Game", HostOptionItems.Return);
			AddMenuItem("Kick Player", HostOptionItems.KickPlayer);
			AddMenuItem("Ban Player", HostOptionItems.BanPlayer);
			AddMenuItem("Restart", HostOptionItems.Restart);
			pubItem = AddMenuItem("Visibility:", HostOptionItems.Public);
			pvpItem = AddMenuItem("PVP:", HostOptionItems.PVP);
			banListItem = AddMenuItem("Clear Ban List", HostOptionItems.ClearBanList);
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			pubItem.Text = (_game.IsPublicGame ? "Visibility: Public" : "Visibility: Private");
			switch (_game.PVPState)
			{
			case CastleMinerZGame.PVPEnum.Everyone:
				pvpItem.Text = "PVP: Everyone";
				break;
			case CastleMinerZGame.PVPEnum.NotFriends:
				pvpItem.Text = "PVP: Non-Friends Only";
				break;
			case CastleMinerZGame.PVPEnum.Off:
				pvpItem.Text = "PVP: Off";
				break;
			}
			banListItem.Visible = _game.PlayerStats.BanList.Count > 0;
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			spriteBatch.Begin();
			Rectangle destinationRectangle = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
			spriteBatch.Draw(_game.DummyTexture, destinationRectangle, new Color(0f, 0f, 0f, 0.5f));
			spriteBatch.End();
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
