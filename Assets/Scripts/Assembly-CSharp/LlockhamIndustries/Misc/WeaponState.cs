using System;

namespace LlockhamIndustries.Misc
{
	[Serializable]
	public struct WeaponState
	{
		public float mass;

		public float drag;

		public float reach;

		public float spring;

		public float damper;

		public WeaponState(float Mass, float Drag, float Reach, float Spring, float Damper)
		{
			mass = Mass;
			drag = Drag;
			reach = Reach;
			spring = Spring;
			damper = Damper;
		}
	}
}
