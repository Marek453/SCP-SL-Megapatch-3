using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Rigidbody))]
	public class Destructable : MonoBehaviour
	{
		private Rigidbody rb;

		private Vector3 position;

		private Quaternion rotation;

		public void OnEnable()
		{
			rb = GetComponent<Rigidbody>();
			position = base.transform.position;
			rotation = base.transform.rotation;
			Register();
		}

		public void OnDisable()
		{
			Deregister();
		}

		public void Register()
		{
			DestructableManager.Register(this);
		}

		public void Deregister()
		{
			DestructableManager.Deregister(this);
		}

		public void Restore()
		{
			rb.position = position;
			rb.rotation = rotation;
		}

		public void Destroy()
		{
		}
	}
}
