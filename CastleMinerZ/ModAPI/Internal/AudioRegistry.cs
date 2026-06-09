using System;
using System.Collections.Generic;
using DNA.Audio;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class AudioRegistry
    {
        private static Dictionary<string, string> _overrides = new Dictionary<string, string>();
        private static Dictionary<string, bool> _suppressed = new Dictionary<string, bool>();

        public static void Initialize()
        {
            SoundManager.BeforePlayInstance = OnBeforePlay;
        }

        public static void Override(string cueName, string substituteName)
        {
            if (cueName == null)
                throw new ArgumentNullException("cueName");
            if (substituteName == null)
                throw new ArgumentNullException("substituteName");
            _suppressed.Remove(cueName);
            _overrides[cueName] = substituteName;
        }

        public static void Suppress(string cueName)
        {
            if (cueName == null)
                throw new ArgumentNullException("cueName");
            _overrides.Remove(cueName);
            _suppressed[cueName] = true;
        }

        public static void Reset(string cueName)
        {
            if (cueName == null)
                throw new ArgumentNullException("cueName");
            _overrides.Remove(cueName);
            _suppressed.Remove(cueName);
        }

        internal static void OnBeforePlay(SoundPlayHookArgs hookArgs)
        {
            if (hookArgs == null || hookArgs.CueName == null)
                return;

            var modArgs = new SoundPlayArgs
            {
                CueName = hookArgs.CueName,
                Suppressed = hookArgs.Suppressed,
            };

            Audio.InvokeSoundPlay(modArgs);

            if (_suppressed.ContainsKey(hookArgs.CueName))
                modArgs.Suppressed = true;

            string substitute;
            if (!modArgs.Suppressed && _overrides.TryGetValue(hookArgs.CueName, out substitute))
                modArgs.CueName = substitute;

            hookArgs.CueName = modArgs.CueName;
            hookArgs.Suppressed = modArgs.Suppressed;
        }
    }
}
