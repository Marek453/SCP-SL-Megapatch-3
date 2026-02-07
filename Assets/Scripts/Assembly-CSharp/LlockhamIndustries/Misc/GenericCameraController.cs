using System;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public abstract class GenericCameraController : MonoBehaviour
	{
		[Header("Rotation")]
		public float rotationSensitivity = 1f;

		[Header("Angle")]
		public float Angle = 44f;

		public float AngleSmooth = 0.3f;

		[Header("Zoom")]
		public float defaultZoom = 14f;

		public float minZoom = 6f;

		public float maxZoom = 40f;

		public float zoomSpeed = 12f;

		[Header("Field of View")]
		public float fieldOfView = 110f;

		public float minFOV = 80f;

		public float maxFOV = 110f;

		protected Camera controlledCamera;

		protected float cameraAngle;

		protected float angleVelocity;

		protected float zoom;

		protected Vector3 cameraOffset;

		protected float initialMouseX;

		protected float currentMousex;

		protected float initialRotOffset;

		protected float currentRotOffset;

		protected bool rotationInput;

		public Camera Camera
		{
			get
			{
				return controlledCamera;
			}
		}

		public float FieldOfView
		{
			set
			{
				if (controlledCamera == null)
				{
					controlledCamera = GetComponentInChildren<Camera>();
				}
				controlledCamera.fieldOfView = HorizontalToVerticalFOV(Mathf.Clamp(fieldOfView, minFOV, maxFOV), controlledCamera.aspect);
			}
		}

		public Vector3 Forward
		{
			get
			{
				return Vector3.Cross(controlledCamera.transform.right, Vector3.up).normalized;
			}
		}

		public Vector3 Right
		{
			get
			{
				return Vector3.Cross(-Forward, Vector3.up).normalized;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return Quaternion.LookRotation(base.transform.position - controlledCamera.transform.position);
			}
		}

		public Quaternion InverseRotation
		{
			get
			{
				return Quaternion.LookRotation(controlledCamera.transform.position - base.transform.position);
			}
		}

		public Quaternion FlattenedRotation
		{
			get
			{
				Vector3 vector = base.transform.position - controlledCamera.transform.position;
				vector.y = 0f;
				return Quaternion.LookRotation(vector.normalized);
			}
		}

		protected void Awake()
		{
			controlledCamera = GetComponentInChildren<Camera>();
		}

		protected void Start()
		{
			zoom = defaultZoom;
			cameraAngle = Angle;
		}

		protected void RotationZoomInput()
		{
			zoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
			if (Input.GetMouseButtonDown(2))
			{
				initialMouseX = Input.mousePosition.x;
				initialRotOffset = currentRotOffset;
			}
			if (Input.GetMouseButton(2))
			{
				rotationInput = true;
			}
			else
			{
				rotationInput = false;
			}
		}

		protected void ApplyRotationZoom()
		{
			if (rotationInput)
			{
				currentMousex = Input.mousePosition.x;
				currentRotOffset = initialRotOffset - (currentMousex - initialMouseX) * rotationSensitivity;
			}
			cameraAngle = Mathf.SmoothDampAngle(cameraAngle, Angle, ref angleVelocity, AngleSmooth);
			float num = Mathf.Pow(fieldOfView / maxFOV, -1f);
			cameraOffset = Vector3.zero;
			cameraOffset.y = zoom * num * (1.41f * Mathf.Sin((float)Math.PI / 180f * cameraAngle));
			cameraOffset.z = (0f - zoom) * num * (1.41f * Mathf.Cos((float)Math.PI / 180f * cameraAngle));
			controlledCamera.transform.position = RotateAroundPoint(base.transform.position + cameraOffset, base.transform.position, Quaternion.Euler(new Vector3(0f, currentRotOffset, 0f)));
			controlledCamera.transform.rotation = Quaternion.LookRotation(base.transform.position - controlledCamera.transform.position);
		}

		protected static float HorizontalToVerticalFOV(float horizontalFOV, float aspect)
		{
			return 114.59156f * Mathf.Atan(Mathf.Tan(horizontalFOV * ((float)Math.PI / 180f) / 2f) / aspect);
		}

		protected Vector3 RotateAroundPoint(Vector3 point, Vector3 pivot, Quaternion Angle)
		{
			return Angle * (point - pivot) + pivot;
		}
	}
}
