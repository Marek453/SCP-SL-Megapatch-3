using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public abstract class Projection : ScriptableObject
	{
		private bool marked;

		protected Material material;

		protected ProjectionProperty[] properties;

		[SerializeField]
		protected ProjectionType type;

		[SerializeField]
		protected bool instanced;

		[SerializeField]
		protected bool forceForward;

		[SerializeField]
		protected int priority;

		[SerializeField]
		protected TransparencyType transparencyType;

		[SerializeField]
		protected float cutoff = 0.2f;

		[SerializeField]
		protected Vector2 tiling;

		[SerializeField]
		protected Vector2 offset;

		[SerializeField]
		protected MaskMethod maskMethod;

		[SerializeField]
		protected bool[] masks = new bool[4];

		[SerializeField]
		protected float projectionLimit = 80f;

		public int _Cutoff;

		public int _TilingOffset;

		public int _MaskBase;

		public int _MaskLayers;

		protected int _NormalCutoff;

		public Material Mat
		{
			get
			{
				if (SupportedRendering == RenderingPaths.Forward || DynamicDecals.System.SystemPath == SystemPath.Forward || ForceForward)
				{
					return Forward;
				}
				switch (TransparencyType)
				{
				case TransparencyType.Cutout:
					return DeferredOpaque;
				case TransparencyType.Blend:
					return DeferredTransparent;
				default:
					return null;
				}
			}
		}

		private Material Forward
		{
			get
			{
				switch (DynamicDecals.System.Settings.Replacement)
				{
				case ShaderReplacementType.Mobile:
					return MobileForward;
				case ShaderReplacementType.VR:
					return PackedForward;
				default:
					return StandardForward;
				}
			}
		}

		private Material DeferredOpaque
		{
			get
			{
				switch (DynamicDecals.System.Settings.Replacement)
				{
				case ShaderReplacementType.Mobile:
					return MobileDeferredOpaque;
				case ShaderReplacementType.VR:
					return PackedDeferredOpaque;
				default:
					return StandardDeferredOpaque;
				}
			}
		}

		private Material DeferredTransparent
		{
			get
			{
				switch (DynamicDecals.System.Settings.Replacement)
				{
				case ShaderReplacementType.Mobile:
					return MobileDeferredTransparent;
				case ShaderReplacementType.VR:
					return PackedDeferredTransparent;
				default:
					return StandardDeferredTransparent;
				}
			}
		}

		public virtual Material MobileForward
		{
			get
			{
				return null;
			}
		}

		public virtual Material MobileDeferredOpaque
		{
			get
			{
				return null;
			}
		}

		public virtual Material MobileDeferredTransparent
		{
			get
			{
				return null;
			}
		}

		public virtual Material StandardForward
		{
			get
			{
				return null;
			}
		}

		public virtual Material StandardDeferredOpaque
		{
			get
			{
				return null;
			}
		}

		public virtual Material StandardDeferredTransparent
		{
			get
			{
				return null;
			}
		}

		public virtual Material PackedForward
		{
			get
			{
				return null;
			}
		}

		public virtual Material PackedDeferredOpaque
		{
			get
			{
				return null;
			}
		}

		public virtual Material PackedDeferredTransparent
		{
			get
			{
				return null;
			}
		}

		public abstract RenderingPaths SupportedRendering { get; }

		public bool Valid
		{
			get
			{
				if (SupportedRendering == RenderingPaths.Deferred && DynamicDecals.System.SystemPath == SystemPath.Forward)
				{
					return false;
				}
				return true;
			}
		}

		public ProjectionType ProjectionType
		{
			get
			{
				return type;
			}
			set
			{
				if (type != value)
				{
					type = value;
					Mark();
				}
			}
		}

		public int Priority
		{
			get
			{
				return priority;
			}
			set
			{
				if (priority != value)
				{
					priority = value;
					Mark();
				}
			}
		}

		public TransparencyType TransparencyType
		{
			get
			{
				return transparencyType;
			}
			set
			{
				if (transparencyType != value)
				{
					transparencyType = value;
					Mark();
				}
			}
		}

		public float Cutoff
		{
			get
			{
				return cutoff;
			}
			set
			{
				if (cutoff != value)
				{
					cutoff = value;
					Mark();
				}
			}
		}

		public Vector2 Tiling
		{
			get
			{
				return tiling;
			}
			set
			{
				if (tiling != value)
				{
					tiling = value;
					Mark();
				}
			}
		}

		public Vector2 Offset
		{
			get
			{
				return offset;
			}
			set
			{
				if (offset != value)
				{
					offset = value;
					Mark();
				}
			}
		}

		public MaskMethod MaskMethod
		{
			get
			{
				return maskMethod;
			}
			set
			{
				if (maskMethod != value)
				{
					maskMethod = value;
					Mark();
				}
			}
		}

		public bool MaskLayer1
		{
			get
			{
				return masks[0];
			}
			set
			{
				if (masks[0] != value)
				{
					masks[0] = value;
					Mark();
				}
			}
		}

		public bool MaskLayer2
		{
			get
			{
				return masks[1];
			}
			set
			{
				if (masks[1] != value)
				{
					masks[1] = value;
					Mark();
				}
			}
		}

		public bool MaskLayer3
		{
			get
			{
				return masks[2];
			}
			set
			{
				if (masks[2] != value)
				{
					masks[2] = value;
					Mark();
				}
			}
		}

		public bool MaskLayer4
		{
			get
			{
				return masks[3];
			}
			set
			{
				if (masks[3] != value)
				{
					masks[3] = value;
					Mark();
				}
			}
		}

		public float ProjectionLimit
		{
			get
			{
				return projectionLimit;
			}
			set
			{
				if (projectionLimit != value)
				{
					projectionLimit = Mathf.Clamp(value, 0f, 180f);
					Mark();
				}
			}
		}

		public bool Instanced
		{
			get
			{
				return DynamicDecals.System.Instanced && instanced;
			}
			set
			{
				instanced = value;
			}
		}

		public abstract int InstanceLimit { get; }

		public bool ForceForward
		{
			get
			{
				return forceForward;
			}
			set
			{
				forceForward = value;
			}
		}

		public ProjectionProperty[] Properties
		{
			get
			{
				UpdateProperties();
				return properties;
			}
		}

		protected Material MaterialFromShader(Shader p_Shader)
		{
			if (material == null)
			{
				material = new Material(p_Shader);
				material.hideFlags = HideFlags.DontSave;
				Apply(material);
			}
			if (material.shader != p_Shader)
			{
				material.shader = p_Shader;
			}
			return material;
		}

		protected void DestroyMaterial()
		{
			Material mat = Mat;
			if (mat != null)
			{
				if (Application.isPlaying)
				{
					UnityEngine.Object.Destroy(mat);
				}
				else
				{
					UnityEngine.Object.DestroyImmediate(mat, true);
				}
			}
		}

		protected void UpdateMaterial()
		{
			Material mat = Mat;
			if (mat != null)
			{
				Apply(mat);
			}
		}

		protected virtual void Apply(Material Material)
		{
			switch (type)
			{
			case ProjectionType.Decal:
				Material.DisableKeyword("_Omni");
				break;
			case ProjectionType.OmniDecal:
				Material.EnableKeyword("_Omni");
				break;
			}
			Material.enableInstancing = Instanced;
			switch (transparencyType)
			{
			case TransparencyType.Blend:
				Material.DisableKeyword("_AlphaTest");
				break;
			case TransparencyType.Cutout:
				Material.EnableKeyword("_AlphaTest");
				break;
			}
			Material.SetFloat(_Cutoff, cutoff);
			Material.SetVector(value: new Vector4(tiling.x, tiling.y, offset.x, offset.y), name: _TilingOffset.ToString());
			if (masks.Length == 4)
			{
				switch (maskMethod)
				{
				case MaskMethod.DrawOnEverythingExcept:
				{
					Material.SetFloat(_MaskBase, 1f);
					Color clear2 = Color.clear;
					clear2.r = ((!masks[0]) ? 0.5f : 0f);
					clear2.g = ((!masks[1]) ? 0.5f : 0f);
					clear2.b = ((!masks[2]) ? 0.5f : 0f);
					clear2.a = ((!masks[3]) ? 0.5f : 0f);
					Material.SetVector(_MaskLayers, clear2);
					break;
				}
				case MaskMethod.OnlyDrawOn:
				{
					Material.SetFloat(_MaskBase, 0f);
					Color clear = Color.clear;
					clear.r = ((!masks[0]) ? 0.5f : 1f);
					clear.g = ((!masks[1]) ? 0.5f : 1f);
					clear.b = ((!masks[2]) ? 0.5f : 1f);
					clear.a = ((!masks[3]) ? 0.5f : 1f);
					Material.SetVector(_MaskLayers, clear);
					break;
				}
				}
			}
			float value2 = Mathf.Cos((float)Math.PI / 180f * projectionLimit);
			Material.SetFloat(_NormalCutoff, value2);
		}

		public virtual void UpdateProperties()
		{
		}

		protected virtual void OnEnable()
		{
			GenerateIDs();
		}

		protected virtual void OnDisable()
		{
			DestroyMaterial();
		}

		public void Update()
		{
			if (marked)
			{
				UpdateProperties();
				UpdateMaterial();
				marked = false;
			}
		}

		public void Mark(bool UpdateImmediately = false)
		{
			marked = true;
			if (UpdateImmediately)
			{
				Update();
			}
		}

		protected virtual void GenerateIDs()
		{
			_Cutoff = Shader.PropertyToID("_Cutoff");
			_TilingOffset = Shader.PropertyToID("_TilingOffset");
			_MaskBase = Shader.PropertyToID("_MaskBase");
			_MaskLayers = Shader.PropertyToID("_MaskLayers");
			_NormalCutoff = Shader.PropertyToID("_NormalCutoff");
		}
	}
}
