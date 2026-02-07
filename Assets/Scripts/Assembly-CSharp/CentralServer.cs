using System;
using System.IO;
using UnityEngine;

public class CentralServer : MonoBehaviour
{
	public static string URL
	{
		get
		{
			return (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/testserver.txt")) ? "https://api.scpslgame.com/" : "https://test.scpslgame.com/";
		}
	}
}
