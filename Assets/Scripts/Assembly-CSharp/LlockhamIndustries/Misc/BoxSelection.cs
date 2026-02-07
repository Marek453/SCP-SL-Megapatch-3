using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class BoxSelection
	{
		private Vector3 start;

		private Vector3 end;

		private Transform selection;

		private float width;

		private ProjectionRenderer core;

		private ProjectionRenderer left;

		private ProjectionRenderer right;

		private ProjectionRenderer top;

		private ProjectionRenderer bottom;

		public float XMin
		{
			get
			{
				return Mathf.Min(start.x, end.x);
			}
		}

		public float XMax
		{
			get
			{
				return Mathf.Max(start.x, end.x);
			}
		}

		public float ZMin
		{
			get
			{
				return Mathf.Min(start.z, end.z);
			}
		}

		public float ZMax
		{
			get
			{
				return Mathf.Max(start.z, end.z);
			}
		}

		public BoxSelection(Vector3 StartPosition, Vector3 EndPosition, Quaternion Orientation, ProjectionRenderer Innards, ProjectionRenderer Border, float Width)
		{
			selection = new GameObject("Box Selection").transform;
			selection.rotation = Orientation;
			selection.position = StartPosition;
			start = selection.InverseTransformPoint(StartPosition);
			end = selection.InverseTransformPoint(EndPosition);
			width = Width;
			ProjectionPool pool = ProjectionPool.GetPool(0);
			core = pool.Request(Innards);
			left = pool.Request(Border);
			right = pool.Request(Border);
			top = pool.Request(Border);
			bottom = pool.Request(Border);
			core.transform.SetParent(selection);
			left.transform.SetParent(selection);
			right.transform.SetParent(selection);
			top.transform.SetParent(selection);
			bottom.transform.SetParent(selection);
			UpdateDisplays();
		}

		public void Destroy()
		{
			core.Destroy();
			left.Destroy();
			right.Destroy();
			top.Destroy();
			bottom.Destroy();
			Object.Destroy(selection.gameObject);
		}

		public void Update(Vector3 Position)
		{
			end = selection.InverseTransformPoint(Position);
			UpdateDisplays();
		}

		public void UpdateDisplays()
		{
			float xMin = XMin;
			float xMax = XMax;
			float zMin = ZMin;
			float zMax = ZMax;
			UpdateDisplay(core.transform, xMin, xMax, zMin, zMax);
			UpdateDisplay(right.transform, xMax - width, xMax, zMin, zMax);
			UpdateDisplay(left.transform, xMin, xMin + width, zMin, zMax);
			UpdateDisplay(top.transform, xMin, xMax, zMax - width, zMax);
			UpdateDisplay(bottom.transform, xMin, xMax, zMin, zMin + width);
		}

		public void UpdateDisplay(Transform Display, float XMin, float XMax, float ZMin, float ZMax)
		{
			Display.rotation = selection.rotation * Quaternion.Euler(90f, 0f, 0f);
			Display.localPosition = new Vector3((XMin + XMax) / 2f, 0f, (ZMin + ZMax) / 2f);
			Display.localScale = new Vector3(Mathf.Max(XMax - XMin, 0.01f), Mathf.Max(ZMax - ZMin, 0.01f), 100f);
		}

		public bool Contains(Vector3 Point)
		{
			Vector3 vector = selection.InverseTransformPoint(Point);
			if (vector.x > XMin && vector.x < XMax && vector.z > ZMin && vector.z < ZMax)
			{
				return true;
			}
			return false;
		}
	}
}
