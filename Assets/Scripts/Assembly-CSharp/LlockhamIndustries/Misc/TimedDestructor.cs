using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class TimedDestructor : MonoBehaviour
	{
		public float time = 10f;

		private float t;

		private void Update()
		{
			t += Time.deltaTime;
			if (t > time)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}
}
