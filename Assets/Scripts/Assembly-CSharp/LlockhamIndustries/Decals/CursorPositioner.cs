using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class CursorPositioner : Positioner
	{
		public Camera projectionCamera;

		protected override void Start()
		{
			if (projectionCamera == null)
			{
				projectionCamera = Camera.main;
			}
			base.Start();
		}

		private void LateUpdate()
		{
			Reproject(projectionCamera.ScreenPointToRay(Input.mousePosition), float.PositiveInfinity, projectionCamera.transform.up);
		}
	}
}
