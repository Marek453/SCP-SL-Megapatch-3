using System.Collections.Generic;

namespace LlockhamIndustries.Decals
{
	public class ProjectionData
	{
		public Projection projection;

		public List<ProjectionRenderer> instances;

		public ProjectionData(Projection Projection)
		{
			projection = Projection;
			instances = new List<ProjectionRenderer>();
		}

		public void Update()
		{
			projection.Update();
		}

		public void Add(ProjectionRenderer Instance)
		{
			if (!instances.Contains(Instance))
			{
				instances.Add(Instance);
				Instance.Data = this;
				DynamicDecals.System.MarkRenderers();
			}
		}

		public void Remove(ProjectionRenderer Instance)
		{
			instances.Remove(Instance);
			if (Instance.Data == this)
			{
				Instance.Data = null;
			}
		}

		public void MoveToTop(ProjectionRenderer Instance)
		{
			instances.Remove(Instance);
			instances.Add(Instance);
			DynamicDecals.System.MarkRenderers();
		}

		public void MoveToBottom(ProjectionRenderer Instance)
		{
			instances.Remove(Instance);
			instances.Insert(0, Instance);
			DynamicDecals.System.MarkRenderers();
		}

		public void AssertOrder(ref int Order)
		{
			if (projection.Instanced)
			{
				foreach (ProjectionRenderer instance in instances)
				{
					instance.Renderer.sortingOrder = Order;
				}
				Order++;
				return;
			}
			foreach (ProjectionRenderer instance2 in instances)
			{
				instance2.Renderer.sortingOrder = Order;
				Order++;
			}
		}

		public void EnableRenderers()
		{
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].InitializeRenderer(true);
			}
		}

		public void DisableRenderers()
		{
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].TerminateRenderer();
			}
		}

		public void UpdateRenderers()
		{
			for (int i = 0; i < instances.Count; i++)
			{
				instances[i].UpdateProjection();
			}
		}
	}
}
