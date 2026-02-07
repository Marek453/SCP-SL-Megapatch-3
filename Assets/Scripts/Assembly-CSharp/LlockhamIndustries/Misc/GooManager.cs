using System.Collections.Generic;
using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class GooManager : MonoBehaviour
	{
		private static GooManager singleton;

		private List<ProjectionRenderer> slide;

		private List<ProjectionRenderer> bounce;

		private List<ProjectionRenderer> stick;

		private void Awake()
		{
			if (singleton == null)
			{
				singleton = this;
			}
			else if (singleton != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private static List<ProjectionRenderer> GetGoo(GooType Type)
		{
			if (singleton != null)
			{
				switch (Type)
				{
				case GooType.Slide:
					if (singleton.slide == null)
					{
						singleton.slide = new List<ProjectionRenderer>();
					}
					return singleton.slide;
				case GooType.Bounce:
					if (singleton.bounce == null)
					{
						singleton.bounce = new List<ProjectionRenderer>();
					}
					return singleton.bounce;
				case GooType.Stick:
					if (singleton.stick == null)
					{
						singleton.stick = new List<ProjectionRenderer>();
					}
					return singleton.stick;
				}
			}
			return null;
		}

		public static bool Register(ProjectionRenderer Projection, GooType Type)
		{
			List<ProjectionRenderer> goo = GetGoo(Type);
			if (goo != null)
			{
				if (!goo.Contains(Projection))
				{
					goo.Add(Projection);
				}
				return true;
			}
			return false;
		}

		public static void Deregister(ProjectionRenderer Projection, GooType Type)
		{
			List<ProjectionRenderer> goo = GetGoo(Type);
			if (goo != null)
			{
				goo.Remove(Projection);
			}
		}

		public static bool WithinGoo(GooType Type, Vector3 Point, float Strictness)
		{
			List<ProjectionRenderer> goo = GetGoo(Type);
			if (goo != null)
			{
				for (int i = 0; i < goo.Count; i++)
				{
					if (goo[i].CheckIntersecting(Point) > Strictness)
					{
						return true;
					}
				}
			}
			return false;
		}
	}
}
