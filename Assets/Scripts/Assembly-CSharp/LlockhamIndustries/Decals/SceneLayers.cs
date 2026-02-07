using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class SceneLayers : MonoBehaviour
	{
		public ProjectionLayer[] layers;

		private ProjectionLayer[] original;

		private void OnEnable()
		{
			original = DynamicDecals.System.Settings.Layers;
			DynamicDecals.System.Settings.Layers = layers;
		}

		private void OnDisable()
		{
			DynamicDecals.System.Settings.Layers = original;
		}
	}
}
