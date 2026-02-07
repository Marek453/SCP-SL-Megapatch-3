using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class GlossPropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private float glossiness = 1f;

		public int _GlossMap;

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

		public GlossPropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_GlossMap = Shader.PropertyToID("_GlossMap");
			_Glossiness = Shader.PropertyToID("_Glossiness");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_GlossMap, texture);
			Material.SetFloat(_Glossiness, glossiness);
		}
	}
}
