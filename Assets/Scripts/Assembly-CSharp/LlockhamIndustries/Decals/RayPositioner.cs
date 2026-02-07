using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class RayPositioner : Positioner
	{
		public Transform rayTransform;

		public Vector3 positionOffset;

		public Vector3 rotationOffset;

		public float castLength = 100f;

		private void LateUpdate()
		{
			Transform transform = ((!(rayTransform != null)) ? base.transform : rayTransform);
			Quaternion quaternion = transform.rotation * Quaternion.Euler(rotationOffset);
			Vector3 origin = transform.position + quaternion * positionOffset;
			Ray ray = new Ray(origin, quaternion * Vector3.forward);
			Reproject(ray, castLength, quaternion * Vector3.up);
		}

		private void OnDrawGizmosSelected()
		{
			Transform transform = ((!(rayTransform != null)) ? base.transform : rayTransform);
			Quaternion quaternion = transform.rotation * Quaternion.Euler(rotationOffset);
			Vector3 from = transform.position + quaternion * positionOffset;
			Gizmos.color = Color.black;
			Gizmos.DrawRay(from, quaternion * Vector3.up * 0.4f);
			Gizmos.color = Color.white;
			Gizmos.DrawRay(from, quaternion * Vector3.forward * castLength);
		}
	}
}
