using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class TestTrigger : MonoBehaviour
	{
		public Trap[] traps;

		public float delay = 1f;

		private float timeCode;

		private float timeElapsed;

		private void OnTriggerStay(Collider other)
		{
			if (!other.GetComponent<Selectable>())
			{
				return;
			}
			if (Time.timeSinceLevelLoad - 1f > timeCode)
			{
				timeElapsed = 0f;
			}
			else
			{
				timeElapsed += Time.fixedDeltaTime;
			}
			timeCode = Time.timeSinceLevelLoad;
			if (timeElapsed > delay)
			{
				Trap[] array = traps;
				foreach (Trap trap in array)
				{
					trap.Trigger();
				}
			}
		}
	}
}
