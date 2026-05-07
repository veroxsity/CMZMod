using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DNA.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Avatars
{
	public class Avatar : Entity
	{
		public class AvatarVerifyView : View
		{
			private List<Avatar> _toVerify = new List<Avatar>();

			private List<Avatar> toRemove = new List<Avatar>();

			public void VerifyAvatar(Avatar avatar)
			{
				_toVerify.Add(avatar);
			}

			public AvatarVerifyView(GraphicsDevice device)
			{
				int num = 32;
				base.Target = new RenderTarget2D(device, num, num, false, SurfaceFormat.Bgra4444, DepthFormat.Depth16, 1, RenderTargetUsage.PreserveContents);
			}

			protected override void OnDraw(GraphicsDevice device, SpriteBatch spriteBatch, GameTime gameTime)
			{
				base.OnDraw(device, spriteBatch, gameTime);
				int width = base.Target.Width;
				toRemove.Clear();
				for (int i = 0; i < _toVerify.Count; i++)
				{
					if (_toVerify[i].AvatarState != AvatarRendererState.Ready)
					{
						continue;
					}
					Avatar avatar = _toVerify[i];
					toRemove.Add(avatar);
					device.SetRenderTarget(base.Target);
					device.Clear(new Color(0, 0, 0, 0));
					avatar._avatarRenderer.World = Matrix.Identity;
					avatar._avatarRenderer.View = Matrix.CreateLookAt(new Vector3(0f, avatar.AvatarHeight / 2f, 5.5f), new Vector3(0f, avatar.AvatarHeight / 2f, 0f), Vector3.Up);
					avatar._avatarRenderer.Projection = Matrix.CreatePerspectiveFieldOfView((float)Math.PI / 4f, 1f, 0.1f, 1000f);
					avatar._avatarRenderer.Draw(avatar.BindPose, avatar.Expression);
					avatar._avatarRenderer.Draw(avatar.BindPose, avatar.Expression);
					device.SetRenderTarget(null);
					byte[] array = new byte[width * width * 4];
					base.Target.GetData(array);
					int num = 0;
					for (int j = 0; j < array.Length; j++)
					{
						if (array[j] != 0)
						{
							num++;
						}
					}
					float num2 = (float)num / (float)array.Length;
					avatar._invisible = num2 < 0.01f;
				}
				foreach (Avatar item in toRemove)
				{
					_toVerify.Remove(item);
				}
			}
		}

		public const float WalkSpeed = 0.947f;

		public const float RunSpeed = 4f;

		private const float MAX_HEIGHT = 1.6665109f;

		private const float MIN_HEIGHT = 1.4120882f;

		public AvatarLightingManager LightingManager;

		private static AvatarDescription _defaultDescription;

		public static readonly byte[] DefaultDescriptionData;

		public static readonly ReadOnlyCollection<int> DefaultParentBones;

		public static readonly ReadOnlyCollection<string> BoneNames;

		public static readonly ReadOnlyCollection<Matrix> DefaultBindPose;

		private static readonly string[] nativeBoneNames;

		private AvatarRenderer _avatarRenderer;

		private AvatarAnimationCollection _animations;

		private AvatarExpression _expression = default(AvatarExpression);

		private AvatarDescription _avatarDescription;

		private Matrix[] _bonesToAvatar = new Matrix[71];

		private Matrix[] _boneTransformBuffer = new Matrix[71];

		private Matrix[] _wireFrameWorldTransforms;

		private Entity[] _partMap = new Entity[71];

		private VertexPositionColor[] _wireFrameVerts;

		public PerspectiveCamera EyePointCamera = new PerspectiveCamera();

		public bool HideHead;

		public object Tag;

		private bool _invisible;

		private bool _skeletonBuilt;

		private Skeleton _skeleton = Bone.BuildSkeleton(DefaultBindPose, DefaultParentBones, BoneNames);

		private SkinnedModelEntity _proxyModel;

		public static Dictionary<string, int> boneNameLookup;

		private BasicEffect _wireFrameEffect;

		public Gamer _gamer;

		private bool[] _partMask = new bool[71];

		private Matrix eyeToHead = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI) * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI)) * Matrix.CreateTranslation(new Vector3(0f, 0.05f, 0f));

		private Matrix[] _proxyBoneBuffer = new Matrix[71];

		public static AvatarDescription DefaultDescription
		{
			get
			{
				if (_defaultDescription == null)
				{
					_defaultDescription = AvatarDescription.CreateRandom();
				}
				return _defaultDescription;
			}
		}

		public ReadOnlyCollection<int> ParentBones
		{
			get
			{
				if (_avatarRenderer != null && _avatarRenderer.State == AvatarRendererState.Ready)
				{
					return _avatarRenderer.ParentBones;
				}
				return DefaultParentBones;
			}
		}

		public ReadOnlyCollection<Matrix> BindPose
		{
			get
			{
				if (_avatarRenderer != null && _avatarRenderer.State == AvatarRendererState.Ready)
				{
					return _avatarRenderer.BindPose;
				}
				return DefaultBindPose;
			}
		}

		public bool IsInvisible
		{
			get
			{
				return _invisible;
			}
		}

		public Skeleton Skeleton
		{
			get
			{
				return _skeleton;
			}
		}

		public SkinnedModelEntity ProxyModelEntity
		{
			get
			{
				return _proxyModel;
			}
			set
			{
				if (_proxyModel != null)
				{
					_proxyModel.RemoveFromParent();
					_proxyModel = null;
					SetEyePoint(AvatarHeight);
				}
				if (value != null)
				{
					_proxyModel = value;
					base.Children.Add(_proxyModel);
					SetEyePoint(1.6f);
				}
				_proxyModel = value;
			}
		}

		public AvatarDescription Description
		{
			get
			{
				return _avatarDescription;
			}
		}

		public AvatarRendererState AvatarState
		{
			get
			{
				return _avatarRenderer.State;
			}
		}

		public AvatarRenderer AvatarRenderer
		{
			get
			{
				return _avatarRenderer;
			}
		}

		public float AvatarHeight
		{
			get
			{
				return _avatarDescription.Height;
			}
		}

		public AvatarExpression Expression
		{
			get
			{
				return _expression;
			}
			set
			{
				_expression = value;
			}
		}

		public AvatarAnimationCollection Animations
		{
			get
			{
				return _animations;
			}
		}

		public Gamer Gamer
		{
			get
			{
				return _gamer;
			}
		}

		public bool IsMale
		{
			get
			{
				return _avatarDescription.BodyType == AvatarBodyType.Male;
			}
		}

		static Avatar()
		{
			DefaultDescriptionData = new byte[1021]
			{
				1, 0, 0, 0, 0, 191, 0, 0, 0, 191,
				0, 0, 0, 0, 16, 0, 0, 3, 31, 0,
				3, 193, 200, 241, 9, 161, 156, 178, 224, 0,
				8, 0, 0, 3, 43, 0, 3, 193, 200, 241,
				9, 161, 156, 178, 224, 0, 32, 0, 0, 3,
				59, 0, 3, 193, 200, 241, 9, 161, 156, 178,
				224, 0, 0, 128, 0, 2, 234, 0, 3, 193,
				200, 241, 9, 161, 156, 178, 224, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 32, 0, 2, 158, 0,
				3, 193, 200, 241, 9, 161, 156, 178, 224, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 64, 0, 2,
				100, 0, 3, 193, 200, 241, 9, 161, 156, 178,
				224, 63, 128, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 255, 215, 170, 113, 255, 110, 83,
				38, 255, 181, 97, 87, 255, 99, 129, 167, 255,
				73, 52, 33, 255, 83, 149, 202, 255, 73, 52,
				33, 255, 207, 89, 105, 255, 207, 89, 105, 0,
				0, 0, 2, 0, 0, 0, 1, 193, 200, 241,
				9, 161, 156, 178, 224, 0, 2, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 1, 0, 2, 0, 3, 193,
				200, 241, 9, 161, 156, 178, 224, 0, 1, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 4, 1, 178, 0,
				3, 193, 200, 241, 9, 161, 156, 178, 224, 0,
				4, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 8, 0,
				88, 0, 1, 193, 200, 241, 9, 161, 156, 178,
				224, 0, 8, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				16, 0, 144, 0, 1, 193, 200, 241, 9, 161,
				156, 178, 224, 0, 16, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 32, 0, 49, 0, 1, 193, 200, 241,
				9, 161, 156, 178, 224, 0, 32, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 32, 0, 49, 0, 1, 193, 200, 241,
				9, 161, 156, 178, 224, 0, 32, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 16, 0, 144, 0, 1, 193,
				200, 241, 9, 161, 156, 178, 224, 0, 16, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 8, 0, 88, 0,
				1, 193, 200, 241, 9, 161, 156, 178, 224, 0,
				8, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 4, 1,
				178, 0, 3, 193, 200, 241, 9, 161, 156, 178,
				224, 0, 4, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 224, 0, 2,
				77, 216, 48, 81, 160, 3, 51, 5, 26, 3,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
				0, 19, 202, 106, 209, 13, 230, 203, 203, 185,
				0, 179, 142, 247, 181, 126, 186, 157, 86, 192,
				29
			};
			DefaultParentBones = new ReadOnlyCollection<int>(new int[71]
			{
				-1, 0, 0, 0, 0, 1, 2, 2, 3, 3,
				1, 6, 5, 6, 5, 8, 5, 8, 5, 14,
				12, 11, 16, 15, 14, 20, 20, 20, 22, 22,
				22, 25, 25, 25, 28, 28, 28, 33, 33, 33,
				33, 33, 33, 33, 36, 36, 36, 36, 36, 36,
				36, 37, 38, 39, 40, 43, 44, 45, 46, 47,
				50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
				60
			});
			DefaultBindPose = new ReadOnlyCollection<Matrix>(new Matrix[71]
			{
				new Matrix(-0.9999998f, 3.191891E-16f, 1.192093E-07f, 0f, 3.191891E-16f, 1f, -1.665335E-16f, 0f, -1.192093E-07f, -1.665334E-16f, -0.9999998f, 0f, 6.185371E-06f, 0.7769224f, -0.008659153f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -7.071068E-06f, 0.02402931f, -4.082918E-06f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.09005712f, -0.1072602f, 0.008654864f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.09005711f, -0.1072602f, 0.008654864f, 1f),
				new Matrix(0.92f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.96f, 0f, 0f, 0.03059953f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0.1158395f, -0.007715893f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, -0.2889061f, -0.01396209f, 1f),
				new Matrix(0.88f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, -0.144453f, -0.006491148f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, -0.2889061f, -0.01396209f, 1f),
				new Matrix(0.88f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, -0.144453f, -0.006491148f, 1f),
				new Matrix(0.84f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.84f, 0f, 7.071068E-06f, 0.06121063f, -0.005139845f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.006229609f, -0.279172f, -0.02728323f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.007396337f, 0.1224098f, 0.01427236f, 1f),
				new Matrix(0.88f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0.002743572f, -0.1371553f, -0.01205149f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0.1737708f, -0.01882024f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.006243758f, -0.279172f, -0.02728323f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.007396337f, 0.1224098f, 0.01427236f, 1f),
				new Matrix(0.88f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.88f, 0f, -0.002757721f, -0.1371553f, -0.01205149f, 1f),
				new Matrix(0.96f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.92f, 0f, 0f, 0.04343981f, -0.004703019f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0.1005861f, 0.01940812f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.1176201f, 6.937981E-05f, -0.02792418f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.006682158f, -0.08587508f, 0.1141176f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.1176201f, 6.937981E-05f, -0.02792418f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.006682158f, -0.08587508f, 0.1141176f, 1f),
				new Matrix(0.88f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0.0335325f, 0.006466653f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.2201223f, -0.005196214f, 0.0007593427f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0.1100541f, -0.001905322f, 0.0002776086f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0.004398212f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.2201223f, -0.005196214f, 0.0007593427f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, -0.1100541f, -0.001905322f, 0.0002776086f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, -0.004398197f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.1130734f, -0.002655864f, 0.00172689f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0.08480331f, -0.001720548f, 0.001122681f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.1696137f, -0.003995299f, 0.002584212f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.1130735f, -0.002655864f, 0.00172689f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 0.88f, 0f, 0f, 0f, 0f, 0.88f, 0f, -0.08480334f, -0.001720548f, 0.001122681f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.1696137f, -0.003995299f, 0.002584212f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.0921219f, -0.02434099f, 0.03605649f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.09366339f, -0.02339423f, 0.009479525f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.08793581f, -0.02604997f, -0.01582371f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.07674938f, -0.02977967f, -0.03964907f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.06290424f, -0.1572471f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.09434927f, -0.09435052f, -4.082918E-06f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -7.092953E-06f, 0f, 0.008413997f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.0921219f, -0.02434099f, 0.03605649f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.09366339f, -0.02339423f, 0.009479525f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.08793581f, -0.02604997f, -0.01582371f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.07673526f, -0.02977967f, -0.03964907f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.06290424f, -0.1572471f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.09434932f, -0.09435052f, -4.082918E-06f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 7.033348E-06f, 0f, 0.008413997f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.03987372f, 0f, 0.000330681f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.04226375f, 0f, -1.224689E-05f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.0405314f, 0f, -0.0002939366f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.03423101f, 0f, -0.0003061891f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.05561399f, -0.03163874f, 0.04206999f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.03987378f, 0f, 0.000330681f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.04226381f, 0f, -1.224689E-05f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.04051721f, 0f, -0.0002939366f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.03424519f, 0f, -0.0003061891f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.05561393f, -0.03163874f, 0.04206999f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.02873683f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.0298894f, 0f, 1.224689E-05f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.02934492f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.02515888f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0.02728015f, -0.01478016f, 0.01658305f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.02873683f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.02987522f, 0f, 1.224689E-05f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.0293591f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.02515888f, 0f, 0f, 1f),
				new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, -0.02728015f, -0.01478016f, 0.01658305f, 1f)
			});
			nativeBoneNames = new string[71]
			{
				"BASE__Skeleton", "BACKA__Skeleton", "LF_H__Skeleton", "RT_H__Skeleton", "SC_BASE__Skeleton", "BACKB__Skeleton", "LF_K__Skeleton", "LF_SC_H__Skeleton", "RT_K__Skeleton", "RT_SC_H__Skeleton",
				"SC_BACKA__Skeleton", "LF_A__Skeleton", "LF_C__Skeleton", "LF_SC_K__Skeleton", "NECK__Skeleton", "RT_A__Skeleton", "RT_C__Skeleton", "RT_SC_K__Skeleton", "SC_BACKB__Skeleton", "HEAD__Skeleton",
				"LF_S__Skeleton", "LF_T__Skeleton", "RT_S__Skeleton", "RT_T__Skeleton", "SC_NECK__Skeleton", "LF_E__Skeleton", "LF_SC_S__Skeleton", "LF_SC_TWIST_S__Skeleton", "RT_E__Skeleton", "RT_SC_S__Skeleton",
				"RT_SC_TWIST_S__Skeleton", "LF_E_TWIST__Skeleton", "LF_SC_E__Skeleton", "LF_W__Skeleton", "RT_E_TWIST__Skeleton", "RT_SC_E__Skeleton", "RT_W__Skeleton", "LF_FINGA__Skeleton", "LF_FINGB__Skeleton", "LF_FINGC__Skeleton",
				"LF_FINGD__Skeleton", "LF_PROP__Skeleton", "LF_SPECIAL__Skeleton", "LF_THUMB__Skeleton", "RT_FINGA__Skeleton", "RT_FINGB__Skeleton", "RT_FINGC__Skeleton", "RT_FINGD__Skeleton", "RT_PROP__Skeleton", "RT_SPECIAL__Skeleton",
				"RT_THUMB__Skeleton", "LF_FINGA1__Skeleton", "LF_FINGB1__Skeleton", "LF_FINGC1__Skeleton", "LF_FINGD1__Skeleton", "LF_THUMB1__Skeleton", "RT_FINGA1__Skeleton", "RT_FINGB1__Skeleton", "RT_FINGC1__Skeleton", "RT_FINGD1__Skeleton",
				"RT_THUMB1__Skeleton", "LF_FINGA2__Skeleton", "LF_FINGB2__Skeleton", "LF_FINGC2__Skeleton", "LF_FINGD2__Skeleton", "LF_THUMB2__Skeleton", "RT_FINGA2__Skeleton", "RT_FINGB2__Skeleton", "RT_FINGC2__Skeleton", "RT_FINGD2__Skeleton",
				"RT_THUMB2__Skeleton"
			};
			boneNameLookup = new Dictionary<string, int>();
			List<string> list = new List<string>();
			for (int i = 0; i < 71; i++)
			{
				AvatarBone avatarBone = (AvatarBone)i;
				list.Add(avatarBone.ToString());
			}
			BoneNames = new ReadOnlyCollection<string>(list);
			for (int j = 0; j < nativeBoneNames.Length; j++)
			{
				boneNameLookup[nativeBoneNames[j]] = j;
			}
		}

		public Entity GetAvatarPart(AvatarBone bone)
		{
			if (_partMap[(int)bone] != null)
			{
				return _partMap[(int)bone];
			}
			Entity entity = new Entity();
			_partMap[(int)bone] = entity;
			base.Children.Add(entity);
			int num = (int)bone;
			do
			{
				_partMask[num] = true;
				num = DefaultParentBones[num];
			}
			while (num >= 0 && !_partMask[num]);
			Skeleton.CopyTransformsTo(_bonesToAvatar);
			UpdateParts(_bonesToAvatar);
			entity.LocalToParent = _bonesToAvatar[(int)bone];
			return entity;
		}

		private void InitalizeParts()
		{
			for (int i = 0; i < 71; i++)
			{
				_bonesToAvatar[i] = Matrix.Identity;
				_partMask[i] = false;
			}
			GetAvatarPart(AvatarBone.Head);
			base.Children.Add(EyePointCamera);
		}

		public void SetAsPlayerAvatar(PlayerIndex index)
		{
			SetAsPlayerAvatar(Gamer.SignedInGamers[index]);
			IAsyncResult asyncResult = AvatarDescription.BeginGetFromGamer(_gamer, delegate
			{
			}, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			_avatarDescription = AvatarDescription.EndGetFromGamer(asyncResult);
			_avatarRenderer = new AvatarRenderer(_avatarDescription, false);
			_skeletonBuilt = false;
		}

		public void SetAsPlayerAvatar(Gamer gamer)
		{
			_gamer = gamer;
			IAsyncResult asyncResult = AvatarDescription.BeginGetFromGamer(_gamer, delegate
			{
			}, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			_avatarDescription = AvatarDescription.EndGetFromGamer(asyncResult);
			_avatarRenderer = new AvatarRenderer(_avatarDescription, false);
			_skeletonBuilt = false;
		}

		public Avatar(Gamer gamer)
		{
			_animations = new AvatarAnimationCollection(this);
			_expression.Mouth = AvatarMouth.Neutral;
			_gamer = gamer;
			IAsyncResult asyncResult = AvatarDescription.BeginGetFromGamer(_gamer, delegate
			{
			}, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			_avatarDescription = AvatarDescription.EndGetFromGamer(asyncResult);
			_avatarRenderer = new AvatarRenderer(_avatarDescription, false);
			if (!_avatarDescription.IsValid)
			{
				_avatarDescription = AvatarDescription.CreateRandom();
			}
			_expression = default(AvatarExpression);
			InitalizeParts();
		}

		public Avatar(PlayerIndex index)
		{
			_animations = new AvatarAnimationCollection(this);
			_expression.Mouth = AvatarMouth.Neutral;
			_gamer = Gamer.SignedInGamers[index];
			IAsyncResult asyncResult = AvatarDescription.BeginGetFromGamer(_gamer, delegate
			{
			}, null);
			asyncResult.AsyncWaitHandle.WaitOne();
			_avatarDescription = AvatarDescription.EndGetFromGamer(asyncResult);
			_avatarRenderer = new AvatarRenderer(_avatarDescription, false);
			if (!_avatarDescription.IsValid)
			{
				_avatarDescription = AvatarDescription.CreateRandom();
			}
			_expression = default(AvatarExpression);
			InitalizeParts();
		}

		public Avatar(AvatarDescription description)
		{
			_animations = new AvatarAnimationCollection(this);
			_expression.Mouth = AvatarMouth.Neutral;
			_avatarDescription = description;
			_avatarRenderer = new AvatarRenderer(_avatarDescription, false);
			_expression = default(AvatarExpression);
			InitalizeParts();
		}

		public Avatar(bool useRandom)
		{
			_animations = new AvatarAnimationCollection(this);
			_expression.Mouth = AvatarMouth.Neutral;
			if (useRandom)
			{
				MakeRandom();
			}
			else
			{
				MakeDefault();
			}
			InitalizeParts();
		}

		public Matrix GetBoneToAvatar(AvatarBone bone)
		{
			return _bonesToAvatar[(int)bone];
		}

		public void UpdateParts(Matrix[] skeltonBones)
		{
			ReadOnlyCollection<Matrix> bindPose = BindPose;
			_bonesToAvatar[0] = skeltonBones[0] * bindPose[0];
			for (int i = 1; i < 71; i++)
			{
				if (_partMask[i])
				{
					_bonesToAvatar[i] = skeltonBones[i];
					_bonesToAvatar[i].Translation = bindPose[i].Translation;
					Matrix.Multiply(ref _bonesToAvatar[i], ref _bonesToAvatar[DefaultParentBones[i]], out _bonesToAvatar[i]);
				}
			}
			for (int j = 0; j < 71; j++)
			{
				if (_partMap[j] != null)
				{
					_partMap[j].LocalToParent = _bonesToAvatar[j];
				}
			}
		}

		public static List<int> FindInfluencedBones(AvatarBone avatarBone)
		{
			List<int> list = new List<int>();
			list.Add((int)avatarBone);
			for (int i = list[0] + 1; i < DefaultParentBones.Count; i++)
			{
				if (list.Contains(DefaultParentBones[i]))
				{
					list.Add(i);
				}
			}
			return list;
		}

		public static bool[] GetInfluncedBoneList(AvatarBone bone)
		{
			return GetInfluncedBoneList(new AvatarBone[1] { bone });
		}

		public static bool[] GetInfluncedBoneList(IList<AvatarBone> bones)
		{
			new Dictionary<int, int>();
			bool[] array = new bool[71];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = false;
			}
			foreach (AvatarBone bone in bones)
			{
				List<int> list = FindInfluencedBones(bone);
				foreach (int item in list)
				{
					array[item] = true;
				}
			}
			return array;
		}

		public static bool[] GetInfluncedBoneList(IList<AvatarBone> bones, IList<AvatarBone> maskedBones)
		{
			new Dictionary<int, int>();
			bool[] array;
			if (bones != null)
			{
				array = GetInfluncedBoneList(bones);
			}
			else
			{
				array = new bool[71];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = true;
				}
			}
			bool[] influncedBoneList = GetInfluncedBoneList(maskedBones);
			for (int j = 0; j < influncedBoneList.Length; j++)
			{
				if (influncedBoneList[j])
				{
					array[j] = false;
				}
			}
			return array;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (!_skeletonBuilt && _avatarRenderer.State == AvatarRendererState.Ready)
			{
				BuildSkeleton();
			}
			_animations.Update(gameTime.ElapsedGameTime, Skeleton);
			Matrix matrix = _bonesToAvatar[19];
			Vector3 translation = matrix.Translation;
			Vector3 forward = matrix.Forward;
			Vector3 up = matrix.Up;
			forward.Normalize();
			up.Normalize();
			matrix = Matrix.CreateWorld(translation, forward, up);
			Matrix localToParent = eyeToHead * matrix;
			EyePointCamera.LocalToParent = localToParent;
			if (HideHead)
			{
				Bone bone = Skeleton[19];
				bone.Scale = new Vector3(0.001f, 0.001f, 0.001f);
			}
			Skeleton.CopyTransformsTo(_boneTransformBuffer);
			if (ProxyModelEntity != null)
			{
				Matrix matrix2 = Matrix.CreateRotationY((float)Math.PI);
				matrix2.Translation = new Vector3(0f, 0.7769f, -0.008664f);
				ProxyModelEntity.DefaultPose[0] = _boneTransformBuffer[0] * matrix2;
				for (int i = 1; i < ProxyModelEntity.DefaultPose.Length; i++)
				{
					string name = ProxyModelEntity.Skeleton[i].Name;
					int num = boneNameLookup[name];
					Matrix matrix3 = _boneTransformBuffer[num];
					matrix3.Translation = ProxyModelEntity.BindPose[i].Translation;
					ProxyModelEntity.DefaultPose[i] = matrix3;
					ProxyModelEntity.Skeleton[i].SetTransform(matrix3);
				}
			}
			UpdateParts(_boneTransformBuffer);
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (ProxyModelEntity == null)
			{
				if (LightingManager != null)
				{
					LightingManager.SetAvatarLighting(this);
				}
				_avatarRenderer.World = base.LocalToWorld;
				_avatarRenderer.View = view;
				_avatarRenderer.Projection = projection;
				try
				{
					_avatarRenderer.Draw(_boneTransformBuffer, _expression);
				}
				catch
				{
				}
				base.Draw(device, gameTime, view, projection);
			}
		}

		public void MakeDefault()
		{
			_avatarDescription = DefaultDescription;
			_avatarRenderer = new AvatarRenderer(_avatarDescription);
			_expression = default(AvatarExpression);
		}

		private void DefaultSkeleton()
		{
			_skeletonBuilt = false;
			_skeleton = Bone.BuildSkeleton(DefaultBindPose, DefaultParentBones, BoneNames);
		}

		protected void SetEyePoint(float playerHeight)
		{
			float num = 0.045f;
			num += (playerHeight - 1.4120882f) / 0.25442278f * 0.01f;
			eyeToHead = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI) * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI)) * Matrix.CreateTranslation(new Vector3(0f, num, 0f));
		}

		private void BuildSkeleton()
		{
			try
			{
				SetEyePoint(AvatarHeight);
				_skeleton = Bone.BuildSkeleton(_avatarRenderer);
				_skeletonBuilt = true;
			}
			catch (InvalidOperationException)
			{
			}
		}

		public void MakeRandom()
		{
			_avatarDescription = AvatarDescription.CreateRandom();
			_avatarRenderer = new AvatarRenderer(_avatarDescription);
			DefaultSkeleton();
			_expression = default(AvatarExpression);
		}

		public static bool Compare(AvatarDescription ad1, AvatarDescription ad2)
		{
			if (ad1.Description.Length != ad1.Description.Length)
			{
				return false;
			}
			for (int i = 0; i < ad1.Description.Length; i++)
			{
				if (ad1.Description[i] != ad2.Description[i])
				{
					return false;
				}
			}
			return true;
		}

		private static bool Compare(byte[] ad1, byte[] ad2)
		{
			if (ad1.Length != ad1.Length)
			{
				return false;
			}
			for (int i = 0; i < ad1.Length; i++)
			{
				if (ad1[i] != ad2[i])
				{
					return false;
				}
			}
			return true;
		}

		protected virtual void OnDescriptionChanged()
		{
		}

		public void SetDescription(byte[] description)
		{
			if (!Compare(_avatarDescription.Description, description))
			{
				_invisible = false;
				OnDescriptionChanged();
				_avatarDescription = new AvatarDescription(description);
				_avatarRenderer = new AvatarRenderer(_avatarDescription);
				_skeletonBuilt = false;
			}
		}

		public void SetDescription(AvatarDescription description)
		{
			if (!Compare(_avatarDescription, description))
			{
				_invisible = false;
				OnDescriptionChanged();
				_avatarDescription = description;
				_avatarRenderer = new AvatarRenderer(_avatarDescription);
				_skeletonBuilt = false;
			}
		}

		private void DrawWireframeBones(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
		{
			if (_wireFrameWorldTransforms == null)
			{
				_wireFrameWorldTransforms = new Matrix[Skeleton.Count];
			}
			Matrix.CreateRotationY((float)Math.PI);
			_wireFrameWorldTransforms[0] = _boneTransformBuffer[0] * DefaultBindPose[0];
			for (int i = 1; i < _wireFrameWorldTransforms.Length; i++)
			{
				Matrix matrix = _boneTransformBuffer[i];
				matrix.Translation = DefaultBindPose[i].Translation;
				_wireFrameWorldTransforms[i] = matrix;
			}
			_wireFrameWorldTransforms[0] *= base.LocalToWorld;
			for (int j = 1; j < _wireFrameWorldTransforms.Length; j++)
			{
				Matrix.Multiply(ref _wireFrameWorldTransforms[j], ref _wireFrameWorldTransforms[DefaultParentBones[j]], out _wireFrameWorldTransforms[j]);
			}
			if (_wireFrameVerts == null)
			{
				_wireFrameVerts = new VertexPositionColor[_wireFrameWorldTransforms.Length * 2];
			}
			_wireFrameVerts[0].Color = Color.Blue;
			_wireFrameVerts[0].Position = _wireFrameWorldTransforms[0].Translation;
			_wireFrameVerts[1] = _wireFrameVerts[0];
			for (int k = 2; k < _wireFrameWorldTransforms.Length * 2; k += 2)
			{
				_wireFrameVerts[k].Position = _wireFrameWorldTransforms[k / 2].Translation;
				_wireFrameVerts[k].Color = Color.Red;
				_wireFrameVerts[k + 1].Position = _wireFrameWorldTransforms[DefaultParentBones[k / 2]].Translation;
				_wireFrameVerts[k + 1].Color = Color.Green;
			}
			if (_wireFrameEffect == null)
			{
				_wireFrameEffect = new BasicEffect(graphicsDevice);
			}
			_wireFrameEffect.LightingEnabled = false;
			_wireFrameEffect.TextureEnabled = false;
			_wireFrameEffect.VertexColorEnabled = true;
			_wireFrameEffect.Projection = projection;
			_wireFrameEffect.View = view;
			_wireFrameEffect.World = Matrix.Identity;
			for (int l = 0; l < _wireFrameEffect.CurrentTechnique.Passes.Count; l++)
			{
				EffectPass effectPass = _wireFrameEffect.CurrentTechnique.Passes[l];
				effectPass.Apply();
				graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _wireFrameVerts, 0, _wireFrameWorldTransforms.Length);
			}
		}
	}
}
