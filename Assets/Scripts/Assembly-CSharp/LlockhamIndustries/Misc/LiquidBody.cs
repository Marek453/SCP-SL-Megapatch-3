using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Collider))]
	[RequireComponent(typeof(Rigidbody))]
	public class LiquidBody : MonoBehaviour
	{
		[Header("Collision ripples")]
		public ProjectionRenderer ripple;

		[Header("Tide")]
		public float tidalHeight = 1f;

		public float tidalScale = 1.2f;

		public float speed = 10f;

		public AnimationCurve curve;

		private float tidalPosition;

		private Vector3 initialPosition;

		private Vector3 initialScale;

		private void Awake()
		{
			initialPosition = base.transform.position;
			initialScale = base.transform.localScale;
		}

		private void FixedUpdate()
		{
			curve.postWrapMode = WrapMode.PingPong;
			tidalPosition += Time.fixedDeltaTime / speed;
			float num = curve.Evaluate(tidalPosition);
			base.transform.position = initialPosition + Vector3.up * tidalHeight * num;
			base.transform.localScale = initialScale * Mathf.Lerp(1f, tidalScale, num);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (ripple != null && other.GetComponent<Rigidbody>() != null)
			{
				ProjectionRenderer projectionRenderer = Object.Instantiate(ripple);
				projectionRenderer.transform.position = other.transform.position;
				projectionRenderer.transform.rotation = Quaternion.LookRotation(Vector3.down);
				Bounds bounds = other.bounds;
				projectionRenderer.transform.localScale = Vector3.one * Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
				projectionRenderer.transform.parent = base.transform;
			}
		}
	}
}
