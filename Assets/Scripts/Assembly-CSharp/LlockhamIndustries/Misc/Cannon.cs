using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class Cannon : MonoBehaviour
	{
		[Header("References")]
		public GameObject ball;

		public Rigidbody barrel;

		public ParticleSystem particles;

		[Header("Firing")]
		public Vector3 offset;

		public Vector3 velocity = new Vector3(0f, -10f, 0f);

		public float fireRate = 0.25f;

		private float timeElapsed;

		private void OnEnable()
		{
			StartCoroutine(FireRoutine());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private IEnumerator FireRoutine()
		{
			while (true)
			{
				timeElapsed += Time.fixedDeltaTime;
				if (timeElapsed > 1f / fireRate)
				{
					Fire();
					timeElapsed -= 1f / fireRate;
				}
				yield return new WaitForFixedUpdate();
			}
		}

		private void Fire()
		{
			if (particles != null)
			{
				particles.Play();
			}
			if (barrel != null && ball != null)
			{
				GameObject gameObject = Object.Instantiate(ball, barrel.transform.position + barrel.transform.rotation * offset, Quaternion.identity, base.transform);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				Vector3 vector2 = (component.velocity = barrel.transform.rotation * velocity);
				Vector3 vector3 = -vector2 * (component.mass / barrel.mass);
				barrel.velocity = vector3;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if (barrel != null)
			{
				Vector3 vector = barrel.transform.position + barrel.transform.rotation * offset;
				Vector3 direction = barrel.transform.rotation * velocity;
				Gizmos.DrawWireSphere(vector, 0.2f);
				Gizmos.DrawRay(vector, direction);
			}
		}
	}
}
