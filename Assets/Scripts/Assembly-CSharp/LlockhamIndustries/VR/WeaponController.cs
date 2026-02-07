using LlockhamIndustries.Misc;

namespace LlockhamIndustries.VR
{
	public class WeaponController : VRController
	{
		public Weapon weapon;

		private WeaponMode mode;

		public void Update()
		{
			if (mode != WeaponMode.Prereleased && base.Trigger)
			{
				mode = WeaponMode.Prereleased;
			}
			else if (mode == WeaponMode.Prereleased && !base.Trigger)
			{
				if (weapon.Velocity > 2f)
				{
					mode = WeaponMode.Released;
				}
				else
				{
					mode = WeaponMode.Held;
				}
			}
			if (weapon != null)
			{
				if (mode == WeaponMode.Prereleased)
				{
					weapon.WeaponState = weapon.PreReleased;
				}
				else if (mode == WeaponMode.Released)
				{
					weapon.WeaponState = weapon.Released;
				}
				else if (mode == WeaponMode.Held && base.Grip)
				{
					weapon.WeaponState = weapon.Extended;
				}
				else
				{
					weapon.WeaponState = weapon.Standard;
				}
			}
			if (base.TrackPad)
			{
				DestructableManager.Restore();
			}
		}
	}
}
