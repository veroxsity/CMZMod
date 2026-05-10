using System.Collections.Generic;
using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class ItemRegistry
    {
        private static Dictionary<string, ItemDef> _items = new Dictionary<string, ItemDef>();
        private static Dictionary<string, InventoryItem.InventoryItemClass> _classes =
            new Dictionary<string, InventoryItem.InventoryItemClass>();

        public static void Register(string id, ItemDef def)
        {
            if (id == null)
                throw new System.ArgumentNullException("id");
            if (!IsValidId(id))
                throw new System.ArgumentException(
                    "Mod item ID must be namespaced (e.g. 'you.my-item') and contain only lowercase letters, digits, dashes, underscores: " + id);
            if (_items.ContainsKey(id))
                throw new System.ArgumentException("Mod item ID already registered: " + id);

            def.Id = id;
            _items[id] = def;
        }

        public static ItemDef Resolve(string id)
        {
            ItemDef def;
            _items.TryGetValue(id, out def);
            return def;
        }

        public static InventoryItem.InventoryItemClass GetClass(string id)
        {
            InventoryItem.InventoryItemClass cls;
            if (_classes.TryGetValue(id, out cls))
                return cls;

            ItemDef def = Resolve(id);
            if (def == null)
                return null;

            cls = def.CreateClass();
            _classes[id] = cls;
            return cls;
        }

        public static Dictionary<string, ItemDef>.KeyCollection RegisteredIds
        {
            get { return _items.Keys; }
        }

        public static int ModItemCount
        {
            get { return _items.Count; }
        }

        public static void EnsureAllClassesCreated()
        {
            foreach (string id in _items.Keys)
            {
                GetClass(id);
            }
        }

        public static System.Collections.Generic.List<InventoryItem.InventoryItemClass> GetAllClasses()
        {
            System.Collections.Generic.List<InventoryItem.InventoryItemClass> result =
                new System.Collections.Generic.List<InventoryItem.InventoryItemClass>();
            foreach (string id in _items.Keys)
            {
                InventoryItem.InventoryItemClass cls = GetClass(id);
                if (cls != null && !(cls is PlaceholderItemClass))
                    result.Add(cls);
            }
            return result;
        }

        public static bool IsValidId(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;
            if (id.IndexOf('.') < 0)
                return false;
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (c >= 'a' && c <= 'z')
                    continue;
                if (c >= '0' && c <= '9')
                    continue;
                if (c == '_' || c == '-' || c == '.')
                    continue;
                return false;
            }
            return true;
        }
    }
}
