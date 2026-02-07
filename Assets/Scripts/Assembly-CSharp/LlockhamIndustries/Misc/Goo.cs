using System.Collections;
using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(ProjectionRenderer))]
	public class Goo : MonoBehaviour
	{
		public GooType type;

		private ProjectionRenderer projection;

		private void OnEnable()
		{
			projection = GetComponent<ProjectionRenderer>();
			StartCoroutine(Register());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			Deregister();
		}

		private IEnumerator Register()
		{
			while (!GooManager.Register(projection, type))
			{
				yield return new WaitForFixedUpdate();
			}
		}

		private void Deregister()
		{
			GooManager.Deregister(projection, type);
		}
	}
}
