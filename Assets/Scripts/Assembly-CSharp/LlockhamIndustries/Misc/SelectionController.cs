using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Selector))]
	[RequireComponent(typeof(GenericCameraController))]
	public class SelectionController : MonoBehaviour
	{
		public LayerMask Layers;

		private Selector selector;

		private GenericCameraController controller;

		private void Awake()
		{
			selector = GetComponent<Selector>();
			controller = GetComponent<GenericCameraController>();
		}

		private void Update()
		{
			if (controller.Camera != null && Input.GetMouseButtonDown(1))
			{
				Ray ray = controller.Camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, Layers.value))
				{
					CommandSelectables(hitInfo.point);
				}
			}
		}

		private void CommandSelectables(Vector3 Point)
		{
			List<Selectable> selection = selector.Selection;
			if (selection == null)
			{
				return;
			}
			foreach (Selectable item in selection)
			{
				Locomotion component = item.GetComponent<Locomotion>();
				if (component != null)
				{
					CommandUnit(component, Point);
				}
			}
		}

		private void CommandUnit(Locomotion Unit, Vector3 Point)
		{
			Point.y = 0f;
			Vector3 position = Unit.transform.position;
			position.y = 0f;
			Vector3 direction = (Unit.Movement = (position - Point).normalized);
			Unit.Direction = direction;
		}
	}
}
