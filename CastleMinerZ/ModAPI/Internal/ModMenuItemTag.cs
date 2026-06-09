using System;

namespace DNA.CastleMinerZ.ModAPI.Internal
{
    internal class ModMenuItemTag
    {
        public string Id;
        public Action OnSelect;

        public ModMenuItemTag(string id, Action onSelect)
        {
            Id = id;
            OnSelect = onSelect;
        }
    }
}
