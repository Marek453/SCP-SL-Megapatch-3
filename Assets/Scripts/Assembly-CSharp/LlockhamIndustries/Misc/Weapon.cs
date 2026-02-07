using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(SpringJoint))]
	[RequireComponent(typeof(Rigidbody))]
	public class Weapon : MonoBehaviour
	{
		private WeaponState weaponState;

		public WeaponState Standard;

		public WeaponState Extended;

		public WeaponState PreReleased;

		public WeaponState Released;

		public TrailRenderer trail;

		public float trailTime = 0.2f;

		public float trailStartVelocity = 8f;

		public float trailEndVelocity = 6f;

		private float time;

		private float goalTime;

		private float timeDampVelocity;

		private Rigidbody rb;

		private SpringJoint joint;

		public WeaponState WeaponState
		{
			get
			{
				return weaponState;
			}
			set
			{
				if (!weaponState.Equals(value))
				{
					weaponState = value;
					UpdateWeaponState();
				}
			}
		}

		public float Velocity
		{
			get
			{
				return rb.velocity.magnitude;
			}
		}

		private void OnEnable()
		{
			rb = GetComponent<Rigidbody>();
			joint = GetComponent<SpringJoint>();
			WeaponState = Standard;
		}

		private void FixedUpdate()
		{
			if (trail != null && rb != null)
			{
				float magnitude = rb.velocity.magnitude;
				if (goalTime == 0f && magnitude > trailStartVelocity)
				{
					goalTime = trailTime;
				}
				if (goalTime != 0f && magnitude < trailEndVelocity)
				{
					goalTime = 0f;
				}
				time = Mathf.SmoothDamp(time, goalTime, ref timeDampVelocity, 0.1f);
				trail.time = time;
			}
		}

		private void UpdateWeaponState()
		{
			rb.mass = weaponState.mass;
			rb.drag = weaponState.drag;
			joint.maxDistance = weaponState.reach;
			joint.spring = weaponState.spring;
			joint.damper = weaponState.damper;
		}
	}
}
