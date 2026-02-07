using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class NormalPropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private float strength = 1f;

		public int _BumpMap;

		public int _BumpScale;

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

		public float Strength
		{
			get
			{
				return strength;
			}
			set
			{
				if (strength != value)
				{
					strength = value;
					Mark();
				}
			}
		}

		public NormalPropertyGroup(Projection Projection)
		{
			projection = Projection;
		}

		protected void Mark()
		{
			projection.Mark();
		}

		public void GenerateIDs()
		{
			_BumpMap = Shader.PropertyToID("_BumpMap");
			_BumpScale = Shader.PropertyToID("_BumpScale");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_BumpMap, texture);
			Material.SetFloat(_BumpScale, strength);
		}
	}
}
