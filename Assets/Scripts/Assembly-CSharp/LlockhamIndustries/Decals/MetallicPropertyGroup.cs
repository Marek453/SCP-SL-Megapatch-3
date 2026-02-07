using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class MetallicPropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private float metallicity = 0.5f;

		[SerializeField]
		private float glossiness = 1f;

		public int _MetallicGlossMap;

		public int _Metallic;

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

		public float Metallicity
		{
			get
			{
				return metallicity;
			}
			set
			{
				if (metallicity != value)
				{
					metallicity = value;
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

		public MetallicPropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");
			_Metallic = Shader.PropertyToID("_Metallic");
			_Glossiness = Shader.PropertyToID("_Glossiness");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_MetallicGlossMap, texture);
			Material.SetFloat(_Metallic, metallicity);
			Material.SetFloat(_Glossiness, glossiness);
		}
	}
}
