using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class ParticleWeaponController : WeaponController
	{
		[Header("Particle Fire")]
		public ParticleSystem blueParticles;

		public ParticleSystem orangeParticles;

		public int fireRate = 250;

		[Header("Recoil")]
		public float recoil = 2f;

		private ParticleSystem.EmissionModule blueEmissionModule;

		private ParticleSystem.EmissionModule orangeEmissionModule;

		public void OnEnable()
		{
			if (blueParticles != null)
			{
				blueEmissionModule = blueParticles.emission;
			}
			if (orangeParticles != null)
			{
				orangeEmissionModule = orangeParticles.emission;
			}
		}

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
			if (blueParticles != null)
			{
				if (primary)
				{
					blueEmissionModule.rateOverTimeMultiplier = fireRate;
					if (controller != null)
					{
						controller.ApplyRecoil(recoil, 0.2f);
					}
				}
				else
				{
					blueEmissionModule.rateOverTimeMultiplier = 0f;
				}
			}
			if (!(orangeParticles != null))
			{
				return;
			}
			if (secondary)
			{
				orangeEmissionModule.rateOverTimeMultiplier = fireRate;
				if (controller != null)
				{
					controller.ApplyRecoil(recoil, 0.2f);
				}
			}
			else
			{
				orangeEmissionModule.rateOverTimeMultiplier = 0f;
			}
		}
	}
}
