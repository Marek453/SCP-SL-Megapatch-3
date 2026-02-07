using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DiscordManager : MonoBehaviour
{
	private CharacterClassManager ccm;

	private DiscordController discordController;

	private CustomNetworkManager nm;

	public static DiscordManager singleton;

	public DiscordRpc.RichPresence menuPreset;

	public DiscordRpc.RichPresence waitingPreset;

	public DiscordRpc.RichPresence[] classPresets;

	private void Start()
	{
		singleton = this;
		nm = GetComponent<CustomNetworkManager>();
		ccm = Resources.FindObjectsOfTypeAll<CharacterClassManager>()[0];
		discordController = GetComponent<DiscordController>();
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
	}

	public void ChangePreset(int classID)
	{
		if (classID < 0)
		{
			discordController.presence = ((classID != -1) ? menuPreset : waitingPreset);
		}
		else
		{
			try
			{
				discordController.presence.state = classPresets[classID].state;
				discordController.presence.largeImageKey = classPresets[classID].largeImageKey;
				discordController.presence.smallImageKey = classPresets[classID].smallImageKey;
			}
			catch
			{
			}
		}
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		discordController.presence.startTimestamp = startTimestamp;
		string text = ((classID == -2 || !nm.networkAddress.Contains(".")) ? string.Empty : Convert.ToBase64String(Encoding.UTF8.GetBytes(nm.networkAddress + ":" + nm.networkPort + ":" + nm.CompatibleVersions[0])));
		discordController.presence.joinSecret = text;
		discordController.presence.partyId = "LOBBY#" + text;
		if (text == string.Empty)
		{
			discordController.presence.partySize = 0;
			discordController.presence.partyMax = 0;
		}
		DiscordRpc.UpdatePresence(ref discordController.presence);
	}

	public void ChangeLobbyStatus(int cur, int max)
	{
		discordController.presence.partySize = cur;
		discordController.presence.partyMax = max;
		DiscordRpc.UpdatePresence(ref discordController.presence);
	}

	public void PrintMessage(string msg)
	{
		Debug.Log(msg);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.LeftControl))
		{
			if (Input.GetKeyDown(KeyCode.Y))
			{
				discordController.RequestRespondYes();
			}
			if (Input.GetKeyDown(KeyCode.N))
			{
				discordController.RequestRespondNo();
			}
		}
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (scene.buildIndex == 1)
		{
			discordController.presence.partySize = 0;
			discordController.presence.partyMax = 0;
			ChangePreset(-2);
		}
		if (scene.name == "Facility")
		{
			ChangePreset(-1);
		}
	}
}
