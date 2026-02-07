using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class ProjectionPool
	{
		private PoolInstance instance;

		private Transform parent;

		internal List<PoolItem> activePool;

		internal List<PoolItem> inactivePool;

		public string Title
		{
			get
			{
				return instance.title;
			}
		}

		private int Limit
		{
			get
			{
				return instance.limits[QualitySettings.GetQualityLevel()];
			}
		}

		public int ID
		{
			get
			{
				return instance.id;
			}
		}

		internal Transform Parent
		{
			get
			{
				if (parent == null)
				{
					GameObject gameObject = new GameObject(instance.title + " Pool");
					Object.DontDestroyOnLoad(gameObject);
					parent = gameObject.transform;
				}
				return parent;
			}
		}

		public ProjectionPool(PoolInstance Instance)
		{
			instance = Instance;
		}

		public static ProjectionPool GetPool(string Title)
		{
			return DynamicDecals.System.GetPool(Title);
		}

		public static ProjectionPool GetPool(int ID)
		{
			return DynamicDecals.System.GetPool(ID);
		}

		public bool CheckIntersecting(Vector3 Point, float intersectionStrength)
		{
			if (activePool != null && activePool.Count > 0)
			{
				for (int num = activePool.Count - 1; num >= 0; num--)
				{
					if (activePool[num].Renderer != null)
					{
						if (activePool[num].Renderer.CheckIntersecting(Point) > intersectionStrength)
						{
							return true;
						}
					}
					else
					{
						activePool.RemoveAt(num);
					}
				}
			}
			return false;
		}

		public ProjectionRenderer Request(ProjectionRenderer Renderer = null, bool IncludeBehaviours = false)
		{
			ProjectionRenderer projectionRenderer = null;
			while (projectionRenderer == null)
			{
				projectionRenderer = RequestRenderer(Renderer, IncludeBehaviours);
			}
			return projectionRenderer;
		}

		private ProjectionRenderer RequestRenderer(ProjectionRenderer Renderer = null, bool IncludeBehaviours = false)
		{
			if (activePool == null)
			{
				activePool = new List<PoolItem>();
			}
			if (inactivePool != null && inactivePool.Count > 0)
			{
				PoolItem poolItem = inactivePool[0];
				inactivePool.RemoveAt(0);
				activePool.Add(poolItem);
				poolItem.Initialize(Renderer, IncludeBehaviours);
				return poolItem.Renderer;
			}
			if (activePool.Count < Limit)
			{
				PoolItem poolItem2 = new PoolItem(this);
				poolItem2.Initialize(Renderer, IncludeBehaviours);
				activePool.Add(poolItem2);
				return poolItem2.Renderer;
			}
			PoolItem poolItem3 = activePool[0];
			poolItem3.Terminate();
			activePool.RemoveAt(0);
			activePool.Add(poolItem3);
			poolItem3.Initialize(Renderer, IncludeBehaviours);
			return poolItem3.Renderer;
		}
	}
}
