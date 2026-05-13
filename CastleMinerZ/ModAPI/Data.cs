using System;
using System.Collections.Generic;
using System.IO;
using DNA.IO.Storage;

namespace DNA.CastleMinerZ.ModAPI
{
    public static class Data
    {
        private static Dictionary<string, Dictionary<string, string>> _modData
            = new Dictionary<string, Dictionary<string, string>>();

        private const string DataFileName = "mod_data.bin";

        public static void SetWorld(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            SetWorld(GetCurrentModId(), key, value);
        }

        public static void SetWorld(string modId, string key, string value)
        {
            if (modId == null)
                throw new ArgumentNullException("modId");
            if (key == null)
                throw new ArgumentNullException("key");
            GetModDict(modId)[key] = value;
        }

        public static string GetWorld(string key, string defaultValue = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            return GetWorld(GetCurrentModId(), key, defaultValue);
        }

        public static string GetWorld(string modId, string key, string defaultValue = null)
        {
            if (modId == null)
                throw new ArgumentNullException("modId");
            if (key == null)
                throw new ArgumentNullException("key");
            Dictionary<string, string> dict;
            if (_modData.TryGetValue(modId, out dict))
            {
                string val;
                if (dict.TryGetValue(key, out val))
                    return val;
            }
            return defaultValue;
        }

        public static void SetWorldInt(string key, int value)
        {
            SetWorld(key, value.ToString());
        }

        public static void SetWorldInt(string modId, string key, int value)
        {
            SetWorld(modId, key, value.ToString());
        }

        public static int GetWorldInt(string key, int defaultValue = 0)
        {
            return GetWorldInt(GetCurrentModId(), key, defaultValue);
        }

        public static int GetWorldInt(string modId, string key, int defaultValue = 0)
        {
            string val = GetWorld(modId, key, null);
            if (val == null)
                return defaultValue;
            int result;
            if (int.TryParse(val, out result))
                return result;
            return defaultValue;
        }

        public static void SetGlobal(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            SetGlobal(GetCurrentModId(), key, value);
        }

        public static void SetGlobal(string modId, string key, string value)
        {
            if (modId == null)
                throw new ArgumentNullException("modId");
            if (key == null)
                throw new ArgumentNullException("key");
            GetModDict(modId)["__global__" + key] = value;
        }

        public static string GetGlobal(string key, string defaultValue = null)
        {
            if (key == null)
                throw new ArgumentNullException("key");
            return GetGlobal(GetCurrentModId(), key, defaultValue);
        }

        public static string GetGlobal(string modId, string key, string defaultValue = null)
        {
            if (modId == null)
                throw new ArgumentNullException("modId");
            if (key == null)
                throw new ArgumentNullException("key");
            Dictionary<string, string> dict;
            if (_modData.TryGetValue(modId, out dict))
            {
                string val;
                if (dict.TryGetValue("__global__" + key, out val))
                    return val;
            }
            return defaultValue;
        }

        private static string GetCurrentModId()
        {
            string modId = Internal.ModRegistry.CurrentLoadingModId;
            if (modId == null)
                throw new InvalidOperationException(
                    "Data methods must be called from a mod's OnLoad or event handler");
            return modId;
        }

        private static Dictionary<string, string> GetModDict(string modId)
        {
            Dictionary<string, string> dict;
            if (!_modData.TryGetValue(modId, out dict))
            {
                dict = new Dictionary<string, string>();
                _modData[modId] = dict;
            }
            return dict;
        }

        internal static void SaveAll(SaveDevice device, string worldPath)
        {
            if (device == null || worldPath == null)
                return;
            if (_modData.Count == 0)
                return;
            try
            {
                string path = Path.Combine(worldPath, DataFileName);
                device.Save(path, true, true, delegate(Stream stream)
                {
                    BinaryWriter w = new BinaryWriter(stream);
                    w.Write(_modData.Count);
                    foreach (var kvp in _modData)
                    {
                        w.Write(kvp.Key);
                        w.Write(kvp.Value.Count);
                        foreach (var data in kvp.Value)
                        {
                            w.Write(data.Key);
                            w.Write(data.Value);
                        }
                    }
                    w.Flush();
                });
            }
            catch
            {
            }
        }

        internal static void LoadAll(SaveDevice device, string worldPath)
        {
            if (device == null || worldPath == null)
                return;
            _modData.Clear();
            try
            {
                string path = Path.Combine(worldPath, DataFileName);
                device.Load(path, delegate(Stream stream)
                {
                    BinaryReader r = new BinaryReader(stream);
                    int modCount = r.ReadInt32();
                    for (int i = 0; i < modCount; i++)
                    {
                        string modId = r.ReadString();
                        int keyCount = r.ReadInt32();
                        var dict = new Dictionary<string, string>();
                        for (int j = 0; j < keyCount; j++)
                        {
                            string key = r.ReadString();
                            string value = r.ReadString();
                            dict[key] = value;
                        }
                        _modData[modId] = dict;
                    }
                });
            }
            catch
            {
            }
        }
    }
}
