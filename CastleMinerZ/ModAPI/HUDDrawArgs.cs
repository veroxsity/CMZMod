using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI
{
    public class HUDDrawArgs
    {
        public GraphicsDevice Device;
        public SpriteBatch SpriteBatch;
        public GameTime GameTime;
        public Rectangle Bounds;
        public Player Player;

        /// <summary>Y coordinate to draw below the vanilla "Distance - Max" HUD block (title-safe).</summary>
        public float BelowDistanceReadoutY;
    }
}
