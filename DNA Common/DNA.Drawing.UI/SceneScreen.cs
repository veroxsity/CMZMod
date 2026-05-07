using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.UI
{
	public class SceneScreen : Screen
	{
		private List<Scene> _scenes = new List<Scene>();

		private List<View> _views = new List<View>();

		private Dictionary<Scene, int> scenes = new Dictionary<Scene, int>();

		public float TimeFactor = 1f;

		public List<Scene> Scenes
		{
			get
			{
				return _scenes;
			}
		}

		public List<View> Views
		{
			get
			{
				return _views;
			}
		}

		public SceneScreen(bool acceptInput, bool drawBehind)
			: base(acceptInput, drawBehind)
		{
		}

		protected override void OnUpdate(DNAGame game, GameTime gameTime)
		{
			if (TimeFactor != 1f)
			{
				gameTime = new GameTime(TimeSpan.FromSeconds(gameTime.TotalGameTime.TotalSeconds * (double)TimeFactor), TimeSpan.FromSeconds(gameTime.ElapsedGameTime.TotalSeconds * (double)TimeFactor));
			}
			for (int i = 0; i < _scenes.Count; i++)
			{
				_scenes[i].Update(game, gameTime);
			}
			base.OnUpdate(game, gameTime);
		}

		protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
		{
			for (int i = 0; i < _views.Count; i++)
			{
				if (_views[i].Enabled)
				{
					_views[i].Draw(device, spriteBatch, gameTime);
				}
			}
			for (int j = 0; j < _scenes.Count; j++)
			{
				_scenes[j].AfterFrame();
			}
			base.OnDraw(device, spriteBatch, gameTime);
		}
	}
}
