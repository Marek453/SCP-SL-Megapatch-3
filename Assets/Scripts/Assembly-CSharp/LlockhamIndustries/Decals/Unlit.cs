using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class Unlit : Forward
	{
		public override Material MobileForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Mobile/Unlit"));
			}
		}

		public override Material StandardForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Standard/Unlit"));
			}
		}

		public override Material PackedForward
		{
			get
			{
				return MaterialFromShader(Shader.Find("Projection/Decal/Packed/Unlit"));
			}
		}
	}
}
