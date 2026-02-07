using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class FreeCameraController : GenericCameraController
	{
		[Header("Movement")]
		public float movementSpeed = 0.1f;

		public float movementThreshold = 0.1f;

		[Header("Limits")]
		public float minX = -10f;

		public float maxX = 10f;

		public float minZ = -10f;

		public float maxZ = 10f;

		private Vector2 mousePosition;

		private Vector3 cameraVelocity;

		private void Update()
		{
			EdgeScrollInput();
			RotationZoomInput();
		}

		private void LateUpdate()
		{
			ApplyEdgeScroll();
			ApplyRotationZoom();
		}

		private void EdgeScrollInput()
		{
			mousePosition = new Vector2(Input.mousePosition.x / (float)Screen.width, Input.mousePosition.y / (float)Screen.height);
		}

		private void ApplyEdgeScroll()
		{
			Vector3 zero = Vector3.zero;
			if (mousePosition.x < movementThreshold)
			{
				zero -= base.Right * (movementThreshold - mousePosition.x) / movementThreshold * movementSpeed;
			}
			if (1f - mousePosition.x < movementThreshold)
			{
				zero += base.Right * (movementThreshold - (1f - mousePosition.x)) / movementThreshold * movementSpeed;
			}
			if (mousePosition.y < movementThreshold)
			{
				zero -= base.Forward * (movementThreshold - mousePosition.y) / movementThreshold * movementSpeed;
			}
			if (1f - mousePosition.y < movementThreshold)
			{
				zero += base.Forward * (movementThreshold - (1f - mousePosition.y)) / movementThreshold * movementSpeed;
			}
			zero *= zoom / maxZoom;
			Vector3 target = base.transform.position + zero;
			target.x = Mathf.Clamp(target.x, minX, maxX);
			target.z = Mathf.Clamp(target.z, minZ, maxZ);
			base.transform.position = Vector3.SmoothDamp(base.transform.position, target, ref cameraVelocity, 0.1f);
		}
	}
}
