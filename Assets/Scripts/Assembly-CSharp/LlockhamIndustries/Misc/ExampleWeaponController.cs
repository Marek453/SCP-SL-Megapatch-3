using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[ExecuteInEditMode]
	public class ExampleWeaponController : WeaponController
	{
		[Header("Projectile Parent")]
		public Transform parent;

		[Header("Ray Projectile Fire")]
		public GameObject rayProjectile;

		public Vector3 raySourceOffset;

		public float rayFireRate = 60f;

		public float raySpread = 0.3f;

		public float raySpeed = 40f;

		[Header("Collision Projectile Fire")]
		public GameObject colliderProjectile;

		public Vector3 colliderSourceOffset;

		public float colliderFireRate = 3f;

		public float colliderSpread = 0.1f;

		public float colliderSpeed = 20f;

		[Header("Hitscan Fire")]
		public RayPrinter printer;

		public float hitScanFireRate = 1f;

		public override void UpdateWeapon()
		{
			base.UpdateWeapon();
			Fire();
		}

		private void Fire()
		{
			if (timeToFire != 0f)
			{
				return;
			}
			if (primary && rayProjectile != null)
			{
				Vector3 vector = Vector3.Slerp(base.transform.up, Random.insideUnitSphere.normalized, raySpread / 10f);
				Quaternion rotation = Quaternion.LookRotation(vector, base.transform.forward);
				GameObject gameObject = Object.Instantiate(rayProjectile, base.transform.TransformPoint(raySourceOffset), rotation, parent);
				gameObject.name = "Ray";
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				if (controller != null)
				{
					component.velocity = controller.GetComponent<Rigidbody>().velocity;
				}
				component.AddForce(vector * raySpeed, ForceMode.VelocityChange);
				if (controller != null)
				{
					controller.ApplyRecoil(20f, 0.2f);
				}
				timeToFire = 1f / rayFireRate;
			}
			if (secondary && colliderProjectile != null)
			{
				Vector3 vector2 = Vector3.Slerp(base.transform.up, Random.insideUnitSphere.normalized, colliderSpread / 10f);
				Quaternion rotation2 = Quaternion.LookRotation(vector2, base.transform.forward);
				GameObject gameObject2 = Object.Instantiate(colliderProjectile, base.transform.TransformPoint(colliderSourceOffset), rotation2, parent);
				gameObject2.name = "Collider";
				Rigidbody component2 = gameObject2.GetComponent<Rigidbody>();
				if (controller != null)
				{
					component2.velocity = controller.GetComponent<Rigidbody>().velocity;
				}
				component2.AddForce(vector2 * colliderSpeed, ForceMode.VelocityChange);
				if (controller != null)
				{
					controller.ApplyRecoil(100f, 0.3f);
				}
				timeToFire = 1f / colliderFireRate;
			}
			if (!alternate || !(printer != null))
			{
				return;
			}
			if (cameraController == null)
			{
				Debug.Log("No Camera Set! Please set a camera for the weapon to aim with");
				return;
			}
			Vector3 position = cameraController.transform.position;
			Vector3 forward = cameraController.transform.forward;
			Ray ray = new Ray(position, forward);
			printer.PrintOnRay(ray, 100f, cameraController.transform.up);
			if (controller != null)
			{
				controller.ApplyRecoil(200f, 0.4f);
			}
			timeToFire = 1f / hitScanFireRate;
		}
	}
}
