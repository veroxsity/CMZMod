using System;
using System.Collections.Generic;
using System.IO;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Inventory;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    /// <summary>
    /// Overrides VANILLA item hotbar/inventory icons with mod PNGs.
    ///
    /// Vanilla item icons are not textures on disk: at startup the game renders each item's
    /// 3D model into a 64px cell of the _2DImages render target (InventoryItem.
    /// FinishInitialization) and Draw2D samples cells by item id. We intercept Draw2D per
    /// item id and draw the PNG instead - the exact code path mod-item PNG icons already
    /// use (the Assets.LoadTexture branch), so blend/scale behavior matches the proven
    /// diamond-sword icon.
    ///
    /// Mods drop PNGs in assets/items/ named by alias (iron_pickaxe.png, compass.png, ...).
    /// deploy.ps1 stages them under Content\ModAssets\&lt;modid&gt;\items\ and emits Register()
    /// calls into GeneratedBlockTextureManifest. Items without an override keep their
    /// vanilla 3D-rendered icon; unknown aliases just log a warning.
    /// </summary>
    public static class VanillaItemIconRegistry
    {
        private static readonly Dictionary<string, InventoryItemIDs> _aliasToItem = BuildAliasMap();
        private static readonly Dictionary<int, string> _pngByItem = new Dictionary<int, string>();
        private static readonly Dictionary<int, Texture2D> _cache = new Dictionary<int, Texture2D>();

        public static void Register(string alias, string pngRelativePath)
        {
            if (string.IsNullOrEmpty(alias) || string.IsNullOrEmpty(pngRelativePath))
                return;

            InventoryItemIDs id;
            if (!_aliasToItem.TryGetValue(alias.ToLowerInvariant(), out id))
            {
                ModLog.Warn("VanillaItemIconRegistry: unknown item alias '" + alias + "'");
                return;
            }

            _pngByItem[(int)id] = pngRelativePath; // last registration wins
        }

        /// <summary>
        /// Called from InventoryItem.Draw2D on the render thread. Returns null when the
        /// item has no override (draw the vanilla atlas cell). Textures load lazily on
        /// first draw and are cached; failed loads cache null and warn once.
        /// </summary>
        public static Texture2D GetIcon(InventoryItemIDs id)
        {
            if (_pngByItem.Count == 0)
                return null;

            int key = (int)id;
            Texture2D cached;
            if (_cache.TryGetValue(key, out cached))
                return cached;

            string path;
            if (!_pngByItem.TryGetValue(key, out path))
                return null;

            Texture2D tex = LoadPng(path);
            _cache[key] = tex;
            if (tex == null)
                ModLog.Warn("VanillaItemIconRegistry: failed to load " + path);
            else
                ModLog.Info("VanillaItemIconRegistry: icon override active for " + id);
            return tex;
        }

        private static Texture2D LoadPng(string pngRelativePath)
        {
            Stream stream = OpenPngStream(pngRelativePath);
            if (stream == null)
                return null;

            try
            {
                return Texture2D.FromStream(CastleMinerZGame.Instance.GraphicsDevice, stream);
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                stream.Dispose();
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

        private static Dictionary<string, InventoryItemIDs> BuildAliasMap()
        {
            var map = new Dictionary<string, InventoryItemIDs>(StringComparer.OrdinalIgnoreCase);

            // Pickaxes (enum quirk: BloodstonePickAxe has a lowercase 's')
            map["stone_pickaxe"] = InventoryItemIDs.StonePickAxe;
            map["copper_pickaxe"] = InventoryItemIDs.CopperPickAxe;
            map["iron_pickaxe"] = InventoryItemIDs.IronPickAxe;
            map["gold_pickaxe"] = InventoryItemIDs.GoldPickAxe;
            map["golden_pickaxe"] = InventoryItemIDs.GoldPickAxe;
            map["diamond_pickaxe"] = InventoryItemIDs.DiamondPickAxe;
            map["bloodstone_pickaxe"] = InventoryItemIDs.BloodstonePickAxe;
            map["netherite_pickaxe"] = InventoryItemIDs.BloodstonePickAxe;

            // Spades / shovels (no bloodstone spade in vanilla CMZ)
            map["stone_spade"] = InventoryItemIDs.StoneSpade;
            map["stone_shovel"] = InventoryItemIDs.StoneSpade;
            map["copper_spade"] = InventoryItemIDs.CopperSpade;
            map["copper_shovel"] = InventoryItemIDs.CopperSpade;
            map["iron_spade"] = InventoryItemIDs.IronSpade;
            map["iron_shovel"] = InventoryItemIDs.IronSpade;
            map["gold_spade"] = InventoryItemIDs.GoldSpade;
            map["gold_shovel"] = InventoryItemIDs.GoldSpade;
            map["golden_shovel"] = InventoryItemIDs.GoldSpade;
            map["diamond_spade"] = InventoryItemIDs.DiamondSpade;
            map["diamond_shovel"] = InventoryItemIDs.DiamondSpade;

            // Knives (map MC sword art here; enum quirk: BloodStoneKnife has a capital 'S')
            map["knife"] = InventoryItemIDs.Knife;
            map["iron_knife"] = InventoryItemIDs.Knife;
            map["gold_knife"] = InventoryItemIDs.GoldKnife;
            map["golden_knife"] = InventoryItemIDs.GoldKnife;
            map["diamond_knife"] = InventoryItemIDs.DiamondKnife;
            map["bloodstone_knife"] = InventoryItemIDs.BloodStoneKnife;
            map["netherite_knife"] = InventoryItemIDs.BloodStoneKnife;

            // Misc
            map["compass"] = InventoryItemIDs.Compass;
            map["clock"] = InventoryItemIDs.Clock;
            map["torch"] = InventoryItemIDs.Torch;
            map["stick"] = InventoryItemIDs.Stick;
            map["door"] = InventoryItemIDs.Door;

            // Ore pickup items (raw_* is the modern MC name for the item form)
            map["copper_ore"] = InventoryItemIDs.CopperOre;
            map["raw_copper"] = InventoryItemIDs.CopperOre;
            map["iron_ore"] = InventoryItemIDs.IronOre;
            map["raw_iron"] = InventoryItemIDs.IronOre;
            map["gold_ore"] = InventoryItemIDs.GoldOre;
            map["raw_gold"] = InventoryItemIDs.GoldOre;

            return map;
        }
    }
}
