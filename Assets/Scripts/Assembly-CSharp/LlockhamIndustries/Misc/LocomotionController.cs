using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Locomotion))]
	public class LocomotionController : MonoBehaviour
	{
		public GenericCameraController cameraController;

		public float standardSpeed = 0.8f;

		public float balancedSpeed = 0.5f;

		public float sprintSpeed = 1.6f;

		private Locomotion locomotion;

		private Plane plane = new Plane(Vector3.up, 0f);

		private bool balanced;

		private float movementSpeed;

		private Vector3 movementVector;

		private float timeSinceDodge;

		private void Awake()
		{
			locomotion = GetComponent<Locomotion>();
		}

		private void Update()
		{
			MovementSpeedInput();
			MovementInput();
			BalanceInput();
		}

		private void MovementSpeedInput()
		{
			movementSpeed = standardSpeed;
			if (!balanced)
			{
				if (Input.GetKey(KeyCode.LeftShift) && movementVector.magnitude > 0f)
				{
					movementSpeed = sprintSpeed;
				}
			}
			else
			{
				movementSpeed = balancedSpeed;
			}
		}

		private void MovementInput()
		{
			movementVector = Vector3.zero;
			if ((Input.GetAxisRaw("Horizontal") != 0f || Input.GetAxisRaw("Vertical") != 0f) && cameraController != null)
			{
				Vector3 zero = Vector3.zero;
				zero -= cameraController.Forward * Input.GetAxisRaw("Vertical");
				zero -= cameraController.Right * Input.GetAxisRaw("Horizontal");
				float num = Mathf.Max(Mathf.Abs(zero.x), Mathf.Abs(zero.z));
				movementVector = zero.normalized * num;
			}
			locomotion.Movement = movementVector * movementSpeed;
		}

		private void BalanceInput()
		{
			if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
			{
				balanced = true;
			}
			else
			{
				balanced = false;
			}
			if (balanced)
			{
				if (cameraController == null)
				{
					Debug.Log("No Camera Controller Assigned! Please assign a valid camera controller.");
					return;
				}
				Ray ray = cameraController.GetComponentInChildren<Camera>().ScreenPointToRay(Input.mousePosition);
				float enter;
				if (plane.Raycast(ray, out enter))
				{
					locomotion.Direction = -(ray.GetPoint(enter) - base.transform.position).normalized;
				}
				else
				{
					Debug.Log("Error Casting to Plane, Cannot Determine Cursor Location");
				}
			}
			else
			{
				locomotion.Direction = movementVector.normalized;
			}
		}
	}
}
