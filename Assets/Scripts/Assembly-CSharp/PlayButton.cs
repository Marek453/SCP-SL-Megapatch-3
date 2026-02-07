using GameConsole;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayButton : MonoBehaviour
{
	public string Ip;

	public string Port;

	public string InfoType;

	public Text Motd;

	public Text Players;

	public static int maxPlayers = 20;

	private void Start()
	{
		maxPlayers = 20;
	}

	private void SetMaxPlayers(string s)
	{
		try
		{
			s = s.Split('/')[1];
			maxPlayers = int.Parse(s);
		}
		catch
		{
			maxPlayers = 20;
		}
	}

	public void Click()
	{
		if (!CrashDetector.Show())
		{
			CustomNetworkManager customNetworkManager = Object.FindObjectOfType<CustomNetworkManager>();
			if (NetworkClient.active)
			{
				customNetworkManager.StopClient();
			}
			NetworkServer.Reset();
			customNetworkManager.ShowLog(13);
			customNetworkManager.networkAddress = Ip;
			CustomNetworkManager.ConnectionIp = Ip;
			try
			{
				customNetworkManager.networkPort = int.Parse(Port);
			}
			catch
			{
				Console.singleton.AddLog("Wrong server port, parsing to 7777!", new Color32(182, 182, 182, byte.MaxValue));
				customNetworkManager.networkPort = 7777;
			}
			Console.singleton.AddLog("Connecting to " + Ip + ":" + Port + "!", new Color32(182, 182, 182, byte.MaxValue));
			customNetworkManager.StartClient();
			SetMaxPlayers(Players.text);
		}
	}

	public void ShowInfo()
	{
		ServerInfo.ShowInfo(InfoType);
	}
}
