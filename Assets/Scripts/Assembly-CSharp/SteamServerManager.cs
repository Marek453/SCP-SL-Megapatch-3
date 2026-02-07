using GameConsole;
using Steamworks;
using UnityEngine;

public class SteamServerManager : MonoBehaviour
{
	public static SteamServerManager _instance;

	private bool gs_Initialized;

	private Callback<SteamServersConnected_t> Callback_ServerConnected;

	private Console console;

	private void Start()
	{
		console = Console.singleton;
		_instance = this;
	}
}
