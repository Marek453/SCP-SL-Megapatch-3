using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleCollisionPrinter : Printer
	{
		public RotationSource rotationSource;

		public float ratio = 1f;

		private ParticleSystem partSystem;

		private float maxparticleCollisionSize;

		private List<ParticleCollisionEvent> collisionEvents;

		private void Start()
		{
			partSystem = GetComponent<ParticleSystem>();
			if (Application.isPlaying)
			{
				collisionEvents = new List<ParticleCollisionEvent>();
			}
		}

		private void Update()
		{
			if (!partSystem.collision.enabled)
			{
				Debug.LogWarning("Particle system collisions must be enabled for the particle system to print decals");
			}
			else if (!partSystem.collision.sendCollisionMessages)
			{
				Debug.LogWarning("Particle system must send collision messages for the particle system to print decals. This option can be enabled under the collisions menu.");
			}
		}

		private void OnParticleCollision(GameObject other)
		{
			if (!base.enabled || !Application.isPlaying || !(ratio > 0f))
			{
				return;
			}
			int num = partSystem.GetCollisionEvents(other, collisionEvents);
			for (int i = 0; i < num; i++)
			{
				if (ratio == 1f || ratio > Random.Range(0f, 1f))
				{
					Vector3 intersection = collisionEvents[i].intersection;
					Vector3 normal = collisionEvents[i].normal;
					Transform transform = other.transform;
					int layerMask = 1 << other.layer;
					RaycastHit hitInfo;
					if (Physics.Raycast(intersection, -normal, out hitInfo, float.PositiveInfinity, layerMask))
					{
						intersection = hitInfo.point;
						normal = hitInfo.normal;
						transform = hitInfo.collider.transform;
						Print(intersection, Quaternion.LookRotation(upwards: (rotationSource == RotationSource.Velocity && collisionEvents[i].velocity != Vector3.zero) ? collisionEvents[i].velocity.normalized : ((rotationSource != RotationSource.Random) ? Vector3.up : Random.insideUnitSphere.normalized), forward: -normal), transform, hitInfo.collider.gameObject.layer);
					}
				}
			}
		}
	}
}
