using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DNA.Drawing.Animation;
using DNA.Drawing.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing
{
	public class ModelEntity : Entity
	{
		protected AnimationData _animationData;

		private LayeredAnimationPlayer _animations = new LayeredAnimationPlayer(16);

		public bool ShowSkeleton;

		private BasicEffect _wireFrameEffect;

		private VertexPositionColor[] _wireFrameVerts;

		public string Technique;

		protected Matrix[] _worldBoneTransforms;

		protected Matrix[] _defaultPose;

		protected ReadOnlyCollection<Matrix> _bindPose;

		private Skeleton _skeleton;

		private bool _lighting = true;

		private Model _model;

		private bool _getWorldBones = true;

		private Vector3 _cachedColor;

		private float _cachedAlpha;

		public bool Animated
		{
			get
			{
				return _animationData != null;
			}
		}

		public LayeredAnimationPlayer Animations
		{
			get
			{
				return _animations;
			}
		}

		public ReadOnlyCollection<Matrix> BindPose
		{
			get
			{
				return _bindPose;
			}
		}

		public Matrix[] DefaultPose
		{
			get
			{
				return _defaultPose;
			}
		}

		public Matrix[] WorldBoneTransforms
		{
			get
			{
				return _worldBoneTransforms;
			}
		}

		public Skeleton Skeleton
		{
			get
			{
				return _skeleton;
			}
		}

		public bool Lighting
		{
			get
			{
				return _lighting;
			}
			set
			{
				_lighting = value;
			}
		}

		protected Model Model
		{
			get
			{
				return _model;
			}
			set
			{
				SetupModel(value);
			}
		}

		private void GetDefaultPose(Matrix[] pose)
		{
			Skeleton.CopyTransformsTo(pose);
		}

		private void AssumeDefaultPose()
		{
			Skeleton skeleton = Skeleton;
			for (int i = 0; i < skeleton.Count; i++)
			{
				skeleton[i].SetTransform(_defaultPose[i]);
			}
		}

		protected static void ChangeEffectUsedByMesh(ModelMesh mesh, Effect replacementEffect)
		{
			Dictionary<Effect, Effect> dictionary = new Dictionary<Effect, Effect>();
			foreach (Effect effect in mesh.Effects)
			{
				if (!dictionary.ContainsKey(effect))
				{
					Effect value = replacementEffect.Clone();
					dictionary[effect] = value;
				}
			}
			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				meshPart.Effect = dictionary[meshPart.Effect];
			}
		}

		protected static void ChangeEffectUsedByModel(Model model, Effect replacementEffect)
		{
			new Dictionary<Effect, Effect>();
			foreach (ModelMesh mesh in model.Meshes)
			{
				ChangeEffectUsedByMesh(mesh, replacementEffect);
			}
		}

		protected virtual Skeleton GetSkeleton()
		{
			return Bone.BuildSkeleton(_model);
		}

		protected override void OnApplyEffect(Effect sourceEffect)
		{
			ChangeEffectUsedByModel(_model, sourceEffect);
		}

		public void EnableDefaultLighting()
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.EnableDefaultLighting();
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0)
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = true;
						basicEffect.DirectionalLight1.Enabled = false;
						basicEffect.DirectionalLight2.Enabled = false;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1)
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = false;
						directionalLight = basicEffect.DirectionalLight1;
						directionalLight.DiffuseColor = DColor1;
						directionalLight.SpecularColor = SColor1;
						directionalLight.Direction = Direction1;
						directionalLight.Enabled = true;
						basicEffect.DirectionalLight2.Enabled = false;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetLighting(Vector3 ambient, Vector3 Direction0, Vector3 DColor0, Vector3 SColor0, Vector3 Direction1, Vector3 DColor1, Vector3 SColor1, Vector3 Direction2, Vector3 DColor2, Vector3 SColor2)
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.AmbientLightColor = ambient;
						DirectionalLight directionalLight = basicEffect.DirectionalLight0;
						directionalLight.DiffuseColor = DColor0;
						directionalLight.SpecularColor = SColor0;
						directionalLight.Direction = Direction0;
						directionalLight.Enabled = false;
						directionalLight = basicEffect.DirectionalLight1;
						directionalLight.DiffuseColor = DColor1;
						directionalLight.SpecularColor = SColor1;
						directionalLight.Direction = Direction1;
						directionalLight.Enabled = true;
						directionalLight = basicEffect.DirectionalLight2;
						directionalLight.DiffuseColor = DColor2;
						directionalLight.SpecularColor = SColor2;
						directionalLight.Direction = Direction2;
						directionalLight.Enabled = true;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		public void SetAlphaTest(int referenceAlpha, CompareFunction compareFunction)
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					AlphaTestEffect alphaTestEffect = (AlphaTestEffect)modelMesh.Effects[j];
					alphaTestEffect.ReferenceAlpha = referenceAlpha;
					alphaTestEffect.AlphaFunction = compareFunction;
				}
			}
		}

		public void EnablePerPixelLighting()
		{
			for (int i = 0; i < _model.Meshes.Count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				for (int j = 0; j < modelMesh.Effects.Count; j++)
				{
					if (modelMesh.Effects[j] is BasicEffect)
					{
						BasicEffect basicEffect = (BasicEffect)modelMesh.Effects[j];
						basicEffect.PreferPerPixelLighting = true;
						basicEffect.LightingEnabled = true;
					}
				}
			}
		}

		protected void AllocateBoneTransforms()
		{
			_worldBoneTransforms = new Matrix[Skeleton.Count];
			_defaultPose = new Matrix[Skeleton.Count];
		}

		public ModelEntity(Model model)
		{
			SetupModel(model);
		}

		private void SetupModel(Model model)
		{
			_model = model;
			_animationData = (AnimationData)model.Tag;
			_skeleton = GetSkeleton();
			AllocateBoneTransforms();
			GetDefaultPose(_defaultPose);
			Matrix[] array = new Matrix[Skeleton.Count];
			GetDefaultPose(array);
			_bindPose = new ReadOnlyCollection<Matrix>(array);
			Skeleton.CopyAbsoluteBoneTransformsTo(_worldBoneTransforms, base.LocalToWorld);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, IList<string> influenceBoneNames, TimeSpan blendTime)
		{
			return PlayClip(0, clipName, looping, influenceBoneNames, blendTime);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, IList<Bone> influenceBones, TimeSpan blendTime)
		{
			return PlayClip(0, clipName, looping, influenceBones, blendTime);
		}

		public AnimationPlayer PlayClip(string clipName, bool looping, TimeSpan blendTime)
		{
			return PlayClip(0, clipName, looping, blendTime);
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, IList<string> influenceBoneNames, TimeSpan blendTime)
		{
			AnimationClip clip = _animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip, Skeleton.BonesFromNames(influenceBoneNames));
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			_animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, IList<Bone> influenceBones, TimeSpan blendTime)
		{
			AnimationClip clip = _animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip, influenceBones);
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			_animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public AnimationPlayer PlayClip(int channel, string clipName, bool looping, TimeSpan blendTime)
		{
			AnimationClip clip = _animationData.AnimationClips[clipName];
			AnimationPlayer animationPlayer = new AnimationPlayer(clip);
			animationPlayer.Looping = looping;
			animationPlayer.Play();
			_animations.PlayAnimation(channel, animationPlayer, blendTime);
			return animationPlayer;
		}

		public void DumpAnimationNames()
		{
			foreach (string key in _animationData.AnimationClips.Keys)
			{
				string text = key;
			}
		}

		public override BoundingSphere GetLocalBoundingSphere()
		{
			BoundingSphere boundingSphere = _model.Meshes[0].BoundingSphere;
			for (int i = 1; i < _model.Meshes.Count; i++)
			{
				boundingSphere = BoundingSphere.CreateMerged(boundingSphere, _model.Meshes[i].BoundingSphere);
			}
			return boundingSphere;
		}

		public override BoundingBox GetAABB()
		{
			Vector3 vector = new Vector3(GetLocalBoundingSphere().Radius);
			Vector3 worldPosition = base.WorldPosition;
			return new BoundingBox(worldPosition - vector, worldPosition + vector);
		}

		protected override void OnMoved()
		{
			_getWorldBones = true;
			base.OnMoved();
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (Animated)
			{
				AssumeDefaultPose();
				_animations.Update(gameTime.ElapsedGameTime, Skeleton);
				Skeleton.CopyAbsoluteBoneTransformsTo(_worldBoneTransforms, base.LocalToWorld);
			}
			else if (_getWorldBones)
			{
				Skeleton.CopyAbsoluteBoneTransformsTo(_worldBoneTransforms, base.LocalToWorld);
				_getWorldBones = false;
			}
			base.OnUpdate(gameTime);
		}

		protected virtual EffectTechnique GetEffectTechnique(Effect effect)
		{
			return effect.Techniques[0];
		}

		protected virtual bool SetEffectParams(ModelMesh mesh, Effect effect, GameTime gameTime, Matrix world, Matrix view, Matrix projection)
		{
			if (effect is IEffectMatrices)
			{
				IEffectMatrices effectMatrices = (IEffectMatrices)effect;
				effectMatrices.World = world;
				effectMatrices.View = view;
				effectMatrices.Projection = projection;
			}
			if (effect is IEffectTime)
			{
				IEffectTime effectTime = (IEffectTime)effect;
				effectTime.ElaspedTime = gameTime.ElapsedGameTime;
				effectTime.TotalTime = gameTime.TotalGameTime;
			}
			if (effect is IEffectColor)
			{
				IEffectColor effectColor = (IEffectColor)effect;
				if (EntityColor.HasValue)
				{
					effectColor.DiffuseColor = EntityColor.Value;
				}
			}
			if (effect is BasicEffect)
			{
				BasicEffect basicEffect = (BasicEffect)effect;
				if (EntityColor.HasValue)
				{
					Color value = EntityColor.Value;
					basicEffect.DiffuseColor = _cachedColor;
					basicEffect.Alpha = _cachedAlpha;
				}
			}
			else if (effect is AlphaTestEffect)
			{
				AlphaTestEffect alphaTestEffect = (AlphaTestEffect)effect;
				if (EntityColor.HasValue)
				{
					Color value2 = EntityColor.Value;
					alphaTestEffect.DiffuseColor = _cachedColor;
					alphaTestEffect.Alpha = _cachedAlpha;
				}
			}
			if (Technique == null)
			{
				effect.CurrentTechnique = effect.Techniques[0];
			}
			else
			{
				effect.CurrentTechnique = effect.Techniques[Technique];
			}
			return true;
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			if (EntityColor.HasValue && EntityColor.HasValue)
			{
				Color value = EntityColor.Value;
				_cachedColor = value.ToVector3();
				_cachedAlpha = (float)(int)value.A / 255f;
			}
			int count = _model.Meshes.Count;
			for (int i = 0; i < count; i++)
			{
				ModelMesh modelMesh = _model.Meshes[i];
				Matrix world = _worldBoneTransforms[modelMesh.ParentBone.Index];
				int count2 = modelMesh.Effects.Count;
				int num = 0;
				while (true)
				{
					if (num < count2)
					{
						if (!SetEffectParams(modelMesh, modelMesh.Effects[num], gameTime, world, view, projection))
						{
							break;
						}
						num++;
						continue;
					}
					DrawMesh(device, modelMesh);
					break;
				}
			}
			base.Draw(device, gameTime, view, projection);
		}

		protected virtual void DrawMesh(GraphicsDevice device, ModelMesh mesh)
		{
			mesh.Draw();
		}

		protected void DrawWireframeBones(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
		{
			Matrix[] worldBoneTransforms = _worldBoneTransforms;
			if (_wireFrameVerts == null)
			{
				_wireFrameVerts = new VertexPositionColor[worldBoneTransforms.Length * 2];
			}
			_wireFrameVerts[0].Color = Color.Blue;
			_wireFrameVerts[0].Position = worldBoneTransforms[0].Translation;
			_wireFrameVerts[1] = _wireFrameVerts[0];
			for (int i = 2; i < worldBoneTransforms.Length * 2; i += 2)
			{
				_wireFrameVerts[i].Position = worldBoneTransforms[i / 2].Translation;
				_wireFrameVerts[i].Color = Color.Red;
				_wireFrameVerts[i + 1].Position = worldBoneTransforms[Skeleton[i / 2].Parent.Index].Translation;
				_wireFrameVerts[i + 1].Color = Color.Green;
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
			for (int j = 0; j < _wireFrameEffect.CurrentTechnique.Passes.Count; j++)
			{
				EffectPass effectPass = _wireFrameEffect.CurrentTechnique.Passes[j];
				effectPass.Apply();
				graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, _wireFrameVerts, 0, worldBoneTransforms.Length);
			}
		}
	}
}
