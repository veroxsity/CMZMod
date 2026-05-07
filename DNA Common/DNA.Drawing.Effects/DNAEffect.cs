using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace DNA.Drawing.Effects
{
	public class DNAEffect : Effect, IEffectMatrices, IEffectTime, IEffectColor, IEffectTextured
	{
		public enum EffectValueTypes : byte
		{
			intValue,
			stringValue,
			boolValue,
			floatValue,
			Vector2Value,
			Vector3Value,
			Vector4Value,
			MatrixValue
		}

		public class Reader : ContentTypeReader<DNAEffect>
		{
			protected override DNAEffect Read(ContentReader input, DNAEffect existingInstance)
			{
				Effect effect = input.ReadExternalReference<Effect>();
				int num = input.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					string name = input.ReadString();
					Texture value = input.ReadExternalReference<Texture>();
					effect.Parameters[name].SetValue(value);
				}
				int num2 = input.ReadInt32();
				for (int j = 0; j < num2; j++)
				{
					string name2 = input.ReadString();
					EffectParameter effectParameter = effect.Parameters[name2];
					EffectValueTypes effectValueTypes = (EffectValueTypes)input.ReadByte();
					int num3 = input.ReadInt32();
					if (effectParameter == null)
					{
						if (num3 == 0)
						{
							switch (effectValueTypes)
							{
							case EffectValueTypes.intValue:
								input.ReadInt32();
								break;
							case EffectValueTypes.stringValue:
								input.ReadString();
								break;
							case EffectValueTypes.boolValue:
								input.ReadBoolean();
								break;
							case EffectValueTypes.floatValue:
								input.ReadSingle();
								break;
							case EffectValueTypes.Vector2Value:
								input.ReadVector2();
								break;
							case EffectValueTypes.Vector3Value:
								input.ReadVector3();
								break;
							case EffectValueTypes.Vector4Value:
								input.ReadVector4();
								break;
							case EffectValueTypes.MatrixValue:
								input.ReadMatrix();
								break;
							default:
								throw new Exception("Unsupported Value Type");
							}
							continue;
						}
						switch (effectValueTypes)
						{
						case EffectValueTypes.intValue:
						{
							int[] array6 = new int[num3];
							for (int num5 = 0; num5 < array6.Length; num5++)
							{
								array6[num5] = input.ReadInt32();
							}
							break;
						}
						case EffectValueTypes.boolValue:
						{
							bool[] array2 = new bool[num3];
							for (int l = 0; l < array2.Length; l++)
							{
								array2[l] = input.ReadBoolean();
							}
							break;
						}
						case EffectValueTypes.floatValue:
						{
							float[] array4 = new float[num3];
							for (int n = 0; n < array4.Length; n++)
							{
								array4[n] = input.ReadSingle();
							}
							break;
						}
						case EffectValueTypes.Vector2Value:
						{
							Vector2[] array7 = new Vector2[num3];
							for (int num6 = 0; num6 < array7.Length; num6++)
							{
								array7[num6] = input.ReadVector2();
							}
							break;
						}
						case EffectValueTypes.Vector3Value:
						{
							Vector3[] array5 = new Vector3[num3];
							for (int num4 = 0; num4 < array5.Length; num4++)
							{
								array5[num4] = input.ReadVector3();
							}
							break;
						}
						case EffectValueTypes.Vector4Value:
						{
							Vector4[] array3 = new Vector4[num3];
							for (int m = 0; m < array3.Length; m++)
							{
								array3[m] = input.ReadVector4();
							}
							break;
						}
						case EffectValueTypes.MatrixValue:
						{
							Matrix[] array = new Matrix[num3];
							for (int k = 0; k < array.Length; k++)
							{
								array[k] = input.ReadMatrix();
							}
							break;
						}
						default:
							throw new Exception("Unsupported Value Type");
						}
						continue;
					}
					if (num3 == 0)
					{
						switch (effectValueTypes)
						{
						case EffectValueTypes.intValue:
							effectParameter.SetValue(input.ReadInt32());
							break;
						case EffectValueTypes.stringValue:
							effectParameter.SetValue(input.ReadString());
							break;
						case EffectValueTypes.boolValue:
							effectParameter.SetValue(input.ReadBoolean());
							break;
						case EffectValueTypes.floatValue:
							effectParameter.SetValue(input.ReadSingle());
							break;
						case EffectValueTypes.Vector2Value:
							effectParameter.SetValue(input.ReadVector2());
							break;
						case EffectValueTypes.Vector3Value:
							effectParameter.SetValue(input.ReadVector3());
							break;
						case EffectValueTypes.Vector4Value:
						{
							Vector4 value2 = input.ReadVector4();
							if (effectParameter.ColumnCount == 2)
							{
								effectParameter.SetValue(new Vector2(value2.X, value2.Y));
							}
							else if (effectParameter.ColumnCount == 3)
							{
								effectParameter.SetValue(new Vector3(value2.X, value2.Y, value2.Z));
							}
							else
							{
								effectParameter.SetValue(value2);
							}
							break;
						}
						case EffectValueTypes.MatrixValue:
							effect.Parameters[name2].SetValue(input.ReadMatrix());
							break;
						default:
							throw new Exception("Unsupported Value Type");
						}
						continue;
					}
					switch (effectValueTypes)
					{
					case EffectValueTypes.intValue:
					{
						int[] array9 = new int[num3];
						for (int num8 = 0; num8 < array9.Length; num8++)
						{
							array9[num8] = input.ReadInt32();
						}
						effectParameter.SetValue(array9);
						break;
					}
					case EffectValueTypes.boolValue:
					{
						bool[] array13 = new bool[num3];
						for (int num12 = 0; num12 < array13.Length; num12++)
						{
							array13[num12] = input.ReadBoolean();
						}
						effectParameter.SetValue(array13);
						break;
					}
					case EffectValueTypes.floatValue:
					{
						float[] array10 = new float[num3];
						for (int num9 = 0; num9 < array10.Length; num9++)
						{
							array10[num9] = input.ReadSingle();
						}
						effectParameter.SetValue(array10);
						break;
					}
					case EffectValueTypes.Vector2Value:
					{
						Vector2[] array12 = new Vector2[num3];
						for (int num11 = 0; num11 < array12.Length; num11++)
						{
							array12[num11] = input.ReadVector2();
						}
						effectParameter.SetValue(array12);
						break;
					}
					case EffectValueTypes.Vector3Value:
					{
						Vector3[] array14 = new Vector3[num3];
						for (int num13 = 0; num13 < array14.Length; num13++)
						{
							array14[num13] = input.ReadVector3();
						}
						effectParameter.SetValue(array14);
						break;
					}
					case EffectValueTypes.Vector4Value:
					{
						Vector4[] array11 = new Vector4[num3];
						for (int num10 = 0; num10 < array11.Length; num10++)
						{
							array11[num10] = input.ReadVector4();
						}
						effectParameter.SetValue(array11);
						break;
					}
					case EffectValueTypes.MatrixValue:
					{
						Matrix[] array8 = new Matrix[num3];
						for (int num7 = 0; num7 < array8.Length; num7++)
						{
							array8[num7] = input.ReadMatrix();
						}
						effectParameter.SetValue(array8);
						break;
					}
					default:
						throw new Exception("Unsupported Value Type");
					}
				}
				return new DNAEffect(effect);
			}
		}

		[Flags]
		private enum ParamFlags : uint
		{
			None = 0u,
			World = 1u,
			View = 2u,
			Projection = 4u,
			Time = 8u,
			ElaspedTime = 0x10u,
			Diffuse = 0x20u,
			Ambient = 0x40u,
			Emissive = 0x80u,
			Specular = 0x100u,
			DiffuseMap = 0x200u,
			OpacityMap = 0x400u,
			SpecularMap = 0x800u,
			NormalMap = 0x1000u,
			DisplacementMap = 0x2000u,
			LightMap = 0x4000u,
			ReflectionMap = 0x8000u,
			MatrixFlags = 7u,
			TimeFlags = 0x18u,
			ColorFlags = 0x1E0u,
			MapFlags = 0xFE00u,
			AllFlags = uint.MaxValue
		}

		private ParamFlags _alteredParams;

		private EffectParameter _worldParam;

		private EffectParameter _worldInvParam;

		private EffectParameter _worldInvTrnParam;

		private EffectParameter _worldTrnParam;

		private EffectParameter _viewParam;

		private EffectParameter _viewTrnParam;

		private EffectParameter _viewInvParam;

		private EffectParameter _viewInvTrnParam;

		private EffectParameter _projParam;

		private EffectParameter _projTrnParam;

		private EffectParameter _projInvParam;

		private EffectParameter _projInvTrnParam;

		private EffectParameter _worldViewParam;

		private EffectParameter _worldViewInvParam;

		private EffectParameter _worldViewInvTrnParam;

		private EffectParameter _worldViewProjParam;

		private EffectParameter _worldViewProjInvParam;

		private EffectParameter _worldViewProjInvTrnParam;

		private EffectParameter _totalTimeParam;

		private EffectParameter _elaspedTimeParam;

		private EffectParameter _diffuseColorParam;

		private EffectParameter _ambientColorParam;

		private EffectParameter _emissiveColorParam;

		private EffectParameter _specularColorParam;

		private EffectParameter _diffuseMapParam;

		private EffectParameter _opacityMapParam;

		private EffectParameter _specularMapParam;

		private EffectParameter _normalMapParam;

		private EffectParameter _displacementMapParam;

		private EffectParameter _lightMapParam;

		private EffectParameter _reflectionMapParam;

		private Matrix _world;

		private Matrix _view;

		private Matrix _proj;

		private TimeSpan _elaspedTime;

		private TimeSpan _totalTime;

		private ColorF _diffuseColor = Color.White;

		private ColorF _ambientColor = Color.Gray;

		private ColorF _specularColor = Color.White;

		private ColorF _emissiveColor;

		private Texture _diffuseMap;

		private Texture _opacityMap;

		private Texture _specularMap;

		private Texture _normalMap;

		private Texture _displacementMap;

		private Texture _lightMap;

		private Texture _reflectionMap;

		public Matrix Projection
		{
			get
			{
				return _proj;
			}
			set
			{
				_proj = value;
				_alteredParams |= ParamFlags.Projection;
			}
		}

		public Matrix View
		{
			get
			{
				return _view;
			}
			set
			{
				_view = value;
				_alteredParams |= ParamFlags.View;
			}
		}

		public Matrix World
		{
			get
			{
				return _world;
			}
			set
			{
				_world = value;
				_alteredParams |= ParamFlags.World;
			}
		}

		public TimeSpan TotalTime
		{
			get
			{
				return _totalTime;
			}
			set
			{
				_totalTime = value;
				if (_totalTimeParam != null)
				{
					_alteredParams |= ParamFlags.Time;
				}
			}
		}

		public TimeSpan ElaspedTime
		{
			get
			{
				return _elaspedTime;
			}
			set
			{
				_elaspedTime = value;
				if (_elaspedTimeParam != null)
				{
					_alteredParams |= ParamFlags.ElaspedTime;
				}
			}
		}

		public ColorF DiffuseColor
		{
			get
			{
				return _diffuseColor;
			}
			set
			{
				_diffuseColor = value;
				if (_diffuseColorParam != null)
				{
					_alteredParams |= ParamFlags.Diffuse;
				}
			}
		}

		public ColorF AmbientColor
		{
			get
			{
				return _ambientColor;
			}
			set
			{
				_ambientColor = value;
				if (_ambientColorParam != null)
				{
					_alteredParams |= ParamFlags.Ambient;
				}
			}
		}

		public ColorF SpecularColor
		{
			get
			{
				return _specularColor;
			}
			set
			{
				_specularColor = value;
				if (_specularColorParam != null)
				{
					_alteredParams |= ParamFlags.Specular;
				}
			}
		}

		public ColorF EmissiveColor
		{
			get
			{
				return _emissiveColor;
			}
			set
			{
				_emissiveColor = value;
				if (_emissiveColorParam != null)
				{
					_alteredParams |= ParamFlags.Emissive;
				}
			}
		}

		public Texture DiffuseMap
		{
			get
			{
				return _diffuseMap;
			}
			set
			{
				_diffuseMap = value;
				if (_diffuseMap != null)
				{
					if (_diffuseMapParam != null)
					{
						_alteredParams |= ParamFlags.DiffuseMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.DiffuseMap;
				}
			}
		}

		public Texture OpacityMap
		{
			get
			{
				return _opacityMap;
			}
			set
			{
				_opacityMap = value;
				if (_opacityMap != null)
				{
					if (_opacityMapParam != null)
					{
						_alteredParams |= ParamFlags.OpacityMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.OpacityMap;
				}
			}
		}

		public Texture SpecularMap
		{
			get
			{
				return _specularMap;
			}
			set
			{
				_specularMap = value;
				if (_specularMap != null)
				{
					if (_specularMapParam != null)
					{
						_alteredParams |= ParamFlags.SpecularMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.SpecularMap;
				}
			}
		}

		public Texture NormalMap
		{
			get
			{
				return _normalMap;
			}
			set
			{
				_normalMap = value;
				if (_normalMap != null)
				{
					if (_normalMapParam != null)
					{
						_alteredParams |= ParamFlags.NormalMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.NormalMap;
				}
			}
		}

		public Texture DisplacementMap
		{
			get
			{
				return _displacementMap;
			}
			set
			{
				_displacementMap = value;
				if (_displacementMap != null)
				{
					if (_displacementMapParam != null)
					{
						_alteredParams |= ParamFlags.DisplacementMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.DisplacementMap;
				}
			}
		}

		public Texture LightMap
		{
			get
			{
				return _lightMap;
			}
			set
			{
				_lightMap = value;
				if (_lightMap != null)
				{
					if (_lightMapParam != null)
					{
						_alteredParams |= ParamFlags.LightMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.LightMap;
				}
			}
		}

		public Texture ReflectionMap
		{
			get
			{
				return _reflectionMap;
			}
			set
			{
				_reflectionMap = value;
				if (_reflectionMap != null)
				{
					if (_reflectionMapParam != null)
					{
						_alteredParams |= ParamFlags.ReflectionMap;
					}
				}
				else
				{
					_alteredParams &= ~ParamFlags.ReflectionMap;
				}
			}
		}

		public DNAEffect(DNAEffect cloneSource)
			: base(cloneSource)
		{
			SetupParams();
		}

		public DNAEffect(Effect cloneSource)
			: base(cloneSource)
		{
			SetupParams();
		}

		public void SetupParams()
		{
			_worldParam = base.Parameters.GetParameterBySemantic("WORLD");
			_worldInvParam = base.Parameters.GetParameterBySemantic("WORLDI");
			_worldInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDIT");
			_worldTrnParam = base.Parameters.GetParameterBySemantic("WORLDT");
			_viewParam = base.Parameters.GetParameterBySemantic("VIEW");
			_viewTrnParam = base.Parameters.GetParameterBySemantic("VIEWT");
			_viewInvParam = base.Parameters.GetParameterBySemantic("VIEWI");
			_viewInvTrnParam = base.Parameters.GetParameterBySemantic("VIEWIT");
			_projParam = base.Parameters.GetParameterBySemantic("PROJECTION");
			_projTrnParam = base.Parameters.GetParameterBySemantic("PROJECTIONT");
			_projInvParam = base.Parameters.GetParameterBySemantic("PROJECTIONI");
			_projInvTrnParam = base.Parameters.GetParameterBySemantic("PROJECTIONIT");
			_worldViewParam = base.Parameters.GetParameterBySemantic("WORLDVIEW");
			_worldViewInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWI");
			_worldViewInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWIT");
			_worldViewProjParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJ");
			if (_worldViewProjParam == null)
			{
				_worldViewProjParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTION");
			}
			_worldViewProjInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJI");
			if (_worldViewProjInvParam == null)
			{
				_worldViewProjInvParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTIONI");
			}
			_worldViewProjInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJIT");
			if (_worldViewProjInvTrnParam == null)
			{
				_worldViewProjInvTrnParam = base.Parameters.GetParameterBySemantic("WORLDVIEWPROJECTIONIT");
			}
			_totalTimeParam = base.Parameters.GetParameterBySemantic("TIMETOTAL");
			if (_totalTimeParam == null)
			{
				_totalTimeParam = base.Parameters.GetParameterBySemantic("TIME");
			}
			_elaspedTimeParam = base.Parameters.GetParameterBySemantic("TIMEELASPED");
			_diffuseColorParam = base.Parameters.GetParameterBySemantic("DIFFUSECOLOR");
			_ambientColorParam = base.Parameters.GetParameterBySemantic("AMBIENTCOLOR");
			_emissiveColorParam = base.Parameters.GetParameterBySemantic("EMISSIVECOLOR");
			_specularColorParam = base.Parameters.GetParameterBySemantic("SPECULARCOLOR");
			if (_diffuseColorParam != null)
			{
				_diffuseColor = GetColor(_diffuseColorParam);
			}
			if (_ambientColorParam != null)
			{
				_ambientColor = GetColor(_ambientColorParam);
			}
			if (_emissiveColorParam != null)
			{
				_emissiveColor = GetColor(_emissiveColorParam);
			}
			if (_specularColorParam != null)
			{
				_specularColor = GetColor(_specularColorParam);
			}
			_diffuseColorParam = base.Parameters.GetParameterBySemantic("DIFFUSECOLOR");
			_diffuseMapParam = base.Parameters.GetParameterBySemantic("DIFFUSEMAP");
			_opacityMapParam = base.Parameters.GetParameterBySemantic("OPACITYMAP");
			_specularMapParam = base.Parameters.GetParameterBySemantic("SPECULARMAP");
			_normalMapParam = base.Parameters.GetParameterBySemantic("NORMALMAP");
			if (_normalMapParam == null)
			{
				_normalMapParam = base.Parameters.GetParameterBySemantic("BUMPMAP");
			}
			_displacementMapParam = base.Parameters.GetParameterBySemantic("DISPLACEMENTMAP");
			_lightMapParam = base.Parameters.GetParameterBySemantic("LIGHTMAP");
			_reflectionMapParam = base.Parameters.GetParameterBySemantic("REFLECTIONMAP");
			_diffuseMap = GetTexture(_diffuseMapParam);
			_opacityMap = GetTexture(_opacityMapParam);
			_specularMap = GetTexture(_specularMapParam);
			_normalMap = GetTexture(_normalMapParam);
			_displacementMap = GetTexture(_displacementMapParam);
			_lightMap = GetTexture(_lightMapParam);
			_reflectionMap = GetTexture(_reflectionMapParam);
		}

		private ColorF GetColor(EffectParameter param)
		{
			if (param.ColumnCount == 3)
			{
				return ColorF.FromVector3(param.GetValueVector3());
			}
			if (param.ColumnCount == 4)
			{
				return ColorF.FromVector4(param.GetValueVector4());
			}
			throw new Exception("Bad Color Value:" + param.ColumnCount);
		}

		private Texture GetTexture(EffectParameter param)
		{
			if (param == null)
			{
				return null;
			}
			switch (param.ParameterType)
			{
			case EffectParameterType.Texture:
				return param.GetValueTexture2D();
			case EffectParameterType.Texture1D:
				return null;
			case EffectParameterType.Texture2D:
				return param.GetValueTexture2D();
			case EffectParameterType.Texture3D:
				return param.GetValueTexture3D();
			case EffectParameterType.TextureCube:
				return param.GetValueTextureCube();
			default:
				return null;
			}
		}

		public override Effect Clone()
		{
			return new DNAEffect(this);
		}

		protected override void OnApply()
		{
			if ((_alteredParams & ParamFlags.MatrixFlags) != ParamFlags.None)
			{
				if (_worldParam != null)
				{
					_worldParam.SetValue(_world);
				}
				if (_worldInvParam != null || _worldInvTrnParam != null)
				{
					Matrix result;
					Matrix.Invert(ref _world, out result);
					if (_worldInvParam != null)
					{
						_worldInvParam.SetValue(result);
					}
					if (_worldInvTrnParam != null)
					{
						_worldInvTrnParam.SetValue(Matrix.Transpose(result));
					}
				}
				if (_worldTrnParam != null)
				{
					_worldTrnParam.SetValue(Matrix.Transpose(_world));
				}
				if (_viewParam != null)
				{
					_viewParam.SetValue(_view);
				}
				if (_viewInvParam != null || _viewInvTrnParam != null)
				{
					Matrix result2;
					Matrix.Invert(ref _view, out result2);
					if (_viewInvParam != null)
					{
						_viewInvParam.SetValue(result2);
					}
					if (_viewInvTrnParam != null)
					{
						_viewInvTrnParam.SetValue(Matrix.Transpose(result2));
					}
				}
				if (_viewTrnParam != null)
				{
					_viewTrnParam.SetValue(Matrix.Transpose(_view));
				}
				if (_projParam != null)
				{
					_projParam.SetValue(_proj);
				}
				if (_projInvParam != null || _projInvTrnParam != null)
				{
					Matrix result3;
					Matrix.Invert(ref _proj, out result3);
					if (_projInvParam != null)
					{
						_projInvParam.SetValue(result3);
					}
					if (_projInvTrnParam != null)
					{
						_projInvTrnParam.SetValue(Matrix.Transpose(result3));
					}
				}
				if (_projTrnParam != null)
				{
					_projTrnParam.SetValue(Matrix.Transpose(_proj));
				}
				if (_worldViewParam != null || _worldViewInvParam != null || _worldViewInvTrnParam != null || _worldViewProjParam != null || _worldViewProjInvParam != null || _worldViewProjInvTrnParam != null)
				{
					Matrix result4;
					Matrix.Multiply(ref _world, ref _view, out result4);
					if (_worldViewParam != null)
					{
						_worldViewParam.SetValue(result4);
					}
					if (_worldViewInvParam != null || _worldViewInvTrnParam != null)
					{
						Matrix matrix = Matrix.Invert(result4);
						if (_worldViewInvParam != null)
						{
							_worldViewInvParam.SetValue(matrix);
						}
						if (_worldViewInvTrnParam != null)
						{
							_worldViewInvTrnParam.SetValue(Matrix.Transpose(matrix));
						}
					}
					if (_worldViewProjParam != null || _worldViewProjInvParam != null || _worldViewProjInvTrnParam != null)
					{
						Matrix result5;
						Matrix.Multiply(ref result4, ref _proj, out result5);
						if (_worldViewProjParam != null)
						{
							_worldViewProjParam.SetValue(result5);
						}
						if (_worldViewProjInvParam != null || _worldViewProjInvTrnParam != null)
						{
							Matrix matrix2 = Matrix.Invert(result5);
							if (_worldViewProjInvParam != null)
							{
								_worldViewProjInvParam.SetValue(matrix2);
							}
							if (_worldViewProjInvTrnParam != null)
							{
								_worldViewProjInvTrnParam.SetValue(Matrix.Transpose(matrix2));
							}
						}
					}
				}
			}
			if (_totalTimeParam != null)
			{
				_totalTimeParam.SetValue((float)_totalTime.TotalSeconds);
			}
			if (_elaspedTimeParam != null)
			{
				_elaspedTimeParam.SetValue((float)_elaspedTime.TotalSeconds);
			}
			if ((_alteredParams & ParamFlags.ColorFlags) != ParamFlags.None)
			{
				if ((_alteredParams & ParamFlags.Diffuse) != ParamFlags.None)
				{
					SetColor(_diffuseColorParam, _diffuseColor);
				}
				if ((_alteredParams & ParamFlags.Ambient) != ParamFlags.None)
				{
					SetColor(_ambientColorParam, _ambientColor);
				}
				if ((_alteredParams & ParamFlags.Specular) != ParamFlags.None)
				{
					SetColor(_specularColorParam, _specularColor);
				}
				if ((_alteredParams & ParamFlags.Emissive) != ParamFlags.None)
				{
					SetColor(_emissiveColorParam, _emissiveColor);
				}
			}
			if ((_alteredParams & ParamFlags.MapFlags) != ParamFlags.None)
			{
				if (_diffuseMapParam != null && _diffuseMap != null)
				{
					_diffuseMapParam.SetValue(_diffuseMap);
				}
				if (_opacityMapParam != null && _opacityMap != null)
				{
					_opacityMapParam.SetValue(_opacityMap);
				}
				if (_specularMapParam != null && _specularMap != null)
				{
					_specularMapParam.SetValue(_specularMap);
				}
				if (_normalMapParam != null && _normalMap != null)
				{
					_normalMapParam.SetValue(_normalMap);
				}
				if (_displacementMapParam != null && _displacementMap != null)
				{
					_displacementMapParam.SetValue(_displacementMap);
				}
				if (_lightMapParam != null && _lightMap != null)
				{
					_lightMapParam.SetValue(_lightMap);
				}
				if (_reflectionMapParam != null && _reflectionMap != null)
				{
					_reflectionMapParam.SetValue(_reflectionMap);
				}
			}
			base.OnApply();
			_alteredParams = ParamFlags.None;
		}

		private void SetColor(EffectParameter param, ColorF color)
		{
			if (param == null)
			{
				return;
			}
			if (param.ColumnCount == 3)
			{
				param.SetValue(color.ToVector3());
				return;
			}
			if (param.ColumnCount == 4)
			{
				param.SetValue(color.ToVector4());
				return;
			}
			throw new Exception("Bad Color Value:" + param.Name);
		}
	}
}
