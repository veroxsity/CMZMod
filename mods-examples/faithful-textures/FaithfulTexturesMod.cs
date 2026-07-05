using DNA.CastleMinerZ.ModAPI;

namespace ExampleFaithfulTextures
{
    [Mod(Id = "example.faithful-textures", Name = "Faithful Block Textures", Version = "1.0.0")]
    public static class FaithfulTexturesMod
    {
        public static void OnLoad()
        {
            ModLog.Info("Faithful block textures loaded from assets/blocks/");
        }
    }
}
