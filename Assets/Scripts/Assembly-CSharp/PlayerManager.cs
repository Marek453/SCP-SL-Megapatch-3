using System.Collections.Generic;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerManager : MonoBehaviour
{
	public GameObject[] players;

	public static PlayerManager singleton;

	public static int playerID;

	public static GameObject localPlayer;

	public static SpectatorManager spect;

	private void Awake()
	{
		singleton = this;
	}
	
	public void AddPlayer(GameObject player)
	{
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = players;
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		if (!list.Contains(player))
		{
			list.Add(player);
		}
		players = list.ToArray();
		DiscordManager.singleton.ChangeLobbyStatus(players.Length, PlayButton.maxPlayers);
		PlayerList.AddPlayer(player);
		if (spect != null)
		{
			spect.RefreshList();
		}
		QueryProcessor.StaticRefreshPlayerList();
	}

	public void RemovePlayer(GameObject player)
	{
		PlayerList.DestroyPlayer(player);
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = players;
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		if (list.Contains(player))
		{
			list.Remove(player);
		}
		players = list.ToArray();
		DiscordManager.singleton.ChangeLobbyStatus(players.Length, PlayButton.maxPlayers);
		if (spect != null)
		{
			spect.RefreshList();
		}
		QueryProcessor.StaticRefreshPlayerList();
	}
	void Update()
	{
		foreach (GameObject obj in players)
		{
			if (obj.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				localPlayer = obj;
			}
		}
	}
}
