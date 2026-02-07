using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMenu : MonoBehaviour
{
	public static string targetSceneName;

	private static bool server;

	private void Awake()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (text == "-fastmenu")
			{
				PlayerPrefs.SetInt("fastmenu", 1);
			}
			else if (text == "-nographics")
			{
				server = true;
			}
		}
		Refresh();
	}

	public void ChangeMode()
	{
		PlayerPrefs.SetInt("fastmenu", (PlayerPrefs.GetInt("fastmenu", 0) == 0) ? 1 : 0);
		Refresh();
		SceneManager.LoadScene("Loader");
	}

	private void Refresh()
	{
		if (server)
		{
			targetSceneName = "FastMenu";
		}
		else
		{
			targetSceneName = ((PlayerPrefs.GetInt("fastmenu", 0) != 0) ? "FastMenu" : "MainMenuRemastered");
		}
		UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().offlineScene = targetSceneName;
	}

	public static void LoadCorrectScene()
	{
		SceneManager.LoadScene(targetSceneName);
	}
}
