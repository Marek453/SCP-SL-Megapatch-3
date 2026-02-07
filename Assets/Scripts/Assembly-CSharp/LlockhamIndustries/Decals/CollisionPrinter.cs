using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class CollisionPrinter : Printer
	{
		public RotationSource rotationSource;

		public CollisionCondition condition;

		public float conditionTime;

		public LayerMask layers;

		private float timeElapsed;

		private bool delayPrinted;

		private void OnCollisionEnter(Collision collision)
		{
			if (condition == CollisionCondition.Enter || condition == CollisionCondition.Constant)
			{
				PrintCollision(collision);
			}
			timeElapsed = 0f;
			delayPrinted = false;
		}

		private void OnCollisionStay(Collision collision)
		{
			timeElapsed += Time.deltaTime;
			if (condition == CollisionCondition.Constant)
			{
				PrintCollision(collision);
			}
			if (condition == CollisionCondition.Delay && timeElapsed > conditionTime && !delayPrinted)
			{
				PrintCollision(collision);
				delayPrinted = true;
			}
		}

		private void OnCollisionExit(Collision collision)
		{
			if (condition == CollisionCondition.Exit)
			{
				PrintCollision(collision);
			}
			if (condition == CollisionCondition.Delay && !delayPrinted)
			{
				PrintCollision(collision);
			}
		}

		public void PrintCollision(Collision collision)
		{
			Transform transform = null;
			Vector3 vector = Vector3.zero;
			Vector3 vector2 = Vector3.zero;
			int num = 0;
			ContactPoint[] contacts = collision.contacts;
			for (int i = 0; i < contacts.Length; i++)
			{
				ContactPoint contactPoint = contacts[i];
				if (layers.value == (layers.value | (1 << contactPoint.otherCollider.gameObject.layer)))
				{
					num++;
					if (num == 1)
					{
						transform = contactPoint.otherCollider.transform;
					}
					if (num == 1)
					{
						vector = contactPoint.point;
					}
					if (num == 1)
					{
						vector2 = contactPoint.normal;
					}
				}
			}
			RaycastHit hitInfo;
			if (num > 0 && Physics.Raycast(vector + vector2 * 0.3f, -vector2, out hitInfo, 0.4f, layers.value))
			{
				vector = hitInfo.point;
				vector2 = hitInfo.normal;
				transform = hitInfo.collider.transform;
				Print(vector, Quaternion.LookRotation(upwards: (rotationSource == RotationSource.Velocity && GetComponent<Rigidbody>().velocity != Vector3.zero) ? GetComponent<Rigidbody>().velocity.normalized : ((rotationSource != RotationSource.Random) ? Vector3.up : Random.insideUnitSphere.normalized), forward: -vector2), transform, hitInfo.collider.gameObject.layer);
			}
		}
	}
}
