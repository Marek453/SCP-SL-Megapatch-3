using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class HitScanWeaponController : WeaponController
	{
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
			if (timeToFire == 0f && (primary || secondary) && printer != null)
			{
				Vector3 position = cameraController.transform.position;
				Vector3 forward = cameraController.transform.forward;
				Ray ray = new Ray(position, forward);
				printer.PrintOnRay(ray, 100f, cameraController.transform.up);
				if (controller != null)
				{
					controller.ApplyRecoil(120f, 0.2f);
				}
				timeToFire = 1f / hitScanFireRate;
			}
		}
	}
}
