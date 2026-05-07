using DNA.CastleMinerZ.Terrain;
using DNA.Triggers;

namespace DNA.CastleMinerZ
{
	public class CraterFoundTransitionMusicTrigger : Trigger
	{
		private float _depth;

		private float _currentDepth;

		private string _songName;

		protected override bool IsSastisfied()
		{
			if (CastleMinerZGame.Instance.MusicCue.IsPlaying || CastleMinerZGame.Instance.MusicCue.IsPreparing)
			{
				return false;
			}
			if (_currentDepth >= _depth)
			{
				return true;
			}
			return false;
		}

		public CraterFoundTransitionMusicTrigger(string songName, float depth)
			: base(true)
		{
			_songName = songName;
			_depth = depth;
		}

		public override void OnTriggered()
		{
			CastleMinerZGame.Instance.PlayMusic(_songName);
			base.OnTriggered();
		}

		protected override void OnUpdate()
		{
			_currentDepth = BlockTerrain.Instance.DepthUnderSpaceRock(CastleMinerZGame.Instance.LocalPlayer.LocalPosition);
			base.OnUpdate();
		}
	}
}
