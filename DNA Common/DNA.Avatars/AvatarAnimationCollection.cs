using System;
using DNA.Drawing.Animation;

namespace DNA.Avatars
{
	public class AvatarAnimationCollection : LayeredAnimationPlayer
	{
		private Avatar _avatar;

		private AnimationPlayer[,] players;

		private int[] currentPlayers;

		public AvatarAnimationCollection(Avatar avatar)
			: base(16)
		{
			_avatar = avatar;
			players = new AnimationPlayer[16, 3];
			currentPlayers = new int[16];
		}

		public AnimationPlayer Play(string id, int channel, TimeSpan blendTime)
		{
			int num = currentPlayers[channel];
			AnimationPlayer animationPlayer = players[channel, num];
			if (animationPlayer == null)
			{
				animationPlayer = (players[channel, num] = AvatarAnimationManager.Instance.GetAnimation(id, _avatar.IsMale));
			}
			else
			{
				AvatarAnimationManager.Instance.GetAnimation(animationPlayer, id, _avatar.IsMale);
			}
			currentPlayers[channel] = (currentPlayers[channel] + 1) % 3;
			PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}
	}
}
