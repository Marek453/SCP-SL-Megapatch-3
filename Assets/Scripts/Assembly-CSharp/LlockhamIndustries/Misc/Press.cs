using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class Press : Trap
	{
		[Header("Components")]
		public Rigidbody press;

		[Header("State - Triggered")]
		public Vector3 triggeredPosition = new Vector3(-1.25f, 0f, 0f);

		public float triggeredAcceleration = 10f;

		public float triggeredDuration = 2f;

		[Header("State - Rearmed")]
		public Vector3 rearmedPosition = new Vector3(0f, 0f, 0f);

		public float rearmedAcceleration = 0.1f;

		private Vector3 goalPosition;

		private float acceleration;

		private void Start()
		{
			goalPosition = rearmedPosition;
			acceleration = rearmedAcceleration;
		}

		private void FixedUpdate()
		{
			if (Vector3.Distance(press.transform.localPosition, goalPosition) > 0.01f || press.velocity.magnitude > 0.001f)
			{
				Vector3 direction = (goalPosition - press.transform.localPosition) / Time.fixedDeltaTime;
				float maxDistanceDelta = acceleration / Time.fixedDeltaTime;
				direction = base.transform.TransformDirection(direction);
				direction = Vector3.MoveTowards(press.velocity, direction, maxDistanceDelta);
				press.velocity = direction;
			}
		}

		protected override IEnumerator OnTrigger()
		{
			goalPosition = triggeredPosition;
			acceleration = triggeredAcceleration;
			float timeHeld = 0f;
			while (timeHeld < triggeredDuration)
			{
				if (Vector3.Distance(press.transform.localPosition, goalPosition) < 0.01f && press.velocity.magnitude < 0.001f)
				{
					timeHeld += Time.fixedDeltaTime;
				}
				yield return new WaitForFixedUpdate();
			}
			TriggerComplete();
		}

		protected override IEnumerator OnRearm()
		{
			goalPosition = rearmedPosition;
			acceleration = rearmedAcceleration;
			while (Vector3.Distance(press.transform.localPosition, goalPosition) > 0.01f || press.velocity.magnitude > 0.001f)
			{
				yield return new WaitForFixedUpdate();
			}
			RearmComplete();
		}
	}
}
