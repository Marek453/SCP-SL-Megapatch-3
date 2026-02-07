using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(FirstPersonCharacterController))]
	[RequireComponent(typeof(Rigidbody))]
	public class GooController : MonoBehaviour
	{
		[Header("Slide")]
		public float slideSpeed = 25f;

		[Header("Bounce")]
		public float bouncyness = 1f;

		private PhysicMaterial material;

		private Rigidbody attachedRigidbody;

		private FirstPersonCharacterController controller;

		private float originalSpeed;

		private float originalSFriction;

		private float originalDFriction;

		private PhysicMaterialCombine originalFrictionCombine;

		private void Awake()
		{
			material = GetComponent<Collider>().material;
			attachedRigidbody = GetComponent<Rigidbody>();
			controller = GetComponent<FirstPersonCharacterController>();
			originalSpeed = controller.moveSpeed;
			originalSFriction = material.staticFriction;
			originalDFriction = material.dynamicFriction;
			originalFrictionCombine = material.frictionCombine;
		}

		private void FixedUpdate()
		{
			if (controller.Grounded)
			{
				if (GooManager.WithinGoo(GooType.Slide, base.transform.position + new Vector3(0f, -1f, 0f), 0.2f))
				{
					controller.moveSpeed = slideSpeed;
					material.staticFriction = 0f;
					material.dynamicFriction = 0f;
					material.frictionCombine = PhysicMaterialCombine.Minimum;
				}
				else
				{
					controller.moveSpeed = originalSpeed;
					material.staticFriction = originalSFriction;
					material.dynamicFriction = originalDFriction;
					material.frictionCombine = originalFrictionCombine;
				}
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (GooManager.WithinGoo(GooType.Bounce, base.transform.position + new Vector3(0f, -1f, 0f), 0.2f))
			{
				attachedRigidbody.AddForce(collision.impulse * bouncyness, ForceMode.Impulse);
			}
		}
	}
}
