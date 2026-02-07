using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class AlbedoPropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private Color color = Color.grey;

		public int _MainTex;

		public int _Color;

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

		public AlbedoPropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_MainTex = Shader.PropertyToID("_MainTex");
			_Color = Shader.PropertyToID("_Color");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_MainTex, texture);
			Material.SetColor(_Color, color);
		}
	}
}
