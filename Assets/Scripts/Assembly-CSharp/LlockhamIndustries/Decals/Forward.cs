using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public abstract class Forward : Projection
	{
		public AlbedoPropertyGroup albedo;

		public override int InstanceLimit
		{
			get
			{
				return 500;
			}
		}

		public override RenderingPaths SupportedRendering
		{
			get
			{
				return RenderingPaths.Forward;
			}
		}

		protected override void Apply(Material Material)
		{
			base.Apply(Material);
			albedo.Apply(Material);
		}

		protected override void OnEnable()
		{
			if (albedo == null)
			{
				albedo = new AlbedoPropertyGroup(this);
			}
			base.OnEnable();
		}

		protected override void GenerateIDs()
		{
			base.GenerateIDs();
			albedo.GenerateIDs();
		}

		public override void UpdateProperties()
		{
			if (properties == null || properties.Length != 1)
			{
				properties = new ProjectionProperty[1];
			}
			properties[0] = new ProjectionProperty("Albedo", albedo._Color, albedo.Color);
		}
	}
}
