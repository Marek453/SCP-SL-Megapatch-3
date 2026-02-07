using System;
using System.Collections.Generic;
using System.IO;
using Dissonance.Integrations.UNet_HLAPI;
using System.Collections;
using GameConsole;
using MEC;
using Mono.Nat;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomNetworkManager : NetworkManager
{
	[Serializable]
	public class DisconnectLog
	{
		[Serializable]
		public class LogButton
		{
			public ConnInfoButton[] actions;
		}

		[Multiline]
		public string msg_en;

		public LogButton button;

		public bool autoHideOnSceneLoad;
	}

	public GameObject popup;

	public GameObject createpop;

	public RectTransform contSize;

	public Text content;

	private bool _queryEnabled;

	private int _queryPort;

	public DisconnectLog[] logs;

	public int _curLogId;

	public bool reconnect;

	public static string Ip = string.Empty;

	public string disconnectMessage = string.Empty;

	public static string ConnectionIp;

	private static QueryServer _queryserver;

	private List<INatDevice> _mappedDevices;

	[Space(20f)]
	public string[] CompatibleVersions;

	private bool activated;

	private GameConsole.Console console;

	private void Update()
	{
		if (popup.activeSelf && Input.GetKey(KeyCode.Escape))
		{
			ClickButton();
		}
	}

	public override void OnClientDisconnect(NetworkConnection conn)
	{
		ShowLog((int)conn.lastError);
	}

	public override void OnClientError(NetworkConnection conn, int errorCode)
	{
		ShowLog(errorCode);
	}

    public bool ShouldPlayIntensive()
    {
        if (_curLogId != 13)
        {
            return IsFacilityLoading();
        }
        return true;
    }

    public override void OnStartClient(NetworkClient client)
	{
		base.OnStartClient(client);
		StartCoroutine(_ConnectToServer(client));
	}

	private IEnumerator _ConnectToServer(NetworkClient client)
	{
		while (_curLogId == 13)
		{
			if (client.isConnected)
			{
				ShowLog(17);
			}
			yield return 0f;
		}
	}

	public override void OnServerConnect(NetworkConnection conn)
	{
		base.OnServerConnect(conn);
		if (BanHandler.QueryBan(null, conn.address).Value != null)
		{
			ServerConsole.AddLog("Player tried to connect from banned IP address " + conn.address + ".");
			ServerConsole.Disconnect(conn, "You are banned from this server.");
		}
		else
		{
			ServerConsole.AddLog("Player joined from IP address " + conn.address + ".");
		}
	}

	public override void OnServerDisconnect(NetworkConnection conn)
	{
		base.OnServerDisconnect(conn);
		HlapiServer.OnServerDisconnect(conn);
	}

	public static void PlayerDisconnect(NetworkConnection conn)
	{
		HlapiServer.OnServerDisconnect(conn);
	}

	private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
	{
		if (!activated && scene.name.ToLower().Contains("menu"))
		{
			activated = true;
			base.networkAddress = "none";
			StartClient();
			base.networkAddress = "localhost";
			StopClient();
		}
		if (reconnect)
		{
			ShowLog(14);
			Invoke("Reconnect", 3f);
		}
	}

    public bool IsFacilityLoading()
    {
        return _curLogId == 17;
    }

    public override void OnClientSceneChanged(NetworkConnection conn)
	{
		base.OnClientSceneChanged(conn);
		if (!reconnect && logs[_curLogId].autoHideOnSceneLoad)
		{
			popup.SetActive(false);
		}
	}

	private void Reconnect()
	{
		if (reconnect)
		{
			reconnect = false;
			StartClient();
		}
	}

	public void StopReconnecting()
	{
		reconnect = false;
	}

	public void ShowLog(int id)
	{
		_curLogId = id;
		popup.SetActive(true);
		content.text = TranslationReader.Get("Connection_Errors", id);
		if (!string.IsNullOrEmpty(disconnectMessage))
		{
			string[] array = content.text.Split(new string[1] { Environment.NewLine }, StringSplitOptions.None);
			if (array.Length > 0)
			{
				content.text = array[0] + Environment.NewLine + disconnectMessage;
			}
			disconnectMessage = string.Empty;
		}
		content.rectTransform.sizeDelta = Vector3.zero;
	}

	public void ClickButton()
	{
		ConnInfoButton[] actions = logs[_curLogId].button.actions;
		foreach (ConnInfoButton connInfoButton in actions)
		{
			connInfoButton.UseButton();
		}
	}

	public override void OnClientConnect(NetworkConnection conn)
	{
		base.OnClientConnect(conn);
	}

	private void Start()
	{
		if (File.Exists("hoster_policy.txt"))
		{
			ConfigFile.HosterPolicy = new YamlConfig("hoster_policy.txt");
		}
		else if (File.Exists(FileManager.AppFolder + "hoster_policy.txt"))
		{
			ConfigFile.HosterPolicy = new YamlConfig(FileManager.AppFolder + "hoster_policy.txt");
		}
		else
		{
			ConfigFile.HosterPolicy = new YamlConfig();
		}
		if (ServerStatic.isDedicated)
		{
			return;
		}
		ServerConsole.AddLog("Loading config...");
		ConfigFile.ServerConfig = ConfigFile.ReloadGameConfig(FileManager.AppFolder + "config_gameplay.txt");
		ServerConsole.AddLog("Config file loaded!");
		console = GameConsole.Console.singleton;
		if (!SteamAPI.Init())
		{
			console.AddLog("Failed to init SteamAPI.", new Color32(128, 128, 128, byte.MaxValue));
		}
		else
		{
			if (Directory.Exists("SCPSL_Data\\Managed"))
			{
				if (!File.Exists("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					CreateVersionFile(false);
				}
				else
				{
					string[] array = FileManager.ReadAllLines("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
					if (array.Length < 1 || !ServerRoles.Base64Decode(array[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Contains(";"))
					{
						CreateVersionFile(false);
					}
					else
					{
						string[] array2 = ServerRoles.Base64Decode(array[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Split(';');
						if (array2.Length != 3 || array2[0] != CompatibleVersions[0])
						{
							CreateVersionFile(false);
						}
						else if (array2[2] != SteamUser.GetSteamID().ToString())
						{
							try
							{
								string plainText = array2[0] + ";" + array2[1] + ";" + SteamUser.GetSteamID();
								File.Delete("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
								File.Create("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
								FileManager.AppendFile("UI Build GUID: " + GUIDSplit(ServerRoles.Base64Encode(plainText)), "SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
							}
							catch (Exception)
							{
								GameConsole.Console.singleton.AddLog("IO startup error 2.1", Color.red);
							}
						}
					}
				}
			}
			if (Directory.Exists("PrivateBeta") && Directory.Exists("PrivateBeta\\SCPSL_Data\\Managed"))
			{
				if (!File.Exists("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					CreateVersionFile(true);
				}
				else
				{
					string[] array3 = FileManager.ReadAllLines("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
					if (array3.Length < 1 || !ServerRoles.Base64Decode(array3[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Contains(";"))
					{
						CreateVersionFile(true);
					}
					else
					{
						string[] array4 = ServerRoles.Base64Decode(array3[0].Replace("UI Build GUID: ", string.Empty).Replace("-", string.Empty)).Split(';');
						if (array4.Length != 3 || array4[0] != CompatibleVersions[0])
						{
							CreateVersionFile(true);
						}
						else if (array4[2] != SteamUser.GetSteamID().ToString())
						{
							try
							{
								string plainText2 = array4[0] + ";" + array4[1] + ";" + SteamUser.GetSteamID();
								File.Delete("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
								File.Create("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
								FileManager.AppendFile("UI Build GUID: " + GUIDSplit(ServerRoles.Base64Encode(plainText2)), "PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
							}
							catch (Exception)
							{
								GameConsole.Console.singleton.AddLog("IO startup error 2.2", Color.red);
							}
						}
					}
				}
			}
		}
		SceneManager.sceneLoaded += OnLevelFinishedLoading;
		base.connectionConfig.MaxSentMessageQueueSize = 300;
	}

	private string GUIDSplit(string GUID)
	{
		string text = string.Empty;
		while (GUID.Length > 5)
		{
			text += GUID.Substring(0, 5);
			text += "-";
			GUID = GUID.Substring(5);
		}
		return text + GUID;
	}

	private void CreateVersionFile(bool privbeta)
	{
		if (!privbeta)
		{
			try
			{
				if (File.Exists("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
				{
					File.Delete("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
				}
				File.Create("SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
				FileManager.AppendFile("UI Build GUID: " + GUIDSplit(ServerRoles.Base64Encode(string.Concat(CompatibleVersions[0], ";", SteamUser.GetSteamID(), ";-"))), "SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
				return;
			}
			catch
			{
				GameConsole.Console.singleton.AddLog("IO startup error 1.1", Color.red);
				return;
			}
		}
		if (!Directory.Exists("PrivateBeta"))
		{
			return;
		}
		try
		{
			if (File.Exists("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml"))
			{
				File.Delete("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
			}
			File.Create("PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml").Close();
			FileManager.AppendFile("UI Build GUID: " + GUIDSplit(ServerRoles.Base64Encode(string.Concat(CompatibleVersions[0], ";", SteamUser.GetSteamID(), ";-"))), "PrivateBeta\\SCPSL_Data\\Managed\\UnityEngine.UIVersion.xml");
		}
		catch
		{
			GameConsole.Console.singleton.AddLog("IO startup error 1.2", Color.red);
		}
	}

	public void CreateMatch()
	{
		ShowLog(13);
		createpop.SetActive(false);
		NetworkServer.Reset();
		base.networkPort = GetFreePort();
		base.maxConnections = ConfigFile.ServerConfig.GetInt("max_players", 20);
		ServerConsole.Port = base.networkPort;
		ServerConsole.AddLog("Config file loaded: " + ConfigFile.ConfigPath);
		_queryEnabled = ConfigFile.ServerConfig.GetBool("enable_query", true);
		if (ConfigFile.ServerConfig.GetBool("forward_ports", true))
		{
			UpnpStart();
		}
		string text = FileManager.AppFolder + "config_remoteadmin.txt";
		if (!File.Exists(text))
		{
			File.Copy("MiscData/remoteadmin_template.txt", text);
		}
		ServerStatic.RolesConfigPath = text;
		ServerStatic.RolesConfig = new YamlConfig(text);
		ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig);
		StartCoroutine(_CreateLobby());
		if (!ServerStatic.isDedicated)
		{
			NonsteamHost();
		}
	}

	private IEnumerator _CreateLobby()
	{
		yield return 0f;
		ServerConsole.AddLog((!ConfigFile.ServerConfig.GetBool("online_mode", true)) ? "Online mode is DISABLED - SERVER CANNOT VALIDATE STEAM ID OF CONNECTING PLAYERS!!!" : "Online mode is ENABLED.");
		UnityEngine.Object.FindObjectOfType<ServerConsole>().RunRefreshPublicKey();
		if (_queryEnabled)
		{
			_queryPort = base.networkPort + ConfigFile.ServerConfig.GetInt("query_port_shift");
			ServerConsole.AddLog("Query port will be enabled on port " + _queryPort + " TCP.");
			_queryserver = new QueryServer(_queryPort, ConfigFile.ServerConfig.GetBool("query_use_IPv6", true));
			_queryserver.StartServer();
		}
		else
		{
			ServerConsole.AddLog("Query port disabled in config!");
		}
		ServerConsole.AddLog("Starting server...");
		if (ConfigFile.HosterPolicy.GetString("server_ip", "none") != "none")
		{
			Ip = ConfigFile.HosterPolicy.GetString("server_ip", "none");
			ServerConsole.AddLog("Server IP set to " + Ip + " by your hosting provider.");
		}
		else if (ConfigFile.ServerConfig.GetBool("online_mode", true))
		{
			if (ConfigFile.ServerConfig.GetString("server_ip", "auto") != "auto")
			{
				Ip = ConfigFile.ServerConfig.GetString("server_ip", "auto");
				ServerConsole.AddLog("Custom config detected. Your game-server IP will be " + Ip);
			}
			else
			{
				ServerConsole.AddLog("Obtaining your external IP address...");
				WWW www = new WWW(CentralServer.URL + "ip.php");
				yield return new WaitUntil(() => www.isDone);
				if (!string.IsNullOrEmpty(www.error))
				{
					ServerConsole.AddLog("Error: connection to " + CentralServer.URL + " failed. Website returned: " + www.error + " | Aborting startup... LOGTYPE-8");
					yield break;
				}
				Ip = ((!www.text.EndsWith(".")) ? www.text : www.text.Remove(www.text.Length - 1));
				ServerConsole.AddLog("Done, your game-server IP will be " + Ip);
			}
		}
		else
		{
			Ip = "127.0.0.1";
		}
		ServerConsole.ip = Ip;
		ServerConsole.AddLog("Initializing game server...");
		if (!ServerStatic.isDedicated)
		{
			yield break;
		}
		if (ConfigFile.ServerConfig.GetString("bind_ip", "ANY").ToUpper() == "ANY")
		{
			ServerConsole.AddLog("Server starting at all IP addresses and port " + base.networkPort);
			base.serverBindToIP = false;
			StartHost();
		}
		else
		{
			ServerConsole.AddLog("Server starting at IP " + ConfigFile.ServerConfig.GetString("bind_ip", "ANY") + " and  port " + base.networkPort);
			base.serverBindAddress = ConfigFile.ServerConfig.GetString("bind_ip", "ANY");
			base.serverBindToIP = true;
			StartHost();
		}
		while (SceneManager.GetActiveScene().name != "Facility")
		{
			yield return 0f;
		}
		if (!ConfigFile.ServerConfig.GetBool("online_mode", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to online_mode turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (!ConfigFile.ServerConfig.GetBool("use_vac", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to use_vac turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (!ConfigFile.ServerConfig.GetBool("global_bans_cheating", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to global_bans_cheating turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		if (!ConfigFile.ServerConfig.GetBool("global_bans_griefing", true))
		{
			ServerConsole.AddLog("Server WON'T be visible on the public list due to global_bans_griefing turned off in server configuration.LOGTYPE-8");
			yield break;
		}
		ServerConsole.AddLog("Level loaded. Creating match...");
		string info = ConfigFile.ServerConfig.GetString("server_name", "Unnamed server") + ":[:BREAK:]:" + ConfigFile.ServerConfig.GetString("serverinfo_pastebin_id", "7wV681fT") + ":[:BREAK:]:" + CompatibleVersions[0];
		WWWForm form = new WWWForm();
		form.AddField("update", 1);
		form.AddField("ip", Ip);
		form.AddField("info", info);
		form.AddField("port", base.networkPort);
		form.AddField("players", 0);
		string pth = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/verkey.txt";
		string scpMainServer = CentralServer.URL + "authenticator.php";
		string backwardsCompatibleServer = ConfigFile.ServerConfig.GetString("master_server_to_contact", string.Empty);
		List<string> addressArr = ConfigFile.ServerConfig.GetStringList("secondary_servers_to_contact");
		List<string> serverAddresses = new List<string> { scpMainServer };
		if (!string.IsNullOrEmpty(backwardsCompatibleServer))
		{
			serverAddresses.Add(backwardsCompatibleServer);
		}
		foreach (string item in addressArr)
		{
			if (!string.IsNullOrEmpty(item) && !serverAddresses.Contains(item))
			{
				serverAddresses.Add(item);
			}
		}
		foreach (string serverAddress in serverAddresses)
		{
			if (File.Exists(pth) && serverAddress.Equals(scpMainServer))
			{
				StreamReader streamReader = new StreamReader(pth);
				string text = streamReader.ReadToEnd();
				form.AddField("passcode", text);
				form.AddField("version", 2);
				ServerConsole.password = text;
				streamReader.Close();
			}
			else
			{
				form.AddField("passcode", string.Empty);
			}
			WWW www2 = new WWW(CentralServer.URL + "authenticator.php", form);
			yield return new WaitUntil(() => www2.isDone);
			if (string.IsNullOrEmpty(www2.error))
			{
				if (www2.text.Contains("YES"))
				{
					if (www2.text.StartsWith("New code generated:"))
					{
						try
						{
							StreamWriter streamWriter = new StreamWriter(pth);
							string text2 = www2.text.Remove(0, www2.text.IndexOf(":")).Remove(www2.text.IndexOf(":"));
							while (text2.Contains(":"))
							{
								text2 = text2.Replace(":", string.Empty);
							}
							streamWriter.WriteLine(text2);
							streamWriter.Close();
							ServerConsole.AddLog("New password saved.LOGTYPE-8");
							UnityEngine.Object.FindObjectOfType<ServerConsole>().RefreshToken();
						}
						catch
						{
							ServerConsole.AddLog("New password could not be saved.LOGTYPE-8");
						}
					}
					ServerConsole.AddLog("The match is now on public list!LOGTYPE-8");
					ServerStatic.PermissionsHandler.SetServerAsVerified();
				}
				else
				{
					ServerConsole.AddLog("Your server won't be visible on the public server list - " + www2.text + " (" + Ip + ")LOGTYPE-8");
					if (string.IsNullOrEmpty(ConfigFile.ServerConfig.GetString("contact_email", string.Empty)))
					{
						ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC, please set up your email in configuration file (\"contact_email\" value) and restart the server. LOGTYPE-8");
						continue;
					}
					ServerConsole.AddLog("If you are 100% sure that the server is working, can be accessed from the Internet and YOU WANT TO MAKE IT PUBLIC please email following information: LOGTYPE-8");
					ServerConsole.AddLog("- IP address of server (probably " + Ip + ") LOGTYPE-8");
					ServerConsole.AddLog("- is this static or dynamic IP address (most of home adresses are dynamic) LOGTYPE-8");
					ServerConsole.AddLog("PLEASE READ rules for verified servers first: https://scpslgame.com/Verified_server_rules.pdf LOGTYPE-8");
					ServerConsole.AddLog("send us that information to: server.verification@scpslgame.com LOGTYPE-8");
					ServerConsole.AddLog("email must be sent from email address set as \"contact_email\" in your config file (current value: " + ConfigFile.ServerConfig.GetString("contact_email", string.Empty) + "). LOGTYPE-8");
				}
			}
			else
			{
				ServerConsole.AddLog("Could not create the match - " + www2.error + "LOGTYPE-8");
			}
		}
		UnityEngine.Object.FindObjectOfType<ServerConsole>().RunServer();
	}

	private void NonsteamHost()
	{
		base.onlineScene = "Facility";
		base.maxConnections = 20;
		StartHostWithPort();
	}

	public void StartHostWithPort()
	{
		if (ConfigFile.ServerConfig.GetString("bind_ip", "ANY").ToUpper() == "ANY")
		{
			ServerConsole.AddLog("Server starting at all IP addresses and port " + base.networkPort);
			base.serverBindToIP = false;
			StartHost();
			return;
		}
		ServerConsole.AddLog("Server starting at IP " + ConfigFile.ServerConfig.GetString("bind_ip", "ANY") + " and  port " + base.networkPort);
		base.serverBindAddress = ConfigFile.ServerConfig.GetString("bind_ip", "ANY");
		base.serverBindToIP = true;
		StartHost();
	}

	public int GetFreePort()
	{
		ServerConsole.AddLog("Loading config...");
		ConfigFile.ServerConfig = ConfigFile.ReloadGameConfig(FileManager.AppFolder + "config_gameplay.txt");
		string q = string.Empty;
		try
		{
			q = "Failed to split ports.";
			int[] array = ConfigFile.ServerConfig.GetIntList("port_queue").ToArray();
			if (array.Length == 0)
			{
				array = new int[8] { 7777, 7778, 7779, 7780, 7781, 7782, 7783, 7784 };
			}
			string text = string.Join(", ", new List<int>(array).ConvertAll((int i) => i.ToString()).ToArray());
			if (array.Length == 0)
			{
				q = "Failed to detect ports.";
				throw new Exception();
			}
			ServerConsole.AddLog("Port queue loaded: " + text);
			int[] array2 = array;
			foreach (int num in array2)
			{
				ServerConsole.AddLog("Trying to init port: " + num + "...");
				if (NetworkServer.Listen(num))
				{
					NetworkServer.Reset();
					ServerConsole.AddLog("Done!LOGTYPE-10");
					return num;
				}
				ServerConsole.AddLog("...failed.LOGTYPE-6");
			}
		}
		catch
		{
			ServerConsole.AddLog(q);
		}
		return 7777;
	}

	private void UpnpStart()
	{
		if (_mappedDevices == null)
		{
			ServerConsole.AddLog("Automatic port forwarding using UPnP enabled!LOGTYPE-9");
			_mappedDevices = new List<INatDevice>();
		}
		NatUtility.DeviceFound += DeviceFound;
		NatUtility.DeviceLost += DeviceLost;
		NatUtility.StartDiscovery();
	}

	private void UpnpStop()
	{
		NatUtility.StopDiscovery();
		foreach (INatDevice mappedDevice in _mappedDevices)
		{
			try
			{
				mappedDevice.DeletePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
				if (_mappedDevices.Contains(mappedDevice))
				{
					_mappedDevices.Remove(mappedDevice);
				}
				ServerConsole.AddLog(string.Concat("Removed forwarding rule on port ", base.networkPort, " from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-10"));
			}
			catch
			{
				ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on port ", base.networkPort, " UDP from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-12"));
			}
			if (_queryEnabled)
			{
				try
				{
					mappedDevice.DeletePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
					ServerConsole.AddLog(string.Concat("Removed forwarding rule on query port ", _queryPort, " from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-10"));
				}
				catch
				{
					ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on query port ", _queryPort, " UDP from ", mappedDevice.GetExternalIP(), " to this device.LOGTYPE-12"));
				}
			}
		}
	}

	private void DeviceFound(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		try
		{
			device = args.Device;
			_mappedDevices.Add(device);
			device.CreatePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
			ServerConsole.AddLog(string.Concat("Forwarded port ", base.networkPort, " UDP (game port) from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog(string.Concat("Can't forward port ", base.networkPort, " UDP from ", device.GetExternalIP(), " to this device. Error: ", ex.Message, "LOGTYPE-12"));
		}
		if (!_queryEnabled)
		{
			return;
		}
		try
		{
			if (_queryEnabled)
			{
				device.CreatePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
				ServerConsole.AddLog(string.Concat("Forwarded port ", _queryPort, " TCP (query port) from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
			}
		}
		catch (Exception ex2)
		{
			ServerConsole.AddLog(string.Concat("Can't forward query port ", _queryPort, " TCP from ", device.GetExternalIP(), " to this device. Error: ", ex2.Message, "LOGTYPE-12"));
		}
	}

	private void DeviceLost(object sender, DeviceEventArgs args)
	{
		INatDevice device = args.Device;
		try
		{
			device.DeletePortMap(new Mapping(Protocol.Udp, base.networkPort, base.networkPort));
			if (_mappedDevices.Contains(device))
			{
				_mappedDevices.Remove(device);
			}
			ServerConsole.AddLog(string.Concat("Removed forwarding rule on port ", base.networkPort, " from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch
		{
			ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on port ", base.networkPort, " UDP from ", device.GetExternalIP(), " to this device.LOGTYPE-12"));
		}
		if (!_queryEnabled)
		{
			return;
		}
		try
		{
			device.DeletePortMap(new Mapping(Protocol.Tcp, _queryPort, _queryPort));
			ServerConsole.AddLog(string.Concat("Removed forwarding rule on query port ", _queryPort, " from ", device.GetExternalIP(), " to this device.LOGTYPE-10"));
		}
		catch
		{
			ServerConsole.AddLog(string.Concat("Can't remove forwarding rule on query port ", _queryPort, " UDP from ", device.GetExternalIP(), " to this device.LOGTYPE-12"));
		}
	}
}
