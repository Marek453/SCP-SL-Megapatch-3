using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class TargetedCameraController : GenericCameraController
	{
		[Header("Target")]
		public Transform target;

		public float trackingSpeed = 0.1f;

		[Header("Look")]
		public float lookSensitivity = 0.3f;

		public float lookSpeed = 0.2f;

		public AnimationCurve lookCurve;

		private Vector2 screenOffset;

		private Vector3 basePos;

		private Vector3 cameraVelocity;

		private Vector3 offset;

		private Vector3 offsetVelocity;

		private void Update()
		{
			OffsetInput();
			RotationZoomInput();
		}

		private void LateUpdate()
		{
			ApplyPosition();
			ApplyRotationZoom();
		}

		private void OffsetInput()
		{
			Vector2 vector = new Vector2(Screen.width / 2, Screen.height / 2);
			screenOffset.x = (Input.mousePosition.x - vector.x) / vector.x;
			screenOffset.y = (Input.mousePosition.y - vector.y) / vector.y;
		}

		private void ApplyPosition()
		{
			if (target != null)
			{
				basePos = Vector3.SmoothDamp(basePos, target.position, ref cameraVelocity, trackingSpeed);
				screenOffset = screenOffset.normalized * lookCurve.Evaluate(screenOffset.magnitude) * lookSensitivity;
				Vector3 vector = base.Forward * screenOffset.y + base.Right * screenOffset.x;
				offset = Vector3.SmoothDamp(offset, vector, ref offsetVelocity, lookSpeed);
				base.transform.position = basePos + offset * zoom;
			}
		}
	}
}
