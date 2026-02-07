using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class Specular : Projection
	{
		public AlbedoPropertyGroup albedo;

		public SpecularPropertyGroup specular;

		public NormalPropertyGroup normal;

		public EmissivePropertyGroup emissive;

		protected Material[] forwardMaterials;

		protected Material[] deferredOpaqueMaterials;

		protected Material[] deferredTransparentMaterials;

		public override Material MobileForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Specular/Forward"));
			}
		}

		public override Material MobileDeferredOpaque
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Specular/DeferredOpaque"));
			}
		}

		public override Material MobileDeferredTransparent
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Specular/DeferredTransparent"));
			}
		}

		public override Material StandardForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Specular/Forward"));
			}
		}

		public override Material StandardDeferredOpaque
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Specular/DeferredOpaque"));
			}
		}

		public override Material StandardDeferredTransparent
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Specular/DeferredTransparent"));
			}
		}

		public override Material PackedForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Specular/Forward"));
			}
		}

		public override Material PackedDeferredOpaque
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Specular/DeferredOpaque"));
			}
		}

		public override Material PackedDeferredTransparent
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Specular/DeferredTransparent"));
			}
		}

		public override RenderingPaths SupportedRendering
		{
			get
			{
				return RenderingPaths.Both;
			}
		}

		public override int InstanceLimit
		{
			get
			{
				return 500;
			}
		}

		protected override void Apply(Material Material)
		{
			base.Apply(Material);
			albedo.Apply(Material);
			specular.Apply(Material);
			normal.Apply(Material);
			emissive.Apply(Material);
		}

		protected override void OnEnable()
		{
			if (albedo == null)
			{
				albedo = new AlbedoPropertyGroup(this);
			}
			if (specular == null)
			{
				specular = new SpecularPropertyGroup(this);
			}
			if (normal == null)
			{
				normal = new NormalPropertyGroup(this);
			}
			if (emissive == null)
			{
				emissive = new EmissivePropertyGroup(this);
			}
			base.OnEnable();
		}

		protected override void GenerateIDs()
		{
			base.GenerateIDs();
			albedo.GenerateIDs();
			specular.GenerateIDs();
			normal.GenerateIDs();
			emissive.GenerateIDs();
		}

		public override void UpdateProperties()
		{
			if (properties == null || properties.Length != 2)
			{
				properties = new ProjectionProperty[2];
			}
			properties[0] = new ProjectionProperty("Albedo", albedo._Color, albedo.Color);
			properties[1] = new ProjectionProperty("Emission", emissive._EmissionColor, emissive.Color, emissive.Intensity);
		}
	}
}
