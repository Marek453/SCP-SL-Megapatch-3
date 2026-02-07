using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[RequireComponent(typeof(ProjectionRenderer))]
	public class Cull : Modifier
	{
		public float cullTime = 4f;

		private ProjectionRenderer projection;

		private float timeElapsed;

		private void Awake()
		{
			projection = GetComponent<ProjectionRenderer>();
		}

		protected override void Begin()
		{
			timeElapsed = 0f;
		}

		public override void Perform()
		{
			if (timeElapsed < cullTime)
			{
				timeElapsed += base.UpdateRate;
				if (projection.Renderer.isVisible)
				{
					timeElapsed = 0f;
				}
			}
			else
			{
				projection.Destroy();
			}
		}
	}
}
