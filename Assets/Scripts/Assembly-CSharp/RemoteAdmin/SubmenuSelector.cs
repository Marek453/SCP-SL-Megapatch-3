using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RemoteAdmin
{
	public class SubmenuSelector : MonoBehaviour
	{
		[Serializable]
		public class SubMenu
		{
			public Button button;

			public int argumentsCount;

			public string commandTemplate;

			public GameObject panel;

			public TextMeshProUGUI optionalDisplay;

			public Button submitButton;
		}

		public Color c_selected;

		public Color c_deselected;

		public SubMenu[] menus;

		private string[] arguments;

		public static SubmenuSelector singleton;

		private int currentMenu;

		private void Awake()
		{
			singleton = this;
		}

		private void Start()
		{
			menus[0].panel.SetActive(true);
			SelectMenu(0);
			SubMenu[] array = menus;
			foreach (SubMenu subMenu in array)
			{
				subMenu.button.interactable = true;
			}
		}

		public void SetProperty(int field, string value)
		{
			arguments[field - 1] = value;
			if (!(menus[currentMenu].submitButton != null))
			{
				return;
			}
			menus[currentMenu].submitButton.interactable = true;
			string[] array = arguments;
			foreach (string value2 in array)
			{
				if (string.IsNullOrEmpty(value2) && arguments.Length > 0)
				{
					menus[currentMenu].submitButton.interactable = false;
				}
			}
		}

		public void Confirm()
		{
			if (menus[currentMenu].optionalDisplay != null)
			{
				menus[currentMenu].optionalDisplay.text = string.Empty;
			}
			string text = menus[currentMenu].commandTemplate;
			List<string> list = new List<string>();
			string text2 = string.Empty;
			foreach (PlayerRecord record in PlayerRecord.records)
			{
				if (record.isSelected)
				{
					text2 = text2 + record.playerId + ".";
				}
			}
			list.Add(text2);
			string[] array = arguments;
			foreach (string item in array)
			{
				list.Add(item);
			}
			if (text.Contains("{0}"))
			{
				try
				{
					text = string.Format(text, list.ToArray());
				}
				catch
				{
					Debug.Log(text + ":" + list.Count);
				}
			}
			PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery(text);
		}

		public void AdminToolsConfirm(string operation)
		{
			string text = string.Empty;
			foreach (PlayerRecord record in PlayerRecord.records)
			{
				if (record.isSelected)
				{
					text = text + record.playerId + ".";
				}
			}
			switch (operation)
			{
			case "OverwatchEnable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("overwatch " + text + " 1");
				break;
			case "OverwatchDisable":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("overwatch " + text + " 0");
				break;
			case "Open":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("open " + DoorPrinter.SelectedDoors);
				break;
			case "Close":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("close " + DoorPrinter.SelectedDoors);
				break;
			case "Lock":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("lock " + DoorPrinter.SelectedDoors);
				break;
			case "Unlock":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("unlock " + DoorPrinter.SelectedDoors);
				break;
			case "Destroy":
				PlayerManager.localPlayer.GetComponent<QueryProcessor>().CmdSendQuery("destroy " + DoorPrinter.SelectedDoors);
				break;
			}
		}

		public void SelectMenu(Button b)
		{
			for (int i = 0; i < menus.Length; i++)
			{
				bool flag = menus[i].button == b;
				menus[i].button.GetComponent<Text>().color = ((!flag) ? c_deselected : c_selected);
				menus[i].panel.SetActive(flag);
				if (flag)
				{
					SelectMenu(i);
				}
			}
		}

		public void SelectMenu(int i)
		{
			currentMenu = i;
			arguments = new string[menus[i].argumentsCount];
		}
	}
}
