using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class Multiplicative : Forward
	{
		public override Material MobileForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Multiplicative"));
			}
		}

		public override Material StandardForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Multiplicative"));
			}
		}

		public override Material PackedForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Multiplicative"));
			}
		}
	}
}
