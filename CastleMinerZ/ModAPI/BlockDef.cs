using DNA.CastleMinerZ.Terrain;

namespace DNA.CastleMinerZ.ModAPI
{
    public class BlockDef
    {
        public string Id { get; internal set; }
        public string DisplayName { get; set; }
        public int Hardness { get; set; }
        public float LightTransmission { get; set; }
        public float SelfIllumination { get; set; }
        public int[] TileIndices { get; set; }
        public bool BlockPlayer { get; set; }
        public bool CanBeDug { get; set; }
        public bool CanBeTouched { get; set; }
        public bool CanBuildOn { get; set; }
        public bool HasAlpha { get; set; }
        public bool DrawFullBright { get; set; }
        public bool BouncesLasers { get; set; }
        public float BounceRestitution { get; set; }
        public bool SpawnEntity { get; set; }
        public float DamageTransmission { get; set; }
        public bool IsItemEntity { get; set; }
        public bool LightAsTranslucent { get; set; }
        public bool NeedsFancyLighting { get; set; }
        public bool InteriorFaces { get; set; }
        public bool AllowSlopes { get; set; }
        public BlockFace Facing { get; set; }
        public BlockTypeEnum ParentBlockType { get; set; }

        internal BlockTypeEnum Slot { get; set; }

        public BlockDef()
        {
            Hardness = 2;
            LightTransmission = 0f;
            SelfIllumination = 0f;
            DamageTransmission = 0.6f;
            BlockPlayer = true;
            CanBeDug = true;
            CanBeTouched = true;
            CanBuildOn = true;
            HasAlpha = false;
            DrawFullBright = false;
            BouncesLasers = false;
            BounceRestitution = 0.6f;
            SpawnEntity = false;
            IsItemEntity = false;
            LightAsTranslucent = false;
            NeedsFancyLighting = false;
            InteriorFaces = false;
            AllowSlopes = true;
            Facing = BlockFace.NUM_FACES;
            ParentBlockType = (BlockTypeEnum)0;
            TileIndices = new int[6];
        }
    }
}
