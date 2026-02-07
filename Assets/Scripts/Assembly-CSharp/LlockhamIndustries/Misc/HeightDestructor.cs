using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class HeightDestructor : MonoBehaviour
	{
		public float height = -10f;

		private void Update()
		{
			if (base.transform.position.y < height)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
