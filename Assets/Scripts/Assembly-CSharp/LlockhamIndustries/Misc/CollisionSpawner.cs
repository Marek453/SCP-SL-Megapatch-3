using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class CollisionSpawner : MonoBehaviour
	{
		public GameObject[] colliders;

		public LayerMask layers;

		public Transform parent;

		private int colliderIndex;

		private void Update()
		{
			RaycastHit hitInfo;
			if (Input.GetMouseButtonDown(0) && Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, float.PositiveInfinity, layers.value))
			{
				SpawnCollider(hitInfo.point + Vector3.up * 10f);
			}
		}

		public void SpawnCollider(Vector3 Position)
		{
			if (colliders != null && colliders.Length > 0)
			{
				if (colliders[colliderIndex] != null)
				{
					GameObject gameObject = Object.Instantiate(colliders[colliderIndex], Position, Quaternion.identity, parent);
					gameObject.name = "Collider";
					gameObject.GetComponent<Rigidbody>().velocity = Vector3.down * 4f;
				}
				colliderIndex = ((colliderIndex < colliders.Length - 1) ? (colliderIndex + 1) : 0);
			}
		}
	}
}
