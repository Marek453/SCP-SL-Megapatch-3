using System;
using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class Printer : MonoBehaviour
	{
		public ProjectionRenderer[] prints = new ProjectionRenderer[1];

		public LayerMask[] printLayers;

		public string[] printTags;

		public PrintSelection printMethod;

		private ProjectionPool pool;

		[SerializeField]
		private int poolID;

		public PrintParent parent;

		public bool printBehaviours;

		public bool destroyOnPrint;

		public float frequencyTime;

		public float frequencyDistance;

		[SerializeField]
		protected PrinterOverlap[] overlaps = new PrinterOverlap[0];

		private float timeSincePrint = float.PositiveInfinity;

		private Vector3 lastPrintPos = Vector3.zero;

		public ProjectionPool Pool
		{
			get
			{
				if (pool == null)
				{
					pool = DynamicDecals.System.GetPool(poolID);
				}
				return pool;
			}
			set
			{
				poolID = value.ID;
			}
		}

		private void Update()
		{
			timeSincePrint += Time.deltaTime;
		}

		public void Print(Vector3 Position, Quaternion Rotation, Transform Surface, int Layer = 0)
		{
			if (prints == null || prints.Length < 1)
			{
				Debug.LogError("No Projections to print. Please set at least one projection to print.");
			}
			else
			{
				if (!(timeSincePrint >= frequencyTime) || !(Vector3.Distance(Position, lastPrintPos) >= frequencyDistance))
				{
					return;
				}
				if (overlaps.Length > 0)
				{
					for (int i = 0; i < overlaps.Length; i++)
					{
						ProjectionPool projectionPool = DynamicDecals.System.GetPool(overlaps[i].poolId);
						if (projectionPool.ID == overlaps[i].poolId && projectionPool.CheckIntersecting(Position, overlaps[i].intersectionStrength))
						{
							if (destroyOnPrint)
							{
								UnityEngine.Object.Destroy(base.gameObject);
							}
							return;
						}
					}
				}
				switch (printMethod)
				{
				case PrintSelection.Layer:
				{
					if (printLayers == null || printLayers.Length == 0)
					{
						PrintProjection(prints[0], Position, Rotation, Surface);
						break;
					}
					for (int k = 0; k < printLayers.Length; k++)
					{
						if ((int)printLayers[k] == ((int)printLayers[k] | (1 << Layer)))
						{
							PrintProjection(prints[k], Position, Rotation, Surface);
						}
					}
					break;
				}
				case PrintSelection.Tag:
				{
					if (printLayers == null || printLayers.Length == 0)
					{
						PrintProjection(prints[0], Position, Rotation, Surface);
						break;
					}
					bool flag = false;
					for (int l = 1; l < printTags.Length; l++)
					{
						if (printTags[l] == Surface.tag)
						{
							PrintProjection(prints[l], Position, Rotation, Surface);
							flag = true;
						}
					}
					if (!flag)
					{
						PrintProjection(prints[0], Position, Rotation, Surface);
					}
					break;
				}
				case PrintSelection.Random:
				{
					int num = UnityEngine.Random.Range(0, prints.Length);
					PrintProjection(prints[num], Position, Rotation, Surface);
					break;
				}
				case PrintSelection.All:
				{
					ProjectionRenderer[] array = prints;
					foreach (ProjectionRenderer projection in array)
					{
						PrintProjection(projection, Position, Rotation, Surface);
					}
					break;
				}
				}
				if (destroyOnPrint)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
				timeSincePrint = 0f;
				lastPrintPos = Position;
			}
		}

		private void PrintProjection(ProjectionRenderer Projection, Vector3 Position, Quaternion Rotation, Transform Surface)
		{
			if (!(Projection != null))
			{
				return;
			}
			ProjectionRenderer projectionRenderer = Pool.Request(Projection, printBehaviours);
			projectionRenderer.transform.position = Position;
			projectionRenderer.transform.rotation = Rotation;
			if (parent != PrintParent.Surface)
			{
				return;
			}
			Transform transform = null;
			IEnumerator enumerator = Surface.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform2 = (Transform)enumerator.Current;
					if (transform2.name == "Projections")
					{
						transform = transform2;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			if (transform == null)
			{
				transform = new GameObject("Projections").transform;
				transform.SetParent(Surface);
			}
			projectionRenderer.transform.SetParent(transform);
		}
	}
}
