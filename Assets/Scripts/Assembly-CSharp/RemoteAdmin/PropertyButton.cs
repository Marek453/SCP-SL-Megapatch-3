using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class PropertyButton : MonoBehaviour
	{
		private PropertyButton[] otherbuttons;

		private Outline outline;

		private Color color;

		public int argumentId;

		public string value;

		private void Start()
		{
			color = GetComponentInParent<SubmenuSelector>().c_selected;
			otherbuttons = base.transform.parent.GetComponentsInChildren<PropertyButton>(true);
		}

		public void Click()
		{
			PropertyButton[] array = otherbuttons;
			foreach (PropertyButton propertyButton in array)
			{
				propertyButton.SetStatus(false);
			}
			SetStatus(true);
		}

		private void OnEnable()
		{
			SetStatus(false);
		}

		private void SetStatus(bool b)
		{
			if (outline == null)
			{
				outline = GetComponent<Outline>();
			}
			outline.effectColor = ((!b) ? Color.white : color);
			if (b)
			{
				GetComponentInParent<SubmenuSelector>().SetProperty(argumentId, value);
			}
		}
	}
}
