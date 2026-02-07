using System.Collections.Generic;
using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(GenericCameraController))]
	public class Selector : MonoBehaviour
	{
		[Header("Projections")]
		public ProjectionRenderer innards;

		public ProjectionRenderer border;

		public float width = 0.05f;

		[Header("Layers")]
		public LayerMask select;

		public LayerMask boxSelect;

		private static Selector selector;

		private GenericCameraController controller;

		private List<Selectable> available;

		private List<Selectable> selection;

		private List<Selectable> softSelection;

		private BoxSelection boxSelection;

		public static bool Initialized
		{
			get
			{
				return selector != null;
			}
		}

		public List<Selectable> Selection
		{
			get
			{
				return selection;
			}
		}

		private void Awake()
		{
			if (selector == null)
			{
				selector = this;
			}
			else if (selector != this)
			{
				Object.Destroy(this);
			}
		}

		public static void Register(Selectable Selectable)
		{
			if (selector.available == null)
			{
				selector.available = new List<Selectable>();
			}
			if (!selector.available.Contains(Selectable))
			{
				selector.available.Add(Selectable);
			}
		}

		public static void Deregister(Selectable Selectable)
		{
			if (selector.available != null)
			{
				selector.available.Remove(Selectable);
			}
			if (selector.selection != null)
			{
				selector.selection.Remove(Selectable);
			}
			if (selector.softSelection != null)
			{
				selector.softSelection.Remove(Selectable);
			}
		}

		private void Start()
		{
			controller = GetComponent<FreeCameraController>();
		}

		private void Update()
		{
			bool key = Input.GetKey(KeyCode.LeftShift);
			if (!SelectionInput(key))
			{
				BoxSelectionInput(key);
			}
		}

		private bool SelectionInput(bool Additive)
		{
			if (controller.Camera != null && Input.GetMouseButtonDown(0))
			{
				Ray ray = controller.Camera.ScreenPointToRay(Input.mousePosition);
				RaycastHit hitInfo;
				if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, select.value))
				{
					Selectable component = hitInfo.collider.gameObject.GetComponent<Selectable>();
					if (component != null)
					{
						Select(component, Additive);
						return true;
					}
				}
			}
			return false;
		}

		private void BoxSelectionInput(bool Additive)
		{
			if (controller.Camera != null)
			{
				RaycastHit hit;
				if (Input.GetMouseButtonDown(0) && CursorCast(out hit, boxSelect))
				{
					boxSelection = new BoxSelection(hit.point, hit.point, controller.FlattenedRotation, innards, border, width);
				}
				RaycastHit hit2;
				if (Input.GetMouseButton(0) && boxSelection != null && CursorCast(out hit2, boxSelect))
				{
					boxSelection.Update(hit2.point);
					UpdateSoftSelection();
				}
				if (Input.GetMouseButtonUp(0) && boxSelection != null)
				{
					boxSelection.Destroy();
					boxSelection = null;
					ApplySoftSelection(Additive);
				}
			}
		}

		public void ClearSelection()
		{
			if (selection == null)
			{
				return;
			}
			foreach (Selectable item in selection)
			{
				item.Selected = false;
			}
			selection.Clear();
		}

		public void Select(Selectable Selectable, bool Additive)
		{
			if (selection == null)
			{
				selection = new List<Selectable>();
			}
			else if (!Additive)
			{
				ClearSelection();
			}
			Selectable.Selected = true;
			selection.Add(Selectable);
		}

		public void ClearSoftSelection()
		{
			if (softSelection == null)
			{
				return;
			}
			foreach (Selectable item in softSelection)
			{
				item.Selected = false;
			}
			softSelection.Clear();
		}

		public void UpdateSoftSelection()
		{
			if (boxSelection == null || available == null)
			{
				return;
			}
			ClearSoftSelection();
			if (softSelection == null)
			{
				softSelection = new List<Selectable>();
			}
			foreach (Selectable item in available)
			{
				if (boxSelection.Contains(item.transform.position))
				{
					item.Selected = true;
					softSelection.Add(item);
				}
			}
		}

		public void ApplySoftSelection(bool Additive)
		{
			if (softSelection == null)
			{
				return;
			}
			if (selection == null)
			{
				selection = new List<Selectable>();
			}
			else if (!Additive)
			{
				ClearSelection();
			}
			foreach (Selectable item in softSelection)
			{
				item.Selected = true;
				selection.Add(item);
			}
			softSelection.Clear();
		}

		private bool CursorCast(out RaycastHit hit, LayerMask Layers)
		{
			Ray ray = controller.Camera.ScreenPointToRay(Input.mousePosition);
			return Physics.Raycast(ray, out hit, float.PositiveInfinity, Layers.value);
		}
	}
}
