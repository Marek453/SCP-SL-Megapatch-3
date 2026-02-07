using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public class ReplacementPass
	{
		public Vector4 vector;

		public LayerMask layers;

		public ReplacementPass(LayerMask Mask, Vector4 LayerVector)
		{
			vector = LayerVector;
			layers = Mask;
		}

		public ReplacementPass(int LayerIndex, Vector4 LayerVector)
		{
			vector = LayerVector;
			layers = 1 << LayerIndex;
		}
	}
}
