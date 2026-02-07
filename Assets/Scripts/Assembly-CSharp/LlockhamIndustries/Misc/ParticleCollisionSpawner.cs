using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Rigidbody))]
	public class ParticleCollisionSpawner : MonoBehaviour
	{
		[Header("Particle System")]
		public ParticleSystem particles;

		[Header("Conditions")]
		public float requiredVelocity = 10f;

		public LayerMask layers;

		[Header("Pool Parent")]
		public Transform parent;

		private Rigidbody rb;

		private void Awake()
		{
			rb = GetComponent<Rigidbody>();
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!(particles != null) || !(rb.velocity.magnitude > requiredVelocity))
			{
				return;
			}
			for (int i = 0; i < collision.contacts.Length; i++)
			{
				if (layers.Contains(collision.contacts[i].otherCollider.gameObject.layer))
				{
					ParticleSystem particleSystem = null;
					particleSystem = ((!(parent != null)) ? Object.Instantiate(particles, collision.contacts[i].point, Quaternion.LookRotation(collision.contacts[i].normal), base.transform) : Object.Instantiate(particles, collision.contacts[i].point, Quaternion.LookRotation(collision.contacts[i].normal), parent));
					particleSystem.Play();
				}
			}
		}
	}
}
