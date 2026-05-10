using DNA.CastleMinerZ.Inventory;

namespace DNA.CastleMinerZ.ModAPI
{
    public interface IItemId
    {
        bool IsVanilla { get; }
        InventoryItemIDs VanillaId { get; }
        string ModId { get; }
    }

    public struct VanillaItemId : IItemId
    {
        public InventoryItemIDs Value;

        public bool IsVanilla { get { return true; } }
        public InventoryItemIDs VanillaId { get { return Value; } }
        public string ModId { get { return null; } }

        public VanillaItemId(InventoryItemIDs id)
        {
            Value = id;
        }

        public static implicit operator VanillaItemId(InventoryItemIDs id)
        {
            return new VanillaItemId(id);
        }
    }

    public struct ModItemId : IItemId
    {
        public string Id;

        public bool IsVanilla { get { return false; } }
        public InventoryItemIDs VanillaId { get { return InventoryItemIDs.BareHands; } }
        public string ModId { get { return Id; } }

        public ModItemId(string id)
        {
            Id = id;
        }
    }
}
