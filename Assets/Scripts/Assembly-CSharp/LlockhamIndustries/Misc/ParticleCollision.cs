using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class ParticleCollision : MonoBehaviour
	{
		public ParticleSystem partSystem;

		private void OnCollisionEnter(Collision collision)
		{
			GameObject gameObject = Object.Instantiate(partSystem, base.transform.position, partSystem.transform.rotation, base.transform.parent).gameObject;
			gameObject.name = "Splash Particles";
		}
	}
}
