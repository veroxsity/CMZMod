using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;

namespace DNA.Audio
{
	public class SoundManager
	{
		public static AudioListener ActiveListener;

		public static SoundManager Instance;

		private AudioEngine _engine;

		private SoundBank _soundBank;

		private WaveBank _waveBank;

		private WaveBank _streamBank;

		public ReverbSettings ReverbSettings = ReverbSettings.Mountains;

		private Dictionary<string, float> _globalVars = new Dictionary<string, float>();

		private LinkedList<SoundCue3D> _activeSounds = new LinkedList<SoundCue3D>();

		private bool DoReverb = true;

		static SoundManager()
		{
			Instance = new SoundManager();
		}

		public void Load(string name)
		{
			_engine = new AudioEngine("Content\\" + name + ".xgs");
			_soundBank = new SoundBank(_engine, "Content\\" + name + ".xsb");
			_waveBank = new WaveBank(_engine, "Content\\" + name + ".xwb");
			try
			{
				_streamBank = new WaveBank(_engine, "Content\\" + name + "Streaming.xwb", 0, 32);
			}
			catch
			{
			}
			do
			{
				_engine.Update();
			}
			while ((_streamBank != null && !_streamBank.IsPrepared) || !_waveBank.IsPrepared);
		}

		public float GetGlobalVarible(string varibleName)
		{
			return _engine.GetGlobalVariable(varibleName);
		}

		public void SetGlobalVarible(string varibleName, float value)
		{
			float value2;
			if (!_globalVars.TryGetValue(varibleName, out value2) || value2 != value)
			{
				_engine.SetGlobalVariable(varibleName, value);
				_globalVars[varibleName] = value;
			}
		}

		public AudioCategory GetCatagory(string name)
		{
			return _engine.GetCategory(name);
		}

		public Cue GetCue(string name)
		{
			return _soundBank.GetCue(name);
		}

		public Cue PlayInstance(string name)
		{
			Cue cue = GetCue(name);
			cue.Play();
			return cue;
		}

		public SoundCue3D PlayInstance(string name, AudioEmitter emitter)
		{
			Cue cue = GetCue(name);
			cue.Apply3D(ActiveListener, emitter);
			cue.Play();
			SoundCue3D soundCue3D = new SoundCue3D(cue, emitter);
			_activeSounds.AddLast(soundCue3D);
			return soundCue3D;
		}

		public void Update()
		{
			if (_engine != null)
			{
				_engine.Update();
				if (DoReverb)
				{
					try
					{
						SetGlobalVarible("ReverbReflectionsGain", ReverbSettings.ReflectionsGain);
						SetGlobalVarible("ReverbReverbGain", ReverbSettings.ReverbGain);
						SetGlobalVarible("ReverbDecayTime", ReverbSettings.DecayTime);
						SetGlobalVarible("ReverbReflectionsDelay", ReverbSettings.ReflectionsDelay);
						SetGlobalVarible("ReverbReverbDelay", ReverbSettings.ReverbDelay);
						SetGlobalVarible("ReverbRearDelay", ReverbSettings.RearDelay);
						SetGlobalVarible("ReverbRoomSize", ReverbSettings.RoomSize);
						SetGlobalVarible("ReverbDensity", ReverbSettings.Density);
						SetGlobalVarible("ReverbLowEQGain", ReverbSettings.LowEQGain);
						SetGlobalVarible("ReverbLowEQCutoff", ReverbSettings.LowEQCutoff);
						SetGlobalVarible("ReverbHighEQGain", ReverbSettings.HighEQGain);
						SetGlobalVarible("ReverbHighEQCutoff", ReverbSettings.HighEQCutoff);
						SetGlobalVarible("ReverbPositionLeft", ReverbSettings.PositionLeft);
						SetGlobalVarible("ReverbPositionRight", ReverbSettings.PositionRight);
						SetGlobalVarible("ReverbPositionLeftMatrix", ReverbSettings.PositionLeftMatrix);
						SetGlobalVarible("ReverbPositionRightMatrix", ReverbSettings.PositionRightMatrix);
						SetGlobalVarible("ReverbEarlyDiffusion", ReverbSettings.EarlyDiffusion);
						SetGlobalVarible("ReverbLateDiffusion", ReverbSettings.LateDiffusion);
						SetGlobalVarible("ReverbRoomFilterMain", ReverbSettings.RoomFilterMain);
						SetGlobalVarible("ReverbRoomFilterFrequency", ReverbSettings.RoomFilterFrequency);
						SetGlobalVarible("ReverbRoomFilterHighFrequency", ReverbSettings.RoomFilterHighFrequency);
						SetGlobalVarible("ReverbWetDryMix", ReverbSettings.WetDryMix);
					}
					catch
					{
						DoReverb = false;
					}
				}
			}
			LinkedListNode<SoundCue3D> linkedListNode = _activeSounds.First;
			while (linkedListNode != null)
			{
				LinkedListNode<SoundCue3D> next = linkedListNode.Next;
				if (linkedListNode.Value.IsStopped)
				{
					_activeSounds.Remove(linkedListNode);
				}
				else
				{
					linkedListNode.Value.Apply3D(ActiveListener);
				}
				linkedListNode = next;
			}
		}
	}
}
