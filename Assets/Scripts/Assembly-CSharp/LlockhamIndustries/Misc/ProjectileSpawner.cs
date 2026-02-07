using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class ProjectileSpawner : MonoBehaviour
	{
		public GameObject projectile;

		public float spawnRate = 60f;

		public float spread = 0.3f;

		public Transform parent;

		public Vector3 spawnVelocity;

		private float timeToSpawn;

		private void Start()
		{
			if (parent == null)
			{
				parent = base.transform;
			}
		}

		private void Update()
		{
			timeToSpawn = Mathf.Clamp(timeToSpawn - Time.deltaTime, 0f, float.PositiveInfinity);
			if (timeToSpawn == 0f)
			{
				Vector3 vector = Vector3.Slerp(spawnVelocity, Random.insideUnitSphere.normalized * spawnVelocity.magnitude, spread / 10f);
				Quaternion rotation = Quaternion.LookRotation(vector, base.transform.forward);
				GameObject gameObject = Object.Instantiate(projectile, base.transform.position, rotation, parent);
				gameObject.name = "Ray";
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				component.AddForce(vector, ForceMode.VelocityChange);
				timeToSpawn = 1f / spawnRate;
			}
		}
	}
}
