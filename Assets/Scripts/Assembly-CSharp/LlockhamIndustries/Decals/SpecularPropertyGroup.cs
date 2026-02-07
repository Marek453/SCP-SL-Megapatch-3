using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class SpecularPropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private Color color = new Color(0.2f, 0.2f, 0.2f, 1f);

		[SerializeField]
		private float glossiness = 1f;

		public int _SpecGlossMap;

		public int _SpecColor;

		public int _Glossiness;

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

		public float Glossiness
		{
			get
			{
				return glossiness;
			}
			set
			{
				if (glossiness != value)
				{
					glossiness = value;
					Mark();
				}
			}
		}

		public SpecularPropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_SpecGlossMap = Shader.PropertyToID("_SpecGlossMap");
			_SpecColor = Shader.PropertyToID("_SpecColor");
			_Glossiness = Shader.PropertyToID("_Glossiness");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_SpecGlossMap, texture);
			Material.SetColor(_SpecColor, color);
			Material.SetFloat(_Glossiness, glossiness);
		}
	}
}
