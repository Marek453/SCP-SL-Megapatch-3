using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[ExecuteInEditMode]
	public class FirstPersonCharacterController : MonoBehaviour
	{
		[Header("Look")]
		public float lookSensitivity = 3f;

		[Header("Move")]
		public float moveAcceleration = 0.2f;

		public float moveSpeed = 8f;

		[Header("Jump")]
		public float jumpAcceleration = 1f;

		[Header("Camera")]
		public Camera cameraControlled;

		public float cameraSmooth = 0.2f;

		public Vector3 cameraOffset = new Vector3(0f, 0.6f, 0f);

		[Header("Weapon")]
		public WeaponController weapon;

		private Rigidbody attachedRigidbody;

		private CapsuleCollider capsuleCollider;

		private Vector3 cameraRotation;

		private Vector2 lookDelta;

		private float recoil;

		private float recoilDuration;

		private float recoilVelocity;

		private Vector3 moveDelta;

		private bool grounded;

		private int collisions;

		private bool jumpInput;

		private Vector3 cameraVelocity;

		public bool Grounded
		{
			get
			{
				return grounded;
			}
		}

		private void Awake()
		{
			attachedRigidbody = GetComponent<Rigidbody>();
			capsuleCollider = GetComponent<CapsuleCollider>();
			if (Application.isPlaying)
			{
				Cursor.lockState = CursorLockMode.Confined;
				Cursor.visible = false;
			}
		}

		private void OnEnable()
		{
			if (cameraControlled == null)
			{
				cameraControlled = Camera.main;
			}
			cameraRotation = cameraControlled.transform.rotation.eulerAngles;
		}

		private void Update()
		{
			lookDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
			moveDelta = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
			if (Input.GetKey(KeyCode.Space))
			{
				jumpInput = true;
			}
			else
			{
				jumpInput = false;
			}
		}

		private void FixedUpdate()
		{
			Vector3 eulerAngles = base.transform.rotation.eulerAngles;
			eulerAngles.y += lookDelta.x * lookSensitivity;
			base.transform.rotation = Quaternion.Euler(eulerAngles);
			Vector3 velocity = attachedRigidbody.velocity;
			Vector3 vector = base.transform.rotation * moveDelta.normalized * moveAcceleration;
			velocity.x += vector.x;
			velocity.z += vector.z;
			Vector2 vector2 = new Vector2(velocity.x, velocity.z);
			if (vector2.magnitude > moveSpeed)
			{
				velocity.x *= moveSpeed / vector2.magnitude;
				velocity.z *= moveSpeed / vector2.magnitude;
			}
			grounded = CheckGrounded();
			if (jumpInput && grounded)
			{
				velocity.y += jumpAcceleration;
			}
			attachedRigidbody.velocity = velocity;
			cameraRotation.x -= lookDelta.y * lookSensitivity;
			cameraRotation.y += lookDelta.x * lookSensitivity;
			cameraRotation.z = 0f;
			if (cameraRotation.x < 200f)
			{
				cameraRotation.x = Mathf.Clamp(cameraRotation.x, -90f, 90f);
			}
			recoil = Mathf.SmoothDamp(recoil, 0f, ref recoilVelocity, recoilDuration);
			Vector3 euler = cameraRotation;
			euler.x -= recoil;
			cameraControlled.transform.rotation = Quaternion.Euler(euler);
			cameraControlled.transform.position = Vector3.SmoothDamp(cameraControlled.transform.position, base.transform.TransformPoint(cameraOffset), ref cameraVelocity, cameraSmooth);
			if (weapon != null)
			{
				weapon.UpdateWeapon();
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			collisions++;
		}

		private void OnCollisionExit(Collision collision)
		{
			collisions--;
		}

		private bool CheckGrounded()
		{
			return collisions > 0 && Physics.Raycast(base.transform.position, -Vector3.up, capsuleCollider.bounds.extents.y * 1.4f);
		}

		public void ApplyRecoil(float RecoilStrength, float RecoilDuration)
		{
			recoilDuration = RecoilDuration;
			recoilVelocity += RecoilStrength;
		}
	}
}
