using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerStatic : MonoBehaviour
{
	public static bool isDedicated;

	public bool simulate;

	private bool processStarted;

	internal static YamlConfig RolesConfig;

	internal static string RolesConfigPath;

	internal static PermissionsHandler PermissionsHandler;

	private void Awake()
	{
		processStarted = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		string[] array = commandLineArgs;
		foreach (string text in array)
		{
			if (text == "-nographics" && !simulate)
			{
				simulate = true;
			}
			if (text.Contains("-key"))
			{
				ServerConsole.session = text.Remove(0, 4);
			}
			if (!text.Contains("-id"))
			{
				continue;
			}
			Process[] processes = Process.GetProcesses();
			foreach (Process process in processes)
			{
				if (process.Id.ToString() == text.Remove(0, 3))
				{
					ServerConsole.consoleID = process;
				}
			}
		}
		if (simulate)
		{
			isDedicated = true;
			AudioListener.volume = 0f;
			ServerConsole.AddLog("SCP Secret Laboratory process started. Creating match... LOGTYPE02");
		}
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		if (isDedicated && (scene.buildIndex == 1 || scene.buildIndex == 3))
		{
			GetComponent<CustomNetworkManager>().CreateMatch();
		}
	}

	public static PermissionsHandler GetPermissionsHandler()
	{
		return PermissionsHandler;
	}
}
