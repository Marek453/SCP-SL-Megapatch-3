using System.Collections;
using LlockhamIndustries.Decals;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(RayPositioner))]
	public class Selectable : MonoBehaviour
	{
		public Color selectedColor = Color.white;

		public Color unselectedColor = Color.gray;

		private ProjectionRenderer decal;

		private bool selected;

		public bool Selected
		{
			get
			{
				return selected;
			}
			set
			{
				if (decal != null && selected != value)
				{
					selected = value;
					if (selected)
					{
						decal.SetColor(0, selectedColor);
						decal.UpdateProperties();
					}
					else
					{
						decal.SetColor(0, unselectedColor);
						decal.UpdateProperties();
					}
				}
			}
		}

		private ProjectionRenderer Decal
		{
			set
			{
				if (decal != value)
				{
					decal = value;
					if (selected)
					{
						decal.SetColor(0, selectedColor);
						decal.UpdateProperties();
					}
					else
					{
						decal.SetColor(0, unselectedColor);
						decal.UpdateProperties();
					}
				}
			}
		}

		private void OnEnable()
		{
			StartCoroutine(Register());
			StartCoroutine(GrabProjection());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
			Deregister();
		}

		private IEnumerator GrabProjection()
		{
			RayPositioner positioner = GetComponent<RayPositioner>();
			while (decal == null)
			{
				Decal = positioner.Active;
				yield return new WaitForEndOfFrame();
			}
		}

		private IEnumerator Register()
		{
			while (!Selector.Initialized)
			{
				yield return new WaitForEndOfFrame();
			}
			Selector.Register(this);
		}

		private void Deregister()
		{
			if (Selector.Initialized)
			{
				Selector.Deregister(this);
			}
		}
	}
}
