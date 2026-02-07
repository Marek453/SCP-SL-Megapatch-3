using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class ClampedCursor : MonoBehaviour
	{
		public Transform cursor;

		private Vector3 initialPosition;

		private Vector3 velocity;

		private void Start()
		{
			if (cursor != null)
			{
				initialPosition = cursor.transform.position;
			}
		}

		private void Update()
		{
			if (cursor != null)
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity) && InsideTransform(base.transform, hitInfo.point))
				{
					cursor.transform.position = hitInfo.point + hitInfo.normal * 0.2f;
					cursor.transform.rotation = Quaternion.LookRotation(-hitInfo.normal);
					velocity = Vector3.zero;
				}
				else
				{
					cursor.transform.position = Vector3.SmoothDamp(cursor.transform.position, initialPosition, ref velocity, 0.1f);
				}
			}
		}

		private bool InsideTransform(Transform transform, Vector3 point)
		{
			Vector3 vector = transform.InverseTransformPoint(point);
			if (vector.x > 0.5f || vector.x < -0.5f)
			{
				return false;
			}
			if (vector.y > 0.5f || vector.y < -0.5f)
			{
				return false;
			}
			if (vector.z > 0.5f || vector.z < -0.5f)
			{
				return false;
			}
			return true;
		}

		public void OnDrawGizmosSelected()
		{
			DrawGizmo(true);
		}

		private void DrawGizmo(bool Selected)
		{
			if (base.isActiveAndEnabled)
			{
				Color color = new Color(0.8f, 0.8f, 0.8f, 1f);
				Gizmos.matrix = base.transform.localToWorldMatrix;
				color.a = ((!Selected) ? 0.05f : 0.5f);
				Gizmos.color = color;
				Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
			}
		}
	}
}
