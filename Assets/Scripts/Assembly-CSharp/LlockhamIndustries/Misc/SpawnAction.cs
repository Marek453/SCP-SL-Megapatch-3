using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class SpawnAction : ClampedAction
	{
		public GameObject spawnable;

		public float spawnHeight = 10f;

		protected override void Perform(Vector3 point)
		{
			if (spawnable != null)
			{
				Object.Instantiate(spawnable, point + Vector3.up * spawnHeight, Quaternion.identity);
			}
		}
	}
}
