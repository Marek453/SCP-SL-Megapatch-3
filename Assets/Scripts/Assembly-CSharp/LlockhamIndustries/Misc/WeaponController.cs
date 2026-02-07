using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class WeaponController : MonoBehaviour
	{
		[Header("Controllers")]
		public Camera cameraController;

		public FirstPersonCharacterController controller;

		[Header("Aiming")]
		public LayerMask layers;

		public float aimSmooth = 60f;

		public float aimDistance = 40f;

		public Vector3 rotationOffset = new Vector3(0f, 0f, 0f);

		private Vector3 targetPosition;

		protected bool primary;

		protected bool secondary;

		protected bool alternate;

		protected float timeToFire;

		private void OnEnable()
		{
			if (cameraController == null)
			{
				cameraController = Camera.main;
			}
		}

		private void Update()
		{
			primary = (Input.GetMouseButton(0) ? true : false);
			secondary = ((!Input.GetMouseButton(0) && Input.GetMouseButton(1)) ? true : false);
			alternate = ((!Input.GetMouseButton(0) && !Input.GetMouseButton(1) && Input.GetMouseButton(2)) ? true : false);
		}

		public virtual void UpdateWeapon()
		{
			Aim();
			timeToFire = Mathf.Clamp(timeToFire - Time.fixedDeltaTime, 0f, float.PositiveInfinity);
		}

		private void Aim()
		{
			if (Application.isPlaying)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(cameraController.transform.position, cameraController.transform.forward, out hitInfo, float.PositiveInfinity, layers.value))
				{
					targetPosition = hitInfo.point;
				}
				else
				{
					targetPosition = cameraController.transform.position + cameraController.transform.forward * aimDistance;
				}
				Quaternion to = Quaternion.LookRotation((targetPosition - base.transform.position).normalized, Vector3.up) * Quaternion.Euler(rotationOffset);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, to, aimSmooth * Time.deltaTime);
			}
			else
			{
				targetPosition = cameraController.transform.position + cameraController.transform.forward * aimDistance;
				base.transform.rotation = Quaternion.LookRotation((targetPosition - base.transform.position).normalized, Vector3.up) * Quaternion.Euler(rotationOffset);
			}
		}
	}
}
