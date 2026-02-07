using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
	[Serializable]
	public class Instance
	{
		public GameObject text;

		public GameObject owner;
	}

	public Transform parent;

	public Transform template;

	public GameObject panel;

	private static Transform s_parent;

	private static Transform s_template;

	private KeyCode openKey;

	public static List<Instance> instances = new List<Instance>();

	private void Update()
	{
		if (Input.GetKeyDown(openKey))
		{
			if (panel.activeSelf)
			{
				panel.SetActive(false);
			}
			else if (!Cursor.visible)
			{
				panel.SetActive(true);
			}
			CursorManager.plOp = panel.activeSelf;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			CursorManager.plOp = false;
			panel.SetActive(false);
		}
	}

	private void Start()
	{
		openKey = NewInput.GetKey("Player List");
		s_parent = parent;
		s_template = template;
	}

	public static void AddPlayer(GameObject instance)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(s_template.gameObject, s_parent);
		gameObject.transform.localScale = Vector3.one;
		gameObject.GetComponentInChildren<TextMeshProUGUI>().text = instance.GetComponent<NicknameSync>().myNick;
		gameObject.GetComponent<PlayerListElement>().instance = instance;
		instances.Add(new Instance
		{
			owner = instance,
			text = gameObject
		});
		UpdatePlayerRole(instance);
	}

	public static void UpdatePlayerRole(GameObject instance)
	{
		foreach (Instance instance2 in instances)
		{
			if (!(instance != instance2.owner))
			{
				instance2.text.GetComponentInChildren<TextMeshProUGUI>().color = instance.GetComponent<ServerRoles>().GetColor();
				instance2.text.GetComponentInChildren<TextMeshProUGUI>().text = instance.GetComponent<NicknameSync>().myNick + " <size=12>" + instance.GetComponent<ServerRoles>().GetColoredRoleString() + "</size>";
			}
		}
	}

	public static void DestroyPlayer(GameObject instance)
	{
		foreach (Instance instance2 in instances)
		{
			if (instance2.owner != instance)
			{
				continue;
			}
			UnityEngine.Object.Destroy(instance2.text.gameObject);
			instances.Remove(instance2);
			break;
		}
	}
}
