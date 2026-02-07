using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class LaserGuider : MonoBehaviour
	{
		public float xMin = -20f;

		public float xMax = 20f;

		public float zMin = -20f;

		public float zMax = 20f;

		public float laserHeight = 2.2f;

		public float smooth = 0.6f;

		public float retargetDistance = 4f;

		private Vector3 goalPosition;

		private Vector3 NewPosition
		{
			get
			{
				float x = Random.Range(xMin, xMax);
				float z = Random.Range(zMin, zMax);
				return new Vector3(x, laserHeight, z);
			}
		}

		private void Start()
		{
			goalPosition = NewPosition;
		}

		private void Update()
		{
			if (Vector3.Distance(base.transform.position, goalPosition) <= retargetDistance)
			{
				goalPosition = NewPosition;
			}
			base.transform.position = Vector3.MoveTowards(base.transform.position, goalPosition, smooth * Time.deltaTime);
		}
	}
}
