using System;
using System.Collections.Generic;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class ModRegistry
    {
        public static List<string> LoadedModIds { get; private set; }
        public static List<string> FailedModIds { get; private set; }

        static ModRegistry()
        {
            LoadedModIds = new List<string>();
            FailedModIds = new List<string>();
        }

        public static void Initialize()
        {
            ModLog.Info("=== Mod load begin ===");
            GeneratedModRegistry.Initialize();
            ModLog.Info("=== Mod load complete ===");
        }
    }
}
