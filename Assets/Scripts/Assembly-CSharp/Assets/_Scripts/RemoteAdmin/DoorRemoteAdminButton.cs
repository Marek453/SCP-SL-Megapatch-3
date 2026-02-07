using RemoteAdmin;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.RemoteAdmin
{
	internal class DoorRemoteAdminButton : MonoBehaviour
	{
		public Door Door;

		public string OvrValue;

		private Outline _outline;

		public static Color Color;

		public static DoorRemoteAdminButton[] Buttons;

		public void Click()
		{
			DoorRemoteAdminButton[] buttons = Buttons;
			foreach (DoorRemoteAdminButton doorRemoteAdminButton in buttons)
			{
				doorRemoteAdminButton.SetStatus(false);
			}
			SetStatus(true);
		}

		private void OnEnable()
		{
			SetStatus(false);
		}

		public void SetStatus(bool b)
		{
			if (_outline == null)
			{
				_outline = GetComponent<Outline>();
			}
			_outline.effectColor = ((!b) ? Color.white : Color);
			if (b)
			{
				DoorPrinter.SelectedDoors = ((!(Door != null)) ? OvrValue : Door.DoorName);
			}
		}
	}
}
