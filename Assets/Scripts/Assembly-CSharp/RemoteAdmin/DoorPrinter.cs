using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.RemoteAdmin;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	internal class DoorPrinter : MonoBehaviour
	{
		public GameObject Template;

		public Transform Parent;

		public static string SelectedDoors;

		public static readonly string[] SpecialValues = new string[2] { "*", "!*" };

		public static readonly string[] SpecialTexts = new string[2] { "(All listed)", "(All not listed)" };

		private IEnumerator Start()
		{
			while (PlayerManager.localPlayer == null)
			{
				yield return new WaitForEndOfFrame();
			}
			Door[] alldoors = Object.FindObjectsOfType<Door>();
			List<Door> list = alldoors.Where((Door item) => !string.IsNullOrEmpty(item.DoorName)).ToList();
			list.Sort();
			for (int i = 0; i < SpecialValues.Length; i++)
			{
				GameObject gameObject = Object.Instantiate(Template, Parent);
				gameObject.transform.localScale = Vector3.one;
				gameObject.GetComponentInChildren<Text>().text = SpecialTexts[i];
				gameObject.GetComponent<DoorRemoteAdminButton>().OvrValue = SpecialValues[i];
			}
			foreach (Door item in list)
			{
				GameObject gameObject2 = Object.Instantiate(Template, Parent);
				gameObject2.transform.localScale = Vector3.one;
				gameObject2.GetComponentInChildren<Text>().text = item.DoorName;
				gameObject2.GetComponent<DoorRemoteAdminButton>().Door = item;
			}
			DoorRemoteAdminButton.Buttons = base.transform.GetComponentsInChildren<DoorRemoteAdminButton>(true);
			DoorRemoteAdminButton.Color = GetComponentInParent<SubmenuSelector>().c_selected;
		}
	}
}
