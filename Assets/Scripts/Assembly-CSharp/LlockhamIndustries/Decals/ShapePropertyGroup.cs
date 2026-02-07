using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class ShapePropertyGroup
	{
		protected Projection projection;

		[SerializeField]
		private Texture texture;

		[SerializeField]
		private float multiplier = 1f;

		public int _MainTex;

		public int _Multiplier;

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

		public float Multiplier
		{
			get
			{
				return multiplier;
			}
			set
			{
				if (multiplier != value)
				{
					multiplier = value;
					Mark();
				}
			}
		}

		public ShapePropertyGroup(Projection Projection)
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
			_Multiplier = Shader.PropertyToID("_Multiplier");
		}

		public void Apply(Material Material)
		{
			Material.SetTexture(_MainTex, texture);
			Material.SetFloat(_Multiplier, multiplier);
		}
	}
}
