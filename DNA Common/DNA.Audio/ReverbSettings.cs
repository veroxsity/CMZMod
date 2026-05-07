namespace DNA.Audio
{
	public struct ReverbSettings
	{
		public float ReflectionsGain;

		public float ReverbGain;

		public float DecayTime;

		public float ReflectionsDelay;

		public float ReverbDelay;

		public float RearDelay;

		public float RoomSize;

		public float Density;

		public float LowEQGain;

		public float LowEQCutoff;

		public float HighEQGain;

		public float HighEQCutoff;

		public float PositionLeft;

		public float PositionRight;

		public float PositionLeftMatrix;

		public float PositionRightMatrix;

		public float EarlyDiffusion;

		public float LateDiffusion;

		public float RoomFilterMain;

		public float RoomFilterFrequency;

		public float RoomFilterHighFrequency;

		public float WetDryMix;

		public static readonly ReverbSettings Default;

		public static readonly ReverbSettings Generic;

		public static readonly ReverbSettings Mountains;

		public static readonly ReverbSettings Cave;

		public static readonly ReverbSettings Underwater;

		static ReverbSettings()
		{
			Mountains.ReflectionsGain = -27.8f;
			Mountains.ReverbGain = -20.14f;
			Mountains.DecayTime = 1.49f;
			Mountains.ReflectionsDelay = 299f;
			Mountains.ReverbDelay = 84f;
			Mountains.RearDelay = 5f;
			Mountains.RoomSize = 100f;
			Mountains.Density = 100f;
			Mountains.LowEQGain = 8f;
			Mountains.LowEQCutoff = 4f;
			Mountains.HighEQGain = 6f;
			Mountains.HighEQCutoff = 6f;
			Mountains.PositionLeft = 6f;
			Mountains.PositionRight = 6f;
			Mountains.PositionLeftMatrix = 27f;
			Mountains.PositionRightMatrix = 27f;
			Mountains.EarlyDiffusion = 4f;
			Mountains.LateDiffusion = 4f;
			Mountains.RoomFilterMain = -10f;
			Mountains.RoomFilterFrequency = 5000f;
			Mountains.RoomFilterHighFrequency = -25f;
			Mountains.WetDryMix = 100f;
			Cave.ReflectionsGain = -6.02f;
			Cave.ReverbGain = -3.02f;
			Cave.DecayTime = 3.78f;
			Cave.ReflectionsDelay = 15f;
			Cave.ReverbDelay = 22f;
			Cave.RearDelay = 5f;
			Cave.RoomSize = 100f;
			Cave.Density = 100f;
			Cave.LowEQGain = 8f;
			Cave.LowEQCutoff = 4f;
			Cave.HighEQGain = 8f;
			Cave.HighEQCutoff = 6f;
			Cave.PositionLeft = 6f;
			Cave.PositionRight = 6f;
			Cave.PositionLeftMatrix = 27f;
			Cave.PositionRightMatrix = 27f;
			Cave.EarlyDiffusion = 15f;
			Cave.LateDiffusion = 15f;
			Cave.RoomFilterMain = -10f;
			Cave.RoomFilterFrequency = 5000f;
			Cave.RoomFilterHighFrequency = 0f;
			Cave.WetDryMix = 100f;
			Underwater.ReflectionsGain = -4.49f;
			Underwater.ReverbGain = 17f;
			Underwater.DecayTime = 1.49f;
			Underwater.ReflectionsDelay = 7f;
			Underwater.ReverbDelay = 11f;
			Underwater.RearDelay = 5f;
			Underwater.RoomSize = 100f;
			Underwater.Density = 100f;
			Underwater.LowEQGain = 8f;
			Underwater.LowEQCutoff = 4f;
			Underwater.HighEQGain = 5f;
			Underwater.HighEQCutoff = 6f;
			Underwater.PositionLeft = 6f;
			Underwater.PositionRight = 6f;
			Underwater.PositionLeftMatrix = 27f;
			Underwater.PositionRightMatrix = 27f;
			Underwater.EarlyDiffusion = 15f;
			Underwater.LateDiffusion = 15f;
			Underwater.RoomFilterMain = -10f;
			Underwater.RoomFilterFrequency = 5000f;
			Underwater.RoomFilterHighFrequency = -40f;
			Underwater.WetDryMix = 100f;
			Default.ReflectionsGain = -100f;
			Default.ReverbGain = -100f;
			Default.DecayTime = 1f;
			Default.ReflectionsDelay = 20f;
			Default.ReverbDelay = 40f;
			Default.RearDelay = 5f;
			Default.RoomSize = 100f;
			Default.Density = 100f;
			Default.LowEQGain = 8f;
			Default.LowEQCutoff = 4f;
			Default.HighEQGain = 7f;
			Default.HighEQCutoff = 6f;
			Default.PositionLeft = 6f;
			Default.PositionRight = 6f;
			Default.PositionLeftMatrix = 27f;
			Default.PositionRightMatrix = 27f;
			Default.EarlyDiffusion = 15f;
			Default.LateDiffusion = 15f;
			Default.RoomFilterMain = -100f;
			Default.RoomFilterFrequency = 5000f;
			Default.RoomFilterHighFrequency = 0f;
			Default.WetDryMix = 100f;
			Generic.ReflectionsGain = -26.02f;
			Generic.ReverbGain = 2f;
			Generic.DecayTime = 1.49f;
			Generic.ReflectionsDelay = 7f;
			Generic.ReverbDelay = 11f;
			Generic.RearDelay = 5f;
			Generic.RoomSize = 100f;
			Generic.Density = 100f;
			Generic.LowEQGain = 8f;
			Generic.LowEQCutoff = 4f;
			Generic.HighEQGain = 8f;
			Generic.HighEQCutoff = 6f;
			Generic.PositionLeft = 6f;
			Generic.PositionRight = 6f;
			Generic.PositionLeftMatrix = 27f;
			Generic.PositionRightMatrix = 27f;
			Generic.EarlyDiffusion = 15f;
			Generic.LateDiffusion = 15f;
			Generic.RoomFilterMain = -10f;
			Generic.RoomFilterFrequency = 5000f;
			Generic.RoomFilterHighFrequency = -1f;
			Generic.WetDryMix = 100f;
		}

		public void Blend(ReverbSettings settings, float blender)
		{
			float num = 1f - blender;
			ReflectionsGain = ReflectionsGain * num + settings.ReflectionsGain * blender;
			ReverbGain = ReverbGain * num + settings.ReverbGain * blender;
			DecayTime = DecayTime * num + settings.DecayTime * blender;
			ReflectionsDelay = ReflectionsDelay * num + settings.ReflectionsDelay * blender;
			ReverbDelay = ReverbDelay * num + settings.ReverbDelay * blender;
			RearDelay = RearDelay * num + settings.RearDelay * blender;
			RoomSize = RoomSize * num + settings.RoomSize * blender;
			Density = Density * num + settings.Density * blender;
			LowEQGain = LowEQGain * num + settings.LowEQGain * blender;
			LowEQCutoff = LowEQCutoff * num + settings.LowEQCutoff * blender;
			HighEQGain = HighEQGain * num + settings.HighEQGain * blender;
			HighEQCutoff = HighEQCutoff * num + settings.HighEQCutoff * blender;
			PositionLeft = PositionLeft * num + settings.PositionLeft * blender;
			PositionRight = PositionRight * num + settings.PositionRight * blender;
			PositionLeftMatrix = PositionLeftMatrix * num + settings.PositionLeftMatrix * blender;
			PositionRightMatrix = PositionRightMatrix * num + settings.PositionRightMatrix * blender;
			EarlyDiffusion = EarlyDiffusion * num + settings.EarlyDiffusion * blender;
			LateDiffusion = LateDiffusion * num + settings.LateDiffusion * blender;
			RoomFilterMain = RoomFilterMain * num + settings.RoomFilterMain * blender;
			RoomFilterFrequency = RoomFilterFrequency * num + settings.RoomFilterFrequency * blender;
			RoomFilterHighFrequency = RoomFilterHighFrequency * num + settings.RoomFilterHighFrequency * blender;
			WetDryMix = WetDryMix * num + settings.WetDryMix * blender;
		}
	}
}
