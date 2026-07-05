using System;
using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Terrain;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    /// <summary>
    /// Patches CMZ terrain atlases from mod PNGs (Minecraft-style texture packs).
    ///
    /// Verified layout of Terrain\Textures.xnb (retail 1.6.3, Xbox 360):
    ///   [0] _diffuseAlpha  1024x1024 Color mips=1  - near atlas, 8x8 grid of 128px tiles.
    ///                                                Tile interiors are plain RGB with A=255;
    ///                                                ~3px alpha-0 gutters between tiles.
    ///   [1] _normalSpec    1024x1024 Color mips=1  - normal+spec for fancy-lit tiles.
    ///   [2] _metalLight    1024x1024 Color mips=1  - never touched here.
    ///   [3] _mipMapNormals  512x512  Color mips=10 - far-LOD normal/spec, 8-wide 64px cells.
    ///   [4] _mipMapDiffuse  512x512  Color mips=10 - far-LOD diffuse, 8-wide 64px cells.
    ///
    /// Two hard-won rules. Violating either renders BLACK at distance and in the hotbar,
    /// because the far terrain pass AND BlockEntity's UI passes (EntityNoWaterTechnique
    /// passes 2/3) consume the mip atlases:
    ///
    ///  1. The mip atlases' ALPHA channel is structured shader data, NOT opacity
    ///     (_mipMapDiffuse alpha averages 0.09-0.53 per tile: dirt .087, rock .368,
    ///     gold wall .531). Patch RGB only; preserve the vanilla alpha of every texel.
    ///  2. Never recreate these textures or regenerate their GPU mip chains from level 0 -
    ///     lower levels are authored content. Patch the EXISTING textures in place with
    ///     per-level SetData so untouched texels stay bit-identical to retail.
    ///
    /// Far sampling (verified on hardware): the far pass displays the FULL 64px cell of the
    /// mip atlas. Writing a smaller "window" copy into the cell shows up in-game as a mini
    /// texture in the tile's top-left corner - so each patched cell gets a straight 64px
    /// downsample of its tile plus a small RGB-only edge bleed, nothing else. (The 0.0625
    /// constant in the VertexUVs setup is NOT the far sample span; whatever it drives, it
    /// does not shrink the visible cell area.)
    /// </summary>
    public static class BlockTextureRegistry
    {
        private const int TilesPerRow = 8;
        private const int TileBorderPixels = 3;

        private struct PatchEntry
        {
            public string Alias;
            public int TileIndex;
            public string PngRelativePath;
        }

        // Minecraft grass/foliage PNGs are grayscale masks; MC tints them in a shader. CMZ has no
        // biome colormap pass, so we multiply by a fixed plains-green at patch time.
        private static readonly Color GrassTint = new Color(145, 189, 89);
        private static readonly Color FoliageTint = new Color(72, 181, 72);

        private static readonly int[] FancyLitTiles = { 4, 6, 7, 8, 9, 10, 21, 22, 23, 24 };

        private static readonly List<PatchEntry> _patches = new List<PatchEntry>();
        private static readonly Dictionary<string, int> _aliasToTile = BuildAliasMap();
        private static bool _applied;

        public static void Register(string alias, string pngRelativePath)
        {
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(pngRelativePath))
                return;

            string key = alias.ToLowerInvariant();
            int tileIndex;
            if (!_aliasToTile.TryGetValue(key, out tileIndex))
            {
                ModLog.Warn("BlockTextureRegistry: unknown alias '" + alias + "'");
                return;
            }

            _patches.Add(new PatchEntry { Alias = key, TileIndex = tileIndex, PngRelativePath = pngRelativePath });
        }

        /// <summary>Safe to call again if the first Apply() ran before content was ready.</summary>
        public static void EnsureApplied()
        {
            Apply();
        }

        public static void Apply()
        {
            if (_applied || _patches.Count == 0)
                return;

            BlockTerrain terrain = BlockTerrain.Instance;
            if (terrain == null || terrain._diffuseAlpha == null)
            {
                ModLog.Warn("BlockTextureRegistry: terrain not ready");
                return;
            }

            GraphicsDevice device = CastleMinerZGame.Instance.GraphicsDevice;
            int diffuseWidth = terrain._diffuseAlpha.Width;
            int diffuseHeight = terrain._diffuseAlpha.Height;
            int diffuseTileSize = diffuseWidth / TilesPerRow;
            if (diffuseTileSize <= 0)
            {
                ModLog.Error("BlockTextureRegistry: invalid diffuse atlas size");
                return;
            }

            Color[] diffusePixels = new Color[diffuseWidth * diffuseHeight];
            terrain._diffuseAlpha.GetData(diffusePixels);

            var patchedTiles = new List<int>();
            int patched = 0;
            for (int i = 0; i < _patches.Count; i++)
            {
                PatchEntry entry = _patches[i];
                if (!BlitPngIntoAtlas(device, entry.Alias, entry.PngRelativePath, diffusePixels,
                        diffuseWidth, diffuseTileSize, entry.TileIndex))
                    continue;

                // RGB-only bleed keeps the vanilla alpha-0 gutter structure intact.
                ApplyTileEdgeBleed(diffusePixels, diffuseWidth, diffuseTileSize, entry.TileIndex,
                    TileBorderPixels, true);
                if (!patchedTiles.Contains(entry.TileIndex))
                    patchedTiles.Add(entry.TileIndex);
                patched++;
            }

            if (patched == 0)
            {
                ModLog.Warn("BlockTextureRegistry: no PNGs applied");
                return;
            }

            try
            {
                // SetData on a resource bound to the device throws on Xbox; unbind everything first.
                UnbindDeviceTextures(device);

                // Near atlas: update the live texture in place (vanilla has no mip chain here).
                terrain._diffuseAlpha.SetData(diffusePixels);

                PatchMipDiffuseInPlace(terrain, diffusePixels, diffuseWidth, diffuseTileSize, patchedTiles);
                PatchNormalAtlasesInPlace(terrain, patchedTiles);

                RebindTerrainTextures(terrain);
            }
            catch (Exception ex)
            {
                ModLog.Error("BlockTextureRegistry: in-place patch failed: " + ex.Message);
                return;
            }

            _applied = true;

            int mipW = terrain._mipMapDiffuse != null ? terrain._mipMapDiffuse.Width : 0;
            int mipH = terrain._mipMapDiffuse != null ? terrain._mipMapDiffuse.Height : 0;
            ModLog.Info("BlockTextureRegistry: applied " + patched + " tile(s) in place; diffuse "
                + diffuseWidth + "x" + diffuseHeight + " tile=" + diffuseTileSize
                + " mip=" + mipW + "x" + mipH);
        }

        private static void UnbindDeviceTextures(GraphicsDevice device)
        {
            for (int i = 0; i < 16; i++)
                device.Textures[i] = null;
        }

        private static void RebindTerrainTextures(BlockTerrain terrain)
        {
            if (terrain._effect == null)
                return;

            terrain._effect.Parameters["DiffuseAlphaTexture"].SetValue(terrain._diffuseAlpha);
            if (terrain._normalSpec != null)
                terrain._effect.Parameters["NormalSpecTexture"].SetValue(terrain._normalSpec);
            if (terrain._mipMapNormals != null)
                terrain._effect.Parameters["MipMapSpecularTexture"].SetValue(terrain._mipMapNormals);
            if (terrain._mipMapDiffuse != null)
                terrain._effect.Parameters["MipMapDiffuseTexture"].SetValue(terrain._mipMapDiffuse);
        }

        // ------------------------------------------------------------------
        // Mip diffuse atlas (far LOD + hotbar/UI)
        // ------------------------------------------------------------------

        private static void PatchMipDiffuseInPlace(BlockTerrain terrain, Color[] diffusePixels,
            int diffuseWidth, int diffuseTileSize, List<int> patchedTiles)
        {
            Texture2D mipTexture = terrain._mipMapDiffuse;
            if (mipTexture == null || patchedTiles.Count == 0)
                return;

            int mipWidth = mipTexture.Width;
            int mipHeight = mipTexture.Height;
            int mipTileSize = mipWidth / TilesPerRow;
            if (mipTileSize <= 0)
                return;

            Color[] mipPixels = new Color[mipWidth * mipHeight];
            mipTexture.GetData(mipPixels);

            int mipBleedBorder = Math.Max(1, TileBorderPixels * mipTileSize / Math.Max(1, diffuseTileSize));

            for (int i = 0; i < patchedTiles.Count; i++)
            {
                int tileIndex = patchedTiles[i];
                int tileX = tileIndex & (TilesPerRow - 1);
                int tileY = tileIndex / TilesPerRow;

                // The far pass displays the whole 64px cell: straight downsample of the tile.
                // RGB only - the cell keeps its vanilla alpha (shader data).
                DownsampleRegion(diffusePixels, diffuseWidth,
                    tileX * diffuseTileSize, tileY * diffuseTileSize, diffuseTileSize, diffuseTileSize,
                    mipPixels, mipWidth, tileX * mipTileSize, tileY * mipTileSize,
                    mipTileSize, mipTileSize, true);

                ApplyTileEdgeBleed(mipPixels, mipWidth, mipTileSize, tileIndex, mipBleedBorder, true);
            }

            mipTexture.SetData(0, null, mipPixels, 0, mipPixels.Length);

            PatchLowerMipLevels(mipTexture, mipPixels, mipWidth, mipTileSize, patchedTiles, true);

            ModLog.Info("BlockTextureRegistry: mip diffuse patched in place (" + patchedTiles.Count
                + " full cell(s), bleed=" + mipBleedBorder + "px)");
        }

        /// <summary>
        /// Update the patched cells on GPU mip levels 1+ from our patched level 0. With
        /// preserveDstAlpha, RGB is replaced and each level keeps its own authored alpha.
        /// Texels outside the patched cell rects are never read or written.
        /// </summary>
        private static void PatchLowerMipLevels(Texture2D texture, Color[] level0, int level0Width,
            int level0TileSize, List<int> patchedTiles, bool preserveDstAlpha)
        {
            for (int level = 1; level < texture.LevelCount; level++)
            {
                int cellSize = level0TileSize >> level;
                if (cellSize < 1)
                    break;

                for (int i = 0; i < patchedTiles.Count; i++)
                {
                    int tileIndex = patchedTiles[i];
                    int tileX = tileIndex & (TilesPerRow - 1);
                    int tileY = tileIndex / TilesPerRow;

                    Rectangle rect = new Rectangle(tileX * cellSize, tileY * cellSize, cellSize, cellSize);
                    Color[] cell = new Color[cellSize * cellSize];
                    if (preserveDstAlpha)
                        texture.GetData(level, rect, cell, 0, cell.Length);

                    DownsampleRegion(level0, level0Width,
                        tileX * level0TileSize, tileY * level0TileSize, level0TileSize, level0TileSize,
                        cell, cellSize, 0, 0, cellSize, cellSize, preserveDstAlpha);

                    texture.SetData(level, rect, cell, 0, cell.Length);
                }
            }
        }

        // ------------------------------------------------------------------
        // Normal/spec atlases (fancy-lit tiles only)
        // ------------------------------------------------------------------

        /// <summary>
        /// Fancy-lit blocks (ores, walls) sample _normalSpec / _mipMapNormals. Copy a flat
        /// template from rock (tile 5) over patched fancy tiles so their new diffuse is not
        /// lit by stale normal data (shows as black). The template is coherent RGBA
        /// (normal + spec), so unlike the diffuse mip patch we paste all four channels -
        /// but still strictly in place on the existing textures.
        /// </summary>
        private static void PatchNormalAtlasesInPlace(BlockTerrain terrain, List<int> patchedTiles)
        {
            if (terrain._normalSpec == null || patchedTiles.Count == 0)
                return;

            int normalWidth = terrain._normalSpec.Width;
            int normalHeight = terrain._normalSpec.Height;
            int normalTileSize = normalWidth / TilesPerRow;
            if (normalTileSize <= 0)
                return;

            const int templateTile = 5;

            Color[] normalPixels = new Color[normalWidth * normalHeight];
            terrain._normalSpec.GetData(normalPixels);
            Color[] templateTilePixels = CopyTileRegion(normalPixels, normalWidth, normalTileSize, templateTile);

            int normalPatched = 0;
            for (int i = 0; i < patchedTiles.Count; i++)
            {
                int tileIndex = patchedTiles[i];
                if (!IsFancyLitTile(tileIndex))
                    continue;

                PasteTileRegion(normalPixels, normalWidth, normalTileSize, tileIndex, templateTilePixels);
                ApplyTileEdgeBleed(normalPixels, normalWidth, normalTileSize, tileIndex,
                    TileBorderPixels, false);
                normalPatched++;
            }

            if (normalPatched == 0)
                return;

            terrain._normalSpec.SetData(normalPixels);

            Texture2D mipNormals = terrain._mipMapNormals;
            if (mipNormals != null)
            {
                int mipWidth = mipNormals.Width;
                int mipHeight = mipNormals.Height;
                int mipTileSize = mipWidth / TilesPerRow;
                if (mipTileSize > 0)
                {
                    Color[] mipPixels = new Color[mipWidth * mipHeight];
                    mipNormals.GetData(mipPixels);

                    int mipBleedBorder = Math.Max(1, TileBorderPixels * mipTileSize / Math.Max(1, normalTileSize));
                    var fancyPatched = new List<int>();

                    for (int i = 0; i < patchedTiles.Count; i++)
                    {
                        int tileIndex = patchedTiles[i];
                        if (!IsFancyLitTile(tileIndex))
                            continue;

                        int tileX = tileIndex & (TilesPerRow - 1);
                        int tileY = tileIndex / TilesPerRow;

                        // Whole cell from the template (coherent RGBA normal+spec data).
                        DownsampleRegion(templateTilePixels, normalTileSize, 0, 0, normalTileSize, normalTileSize,
                            mipPixels, mipWidth, tileX * mipTileSize, tileY * mipTileSize,
                            mipTileSize, mipTileSize, false);
                        ApplyTileEdgeBleed(mipPixels, mipWidth, mipTileSize, tileIndex, mipBleedBorder, false);

                        fancyPatched.Add(tileIndex);
                    }

                    mipNormals.SetData(0, null, mipPixels, 0, mipPixels.Length);
                    PatchLowerMipLevels(mipNormals, mipPixels, mipWidth, mipTileSize, fancyPatched, false);
                }
            }

            ModLog.Info("BlockTextureRegistry: patched " + normalPatched + " normal/spec tile(s) in place");
        }

        // ------------------------------------------------------------------
        // Pixel helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Box-filter an arbitrary source region into a destination region. With
        /// preserveDstAlpha, only RGB is written and the destination alpha is kept
        /// (the mip atlases' alpha channel is shader data we must not disturb).
        /// </summary>
        private static void DownsampleRegion(Color[] src, int srcStride, int srcX0, int srcY0, int srcW, int srcH,
            Color[] dst, int dstStride, int dstX0, int dstY0, int dstW, int dstH, bool preserveDstAlpha)
        {
            if (srcW <= 0 || srcH <= 0 || dstW <= 0 || dstH <= 0)
                return;

            for (int y = 0; y < dstH; y++)
            {
                int sy0 = srcY0 + y * srcH / dstH;
                int sy1 = srcY0 + (y + 1) * srcH / dstH;
                if (sy1 <= sy0)
                    sy1 = sy0 + 1;

                for (int x = 0; x < dstW; x++)
                {
                    int sx0 = srcX0 + x * srcW / dstW;
                    int sx1 = srcX0 + (x + 1) * srcW / dstW;
                    if (sx1 <= sx0)
                        sx1 = sx0 + 1;

                    Color filtered = BoxFilterRegion(src, srcStride, sx0, sy0, sx1, sy1);
                    int di = (dstY0 + y) * dstStride + dstX0 + x;
                    WritePixel(dst, di, filtered, preserveDstAlpha);
                }
            }
        }

        private static void WritePixel(Color[] pixels, int index, Color src, bool preserveAlpha)
        {
            if (preserveAlpha)
            {
                Color d = pixels[index];
                pixels[index] = new Color(src.R, src.G, src.B, d.A);
            }
            else
            {
                pixels[index] = src;
            }
        }

        private static bool BlitPngIntoAtlas(GraphicsDevice device, string alias, string pngRelativePath,
            Color[] atlasPixels, int atlasWidth, int tileSize, int tileIndex)
        {
            Color? tint = GetColormapTint(alias);
            bool isFoliage = IsFoliageAlias(alias);

            using (Stream stream = OpenPngStream(pngRelativePath))
            {
                if (stream == null)
                {
                    ModLog.Warn("BlockTextureRegistry: missing PNG " + pngRelativePath);
                    return false;
                }

                Texture2D source;
                try
                {
                    source = Texture2D.FromStream(device, stream);
                }
                catch (Exception ex)
                {
                    ModLog.Warn("BlockTextureRegistry: failed to load " + pngRelativePath + ": " + ex.Message);
                    return false;
                }

                Color[] srcPixels = new Color[source.Width * source.Height];
                int srcWidth = source.Width;
                int srcHeight = source.Height;
                source.GetData(srcPixels);
                source.Dispose();

                int tileX = tileIndex & (TilesPerRow - 1);
                int tileY = tileIndex / TilesPerRow;
                int destX0 = tileX * tileSize;
                int destY0 = tileY * tileSize;
                bool nearest = srcWidth == tileSize && srcHeight == tileSize;

                for (int y = 0; y < tileSize; y++)
                {
                    int srcY = nearest ? y : (srcHeight <= 1 ? 0 : y * (srcHeight - 1) / Math.Max(1, tileSize - 1));
                    for (int x = 0; x < tileSize; x++)
                    {
                        int srcX = nearest ? x : (srcWidth <= 1 ? 0 : x * (srcWidth - 1) / Math.Max(1, tileSize - 1));
                        Color src = srcPixels[srcY * srcWidth + srcX];
                        int destIndex = (destY0 + y) * atlasWidth + destX0 + x;
                        byte atlasAlpha = atlasPixels[destIndex].A;

                        Color pixel;
                        if (isFoliage)
                        {
                            if (src.A == 0)
                            {
                                atlasPixels[destIndex] = new Color(0, 0, 0, 0);
                                continue;
                            }
                            // cutout block: opacity comes from the png so leaf edges stay sharp
                            pixel = tint.HasValue
                                ? ApplyColormapTint(src, tint.Value, src.A)
                                : new Color(src.R, src.G, src.B, src.A);
                        }
                        else if (tint.HasValue)
                        {
                            // grayscale mask tinted to a fixed colour; solid block, keep the atlas alpha
                            pixel = ApplyColormapTint(src, tint.Value, atlasAlpha);
                        }
                        else
                        {
                            pixel = new Color(src.R, src.G, src.B, atlasAlpha);
                        }

                        atlasPixels[destIndex] = pixel;
                    }
                }

                return true;
            }
        }

        private static Color? GetColormapTint(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return null;

            switch (alias.ToLowerInvariant())
            {
                case "grass_top":
                    return GrassTint;
                case "leaves":
                case "oak_leaves":
                    return FoliageTint;
                default:
                    return null;
            }
        }

        private static bool IsFoliageAlias(string alias)
        {
            if (string.IsNullOrEmpty(alias))
                return false;

            switch (alias.ToLowerInvariant())
            {
                case "leaves":
                case "oak_leaves":
                    return true;
                default:
                    return false;
            }
        }

        private static Color ApplyColormapTint(Color mask, Color tint, byte alpha)
        {
            int lum = GetColormapLuminance(mask);
            byte r = (byte)(tint.R * lum / 255);
            byte g = (byte)(tint.G * lum / 255);
            byte b = (byte)(tint.B * lum / 255);
            return new Color(r, g, b, alpha);
        }

        private static int GetColormapLuminance(Color mask)
        {
            // rgb only; including A pins lum to 255 on opaque masks and flattens the whole tile to one colour
            int lum = mask.R;
            if (mask.G > lum)
                lum = mask.G;
            if (mask.B > lum)
                lum = mask.B;
            return lum;
        }

        private static Color BoxFilterRegion(Color[] pixels, int width, int x0, int y0, int x1, int y1)
        {
            int r = 0;
            int g = 0;
            int b = 0;
            int a = 0;
            int count = 0;

            for (int y = y0; y < y1; y++)
            {
                int row = y * width;
                for (int x = x0; x < x1; x++)
                {
                    Color pixel = pixels[row + x];
                    r += pixel.R;
                    g += pixel.G;
                    b += pixel.B;
                    a += pixel.A;
                    count++;
                }
            }

            if (count == 0)
                return Color.Magenta;

            return new Color((byte)(r / count), (byte)(g / count), (byte)(b / count), (byte)(a / count));
        }

        private static Color[] CopyTileRegion(Color[] atlas, int atlasWidth, int tileSize, int tileIndex)
        {
            var tile = new Color[tileSize * tileSize];
            int tileX = tileIndex & (TilesPerRow - 1);
            int tileY = tileIndex / TilesPerRow;
            int x0 = tileX * tileSize;
            int y0 = tileY * tileSize;

            for (int y = 0; y < tileSize; y++)
            {
                for (int x = 0; x < tileSize; x++)
                    tile[y * tileSize + x] = atlas[(y0 + y) * atlasWidth + x0 + x];
            }

            return tile;
        }

        private static void PasteTileRegion(Color[] atlas, int atlasWidth, int tileSize, int tileIndex, Color[] tilePixels)
        {
            int tileX = tileIndex & (TilesPerRow - 1);
            int tileY = tileIndex / TilesPerRow;
            int x0 = tileX * tileSize;
            int y0 = tileY * tileSize;

            for (int y = 0; y < tileSize; y++)
            {
                for (int x = 0; x < tileSize; x++)
                    atlas[(y0 + y) * atlasWidth + x0 + x] = tilePixels[y * tileSize + x];
            }
        }

        private static bool IsFancyLitTile(int tileIndex)
        {
            for (int i = 0; i < FancyLitTiles.Length; i++)
            {
                if (FancyLitTiles[i] == tileIndex)
                    return true;
            }
            return false;
        }

        private static void ApplyTileEdgeBleed(Color[] atlasPixels, int atlasWidth, int tileSize, int tileIndex,
            int border, bool preserveAlpha)
        {
            if (border <= 0 || tileSize <= border * 2)
                return;

            int tileX = tileIndex & (TilesPerRow - 1);
            int tileY = tileIndex / TilesPerRow;
            int x0 = tileX * tileSize;
            int y0 = tileY * tileSize;
            int innerLeft = x0 + border;
            int innerRight = x0 + tileSize - border - 1;
            int innerTop = y0 + border;
            int innerBottom = y0 + tileSize - border - 1;

            for (int i = 0; i < border; i++)
            {
                int topRow = y0 + i;
                int bottomRow = y0 + tileSize - 1 - i;
                for (int x = innerLeft; x <= innerRight; x++)
                {
                    WritePixel(atlasPixels, topRow * atlasWidth + x, atlasPixels[innerTop * atlasWidth + x], preserveAlpha);
                    WritePixel(atlasPixels, bottomRow * atlasWidth + x, atlasPixels[innerBottom * atlasWidth + x], preserveAlpha);
                }

                int leftCol = x0 + i;
                int rightCol = x0 + tileSize - 1 - i;
                for (int y = innerTop; y <= innerBottom; y++)
                {
                    WritePixel(atlasPixels, y * atlasWidth + leftCol, atlasPixels[y * atlasWidth + innerLeft], preserveAlpha);
                    WritePixel(atlasPixels, y * atlasWidth + rightCol, atlasPixels[y * atlasWidth + innerRight], preserveAlpha);
                }
            }

            Color tl = atlasPixels[innerTop * atlasWidth + innerLeft];
            Color tr = atlasPixels[innerTop * atlasWidth + innerRight];
            Color bl = atlasPixels[innerBottom * atlasWidth + innerLeft];
            Color br = atlasPixels[innerBottom * atlasWidth + innerRight];
            for (int i = 0; i < border; i++)
            {
                for (int j = 0; j < border; j++)
                {
                    WritePixel(atlasPixels, (y0 + i) * atlasWidth + x0 + j, tl, preserveAlpha);
                    WritePixel(atlasPixels, (y0 + i) * atlasWidth + x0 + tileSize - 1 - j, tr, preserveAlpha);
                    WritePixel(atlasPixels, (y0 + tileSize - 1 - i) * atlasWidth + x0 + j, bl, preserveAlpha);
                    WritePixel(atlasPixels, (y0 + tileSize - 1 - i) * atlasWidth + x0 + tileSize - 1 - j, br, preserveAlpha);
                }
            }
        }

        private static Stream OpenPngStream(string pngRelativePath)
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
                        return File.OpenRead(path);
                    return TitleContainer.OpenStream(path.Replace('\\', '/'));
                }
                catch (Exception)
                {
                }
            }

            return null;
        }

        private static Dictionary<string, int> BuildAliasMap()
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            map["dirt"] = 0;
            map["grass_side"] = 1;
            map["grass_top"] = 2;
            map["sand"] = 3;
            map["lantern"] = 4;
            map["glowstone"] = 4;
            map["rock"] = 5;
            map["stone"] = 5;
            map["cobblestone"] = 5;
            map["gold_ore"] = 6;
            map["iron_ore"] = 7;
            map["copper_ore"] = 8;
            map["coal_ore"] = 9;
            map["coal"] = 9;
            map["diamond_ore"] = 10;
            map["lava"] = 11;
            map["bedrock"] = 12;
            map["snow"] = 13;
            map["ice"] = 14;
            // BlockType.cs Log = [_20,_17,_17,_20,_17,_17] => tile 15 (oct _17) is the 4x
            // bark/side tile, tile 16 (oct _20) is the 2x end-rings tile (top+bottom).
            map["log_side"] = 15;
            map["oak_log"] = 15;
            map["log_top"] = 16;
            map["oak_log_top"] = 16;
            map["leaves"] = 17;
            map["oak_leaves"] = 17;
            map["wood"] = 18;
            map["planks"] = 18;
            map["oak_planks"] = 18;
            map["bloodstone"] = 19;
            map["netherrack"] = 19;
            map["space_rock"] = 20;
            map["iron_wall"] = 21;
            map["copper_wall"] = 22;
            map["gold_wall"] = 23;
            map["golden_wall"] = 23;
            map["diamond_wall"] = 24;
            map["crate"] = 25;
            map["slime"] = 26;
            map["tnt_side"] = 27;
            map["tnt_top"] = 28;
            map["c4_side"] = 29;
            map["c4_top"] = 30;

            return map;
        }
    }
}
