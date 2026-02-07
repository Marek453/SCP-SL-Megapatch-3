using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Cryptography;
using GameConsole;
using MEC;
using Org.BouncyCastle.Crypto;
using UnityEngine;
using UnityEngine.Networking;

public class ServerConsole : MonoBehaviour
{
	public static int logID;

	public static int cycle;

	public static int Port;

	public static Process consoleID;

	public static string session;

	public static string password;

	public static string ip;

	public static AsymmetricKeyParameter Publickey;

	private static bool accepted = true;

	public static bool update = false;

	private static List<string> prompterQueue = new List<string>();

	private bool error_sent;

	public static ServerConsole singleton;

	private static IEnumerator<float> _CheckLog()
	{
		yield return Timing.WaitForSeconds(10f);
		while (true)
		{
			string[] tasks = Directory.GetFiles("SCPSL_Data/Dedicated/" + session, "cs*.mapi", SearchOption.TopDirectoryOnly);
			string[] array = tasks;
			foreach (string task in array)
			{
				string t = task.Remove(0, task.IndexOf("cs"));
				string toLog = string.Empty;
				string exception8 = string.Empty;
				try
				{
					exception8 = "Error while reading the file: " + t;
					StreamReader streamReader = new StreamReader("SCPSL_Data/Dedicated/" + session + "/" + t);
					string text = streamReader.ReadToEnd();
					exception8 = "Error while dedecting 'terminator end-of-message' signal.";
					if (text.Contains("terminator"))
					{
						text = text.Remove(text.LastIndexOf("terminator"));
					}
					exception8 = "Error while sending message.";
					toLog = EnterCommand(text);
					try
					{
						exception8 = "Error while closing the file: " + t + " :: " + text;
					}
					catch
					{
						exception8 = "Error while closing the file.";
					}
					streamReader.Close();
					try
					{
						exception8 = "Error while deleting the file: " + t + " :: " + text;
					}
					catch
					{
						exception8 = "Error while deleting the file.";
					}
					File.Delete("SCPSL_Data/Dedicated/" + session + "/" + t);
				}
				catch
				{
				}
				if (!string.IsNullOrEmpty(toLog))
				{
					AddLog(toLog);
				}
				yield return Timing.WaitForSeconds(0.07f);
			}
			yield return Timing.WaitForSeconds(1f);
			if (consoleID == null || consoleID.HasExited)
			{
				TerminateProcess();
			}
		}
	}

	public static void AddLog(string q)
	{
		if (ServerStatic.isDedicated)
		{
			prompterQueue.Add(q);
		}
		else
		{
			GameConsole.Console.singleton.AddLog(q, Color.grey);
		}
	}

	public static string GetClientInfo(NetworkConnection conn)
	{
		GameObject gameObject = GameConsole.Console.FindConnectedRoot(conn);
		return gameObject.GetComponent<NicknameSync>().myNick + " ( " + gameObject.GetComponent<CharacterClassManager>().SteamId + " | " + conn.address + " )";
	}

	public static string GetClientInfo(GameObject gameObject)
	{
		return gameObject.GetComponent<NicknameSync>().myNick + " ( " + gameObject.GetComponent<CharacterClassManager>().SteamId + " | " + gameObject.GetComponent<NetworkBehaviour>().connectionToClient.address + " )";
	}

	public static void Disconnect(GameObject player, string message)
	{
		if (player == null)
		{
			return;
		}
		NetworkBehaviour component = player.GetComponent<NetworkBehaviour>();
		if (!(component == null) && component.connectionToClient.isConnected)
		{
			CharacterClassManager component2 = player.GetComponent<CharacterClassManager>();
			if (component2 == null)
			{
				component.connectionToClient.Disconnect();
				component.connectionToClient.Dispose();
			}
			else
			{
				component2.DisconnectClient(component.connectionToClient, message);
			}
		}
	}

	public static void Disconnect(NetworkConnection conn, string message)
	{
		GameObject gameObject = GameConsole.Console.FindConnectedRoot(conn);
		if (gameObject == null)
		{
			conn.Disconnect();
			conn.Dispose();
		}
		else
		{
			Disconnect(gameObject, message);
		}
	}

	public IEnumerator<float> _Prompt()
	{
		while (true)
		{
			if (!accepted)
			{
				yield return 0f;
				continue;
			}
			if (prompterQueue.Count > 0)
			{
				string text = prompterQueue[0];
				prompterQueue.RemoveAt(0);
				if (!error_sent || !text.Contains("Could not update the session - Server is not verified."))
				{
					error_sent = true;
					StreamWriter streamWriter = new StreamWriter("SCPSL_Data/Dedicated/" + session + "/sl" + logID + ".mapi");
					logID++;
					streamWriter.WriteLine(text);
					streamWriter.Close();
				}
			}
			yield return 0f;
		}
	}

	private static void ColorText(string text)
	{
		UnityEngine.Debug.Log(string.Format("<color={0}>{1}</color>", GetColor(text), text), null);
	}

	private static string GetColor(string text)
	{
		int num = 9;
		if (text.Contains("LOGTYPE"))
		{
			try
			{
				string text2 = text.Remove(0, text.IndexOf("LOGTYPE") + 7);
				num = int.Parse((!text2.Contains("-")) ? text2 : text2.Remove(0, 1));
				text = text.Remove(text.IndexOf("LOGTYPE") + 9);
			}
			catch
			{
				num = 9;
			}
		}
		string result = string.Empty;
		switch (num)
		{
		case 0:
			result = "#000";
			break;
		case 1:
			result = "#183487";
			break;
		case 2:
			result = "#0b7011";
			break;
		case 3:
			result = "#0a706c";
			break;
		case 4:
			result = "#700a0a";
			break;
		case 5:
			result = "#5b0a40";
			break;
		case 6:
			result = "#aaa800";
			break;
		case 7:
			result = "#afafaf";
			break;
		case 8:
			result = "#5b5b5b";
			break;
		case 9:
			result = "#0055ff";
			break;
		case 10:
			result = "#10ce1a";
			break;
		case 11:
			result = "#0fc7ce";
			break;
		case 12:
			result = "#ce0e0e";
			break;
		case 13:
			result = "#c70dce";
			break;
		case 14:
			result = "#ffff07";
			break;
		case 15:
			result = "#e0e0e0";
			break;
		}
		return result;
	}

	private static string EnterCommand(string cmds)
	{
		string result = "Command accepted.";
		string[] array = cmds.ToUpper().Split(' ');
		if (array.Length > 0)
		{
			string text = array[0];
			if (text == "FORCESTART")
			{
				bool flag = false;
				GameObject gameObject = GameObject.Find("Host");
				if (gameObject != null)
				{
					CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
					if (component != null && component.isLocalPlayer && component.isServer && !component.roundStarted)
					{
						component.ForceRoundStart();
						flag = true;
					}
				}
				result = ((!flag) ? "Failed to force start.LOGTYPE14" : "Forced round start.");
			}
			else if (text == "CONFIG")
			{
				if (File.Exists(ConfigFile.ConfigPath))
				{
					Application.OpenURL(ConfigFile.ConfigPath);
				}
				else
				{
					result = "Config file not found!";
				}
			}
			else
			{
				result = GameConsole.Console.singleton.TypeCommand(cmds);
			}
		}
		return result;
	}

	private void Awake()
	{
		singleton = this;
	}

	private void Start()
	{
		if (ServerStatic.isDedicated)
		{
			logID = 0;
			accepted = true;
			if (Directory.Exists("SCPSL_Data / Dedicated / " + session))
			{
				Directory.Delete("SCPSL_Data / Dedicated / " + session, true);
			}
			Timing.RunCoroutine(_Prompt());
			Timing.RunCoroutine(_CheckLog());
		}
	}

	public void RunServer()
	{
		Timing.RunCoroutine(_RefreshSession());
	}

	public void RunRefreshPublicKey()
	{
		Timing.RunCoroutine(_RefreshPublicKey());
	}

	private IEnumerator<float> _RefreshPublicKey()
	{
		AddLog("Downloading public key from central server...");
		while (true)
		{
			WWW www = new WWW(form: new WWWForm(), url: CentralServer.URL + "publickey.php");
			yield return Timing.WaitUntilDone(www);
			try
			{
				Publickey = ECDSA.PublicKeyFromString(www.text);
			}
			catch (Exception ex)
			{
				AddLog("Can't refresh central server public key - " + ex.Message);
			}
			yield return Timing.WaitForSeconds(360f);
		}
	}

	private IEnumerator<float> _RefreshSession()
	{
		CustomNetworkManager cnm = GetComponent<CustomNetworkManager>();
		string scpMainServer = CentralServer.URL + "authenticator.php";
		string backwardsCompatibleServer = ConfigFile.ServerConfig.GetString("master_server_to_contact", string.Empty);
		string[] addressArr = ConfigFile.ServerConfig.GetStringList("secondary_servers_to_contact").ToArray();
		List<string> serverAddresses = new List<string> { scpMainServer };
		if (!string.IsNullOrEmpty(backwardsCompatibleServer))
		{
			serverAddresses.Add(backwardsCompatibleServer);
		}
		string[] array = addressArr;
		foreach (string text in array)
		{
			if (!string.IsNullOrEmpty(text) && !serverAddresses.Contains(text))
			{
				serverAddresses.Add(text);
			}
		}
		while (true)
		{
			float timeBefore = Time.realtimeSinceStartup;
			cycle++;
			if (string.IsNullOrEmpty(password) && cycle < 15)
			{
				if (cycle == 5 || cycle == 12)
				{
					RefreshToken();
				}
			}
			else
			{
				foreach (string serverAddress in serverAddresses)
				{
					WWWForm form = new WWWForm();
					form.AddField("ip", ip);
					if (!string.IsNullOrEmpty(password))
					{
						form.AddField("passcode", (!serverAddress.Equals(scpMainServer)) ? string.Empty : password);
					}
					int plys = 0;
					try
					{
						plys = GameObject.FindGameObjectsWithTag("Player").Length - 1;
					}
					catch
					{
					}
					form.AddField("players", plys + "/" + cnm.maxConnections);
					form.AddField("port", cnm.networkPort);
					form.AddField("version", 2);
					if (update || cycle == 10)
					{
						update = false;
						string value = ConfigFile.ServerConfig.GetString("server_name", "Unnamed server") + ":[:BREAK:]:" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT") + ":[:BREAK:]:" + UnityEngine.Object.FindObjectOfType<CustomNetworkManager>().CompatibleVersions[0];
						form.AddField("update", 1);
						form.AddField("info", value);
					}
					WWW www = new WWW(serverAddress, form);
					yield return Timing.WaitUntilDone(www);
					if (string.IsNullOrEmpty(www.error) && !(www.text != "YES"))
					{
						continue;
					}
					if (!string.IsNullOrEmpty(www.error))
					{
						AddLog("Could not update data on server list - " + www.error + www.text + "LOGTYPE-8");
						continue;
					}
					if (www.text.StartsWith("New code generated:"))
					{
						ServerStatic.PermissionsHandler.SetServerAsVerified();
						string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
						try
						{
							File.Delete(path);
						}
						catch
						{
							AddLog("New password could not be saved.LOGTYPE-8");
						}
						try
						{
							StreamWriter streamWriter = new StreamWriter(path);
							string text2 = www.text.Remove(0, www.text.IndexOf(":")).Remove(www.text.IndexOf(":"));
							while (text2.Contains(":"))
							{
								text2 = text2.Replace(":", string.Empty);
							}
							streamWriter.WriteLine(text2);
							streamWriter.Close();
							AddLog("New password saved.LOGTYPE-8");
							update = true;
						}
						catch
						{
							AddLog("New password could not be saved.LOGTYPE-8");
						}
					}
					else if (www.text.Contains(":Restart:"))
					{
						AddLog("Server restart requested by central server.LOGTYPE-8");
						Application.Quit();
					}
					else if (www.text.Contains(":RoundRestart:"))
					{
						AddLog("Round restart requested by central server.LOGTYPE-8");
						GameObject[] array2 = GameObject.FindGameObjectsWithTag("Player");
						foreach (GameObject gameObject in array2)
						{
							PlayerStats component = gameObject.GetComponent<PlayerStats>();
							if (component.isLocalPlayer && component.isServer)
							{
								component.Roundrestart();
							}
						}
					}
					else if (www.text.Contains(":UpdateData:"))
					{
						update = true;
					}
					else if (www.text.Contains(":Message - "))
					{
						string text3 = www.text.Substring(www.text.IndexOf(":Message - ", StringComparison.Ordinal) + 11);
						text3 = text3.Substring(0, text3.IndexOf(":::", StringComparison.Ordinal));
						AddLog("[MESSAGE FROM CENTRAL SERVER] " + text3 + " LOGTYPE-3");
					}
					else if (!www.text.Contains("Server is not verified"))
					{
						AddLog("Could not update data on server list - " + www.error + www.text + "LOGTYPE-8");
					}
					RefreshToken();
				}
			}
			if (cycle >= 15)
			{
				cycle = 0;
			}
			yield return Timing.WaitForSeconds(5f - (Time.realtimeSinceStartup - timeBefore));
		}
	}

	public void RefreshToken()
	{
		string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		if (File.Exists(path))
		{
			StreamReader streamReader = new StreamReader(path);
			string text = streamReader.ReadToEnd();
			if (string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(text))
			{
				AddLog("Verification token loaded! Server probably will be listed on public list.");
			}
			if (password != text)
			{
				AddLog("Verification token reloaded.");
				update = true;
			}
			password = text;
			ServerStatic.PermissionsHandler.SetServerAsVerified();
			streamReader.Close();
		}
	}

	private static void TerminateProcess()
	{
		ServerStatic.isDedicated = false;
		Application.Quit();
	}
}
