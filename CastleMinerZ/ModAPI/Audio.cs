using System;
using DNA.Audio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Audio
    {
        private static readonly string[] KnownCueNames = new string[]
        {
            "Click", "Error", "Hit", "Fall", "Heal", "Explosion", "craft",
            "pickupitem", "dropitem", "Dig", "Award", "Teleport", "Reload",
            "Place", "FootStep", "GunShot3", "BulletHitHuman", "BulletHitDirt",
            "DoorOpen", "DoorClose", "ZombieGrowl", "Skeleton", "Alien",
        };

        public static event Action<SoundPlayArgs> SoundPlay;

        public static void Override(string cueName, string substituteName)
        {
            Internal.AudioRegistry.Override(cueName, substituteName);
        }

        public static void Suppress(string cueName)
        {
            Internal.AudioRegistry.Suppress(cueName);
        }

        public static void Reset(string cueName)
        {
            Internal.AudioRegistry.Reset(cueName);
        }

        public static void Play(string cueName)
        {
            if (cueName == null)
                throw new ArgumentNullException("cueName");
            if (SoundManager.Instance == null)
                return;
            try
            {
                SoundManager.Instance.PlayInstance(cueName);
            }
            catch (Exception ex)
            {
                ModLog.Warn("Audio.Play failed for cue '" + cueName + "': " + ex.Message);
            }
        }

        public static void Play(string cueName, Vector3 position)
        {
            if (cueName == null)
                throw new ArgumentNullException("cueName");
            if (SoundManager.Instance == null)
                return;
            try
            {
                AudioEmitter emitter = new AudioEmitter();
                emitter.Position = position;
                SoundManager.Instance.PlayInstance(cueName, emitter);
            }
            catch (Exception ex)
            {
                ModLog.Warn("Audio.Play failed for cue '" + cueName + "': " + ex.Message);
            }
        }

        public static string[] GetAllCueNames()
        {
            return KnownCueNames;
        }

        internal static void InvokeSoundPlay(SoundPlayArgs args)
        {
            var handler = SoundPlay;
            if (handler == null)
                return;
            foreach (Action<SoundPlayArgs> h in handler.GetInvocationList())
            {
                try
                {
                    h(args);
                }
                catch (Exception ex)
                {
                    ModLog.Error("SoundPlay handler failed: " + ex.Message);
                }
            }
        }
    }

    public class SoundPlayArgs
    {
        public string CueName;
        public bool Suppressed;
        public Vector3? Position;
    }
}
