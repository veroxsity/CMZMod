using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    public static class BlockRegistry
    {
        private const int ModSlotStart = 200;
        private const int ModSlotEnd = 255;
        private const int MaxModBlocks = ModSlotEnd - ModSlotStart + 1;

        private static Dictionary<string, BlockDef> _blocks = new Dictionary<string, BlockDef>();
        private static int _nextSlot = ModSlotStart;

        public static void Register(string id, BlockDef def)
        {
            if (id == null)
                throw new ArgumentNullException("id");
            if (def == null)
                throw new ArgumentNullException("def");
            if (_blocks.ContainsKey(id))
                throw new ArgumentException("Mod block ID already registered: " + id);
            if (_nextSlot > ModSlotEnd)
                throw new InvalidOperationException(
                    "Mod block limit reached (" + MaxModBlocks + " blocks max)");

            def.Id = id;
            def.Slot = (BlockTypeEnum)_nextSlot;
            // If the modder didn't explicitly set a ParentBlockType, default to
            // the block's own slot so it drops itself when mined.
            if (def.ParentBlockType == BlockTypeEnum.Empty)
                def.ParentBlockType = def.Slot;
            _blocks[id] = def;
            _nextSlot++;

            BlockType.RegisterModBlock(def);
        }

        public static BlockDef Resolve(string id)
        {
            BlockDef def;
            _blocks.TryGetValue(id, out def);
            return def;
        }

        public static BlockTypeEnum GetSlot(string id)
        {
            BlockDef def = Resolve(id);
            if (def != null)
                return def.Slot;
            return BlockTypeEnum.Empty;
        }
    }
}
