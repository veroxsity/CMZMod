using System;
using System.Collections.Generic;
using DNA.CastleMinerZ.Terrain;
using DNA.Drawing;
using DNA.Drawing.Effects;
using DNA.Drawing.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.CastleMinerZ
{
	public class TorchCloud : Entity
	{
		public struct TorchOffset
		{
			public Vector3 FlameOffset;

			public Vector3 Offset;

			public Quaternion TorchRotation;

			public void SetFlameOffset(Matrix flameTrans)
			{
				FlameOffset = (flameTrans * Matrix.CreateFromQuaternion(TorchRotation) * Matrix.CreateTranslation(Offset)).Translation;
			}
		}

		private static TorchOffset[] Offsets;

		private List<TorchReference> TorchReferences = new List<TorchReference>();

		public static Model _torchModel;

		public static Texture2D _fireTexture;

		private ParticleEmitter _smokeEmitter;

		private ParticleEmitter _fireEmitter;

		private static Matrix[] instancedModelBones;

		private static ParticleEffect _smokeEffect;

		private static ParticleEffect _fireEffect;

		private bool _listsDirty = true;

		private Matrix[] torchMats = new Matrix[0];

		private VertexBuffer instanceVertexBuffer;

		private static VertexDeclaration instanceVertexDeclaration;

		static TorchCloud()
		{
			Offsets = new TorchOffset[6];
			instanceVertexDeclaration = new VertexDeclaration(new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0), new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1), new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2), new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3));
			_torchModel = CastleMinerZGame.Instance.Content.Load<Model>("Torch");
			instancedModelBones = new Matrix[_torchModel.Bones.Count];
			_torchModel.CopyAbsoluteBoneTransformsTo(instancedModelBones);
			_smokeEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("TorchSmoke");
			_fireEffect = CastleMinerZGame.Instance.Content.Load<ParticleEffect>("TorchFire");
			ModelEntity modelEntity = new ModelEntity(_torchModel);
			Matrix transform = modelEntity.Skeleton["Flame"].Transform;
			Offsets[4].Offset = new Vector3(0f, -0.5f, 0f);
			Offsets[4].TorchRotation = Quaternion.Identity;
			Offsets[4].SetFlameOffset(transform);
			Offsets[2].Offset = new Vector3(0.5f, -0.25f, 0f);
			Offsets[2].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI / 4f);
			Offsets[2].SetFlameOffset(transform);
			Offsets[1].Offset = new Vector3(0f, -0.25f, 0.5f);
			Offsets[1].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)Math.PI / 4f);
			Offsets[1].SetFlameOffset(transform);
			Offsets[0].Offset = new Vector3(-0.5f, -0.25f, 0f);
			Offsets[0].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -(float)Math.PI / 4f);
			Offsets[0].SetFlameOffset(transform);
			Offsets[3].Offset = new Vector3(0f, -0.25f, -0.5f);
			Offsets[3].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)Math.PI / 4f);
			Offsets[3].SetFlameOffset(transform);
			Offsets[5].Offset = new Vector3(0f, 0.5f, 0f);
			Offsets[5].TorchRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Math.PI);
			Offsets[5].SetFlameOffset(transform);
		}

		public TorchCloud(DNAGame game)
		{
			DrawPriority = 900;
			_smokeEmitter = _smokeEffect.CreateEmitter(game);
			_smokeEmitter.DrawPriority = 900;
			_smokeEmitter.Emitting = true;
			_fireEmitter = _fireEffect.CreateEmitter(game);
			_fireEmitter.DrawPriority = 900;
			_fireEmitter.Emitting = true;
			ModelEntity modelEntity = new ModelEntity(_torchModel);
			Matrix localToParent = modelEntity.Skeleton["Flame"].Transform * modelEntity.LocalToParent;
			_smokeEmitter.LocalToParent = localToParent;
			_fireEmitter.LocalToParent = localToParent;
			base.Children.Add(_smokeEmitter);
			base.Children.Add(_fireEmitter);
		}

		public bool ContainsTorch(Vector3 blockCenter)
		{
			for (int i = 0; i < TorchReferences.Count; i++)
			{
				if ((double)Vector3.DistanceSquared(blockCenter, TorchReferences[i].Position) < 0.0625)
				{
					return true;
				}
			}
			return false;
		}

		public void AddTorch(Vector3 blockCenter, BlockFace facing)
		{
			if (!ContainsTorch(blockCenter))
			{
				_listsDirty = true;
				TorchReferences.Add(new TorchReference(blockCenter, facing));
			}
		}

		public void RemoveTorch(Vector3 blockCenter)
		{
			for (int i = 0; i < TorchReferences.Count; i++)
			{
				if ((double)Vector3.DistanceSquared(blockCenter, TorchReferences[i].Position) < 0.0625)
				{
					int num = TorchReferences.Count - 1;
					if (i < num)
					{
						TorchReferences[i] = TorchReferences[num];
					}
					TorchReferences.RemoveAt(num);
					_listsDirty = true;
				}
			}
		}

		private void ComputeLists()
		{
			int count = TorchReferences.Count;
			if (torchMats.Length != count)
			{
				torchMats = new Matrix[count];
			}
			if (count != 0)
			{
				Matrix[] array = new Matrix[count];
				for (int i = 0; i < count; i++)
				{
					TorchReference torchReference = TorchReferences[i];
					TorchOffset torchOffset = Offsets[(int)torchReference.Facing];
					array[i] = Matrix.CreateTranslation(torchReference.Position + torchOffset.FlameOffset + new Vector3(0f, -0.5f, 0f));
					Matrix matrix = Matrix.CreateFromQuaternion(torchOffset.TorchRotation);
					matrix.Translation = torchReference.Position + torchOffset.Offset;
					torchMats[i] = matrix;
				}
				if (instanceVertexBuffer == null || torchMats.Length > instanceVertexBuffer.VertexCount)
				{
					if (instanceVertexBuffer != null)
					{
						instanceVertexBuffer.Dispose();
					}
					instanceVertexBuffer = new VertexBuffer(CastleMinerZGame.Instance.GraphicsDevice, instanceVertexDeclaration, torchMats.Length, BufferUsage.WriteOnly);
				}
				instanceVertexBuffer.SetData(torchMats, 0, count);
				ParticleEmitter fireEmitter = _fireEmitter;
				Matrix[] instances = (_smokeEmitter.Instances = array);
				fireEmitter.Instances = instances;
			}
			else
			{
				ParticleEmitter fireEmitter2 = _fireEmitter;
				Matrix[] instances2 = (_smokeEmitter.Instances = new Matrix[0]);
				fireEmitter2.Instances = instances2;
			}
			_listsDirty = false;
		}

		protected override void OnUpdate(GameTime gameTime)
		{
			if (_listsDirty)
			{
				ComputeLists();
			}
			base.OnUpdate(gameTime);
		}

		public override void Draw(GraphicsDevice device, GameTime gameTime, Matrix view, Matrix projection)
		{
			int count = TorchReferences.Count;
			if (torchMats.Length == 0)
			{
				return;
			}
			foreach (ModelMesh mesh in _torchModel.Meshes)
			{
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					device.SetVertexBuffers(new VertexBufferBinding(meshPart.VertexBuffer, meshPart.VertexOffset, 0), new VertexBufferBinding(instanceVertexBuffer, 0, 1));
					device.Indices = meshPart.IndexBuffer;
					DNAEffect dNAEffect = (DNAEffect)meshPart.Effect;
					dNAEffect.World = instancedModelBones[mesh.ParentBone.Index];
					dNAEffect.View = view;
					dNAEffect.Projection = projection;
					dNAEffect.AmbientColor = ColorF.FromARGB(1f, 0.75f, 0.75f, 0.75f);
					dNAEffect.CurrentTechnique = dNAEffect.Techniques["HardwareInstancing"];
					foreach (EffectPass pass in dNAEffect.CurrentTechnique.Passes)
					{
						pass.Apply();
						device.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount, torchMats.Length);
					}
					dNAEffect.CurrentTechnique = dNAEffect.Techniques["NoInstancing"];
				}
			}
			base.Draw(device, gameTime, view, projection);
		}
	}
}
