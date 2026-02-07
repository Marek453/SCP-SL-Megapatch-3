using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public struct ProjectionLayer
	{
		public string name;

		public LayerMask layers;

		public ProjectionLayer(string Name)
		{
			name = Name;
			layers = 0;
		}

		public ProjectionLayer(string Name, int Layer)
		{
			name = Name;
			layers = 1 << Layer;
		}

		public ProjectionLayer(string Name, LayerMask Layers)
		{
			name = Name;
			layers = Layers;
		}
	}
}
