using System;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public abstract class Deferred : Projection
	{
		public override RenderingPaths SupportedRendering
		{
			get
			{
				return RenderingPaths.Deferred;
			}
		}
	}
}
