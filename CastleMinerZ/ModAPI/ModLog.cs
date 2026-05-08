using System;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class ModLog
    {
        public static void Info(string message)
        {
            Console.WriteLine("[CMZMod] " + message);
        }

        public static void Warn(string message)
        {
            Console.WriteLine("[CMZMod WARN] " + message);
        }

        public static void Error(string message)
        {
            Console.WriteLine("[CMZMod ERROR] " + message);
        }

        public static void Loaded(string modId, string version)
        {
            Console.WriteLine("[CMZMod] Loaded " + modId + " v" + version);
        }
    }
}
