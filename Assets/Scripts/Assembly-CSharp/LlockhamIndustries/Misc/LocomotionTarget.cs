using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Locomotion))]
	public class LocomotionTarget : MonoBehaviour
	{
		public float brakeDistance = 0.4f;

		private Vector3 goalPosition;

		private Locomotion locomotion;

		public Vector3 GoalPosition
		{
			set
			{
				goalPosition = value;
			}
		}

		private void Start()
		{
			locomotion = GetComponent<Locomotion>();
			goalPosition = base.transform.position;
		}

		private void Update()
		{
			if (Vector3.Distance(base.transform.position, goalPosition) > brakeDistance)
			{
				Vector3 normalized = (base.transform.position - goalPosition).normalized;
				locomotion.Direction = normalized;
				locomotion.Movement = normalized;
			}
			else
			{
				locomotion.Movement = Vector3.zero;
			}
		}
	}
}
