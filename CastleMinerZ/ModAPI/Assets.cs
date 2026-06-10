using DNA.CastleMinerZ.ModAPI.Internal;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI
{
    /// <summary>Load mod-owned textures staged into Content/ModAssets/ at build time.</summary>
    public static class Assets
    {
        /// <summary>True if a mod asset was registered at build time (mod-id/name).</summary>
        public static bool Exists(string assetName)
        {
            return AssetRegistry.Exists(assetName);
        }

        /// <summary>
        /// Loads a mod asset or vanilla Content texture by name.
        /// Mod assets use the form "mod-id/asset-name".
        /// </summary>
        public static Texture2D LoadTexture(string assetName)
        {
            return AssetRegistry.LoadTexture(assetName);
        }

        /// <summary>Returns "mod-id/fileName" when that asset exists, otherwise null.</summary>
        public static string ResolveIcon(string modId, string fileName)
        {
            return AssetRegistry.ResolveIcon(modId, fileName);
        }
    }
}
