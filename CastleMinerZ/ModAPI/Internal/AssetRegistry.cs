using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class AssetRegistry
    {
        private static Dictionary<string, string> _contentPaths = new Dictionary<string, string>();
        private static Dictionary<string, string> _pngPaths = new Dictionary<string, string>();
        private static Dictionary<string, Texture2D> _loadedTextures = new Dictionary<string, Texture2D>();
        private static List<string> _loggedFailures = new List<string>();

        public static void Register(string logicalName, string contentPath, string pngRelativePath)
        {
            if (string.IsNullOrEmpty(logicalName))
                return;
            if (!string.IsNullOrEmpty(contentPath))
                _contentPaths[logicalName] = contentPath;
            if (!string.IsNullOrEmpty(pngRelativePath))
                _pngPaths[logicalName] = pngRelativePath;
        }

        public static bool Exists(string assetName)
        {
            return !string.IsNullOrEmpty(assetName)
                && (_contentPaths.ContainsKey(assetName) || _pngPaths.ContainsKey(assetName));
        }

        public static string ResolveIcon(string modId, string fileName)
        {
            if (string.IsNullOrEmpty(modId) || string.IsNullOrEmpty(fileName))
                return null;
            string logicalName = modId + "/" + fileName;
            return Exists(logicalName) ? logicalName : null;
        }

        public static Texture2D LoadTexture(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                return null;

            Texture2D cached;
            if (_loadedTextures.TryGetValue(assetName, out cached))
                return cached;

            string pngPath;
            if (_pngPaths.TryGetValue(assetName, out pngPath))
            {
                Texture2D fromPng = TryLoadPng(pngPath, assetName);
                if (fromPng != null)
                    return fromPng;
                return null;
            }

            string contentPath;
            if (_contentPaths.TryGetValue(assetName, out contentPath))
            {
                try
                {
                    Texture2D texture = CastleMinerZGame.Instance.Content.Load<Texture2D>(contentPath);
                    _loadedTextures[assetName] = texture;
                    return texture;
                }
                catch (System.Exception ex)
                {
                    LogLoadFailure(assetName, ex.Message);
                }
            }

            try
            {
                Texture2D vanilla = CastleMinerZGame.Instance.Content.Load<Texture2D>(assetName);
                _loadedTextures[assetName] = vanilla;
                return vanilla;
            }
            catch
            {
                return null;
            }
        }

        private static Texture2D TryLoadPng(string pngRelativePath, string assetName)
        {
            string root = CastleMinerZGame.Instance.Content.RootDirectory;
            string normalized = pngRelativePath.Replace('\\', '/');

            string[] candidates = new string[]
            {
                Path.Combine(root, pngRelativePath),
                Path.Combine(root, normalized),
                normalized,
                "Content/" + normalized,
            };

            for (int i = 0; i < candidates.Length; i++)
            {
                string path = candidates[i];
                try
                {
                    if (File.Exists(path))
                    {
                        using (Stream stream = File.OpenRead(path))
                        {
                            return FinishPngLoad(stream, assetName);
                        }
                    }

                    using (Stream stream = TitleContainer.OpenStream(path.Replace('\\', '/')))
                    {
                        return FinishPngLoad(stream, assetName);
                    }
                }
                catch (System.Exception)
                {
                }
            }

            LogLoadFailure(assetName, "PNG not found at " + pngRelativePath);
            return null;
        }

        private static Texture2D FinishPngLoad(Stream stream, string assetName)
        {
            Texture2D texture = Texture2D.FromStream(
                CastleMinerZGame.Instance.GraphicsDevice, stream);
            _loadedTextures[assetName] = texture;
            return texture;
        }

        private static void LogLoadFailure(string assetName, string message)
        {
            if (_loggedFailures.Contains(assetName))
                return;
            _loggedFailures.Add(assetName);
            ModLog.Error("Failed to load mod asset '" + assetName + "': " + message);
        }

        /// <summary>Prefix a bare icon filename with the loading mod id when a matching asset exists.</summary>
        public static string ResolveItemIcon(string iconName)
        {
            if (string.IsNullOrEmpty(iconName))
                return iconName;
            if (iconName.IndexOf('/') >= 0)
                return iconName;

            string modId = ModRegistry.CurrentLoadingModId;
            if (modId == null)
                return iconName;

            string fullName = modId + "/" + iconName;
            return Exists(fullName) ? fullName : iconName;
        }
    }
}
