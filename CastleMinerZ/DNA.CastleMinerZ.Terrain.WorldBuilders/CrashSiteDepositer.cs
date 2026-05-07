using System;
using DNA.Drawing.Noise;

namespace DNA.CastleMinerZ.Terrain.WorldBuilders
{
	public class CrashSiteDepositer : Biome
	{
		private const float worldScale = 0.0046875f;

		private const int GroundPlane = 66;

		private PerlinNoise _noiseFunction = new PerlinNoise(new Random(1));

		private IntNoise _slimeNoiseFunction = new IntNoise(new Random(1));

		private static float noiseHighestValue;

		public CrashSiteDepositer(WorldInfo worldInfo)
			: base(worldInfo)
		{
			_noiseFunction = new PerlinNoise(new Random(worldInfo.Seed));
		}

		public override void BuildColumn(BlockTerrain terrain, int worldX, int worldZ, int minY, float blender)
		{
			int num = (int)(blender * 10f);
			float num2 = _noiseFunction.ComputeNoise(0.0046875f * (float)worldX, 0.0046875f * (float)worldZ);
			noiseHighestValue = Math.Max(noiseHighestValue, num2);
			if (!(num2 > 0.5f))
			{
				return;
			}
			int num3 = (int)((num2 - 0.5f) * 7f * 20f);
			int num4 = 0;
			if (num2 > 0.55f)
			{
				num4 = Math.Min(num3, (int)((num2 - 0.55f) * 10f * 20f));
			}
			Math.Max(20, 66 - num3 - (int)((float)num4 * 1.5f));
			for (int i = 20; i < 126; i++)
			{
				int y = i + minY;
				IntVector3 intVector = new IntVector3(worldX, y, worldZ);
				int num5 = terrain.MakeIndexFromWorldIndexVector(intVector);
				int num9 = terrain._blocks[num5];
				int y2 = i - 1 + minY;
				IntVector3 a = new IntVector3(worldX, y2, worldZ);
				int num6 = terrain.MakeIndexFromWorldIndexVector(a);
				if (i < 66 - num3 - num4 - 10)
				{
					if (terrain._blocks[num5] != Biome.BloodSToneBlock && (num6 < 0 || terrain._blocks[num6] != Biome.BloodSToneBlock))
					{
						terrain._blocks[num5] = Biome.emptyblock;
					}
				}
				else if (num4 > 0 && i < 66 - num3 + num4)
				{
					terrain._blocks[num5] = Biome.SpaceRockBlock;
				}
				else if (i >= 66 - num3 + num4)
				{
					terrain._blocks[num5] = Biome.emptyblock;
				}
				if (terrain._blocks[num5] == Biome.SpaceRockBlock && i < 66 - num3 + (num4 - 3))
				{
					IntVector3 intVector2 = intVector + new IntVector3(777, 777, 777);
					int num7 = _slimeNoiseFunction.ComputeNoise(intVector2 / 2);
					int num8 = _slimeNoiseFunction.ComputeNoise(intVector2);
					num7 += (num8 - 128) / 8;
					if (num7 > 265 - num)
					{
						terrain._blocks[num5] = Biome.SlimeBlock;
					}
				}
			}
		}
	}
}
