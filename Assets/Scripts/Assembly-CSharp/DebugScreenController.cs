using System.Diagnostics;
using UnityEngine;

public class DebugScreenController : MonoBehaviour
{
	public GameObject GUI;

	public static int asserts;

	public static int errors;

	public static int exceptions;

	private void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
		Application.logMessageReceived += LogMessage;
		if (Process.GetProcessesByName("SharpMonoInjector").Length > 0)
		{
			Application.Quit();
		}
	}

	private void LogMessage(string condition, string stackTrace, LogType type)
	{
		switch (type)
		{
		case LogType.Assert:
			asserts++;
			break;
		case LogType.Error:
			errors++;
			break;
		case LogType.Exception:
			exceptions++;
			break;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F3))
		{
			GUI.SetActive(!GUI.activeSelf);
		}
	}
}
