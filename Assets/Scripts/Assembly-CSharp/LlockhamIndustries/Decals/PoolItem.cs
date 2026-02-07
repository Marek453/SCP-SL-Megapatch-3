using System.Collections.Generic;
using LlockhamIndustries.ExtensionMethods;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class PoolItem
	{
		private ProjectionPool pool;

		private ProjectionRenderer renderer;

		public ProjectionPool Pool
		{
			get
			{
				return pool;
			}
		}

		public ProjectionRenderer Renderer
		{
			get
			{
				return renderer;
			}
		}

		private bool Valid
		{
			get
			{
				if (renderer == null)
				{
					if (pool.activePool != null)
					{
						pool.activePool.Remove(this);
					}
					if (pool.inactivePool != null)
					{
						pool.inactivePool.Remove(this);
					}
					return false;
				}
				return true;
			}
		}

		public PoolItem(ProjectionPool Pool)
		{
			pool = Pool;
			GameObject gameObject = new GameObject("Projection");
			gameObject.transform.SetParent(pool.Parent);
			gameObject.SetActive(false);
			renderer = gameObject.AddComponent<ProjectionRenderer>();
			renderer.PoolItem = this;
		}

		internal void Initialize(ProjectionRenderer Renderer = null, bool IncludeBehaviours = false)
		{
			if (!Valid)
			{
				return;
			}
			renderer.transform.SetParent(pool.Parent);
			if (Renderer != null)
			{
				renderer.Projection = Renderer.Projection;
				renderer.Tiling = Renderer.Tiling;
				renderer.Offset = Renderer.Offset;
				renderer.MaskMethod = Renderer.MaskMethod;
				renderer.MaskLayer1 = Renderer.MaskLayer1;
				renderer.MaskLayer2 = Renderer.MaskLayer2;
				renderer.MaskLayer3 = Renderer.MaskLayer3;
				renderer.MaskLayer4 = Renderer.MaskLayer4;
				renderer.Properties = Renderer.Properties;
				if (IncludeBehaviours)
				{
					MonoBehaviour[] components = Renderer.GetComponents<MonoBehaviour>();
					foreach (MonoBehaviour monoBehaviour in components)
					{
						if (monoBehaviour.GetType() != typeof(Transform) && monoBehaviour.GetType() != typeof(ProjectionRenderer))
						{
							MonoBehaviour monoBehaviour2 = renderer.gameObject.AddComponent(monoBehaviour);
							monoBehaviour2.enabled = monoBehaviour.enabled;
						}
					}
				}
				renderer.transform.localScale = Renderer.transform.localScale;
				renderer.gameObject.layer = Renderer.gameObject.layer;
				renderer.gameObject.tag = Renderer.gameObject.tag;
			}
			else
			{
				renderer.transform.localScale = Vector3.one;
			}
			renderer.gameObject.SetActive(true);
		}

		internal void Terminate()
		{
			if (!Valid)
			{
				return;
			}
			renderer.gameObject.SetActive(false);
			Component[] components = renderer.gameObject.GetComponents<Component>();
			foreach (Component component in components)
			{
				if (component.GetType() != typeof(Transform) && component.GetType() != typeof(ProjectionRenderer))
				{
					Object.Destroy(component);
				}
			}
			renderer.transform.SetParent(pool.Parent);
		}

		public void Return()
		{
			pool.activePool.Remove(this);
			Terminate();
			if (pool.inactivePool == null)
			{
				pool.inactivePool = new List<PoolItem>();
			}
			pool.inactivePool.Add(this);
		}
	}
}
