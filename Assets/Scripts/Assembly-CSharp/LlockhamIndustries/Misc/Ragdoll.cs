using System;
using System.Collections;
using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class Ragdoll : MonoBehaviour
	{
		[Header("GameObjects")]
		public GameObject ragdoll;

		public GameObject chunkdoll;

		[Header("Particles")]
		public ParticleSystem ragParticles;

		public ParticleSystem chunkParticles;

		[Header("Layers")]
		public LayerMask triggerLayers;

		[Header("Ragdoll Triggers")]
		public float ragVelocity = 10f;

		[Header("Chunkdoll Triggers")]
		public float chunkVelocity = 50f;

		public float chunkAngle = 35f;

		private void OnCollisionEnter(Collision collision)
		{
			ContactPoint[] contacts = collision.contacts;
			for (int i = 0; i < contacts.Length; i++)
			{
				ContactPoint contactPoint = contacts[i];
				Rigidbody component = contactPoint.otherCollider.GetComponent<Rigidbody>();
				if (component != null && component.velocity.magnitude > chunkVelocity && triggerLayers.Contains(component.gameObject.layer))
				{
					if (component.velocity.magnitude > chunkVelocity && Vector3.Angle(component.velocity, contactPoint.normal) < chunkAngle)
					{
						TriggerChunkdoll(component.mass, component.velocity);
						Transform child = chunkdoll.transform.GetChild(0);
						SpawnParticles(chunkParticles, contactPoint.point, contactPoint.normal, child);
						break;
					}
					if (component.velocity.magnitude > ragVelocity)
					{
						TriggerRagdoll(component.mass, component.velocity);
						SpawnParticles(ragParticles, contactPoint.point, contactPoint.normal, ragdoll.transform);
						break;
					}
				}
			}
		}

		private void TriggerRagdoll(float ExternalMass, Vector3 ExternalVelocity)
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			ragdoll = UnityEngine.Object.Instantiate(ragdoll, base.transform.position, base.transform.rotation, base.transform.parent);
			SyncTransformRecursively(ragdoll.transform, base.transform);
			float num = CalculateMassRecursively(ragdoll.transform);
			float t = ExternalMass / (ExternalMass + num);
			Vector3 velocity = Vector3.Lerp(Vector3.zero, ExternalVelocity, t);
			SetVelocityRecursively(ragdoll.transform, velocity);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void TriggerChunkdoll(float ExternalMass, Vector3 ExternalVelocity)
		{
			Collider component = GetComponent<Collider>();
			if (component != null)
			{
				component.enabled = false;
			}
			chunkdoll = UnityEngine.Object.Instantiate(chunkdoll, base.transform.position, base.transform.rotation, base.transform.parent);
			float num = CalculateMassRecursively(chunkdoll.transform);
			float t = ExternalMass / (ExternalMass + num);
			Vector3 vector = Vector3.Lerp(Vector3.zero, ExternalVelocity, t);
			SetVelocityRecursively(chunkdoll.transform, vector * 2f);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		private void SpawnParticles(ParticleSystem Particles, Vector3 Position, Vector3 Normal, Transform Parent)
		{
			if (Particles != null)
			{
				ParticleSystem particleSystem = UnityEngine.Object.Instantiate(Particles, Position, Quaternion.LookRotation(Normal), Parent);
				particleSystem.name = Particles.name;
				particleSystem.Play();
			}
		}

		private void SyncTransformRecursively(Transform Transform, Transform Target)
		{
			Transform.localPosition = Target.localPosition;
			Transform.localRotation = Target.localRotation;
			IEnumerator enumerator = Transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					Transform target = Target.Find(transform.name);
					SyncTransformRecursively(transform, target);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private float CalculateMassRecursively(Transform Transform)
		{
			float num = 0f;
			Rigidbody component = Transform.GetComponent<Rigidbody>();
			if (component != null)
			{
				num += component.mass;
			}
			IEnumerator enumerator = Transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					num += CalculateMassRecursively(transform);
				}
				return num;
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}

		private void SetVelocityRecursively(Transform Transform, Vector3 Velocity)
		{
			Rigidbody component = Transform.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.velocity += Vector3.Slerp(Velocity, UnityEngine.Random.rotationUniform.eulerAngles, 0.1f);
			}
			IEnumerator enumerator = Transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					SetVelocityRecursively(transform, Velocity);
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
