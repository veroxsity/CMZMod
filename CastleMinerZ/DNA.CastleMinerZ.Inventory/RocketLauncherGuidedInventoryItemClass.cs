using System;
using DNA.Audio;
using DNA.CastleMinerZ.AI;
using DNA.Drawing;
using DNA.Timers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ.Inventory
{
	public class RocketLauncherGuidedInventoryItemClass : RocketLauncherBaseInventoryItemClass
	{
		private static TracerManager.TracerProbe tp = new TracerManager.TracerProbe();

		private TimeSpan _lockingTime = TimeSpan.Zero;

		private bool _lockedOntoDragon;

		private Rectangle _lockedOnSpriteLocation = Rectangle.Empty;

		private OneShotTimer _beepTimer = new OneShotTimer(TimeSpan.FromSeconds(1.0));

		private Cue _toneCue;

		private CastleMinerZGame _game;

		public bool LockedOnToDragon
		{
			get
			{
				return _lockedOntoDragon;
			}
		}

		public Rectangle LockedOnSpriteLocation
		{
			get
			{
				return _lockedOnSpriteLocation;
			}
		}

		public RocketLauncherGuidedInventoryItemClass(InventoryItemIDs id, string name, string description1, string description2, float damage, float durabilitydamage, InventoryItem.InventoryItemClass ammotype)
			: base(id, CastleMinerZGame.Instance.Content.Load<Model>("Weapons\\Rpg"), name, description1, description2, TimeSpan.FromMinutes(1.0 / 60.0), damage, durabilitydamage, ammotype, "RPGLaunch", "ShotGunReload")
		{
			ShoulderMagnification = 2f;
			_game = CastleMinerZGame.Instance;
		}

		public override InventoryItem CreateItem(int stackCount)
		{
			return new RocketLauncherGuidedItem(this, stackCount);
		}

		public override void OnItemUnequipped()
		{
			if (_toneCue != null && _toneCue.IsPlaying)
			{
				_toneCue.Stop(AudioStopOptions.Immediate);
			}
		}

		public void CheckIfLocked(TimeSpan elapsedGameTime)
		{
			if (_game.LocalPlayer.Shouldering && EnemyManager.Instance.DragonPosition != Vector3.Zero)
			{
				Vector3 worldPosition = _game.LocalPlayer.FPSCamera.WorldPosition;
				LineF3D lineF3D = new LineF3D(worldPosition, EnemyManager.Instance.DragonPosition);
				Angle angle = _game.LocalPlayer.FPSCamera.LocalToWorld.Forward.AngleBetween(lineF3D.Direction);
				Angle angle2 = 0.3f * _game.LocalPlayer.FPSCamera.FieldOfView;
				if (lineF3D.Length < 250f && angle < angle2)
				{
					tp.Init(worldPosition, EnemyManager.Instance.DragonPosition);
					IShootableEnemy shootableEnemy = EnemyManager.Instance.Trace(tp, false);
					if (!tp._collides || (shootableEnemy != null && shootableEnemy is DragonClientEntity))
					{
						Rectangle titleSafeArea = _game.GraphicsDevice.Viewport.TitleSafeArea;
						Matrix view = _game.LocalPlayer.FPSCamera.View;
						Matrix projection = _game.LocalPlayer.FPSCamera.GetProjection(_game.GraphicsDevice);
						Matrix matrix = view * projection;
						Vector3 dragonPosition = EnemyManager.Instance.DragonPosition;
						Vector4 vector = Vector4.Transform(dragonPosition, matrix);
						Vector3 vector2 = new Vector3(vector.X / vector.W, vector.Y / vector.W, vector.Z / vector.W);
						vector2 *= new Vector3(0.5f, -0.5f, 1f);
						vector2 += new Vector3(0.5f, 0.5f, 0f);
						vector2 *= new Vector3(_game.GraphicsDevice.Viewport.Width, _game.GraphicsDevice.Viewport.Height, 1f);
						int num = (int)(35f + 215f * (1f - lineF3D.Length / 250f));
						_lockedOnSpriteLocation = new Rectangle((int)vector2.X - num / 2, (int)vector2.Y - num / 2, num, num);
						_lockingTime += elapsedGameTime;
						TimeSpan timeSpan = TimeSpan.FromSeconds(1.5 + (double)(4f * (lineF3D.Length / 250f)) + (double)(4f * (angle / angle2)));
						if (timeSpan <= _lockingTime)
						{
							_lockedOntoDragon = true;
							if (_toneCue == null || !_toneCue.IsPlaying)
							{
								_toneCue = SoundManager.Instance.PlayInstance("SolidTone");
							}
							return;
						}
						if (_toneCue != null && _toneCue.IsPlaying)
						{
							_toneCue.Stop(AudioStopOptions.Immediate);
						}
						_lockedOntoDragon = false;
						TimeSpan maxTime = TimeSpan.FromSeconds(0.15600000321865082 + 0.844 * ((timeSpan - _lockingTime).TotalSeconds / 9.5));
						_beepTimer.MaxTime = maxTime;
						_beepTimer.Update(elapsedGameTime);
						if (_beepTimer.Expired)
						{
							SoundManager.Instance.PlayInstance("Beep");
							_beepTimer.Reset();
						}
						return;
					}
				}
			}
			ResetLockingBox();
		}

		public void StopSound()
		{
			_toneCue.Stop(AudioStopOptions.Immediate);
		}

		private void ResetLockingBox()
		{
			_lockedOntoDragon = false;
			_lockingTime = TimeSpan.Zero;
			_lockedOnSpriteLocation = Rectangle.Empty;
			if (_toneCue != null && _toneCue.IsPlaying)
			{
				_toneCue.Stop(AudioStopOptions.Immediate);
			}
		}

		public override bool IsGuided()
		{
			return _lockedOntoDragon;
		}
	}
}
