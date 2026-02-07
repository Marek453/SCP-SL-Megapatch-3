using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class RayCollisionPrinter : Printer
	{
		public CollisionCondition condition;

		public float conditionTime = 1f;

		public LayerMask layers;

		public CastMethod method;

		public Transform castCenter;

		public Vector2 castDimensions;

		public Vector3 positionOffset;

		public Vector3 rotationOffset;

		public float castLength = 1f;

		public QueryTriggerInteraction hitTriggers;

		private float timeElapsed;

		private bool delayPrinted;

		private CollisionData collision;

		private void FixedUpdate()
		{
			CastCollision(Time.fixedDeltaTime);
		}

		private void CastCollision(float deltaTime)
		{
			Transform transform = ((!(castCenter != null)) ? base.transform : castCenter);
			Quaternion quaternion = transform.rotation * Quaternion.Euler(rotationOffset);
			Vector3 origin = transform.position + quaternion * positionOffset;
			if (method == CastMethod.Area)
			{
				Vector3 zero = Vector3.zero;
				zero.x = Random.Range(0f - castDimensions.x, castDimensions.x);
				zero.y = Random.Range(0f - castDimensions.y, castDimensions.y);
				origin += quaternion * zero;
			}
			Ray ray = new Ray(origin, quaternion * Vector3.forward);
			RaycastHit hitInfo;
			if (Physics.Raycast(ray, out hitInfo, castLength, layers.value, hitTriggers))
			{
				collision = new CollisionData(hitInfo.point, Quaternion.LookRotation(-hitInfo.normal, quaternion * Vector3.up), hitInfo.transform, hitInfo.collider.gameObject.layer);
				if (condition == CollisionCondition.Constant)
				{
					PrintCollision(collision);
				}
				if (timeElapsed == 0f && condition == CollisionCondition.Enter)
				{
					PrintCollision(collision);
				}
				timeElapsed += deltaTime;
				if (condition == CollisionCondition.Delay && timeElapsed >= conditionTime && !delayPrinted)
				{
					PrintCollision(collision);
					delayPrinted = true;
				}
			}
			else
			{
				if (timeElapsed > 0f && (condition == CollisionCondition.Exit || (condition == CollisionCondition.Delay && timeElapsed < conditionTime)))
				{
					PrintCollision(collision);
				}
				timeElapsed = 0f;
				delayPrinted = false;
			}
		}

		private void PrintCollision(CollisionData collision)
		{
			Print(collision.position, collision.rotation, collision.surface, collision.layer);
		}

		private void OnDrawGizmosSelected()
		{
			Transform transform = ((!(castCenter != null)) ? base.transform : castCenter);
			Quaternion quaternion = transform.rotation * Quaternion.Euler(rotationOffset);
			Vector3 vector = transform.position + quaternion * positionOffset;
			Gizmos.color = Color.white;
			switch (method)
			{
			case CastMethod.Point:
				Gizmos.DrawRay(vector, quaternion * Vector3.forward * castLength);
				break;
			case CastMethod.Area:
				Gizmos.DrawRay(vector + quaternion * new Vector3(castDimensions.x, castDimensions.y, 0f), quaternion * Vector3.forward * castLength);
				Gizmos.DrawRay(vector + quaternion * new Vector3(0f - castDimensions.x, castDimensions.y, 0f), quaternion * Vector3.forward * castLength);
				Gizmos.DrawRay(vector + quaternion * new Vector3(castDimensions.x, 0f - castDimensions.y, 0f), quaternion * Vector3.forward * castLength);
				Gizmos.DrawRay(vector + quaternion * new Vector3(0f - castDimensions.x, 0f - castDimensions.y, 0f), quaternion * Vector3.forward * castLength);
				break;
			}
		}
	}
}
