using System;
using System.Collections.Generic;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.ModAPI.Internal;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class ModRegistry
    {
        public static List<string> LoadedModIds { get; private set; }
        public static List<string> FailedModIds { get; private set; }
        internal static string CurrentLoadingModId;

        static ModRegistry()
        {
            LoadedModIds = new List<string>();
            FailedModIds = new List<string>();
        }

        public static void Initialize()
        {
            AudioRegistry.Initialize();
            GeneratedAssetManifest.Initialize();
            GeneratedBlockTextureManifest.Initialize();
            ModLog.Info("=== Mod load begin ===");
            GeneratedModRegistry.Initialize();
            ItemRegistry.EnsureAllClassesCreated();
            BlockTextureRegistry.Apply();
            if (CastleMinerZGame.Instance != null && CastleMinerZGame.Instance.FrontEnd != null)
                CastleMinerZGame.Instance.FrontEnd.ApplyModMainMenuItems();
            CurrentLoadingModId = null;
            ModLog.Info("=== Mod load complete ===");
        }
    }
}
