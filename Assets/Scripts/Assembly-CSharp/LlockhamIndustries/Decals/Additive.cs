using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class Additive : Forward
	{
		public override Material MobileForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Additive"));
			}
		}

		public override Material StandardForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Additive"));
			}
		}

		public override Material PackedForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Additive"));
			}
		}
	}
}
