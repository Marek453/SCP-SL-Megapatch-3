using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class Bleeder : MonoBehaviour
	{
		public GameObject prefab;

		public int bleedRate = 2;

		public int bleedLimit = 12;

		public LayerMask triggerLayers;

		public float triggerVelocity = 10f;

		private bool Valid
		{
			get
			{
				if (prefab == null)
				{
					return false;
				}
				if (prefab.GetComponent<Collider>() == null)
				{
					return false;
				}
				if (prefab.GetComponent<Rigidbody>() == null)
				{
					return false;
				}
				if (prefab.GetComponent<Blood>() == null)
				{
					return false;
				}
				return true;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!Valid)
			{
				return;
			}
			for (int i = 0; i < Mathf.Min(bleedLimit, collision.contacts.Length); i++)
			{
				Rigidbody component = collision.contacts[i].otherCollider.GetComponent<Rigidbody>();
				if (!(component == null) && !(component.velocity.magnitude <= triggerVelocity))
				{
					Blood component2 = collision.contacts[i].otherCollider.GetComponent<Blood>();
					if ((!(component2 != null) || !(component2.source == this)) && triggerLayers.Contains(collision.contacts[i].otherCollider.gameObject.layer))
					{
						Bleed(collision.contacts[i].point, collision.contacts[i].normal);
					}
				}
			}
		}

		private void Bleed(Vector3 Point, Vector3 Normal)
		{
			Bounds bounds = prefab.GetComponent<Collider>().bounds;
			float num = 1.5f * Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);
			Vector3 point = Point + Normal * num;
			for (int i = 0; i < bleedRate; i++)
			{
				Vector3 normalized = (Normal + Random.onUnitSphere * 0.2f).normalized;
				SpawnDroplet(point, normalized);
			}
		}

		private void SpawnDroplet(Vector3 Point, Vector3 Velocity)
		{
			Blood component = Object.Instantiate(prefab.gameObject, Point, Quaternion.identity).GetComponent<Blood>();
			component.source = this;
			Rigidbody component2 = component.GetComponent<Rigidbody>();
			component2.velocity = Velocity;
		}
	}
}
