using DNA.CastleMinerZ.ModAPI;

namespace ExampleSillySounds
{
    [Mod(Id = "example.silly-sounds", Name = "Silly Sounds", Version = "1.0.0")]
    public static class SillySoundsMod
    {
        public static void OnLoad()
        {
            Audio.Override("Hit", "pickupitem");
            ModLog.Info("Silly Sounds active — Hit plays as PickupGold");
        }
    }
}
