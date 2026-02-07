using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class RayPrinter : Printer
	{
		public LayerMask layers;

		public void PrintOnRay(Ray Ray, float RayLength, Vector3 DecalUp = default(Vector3))
		{
			if (DecalUp == Vector3.zero)
			{
				DecalUp = Vector3.up;
			}
			RaycastHit hitInfo;
			if (Physics.Raycast(Ray, out hitInfo, RayLength, layers.value))
			{
				Print(hitInfo.point, Quaternion.LookRotation(-hitInfo.normal, DecalUp), hitInfo.transform, hitInfo.collider.gameObject.layer);
			}
		}
	}
}
