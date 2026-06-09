using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.ModAPI;

namespace ExampleNoAliens
{
    [Mod(Id = "example.no-aliens", Name = "No Aliens", Version = "1.0.0")]
    public static class NoAliensMod
    {
        public static void OnLoad()
        {
            Entities.SetSpawnEnabled(EnemyTypeEnum.ALIEN, false);
            ModLog.Info("No Aliens active — alien spawns disabled");
        }
    }
}
