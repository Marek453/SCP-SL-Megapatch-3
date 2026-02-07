using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class EmissivePropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private Color color = Color.black;

		[SerializeField]
		private float intensity = 1f;

		public int _EmissionMap;

		public int _EmissionColor;

		public Texture Texture
		{
			get
			{
				return texture;
			}
			set
			{
				if (texture != value)
				{
					texture = value;
					Mark();
				}
			}
		}

		public float Intensity
		{
			get
			{
				return intensity;
			}
			set
			{
				if (intensity != value)
				{
					intensity = value;
					Mark();
				}
			}
		}

		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					Mark();
				}
			}
		}

		public EmissivePropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_EmissionMap = Shader.PropertyToID("_EmissionMap");
			_EmissionColor = Shader.PropertyToID("_EmissionColor");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_EmissionMap, texture);
			Material.SetColor(_EmissionColor, color * intensity);
		}
	}
}
