using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public abstract class Positioner : MonoBehaviour
	{
		public ProjectionRenderer projection;

		public LayerMask layers = -1;

		public bool alwaysVisible;

		private ProjectionRenderer proj;

		public ProjectionRenderer Active
		{
			get
			{
				return proj;
			}
		}

		private void OnDisable()
		{
			if (proj != null)
			{
				proj.gameObject.SetActive(false);
			}
		}

		protected virtual void Start()
		{
			if (projection != null)
			{
				proj = Object.Instantiate(projection.gameObject, DynamicDecals.System.DefaultPool.Parent).GetComponent<ProjectionRenderer>();
				proj.name = "Positioned Projection";
			}
			else
			{
				Debug.LogWarning("Positioner has no projection to position.");
			}
		}

		protected void Reproject(Ray Ray, float CastLength, Vector3 ReferenceUp)
		{
			if (proj != null)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(Ray, out hitInfo, CastLength, layers.value))
				{
					proj.gameObject.SetActive(true);
					proj.transform.rotation = Quaternion.LookRotation(-hitInfo.normal, ReferenceUp);
					proj.transform.position = hitInfo.point;
				}
				else if (!alwaysVisible)
				{
					proj.gameObject.SetActive(false);
				}
			}
		}

		private Vector3 Divide(Vector3 A, Vector3 B)
		{
			return new Vector3(A.x / B.x, A.y / B.y, A.z / B.z);
		}
	}
}
