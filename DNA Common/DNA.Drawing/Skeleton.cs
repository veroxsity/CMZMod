using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using DNA.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace DNA.Drawing
{
	public class Skeleton : ReadOnlyCollection<Bone>
	{
		public class Reader : ContentTypeReader<Skeleton>
		{
			protected override Skeleton Read(ContentReader input, Skeleton existingInstance)
			{
				if (existingInstance != null)
				{
					throw new NotImplementedException();
				}
				return Load(input);
			}
		}

		public Dictionary<string, Bone> boneLookup = new Dictionary<string, Bone>();

		private Bone[] _bones;

		public Bone this[string boneName]
		{
			get
			{
				return boneLookup[boneName];
			}
		}

		public void Reset()
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].Reset();
			}
		}

		public void Save(BinaryWriter writer)
		{
			writer.Write(base.Count);
			for (int i = 0; i < base.Count; i++)
			{
				Bone bone = base[i];
				writer.Write(bone.Name);
				writer.Write((bone.Parent == null) ? (-1) : bone.Parent.Index);
				writer.Write(bone.Transform);
			}
		}

		public static Skeleton Load(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			string[] array = new string[num];
			int[] array2 = new int[num];
			Matrix[] array3 = new Matrix[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = reader.ReadString();
				array2[i] = reader.ReadInt32();
				array3[i] = reader.ReadMatrix();
			}
			return Bone.BuildSkeleton(array3, array2, array);
		}

		public Skeleton(IList<Bone> bones)
			: base(bones)
		{
			_bones = new Bone[bones.Count];
			bones.CopyTo(_bones, 0);
			for (int i = 0; i < bones.Count; i++)
			{
				if (bones[i].Name != null)
				{
					boneLookup[bones[i].Name] = bones[i];
				}
			}
		}

		public int IndexOf(string boneName)
		{
			return boneLookup[boneName].Index;
		}

		public IList<Bone> BonesFromNames(IList<string> boneNames)
		{
			Bone[] array = new Bone[boneNames.Count];
			for (int i = 0; i < boneNames.Count; i++)
			{
				array[i] = boneLookup[boneNames[i]];
			}
			return array;
		}

		public void CopyTransformsFrom(Matrix[] sourceBoneTransforms)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i].SetTransform(sourceBoneTransforms[i]);
			}
		}

		public void CopyTransformsTo(Matrix[] destinationBoneTransforms)
		{
			int num = _bones.Length;
			for (int i = 0; i < num; i++)
			{
				_bones[i].GetTransform(out destinationBoneTransforms[i]);
			}
		}

		public void CopyAbsoluteBoneTransformsTo(Matrix[] worldBoneTransforms, Matrix localToWorld)
		{
			for (int i = 0; i < base.Count; i++)
			{
				Bone bone = base[i];
				if (bone.Parent == null)
				{
					CopyAbsoluteBoneTransformsTo(bone, worldBoneTransforms, ref localToWorld);
				}
			}
		}

		private void CopyAbsoluteBoneTransformsTo(Bone bone, Matrix[] worldBoneTransforms, ref Matrix localToWorld)
		{
			bone.EnsureTransformComposed();
			if (bone.Parent == null)
			{
				Matrix.Multiply(ref bone._transform, ref localToWorld, out worldBoneTransforms[bone.Index]);
			}
			else
			{
				Matrix.Multiply(ref bone._transform, ref worldBoneTransforms[bone.Parent.Index], out worldBoneTransforms[bone.Index]);
			}
			ReadOnlyCollection<Bone> child = bone.Children;
			int count = bone.Children.Count;
			for (int i = 0; i < count; i++)
			{
				CopyAbsoluteBoneTransformsTo(bone.Children[i], worldBoneTransforms, ref localToWorld);
			}
		}
	}
}
