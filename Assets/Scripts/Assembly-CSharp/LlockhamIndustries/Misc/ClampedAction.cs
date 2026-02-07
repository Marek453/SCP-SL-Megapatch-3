using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public abstract class ClampedAction : MonoBehaviour
	{
		protected abstract void Perform(Vector3 point);

		private void Update()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hitInfo, float.PositiveInfinity) && InsideTransform(base.transform, hitInfo.point))
			{
				Perform(hitInfo.point);
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
