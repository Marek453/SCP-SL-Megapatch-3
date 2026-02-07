using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Scp294 : NetworkBehaviour
{
	public Cup[] AnableCups;
	[SyncVar]
	public bool isUsed;

	[SyncVar(hook = "addText")]
	public string curCup;

	public Text text;

	public GameObject player;

	public AudioSource ButtonSource;

	void Update()
	{
		text.text = curCup;
		foreach(var ply in PlayerManager.singleton.players)
		{
			if(ply.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
			player = ply;
			}
		}
	}
	public void OnUse(string Name)
	{
		string cup = Name.ToUpper();
		print(cup);
		bool isUspeh = false;
		addText("DISPENSING...");
		int ID = -1;
		foreach(var up in AnableCups)
		{
			string SelectCup = up.Cupname.ToUpper();
			if(cup == SelectCup)
			{
				ID = up.ID;
				isUspeh = true;
				break;
			}
			else
			{
				isUspeh = false;
			}
		}

		StartCoroutine(Log(isUspeh,ID));
	}

	IEnumerator Log(bool isProssid, int ID)
	{
		yield return new WaitForSeconds(2);
		if(isProssid)
		{
			Inventory inventory = player.GetComponent<Inventory>();
		addText("DONE");
		inventory.AddNewItem(ID);
		player.GetComponent<Scp294PlayerScript>().ToggleMenu();
		yield return new WaitForSeconds(1);
			addText("");
		}
		else
		{
			addText("OUT OF RANGE");
			yield return new WaitForSeconds(1);
			addText("");
		}
	}

	public void addText(string name)
	{
		curCup = name;
	}
	public void Button(string button)
	{
		ButtonSource.Play();
		addText(curCup += button);
	}

	public void DeletButton()
	{
		ButtonSource.Play();
		addText(curCup = "");
	}

	public void EnterButton()
	{
		ButtonSource.Play();
		OnUse(curCup);
	}
}
