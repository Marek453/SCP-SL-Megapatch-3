using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GameConsole;
using MEC;
using RemoteAdmin;
using UnityEngine.Rendering.PostProcessing;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterClassManager : NetworkBehaviour
{
	[SyncVar(hook = "SetUnit")]
	public int ntfUnit;

	public float ciPercentage;

	public int forceClass = -1;

	[SerializeField]
	private AudioClip bell;

	[SerializeField]
	private AudioClip bell_dead;

	[HideInInspector]
	public GameObject myModel;

	[HideInInspector]
	public GameObject charCamera;

	public Class[] klasy;

	public List<Team> classTeamQueue = new List<Team>();

	private CentralAuthInterface _centralAuthInt;

	[SyncVar(hook = "SetClassID")]
	public int curClass;

	private int seed;

	private GameObject plyCam;

	public GameObject unfocusedCamera;

	[SyncVar(hook = "SyncDeathPos")]
	public Vector3 deathPosition;

	[SyncVar(hook = "SetRoundStart")]
	public bool roundStarted;

	public bool onlineMode;

	private bool _commandtokensent;

	internal string AuthToken;

	private Scp049PlayerScript scp049;

	private Scp457PlayerScript scp457;

	private Scp049_2PlayerScript scp049_2;

	private Scp079PlayerScript scp079;

	private Scp106PlayerScript scp106;

	private Scp173PlayerScript scp173;

	private Sco008PlayerScript scp008;

	private Scp096PlayerScript scp096;

	private Scp939PlayerScript scp939;

	private LureSubjectContainer lureSpj;

	private static Class[] staticClasses;

	[SyncVar(hook = "SetVerification")]
	public bool IsVerified;

	private static GameObject host;

	[SyncVar(hook = "SetSteamId")]
	public string SteamId;

	private bool wasAnytimeAlive;

	private float aliveTime;

	private int prevId = -1;

	private static int kRpcRpcPlaceBlood;

	private static int kTargetRpcTargetConsolePrint;

	private static int kCmdCmdSendToken;

	private static int kCmdCmdRequestContactEmail;

	private static int kCmdCmdRequestServerConfig;

	private static int kCmdCmdRequestServerGroups;

	private static int kCmdCmdRequestHideTag;

	private static int kCmdCmdRequestShowTag;

	private static int kCmdCmdSuicide;

	private static int kTargetRpcTargetSetDisconnectError;

	private static int kCmdCmdConfirmDisconnect;

	private static int kCmdCmdRegisterEscape;

	private static int kCmdCmdRequestDeathScreen;

	private static int kTargetRpcTargetDeathScreen;

	public int NetworkntfUnit
	{
		get
		{
			return ntfUnit;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref ntfUnit;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetUnit(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public int NetworkcurClass
	{
		get
		{
			return curClass;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref curClass;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetClassID(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public Vector3 NetworkdeathPosition
	{
		get
		{
			return deathPosition;
		}
		[param: In]
		set
		{
			ref Vector3 fieldValue = ref deathPosition;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SyncDeathPos(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public bool NetworkroundStarted
	{
		get
		{
			return roundStarted;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref roundStarted;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetRoundStart(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	public bool NetworkIsVerified
	{
		get
		{
			return IsVerified;
		}
		[param: In]
		set
		{
			ref bool isVerified = ref IsVerified;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetVerification(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref isVerified, 16u);
		}
	}

	public string NetworkSteamId
	{
		get
		{
			return SteamId;
		}
		[param: In]
		set
		{
			ref string steamId = ref SteamId;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetSteamId(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref steamId, 32u);
		}
	}

	public void SetUnit(int unit)
	{
		NetworkntfUnit = unit;
	}

	public void SyncDeathPos(Vector3 v)
	{
		NetworkdeathPosition = v;
	}

	private void SetVerification(bool b)
	{
		NetworkIsVerified = b;
	}

	[ServerCallback]
	public void AllowContain()
	{
		if (!NetworkServer.active || TutorialManager.status)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (Vector3.Distance(gameObject.transform.position, lureSpj.transform.position) < 1.97f)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
				if (component.klasy[component.curClass].team != 0 && component.curClass != 2)
				{
					component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", "LURE", 0), gameObject);
					lureSpj.SetState(true);
				}
			}
		}
	}

	[ClientRpc]
	public void RpcPlaceBlood(Vector3 pos, int type, float f)
	{
		GetComponent<BloodDrawer>().PlaceUnderneath(pos, type, f);
	}

	[TargetRpc(channel = 2)]
	public void TargetConsolePrint(NetworkConnection connection, string text, string color)
	{
		Color color2 = Color.grey;
		color = color.ToLower();
		switch (color)
		{
		case "red":
			color2 = Color.red;
			break;
		case "cyan":
			color2 = Color.cyan;
			break;
		case "blue":
			color2 = Color.blue;
			break;
		case "magenta":
			color2 = Color.magenta;
			break;
		case "white":
			color2 = Color.white;
			break;
		case "green":
			color2 = Color.green;
			break;
		case "yellow":
			color2 = Color.yellow;
			break;
		case "grey":
		case "gray":
			color2 = Color.grey;
			break;
		}
		Console.singleton.AddLog("[MESSAGE FROM SERVER] " + text, color2);
	}

	public bool IsHuman()
	{
		return curClass > 0 && klasy[curClass].team != 0 && klasy[curClass].team != Team.RIP;
	}

	private void Start()
	{
		onlineMode = ConfigFile.ServerConfig.GetBool("online_mode", true);
		_centralAuthInt = new CentralAuthInterface(this, base.isServer);
		lureSpj = Object.FindObjectOfType<LureSubjectContainer>();
		scp049 = GetComponent<Scp049PlayerScript>();
		scp457 = GetComponent<Scp457PlayerScript>();
		scp008 = GetComponent<Sco008PlayerScript>();
		scp049_2 = GetComponent<Scp049_2PlayerScript>();
		scp079 = GetComponent<Scp079PlayerScript>();
		scp106 = GetComponent<Scp106PlayerScript>();
		scp173 = GetComponent<Scp173PlayerScript>();
		scp096 = GetComponent<Scp096PlayerScript>();
		scp939 = GetComponent<Scp939PlayerScript>();
		forceClass = ConfigFile.ServerConfig.GetInt("server_forced_class", -1);
		ciPercentage = ConfigFile.ServerConfig.GetInt("ci_on_start_percent", 10);
		StartCoroutine(_Init());
		string @string = ConfigFile.ServerConfig.GetString("team_respawn_queue", "401431403144144");
		classTeamQueue.Clear();
		for (int i = 0; i < @string.Length; i++)
		{
			int result = 4;
			if (!int.TryParse(@string[i].ToString(), out result))
			{
				result = 4;
			}
			classTeamQueue.Add((Team)result);
		}
		while (classTeamQueue.Count < NetworkManager.singleton.maxConnections)
		{
			classTeamQueue.Add(Team.CDP);
		}
		if (!base.isLocalPlayer && TutorialManager.status)
		{
			ApplyProperties();
		}
		if (base.isLocalPlayer)
		{
			for (int j = 0; j < klasy.Length; j++)
			{
				if (klasy[j].team != 0)
				{
					klasy[j].fullName = TranslationReader.Get("Class_Names", j);
				}
                klasy[j].description = TranslationReader.Get("Class_Descriptions", j);
            }
			staticClasses = klasy;
			if (SteamManager.Initialized)
			{
				CentralAuth.singleton.GenerateToken(_centralAuthInt);
				return;
			}
			Console.singleton.AddLog("Steam not initialized - sending empty auth token.\nIf server is using online mode, you will probably get kicked.", Color.red);
			CallCmdSendToken(string.Empty);
		}
		else if (staticClasses == null || staticClasses.Length == 0)
		{
			for (int k = 0; k < klasy.Length; k++)
			{
				klasy[k].description = TranslationReader.Get("Class_Descriptions", k);
				if (klasy[k].team != 0)
				{
					klasy[k].fullName = TranslationReader.Get("Class_Names", k);
				}
			}
		}
		else
		{
			klasy = staticClasses;
		}
	}

	private IEnumerator _Init()
	{
		if (NetworkServer.active)
		{
			if (ConfigFile.ServerConfig.GetBool("online_mode", true) && !base.isLocalPlayer)
			{
				float timeout = 0f;
				while (timeout < 12f)
				{
					timeout += Timing.DeltaTime;
					yield return 0f;
					if (!string.IsNullOrEmpty(SteamId))
					{
						NetworkIsVerified = true;
						yield break;
					}
					if (timeout < 12f)
					{
						continue;
					}
					ServerConsole.Disconnect(base.connectionToClient, "Your client has failed to authenticate in time.");
					yield break;
				}
			}
			else
			{
				NetworkIsVerified = true;
			}
		}
		while (host == null)
		{
			host = GameObject.Find("Host");
			yield return 0f;
		}
		if (base.isLocalPlayer)
		{
			while (seed == 0)
			{
				seed = host.GetComponent<RandomSeedSync>().seed;
			}
			if (NetworkServer.active)
			{
				if (ServerStatic.isDedicated)
				{
					ServerConsole.AddLog("Waiting for players..");
				}
				CursorManager.roundStarted = true;
				if (TutorialManager.status)
				{
					ForceRoundStart();
				}
				else
				{
					RoundStart.singleton.ShowButton();
					int timeLeft = 20;
					int mostPlayersSoFar = 1;
					while (RoundStart.singleton.info != "started")
					{
						if (mostPlayersSoFar > 1)
						{
							timeLeft--;
						}
						int count = PlayerManager.singleton.players.Length;
						if (count > mostPlayersSoFar)
						{
							mostPlayersSoFar = count;
							if (mostPlayersSoFar >= NetworkManager.singleton.maxConnections)
							{
								timeLeft = 0;
							}
							else if (timeLeft % 5 > 0)
							{
								timeLeft = timeLeft / 5 * 5 + 5;
							}
						}
						if (timeLeft > 0)
						{
							RoundStart.singleton.Networkinfo = timeLeft.ToString();
                        }
						else
						{
							ForceRoundStart();
						}
						yield return new WaitForSeconds(1.2f);
					}
				}
				CursorManager.roundStarted = false;
				CmdStartRound();
				SetRoundStart(true);
				SetRandomRoles();
			}
			while (!host.GetComponent<CharacterClassManager>().roundStarted)
			{
				yield return 0f;
			}
			yield return new WaitForSeconds(2f);
			while (curClass < 0)
			{
				CallCmdSuicide(default(PlayerStats.HitInfo));
				yield return new WaitForSeconds(1f);
			}
		}
		if (!base.isLocalPlayer)
		{
			yield break;
		}
		int iteration = 0;
		while (true)
		{
			GameObject[] plys = PlayerManager.singleton.players;
			if (iteration >= plys.Length)
			{
				yield return new WaitForSeconds(3f);
				iteration = 0;
			}
			try
			{
				plys[iteration].GetComponent<CharacterClassManager>().InitSCPs();
			}
			catch
			{
			}
			iteration++;
			yield return 0f;
		}
	}

	public void SetSteamId(string i)
	{
		NetworkSteamId = i;
	}

	[Command(channel = 2)]
	public void CmdSendToken(string token)
	{
		if (ConfigFile.ServerConfig.GetBool("online_mode", true))
		{
			if (string.IsNullOrEmpty(token) || _commandtokensent)
			{
				if (!base.isLocalPlayer)
				{
					ServerConsole.Disconnect(base.connectionToClient, "Your client sent an empty or repeated authentication token.");
				}
				else
				{
					SetVerification(true);
				}
			}
			else
			{
				CentralAuth.singleton.StartValidateToken(_centralAuthInt, token);
				AuthToken = token;
			}
		}
		_commandtokensent = true;
	}

	[Command(channel = 2)]
	public void CmdRequestContactEmail()
	{
		if (GetComponent<ServerRoles>().RemoteAdmin || GetComponent<ServerRoles>().Staff)
		{
			CallTargetConsolePrint(base.connectionToClient, "Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", string.Empty), "green");
		}
		else
		{
			CallTargetConsolePrint(base.connectionToClient, "You don't have permissions to execute this command.", "red");
		}
	}

	[Command(channel = 2)]
	public void CmdRequestServerConfig()
	{
		YamlConfig serverConfig = ConfigFile.ServerConfig;
		if (GetComponent<ServerRoles>().RemoteAdmin || GetComponent<ServerRoles>().Staff)
		{
			CallTargetConsolePrint(base.connectionToClient, "Extended server configuration:\nServer name: " + serverConfig.GetString("server_name", string.Empty) + "\nServer IP: " + serverConfig.GetString("server_ip", string.Empty) + "\nCurrent Server IP:: " + CustomNetworkManager.Ip + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", string.Empty) + "\nServer max players: " + serverConfig.GetInt("max_players") + "\nOnline mode: " + serverConfig.GetBool("online_mode") + "\nRA password authentication: " + GetComponent<QueryProcessor>().OverridePasswordEnabled + "\nIP banning: " + serverConfig.GetBool("ip_banning") + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist") + "\nQuery status: " + serverConfig.GetBool("enable_query") + " with port shift " + serverConfig.GetInt("query_port_shift") + "\nFriendly fire: " + serverConfig.GetBool("friendly_fire") + "\nMap seed: " + serverConfig.GetInt("map_seed"), "green");
		}
		else
		{
			CallTargetConsolePrint(base.connectionToClient, "Basic server configuration:\nServer name: " + serverConfig.GetString("server_name", string.Empty) + "\nServer IP: " + serverConfig.GetString("server_ip", string.Empty) + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", string.Empty) + "\nServer max players: " + serverConfig.GetInt("max_players") + "\nRA password authentication: " + GetComponent<QueryProcessor>().OverridePasswordEnabled + "\nOnline mode: " + serverConfig.GetBool("online_mode") + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist") + "\nFriendly fire: " + serverConfig.GetBool("friendly_fire") + "\nMap seed: " + serverConfig.GetInt("map_seed"), "green");
		}
	}

	[Command(channel = 2)]
	public void CmdRequestServerGroups()
	{
		string text = "Groups defined on this server:";
		Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
		ServerRoles.NamedColor[] namedColors = GetComponent<ServerRoles>().NamedColors;
		foreach (KeyValuePair<string, UserGroup> permentry in allGroups)
		{
			try
			{
				string text2 = text;
				text = text2 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - <color=#" + namedColors.FirstOrDefault((ServerRoles.NamedColor x) => x.Name == permentry.Value.BadgeColor).ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
			}
			catch
			{
				string text2 = text;
				text = text2 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
			{
				text += " K";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
			{
				text += " B1";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
			{
				text += " B2";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
			{
				text += " FSE";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
			{
				text += " FSP";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
			{
				text += " FC";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
			{
				text += " G";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
			{
				text += " EW";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
			{
				text += " ERS";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
			{
				text += " ERD";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
			{
				text += " SG";
			}
			if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
			{
				text += " FM";
			}
		}
		CallTargetConsolePrint(base.connectionToClient, "Defined groups on server " + text, "grey");
	}

	[Command(channel = 2)]
	public void CmdRequestHideTag()
	{
		ServerRoles component = GetComponent<ServerRoles>();
		component.SetText(string.Empty);
		component.SetColor("default");
		component.SetBadgeUpdate(string.Empty);
		CallTargetConsolePrint(base.connectionToClient, "Badge hidden.", "green");
	}

	[Command(channel = 2)]
	public void CmdRequestShowTag(bool global)
	{
		ServerRoles component = GetComponent<ServerRoles>();
		if (global)
		{
			if (string.IsNullOrEmpty(component.PrevBadge))
			{
				CallTargetConsolePrint(base.connectionToClient, "You don't have global tag.", "magenta");
				return;
			}
			component.SetBadgeUpdate(component.PrevBadge);
			component.NetworkGlobalSet = true;
			CallTargetConsolePrint(base.connectionToClient, "Global tag refreshed.", "green");
		}
		else
		{
			component.SetBadgeUpdate(string.Empty);
			component.RefreshPermissions();
			CallTargetConsolePrint(base.connectionToClient, "Local tag refreshed.", "green");
		}
	}

	[Command]
	public void CmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		hitInfo.amount = ((hitInfo.amount != 0f) ? hitInfo.amount : 999799f);
		GetComponent<PlayerStats>().HurtPlayer(hitInfo, base.gameObject);
	}

	public void ForceRoundStart()
	{
		if (NetworkServer.active)
		{
			ServerLogs.AddLog(ServerLogs.Modules.Logger, "Round has been started.", ServerLogs.ServerLogType.GameEvent);
			ServerConsole.AddLog("New round has been started.");
			RoundStart.singleton.Networkinfo = "started";
		}
	}

	[TargetRpc(channel = 2)]
	private void TargetSetDisconnectError(NetworkConnection conn, string message)
	{
		((CustomNetworkManager)NetworkManager.singleton).disconnectMessage = message;
		CallCmdConfirmDisconnect();
	}

	[Command(channel = 2)]
	private void CmdConfirmDisconnect()
	{
		if (base.connectionToClient != null && base.connectionToClient.isConnected)
		{
			base.connectionToClient.Disconnect();
			base.connectionToClient.Dispose();
		}
	}

	public void DisconnectClient(NetworkConnection conn, string message)
	{
		CallTargetSetDisconnectError(conn, message);
	StartCoroutine(_DisconnectAfterTimeout(conn));
	}

	private IEnumerator _DisconnectAfterTimeout(NetworkConnection conn)
	{
		yield return new WaitForSeconds(3f);
		if (conn != null && conn.isConnected)
		{
			conn.Disconnect();
			conn.Dispose();
		}
	}

	public void InitSCPs()
	{
		if (curClass != -1 && !TutorialManager.status)
		{
			Class c = klasy[curClass];
			scp457.Init(curClass,c);
			scp049.Init(curClass, c);
			scp008.Init(curClass,c);
			scp049_2.Init(curClass, c);
			scp106.Init(curClass, c);
			scp173.Init(curClass, c);
			scp096.Init(curClass, c);
			scp939.Init(curClass, c);
		}
	}

	public void RegisterEscape()
	{
		CallCmdRegisterEscape();
	}

	[Command(channel = 2)]
	private void CmdRegisterEscape()
	{
		CharacterClassManager component = GetComponent<CharacterClassManager>();
		if (Vector3.Distance(base.transform.position, GetComponent<Escape>().worldPosition) < (float)(GetComponent<Escape>().radius * 2))
		{
			if (klasy[component.curClass].team == Team.CDP)
			{
				SetPlayersClass(8, base.gameObject);
				RoundSummary.escaped_ds++;
			}
			if (klasy[component.curClass].team == Team.RSC)
			{
				SetPlayersClass(4, base.gameObject);
				RoundSummary.escaped_scientists++;
			}
		}
	}

	public void ApplyProperties()
	{
		Class @class = klasy[curClass];
		GetComponent<Sco008PlayerScript>().ResetAll();
		InitSCPs();
		if (curClass != 2)
		{
			wasAnytimeAlive = true;
		}
		if (@class.team == Team.MTF)
		{
			AchievementManager.Achieve("arescue");
		}
		if (@class.team == Team.CHI)
		{
			AchievementManager.Achieve("chaos");
		}
		Inventory component = GetComponent<Inventory>();
		PlyMovementSync component2 = GetComponent<PlyMovementSync>();
		try
		{
			GetComponent<FootstepSync>().SetLoundness(@class.team, @class.fullName.Contains("939"));
		}
		catch
		{
		}
		PlayerManager.localPlayer.GetComponent<SpectatorManager>().RefreshList();
		if (base.isLocalPlayer)
		{
			GetComponent<FirstPersonController>().isSCP = @class.team == Team.SCP;
			DiscordManager.singleton.ChangePreset(curClass);
			GetComponent<Radio>().UpdateClass();
			GetComponent<Handcuffs>().CallCmdTarget(null);
			GetComponent<WeaponManager>().flashlightEnabled = true;
			GetComponent<Searching>().Init((@class.team == Team.SCP) | (@class.team == Team.RIP));
		}
		if (@class.team == Team.RIP)
		{
			if (base.isServer)
			{
				component2.SetPosition(new Vector3(0f, 2048f, 0f));
				component2.SetRotation(0f);
			}
			if (base.isLocalPlayer)
			{
				component.curItem = -1;
				GetComponent<FirstPersonController>().enabled = false;
				if (curClass != 2 || Radio.roundStarted)
				{
					if (wasAnytimeAlive)
					{
						CallCmdRequestDeathScreen();
					}
					else
					{
						FindObjectOfType<StartScreen>().PlayAnimation(curClass);
					}
					GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell_dead);
				}
				GetComponent<PlayerStats>().maxHP = @class.maxHP;
				unfocusedCamera.GetComponent<Camera>().enabled = false;
				unfocusedCamera.GetComponent<PostProcessVolume>().enabled = false;
			}
		}
		else
		{
			if (NetworkServer.active)
			{
				GameObject gameObject = null;
				gameObject = Object.FindObjectOfType<SpawnpointManager>().GetRandomPosition(curClass);
				if (gameObject != null)
				{
					component2.SetPosition(gameObject.transform.position);
					component2.SetRotation(gameObject.transform.rotation.eulerAngles.y);
				}
				else
				{
					component2.SetPosition(deathPosition);
				}
			}
			if (base.isLocalPlayer)
			{
				GetComponent<Scp106PlayerScript>().SetDoors();
				component.curItem = -1;
				FindObjectOfType<StartScreen>().PlayAnimation(curClass);
				if (!GetComponent<HorrorSoundController>().horrorSoundSource.isPlaying)
				{
					GetComponent<HorrorSoundController>().horrorSoundSource.PlayOneShot(bell);
				}
				Invoke("EnableFPC", 0.2f);
				GetComponent<Radio>().curPreset = 0;
				GetComponent<Radio>().CmdUpdatePreset(0);
				FirstPersonController component3 = GetComponent<FirstPersonController>();
				PlayerStats component4 = GetComponent<PlayerStats>();
				if (@class.postprocessingProfile != null && GetComponentInChildren<PostProcessVolume>() != null)
				{
					GetComponentInChildren<PostProcessVolume>().profile = @class.postprocessingProfile;
				}
				unfocusedCamera.GetComponent<Camera>().enabled = true;
				unfocusedCamera.GetComponent<PostProcessVolume>().enabled = true;
				component3.m_WalkSpeed = @class.walkSpeed;
				component3.m_RunSpeed = @class.runSpeed;
				component3.m_UseHeadBob = @class.useHeadBob;
				component3.m_JumpSpeed = @class.jumpSpeed;
				int num = (component4.maxHP = @class.maxHP);
				Object.FindObjectOfType<UserMainInterface>().lerpedHP = num;
				SkyboxFollower.iAm939 = @class.fullName.Contains("939");
			}
			else
			{
				GetComponent<PlayerStats>().maxHP = @class.maxHP;
			}
		}
		if (base.isLocalPlayer)
		{
			Object.FindObjectOfType<InventoryDisplay>().isSCP = (curClass == 2) | (@class.team == Team.SCP);
			Object.FindObjectOfType<InterfaceColorAdjuster>().ChangeColor(@class.classColor);
		}
		RefreshPlyModel();
		QueryProcessor.StaticRefreshPlayerList();
	}

	private void EnableFPC()
	{
		GetComponent<FirstPersonController>().enabled = true;
	}

	public void RefreshPlyModel(int classID = -1)
	{
		GetComponent<AnimationController>().OnChangeClass();
		if (myModel != null)
		{
			Object.Destroy(myModel);
		}
		Class @class = klasy[(classID >= 0) ? classID : curClass];
		if (@class.team != Team.RIP)
		{
			GameObject gameObject = Object.Instantiate(@class.model_player);
			gameObject.transform.SetParent(base.gameObject.transform);
			gameObject.transform.localPosition = @class.model_offset.position;
			gameObject.transform.localRotation = Quaternion.Euler(@class.model_offset.rotation);
			gameObject.transform.localScale = @class.model_offset.scale;
			myModel = gameObject;
			if (myModel.GetComponent<Animator>() != null)
			{
				GetComponent<AnimationController>().animator = myModel.GetComponent<Animator>();
			}
			if (base.isLocalPlayer)
			{
				if (myModel.GetComponent<Renderer>() != null)
				{
					myModel.GetComponent<Renderer>().enabled = false;
				}
				Renderer[] componentsInChildren = myModel.GetComponentsInChildren<Renderer>();
				foreach (Renderer renderer in componentsInChildren)
				{
					renderer.enabled = false;
				}
				Collider[] componentsInChildren2 = myModel.GetComponentsInChildren<Collider>();
				foreach (Collider collider in componentsInChildren2)
				{
					if (collider.name != "LookingTarget")
					{
						collider.enabled = false;
					}
				}
			}
		}
		GetComponent<CapsuleCollider>().enabled = @class.team != Team.RIP;
		if (myModel != null)
		{
			GetComponent<WeaponManager>().hitboxes = myModel.GetComponentsInChildren<HitboxIdentity>(true);
		}
	}

	public void SetClassID(int id)
	{
		if ((IsVerified || id == 2) && (!GetComponent<ServerRoles>().OverwatchEnabled || id == 2))
		{
			NetworkcurClass = id;
			if (id != 2 || base.isLocalPlayer)
			{
				aliveTime = 0f;
				ApplyProperties();
			}
		}
	}

	public void InstantiateRagdoll(int id)
	{
		if (id >= 0)
		{
			Class @class = klasy[curClass];
			GameObject gameObject = Object.Instantiate(@class.model_ragdoll);
			gameObject.transform.position = base.transform.position + @class.ragdoll_offset.position;
			gameObject.transform.rotation = Quaternion.Euler(base.transform.rotation.eulerAngles + @class.ragdoll_offset.rotation);
			gameObject.transform.localScale = @class.ragdoll_offset.scale;
		}
	}

	public void SetRandomRoles()
	{
		MTFRespawn component = GetComponent<MTFRespawn>();
		if (base.isLocalPlayer && base.isServer)
		{
			GameObject[] array = GetShuffledPlayerList().ToArray();
			RoundSummary component2 = GetComponent<RoundSummary>();
			bool flag = (float)Random.Range(0, 100) < ciPercentage;
			RoundSummary.SumInfo_ClassList startClassList = default(RoundSummary.SumInfo_ClassList);
			for (int i = 0; i < array.Length; i++)
			{
				int num = ((forceClass != -1) ? forceClass : Find_Random_ID_Using_Defined_Team(classTeamQueue[i]));
				switch (klasy[num].team)
				{
				case Team.CDP:
					startClassList.class_ds++;
					break;
				case Team.CHI:
					startClassList.chaos_insurgents++;
					break;
				case Team.MTF:
					startClassList.mtf_and_guards++;
					break;
				case Team.RSC:
					startClassList.scientists++;
					break;
				case Team.SCP:
					startClassList.scps_except_zombies++;
					break;
				}
				if (TutorialManager.status)
				{
					SetPlayersClass(14, base.gameObject);
				}
				else
				{
					SetPlayersClass(num, array[i]);
				}
			}
			startClassList.time = (int)Time.realtimeSinceStartup;
			startClassList.warhead_kills = -1;
			Object.FindObjectOfType<RoundSummary>().SetStartClassList(startClassList);
			if (ConfigFile.ServerConfig.GetBool("smart_class_picker", true))
			{
				RunSmartClassPicker();
			}
		}
		if (NetworkServer.active)
		{
			StartCoroutine(MakeSureToSetHP());
		}
	}

	private List<GameObject> GetShuffledPlayerList()
	{
		List<GameObject> list = new List<GameObject>(PlayerManager.singleton.players);
		List<GameObject> list2 = new List<GameObject>();
		while (list.Count > 0)
		{
			int index = Random.Range(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}

	[Command]
	private void CmdRequestDeathScreen()
	{
		CallTargetDeathScreen(base.connectionToClient, GetComponent<PlayerStats>().lastHitInfo);
	}

	[TargetRpc]
	private void TargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
	{
		Object.FindObjectOfType<YouWereKilled>().Play(hitinfo);
	}

	private void RunSmartClassPicker()
	{
		string text = "Before Starting";
		try
		{
			text = "Setting Initial Value";
			if (ConfigFile.smBalancedPicker == null)
			{
				ConfigFile.smBalancedPicker = new Dictionary<string, int[]>();
			}
			text = "Valid Players List Error";
			List<GameObject> shuffledPlayerList = GetShuffledPlayerList();
			text = "Copying Balanced Picker List";
			Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>(ConfigFile.smBalancedPicker);
			text = "Clearing Balanced Picker List";
			ConfigFile.smBalancedPicker.Clear();
			text = "Re-building Balanced Picker List";
			foreach (GameObject item in shuffledPlayerList)
			{
				if (item != null)
				{
					NetworkConnection component = item.GetComponent<NetworkConnection>();
					CharacterClassManager component2 = item.GetComponent<CharacterClassManager>();
					text = "Getting Player ID";
					if (component == null && component2 == null)
					{
						shuffledPlayerList.Remove(item);
						break;
					}
					string text2 = ((component == null) ? string.Empty : component.address);
					string text3 = ((!(component2 != null)) ? string.Empty : component2.SteamId);
					string text4 = text2 + text3;
					text = "Setting up Player \"" + text4 + "\"";
					if (!dictionary.ContainsKey(text4))
					{
						text = "Adding Player \"" + text4 + "\" to smBalancedPicker";
						int[] array = new int[klasy.Length];
						for (int i = 0; i < array.Length; i++)
						{
							array[i] = ConfigFile.ServerConfig.GetInt("smart_cp_starting_weight", 6);
						}
						ConfigFile.smBalancedPicker.Add(text4, array);
					}
					else
					{
						text = "Updating Player \"" + text4 + "\" in smBalancedPicker";
						int[] value = null;
						if (dictionary.TryGetValue(text4, out value))
						{
							ConfigFile.smBalancedPicker.Add(text4, value);
						}
					}
				}
				else
				{
					text = "Removing Player from Balanced Picker List";
					shuffledPlayerList.Remove(item);
				}
			}
			text = "Clearing Copied Balanced Picker List";
			dictionary.Clear();
			List<int> list = new List<int>();
			text = "Getting Available Roles";
			foreach (GameObject item2 in shuffledPlayerList)
			{
				if (item2 != null)
				{
					CharacterClassManager component3 = item2.GetComponent<CharacterClassManager>();
					if (component3 != null)
					{
						list.Add(component3.curClass);
					}
					else
					{
						shuffledPlayerList.Remove(item2);
					}
				}
				else
				{
					shuffledPlayerList.Remove(item2);
				}
			}
			List<GameObject> list2 = new List<GameObject>();
			text = "Setting Roles";
			foreach (GameObject item3 in shuffledPlayerList)
			{
				if (item3 != null)
				{
					NetworkConnection component4 = item3.GetComponent<NetworkConnection>();
					CharacterClassManager component5 = item3.GetComponent<CharacterClassManager>();
					if (component4 == null && component5 == null)
					{
						shuffledPlayerList.Remove(item3);
						break;
					}
					string text5 = ((component4 == null) ? string.Empty : component4.address);
					string text6 = ((!(component5 != null)) ? string.Empty : component5.SteamId);
					string text7 = text5 + text6;
					text = "Setting Player \"" + text7 + "\"'s Class";
					int mostLikelyClass = GetMostLikelyClass(text7, list);
					if (mostLikelyClass != -1)
					{
						SetPlayersClass(mostLikelyClass, item3);
						list.Remove(mostLikelyClass);
					}
					else
					{
						list2.Add(item3);
					}
				}
				else
				{
					shuffledPlayerList.Remove(item3);
				}
			}
			text = "Reversing Additional Classes List";
			list.Reverse();
			text = "Setting Unknown Players Classes";
			foreach (GameObject item4 in list2)
			{
				if (item4 != null)
				{
					if (list.Count > 0)
					{
						int num = list[0];
						SetPlayersClass(num, item4);
						list.Remove(num);
					}
					else
					{
						int classid = 2;
						SetPlayersClass(classid, item4);
					}
				}
			}
			text = "Clearing Unknown Players List";
			list2.Clear();
			text = "Clearing Available Classes List";
			list.Clear();
		}
		catch
		{
			Console.singleton.AddLog("Smart Class Picker Failed: " + text, new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
		}
	}

	private int GetMostLikelyClass(string playerUUID, List<int> availableClasses)
	{
		int[] value = null;
		int num = -1;
		if (availableClasses.Count <= 0 || !ConfigFile.smBalancedPicker.TryGetValue(playerUUID, out value) || value == null || value.Length != klasy.Length)
		{
			return num;
		}
		if (!ContainsPossibleClass(value, availableClasses))
		{
			return num;
		}
		int num2 = 0;
		int[] array = (int[])value.Clone();
		for (int i = 0; i < array.Length; i++)
		{
			num2 = (array[i] = num2 + array[i]);
		}
		while (!availableClasses.Contains(num))
		{
			int num3 = Random.Range(0, num2);
			for (int j = 0; j < array.Length; j++)
			{
				if (num3 < array[j])
				{
					num = j;
					break;
				}
			}
		}
		if (num < 0 || num >= klasy.Length)
		{
			return -1;
		}
		UpdateClassChances(num, value);
		return num;
	}

	private bool ContainsPossibleClass(int[] classChances, List<int> availableClasses)
	{
		foreach (int availableClass in availableClasses)
		{
			if (availableClass >= 0 && availableClass < classChances.Length && classChances[availableClass] > 0)
			{
				return true;
			}
		}
		return false;
	}

	private void UpdateClassChances(int classChoice, int[] classChances)
	{
		int @int = ConfigFile.ServerConfig.GetInt("smart_cp_weight_min", 1);
		@int = ((@int < 0) ? 1 : @int);
		int int2 = ConfigFile.ServerConfig.GetInt("smart_cp_weight_max", 11);
		int2 = ((int2 >= @int) ? int2 : (@int + 10));
		for (int i = 0; i < classChances.Length; i++)
		{
			bool flag = false;
			bool flag2 = false;
			if (ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_decrease"), -99) != -99 && klasy[i].team == klasy[classChoice].team)
			{
				classChances[i] -= ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_decrease"));
				flag2 = true;
			}
			else if (ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_increase"), -99) != -99 && klasy[i].team != klasy[classChoice].team)
			{
				classChances[i] += ConfigFile.ServerConfig.GetInt(string.Concat("smart_cp_team_", klasy[i].team, "_weight_increase"));
				flag = true;
			}
			if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_decrease", -99) != -99 && i == classChoice && !flag)
			{
				classChances[i] -= ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_decrease", 3);
			}
			else if (ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_increase", -99) != -99 && i != classChoice && !flag2)
			{
				classChances[i] += ConfigFile.ServerConfig.GetInt("smart_cp_class_" + i + "_weight_increase", 1);
			}
			else if (!flag && !flag2)
			{
				if (klasy[classChoice].team == Team.MTF && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 2;
					if (i == classChoice)
					{
						classChances[i] -= 2;
					}
				}
				else if (klasy[classChoice].team == Team.CDP && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 3;
				}
				else if (klasy[classChoice].team == Team.SCP && klasy[classChoice].team == klasy[i].team)
				{
					classChances[i] -= 2;
					if (i == classChoice)
					{
						classChances[i]--;
					}
				}
				else if (i == classChoice)
				{
					classChances[i] -= 2;
				}
				else
				{
					classChances[i]++;
				}
			}
			classChances[i] = Mathf.Clamp(classChances[i], @int, int2);
		}
	}

	private void SetRoundStart(bool b)
	{
		NetworkroundStarted = b;
	}

	[ServerCallback]
	private void CmdStartRound()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		if (!TutorialManager.status)
		{
			try
			{
				Door componentInChildren = GameObject.Find("MeshDoor173").GetComponentInChildren<Door>();
				Door[] doors = GameObject.Find("Root_106").GetComponentsInChildren<Door>();
				foreach (var item in doors)
				{
					item.ForceCooldown(600);
				}
				componentInChildren.ForceCooldown(25f);
				Object.FindObjectOfType<ChopperAutostart>().SetState(false);
			}
			catch
			{
			}
		}
		SetRoundStart(true);
	}

	[ServerCallback]
	public void SetPlayersClass(int classid, GameObject ply)
	{
		if (NetworkServer.active && ply.GetComponent<CharacterClassManager>().IsVerified)
		{
			ply.GetComponent<CharacterClassManager>().SetClassID(classid);
			Inventory component = ply.GetComponent<Inventory>();
			ply.GetComponent<AmmoBox>().SetAmmoAmount();
			component.items.Clear();
			int[] startItems = klasy[Mathf.Clamp(classid, 0, klasy.Length - 1)].startItems;
			foreach (int id in startItems)
			{
				component.AddNewItem(id);
			}
			ply.GetComponent<PlayerStats>().SetHPAmount(klasy[classid].maxHP);
		}
	}

	private IEnumerator MakeSureToSetHP()
	{
		for (int i = 0; i < 7; i++)
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
				PlayerStats component2 = gameObject.GetComponent<PlayerStats>();
				if (component2.health <= klasy[component.curClass].maxHP)
				{
					component2.SetHPAmount(klasy[component.curClass].maxHP);
				}
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private int Find_Random_ID_Using_Defined_Team(Team team)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < klasy.Length; i++)
		{
			if (klasy[i].team == team && !klasy[i].banClass)
			{
				list.Add(i);
			}
		}
		if (list.Count == 0)
		{
			return 1;
		}
		int index = Random.Range(0, list.Count);
		if (klasy[list[index]].team == Team.SCP)
		{
			klasy[list[index]].banClass = true;
		}
		return list[index];
	}

	public bool SpawnProtection()
	{
		return aliveTime < 2f;
	}

	private void Update()
	{
		if (curClass == 2)
		{
			aliveTime = 0f;
		}
		else
		{
			aliveTime += Time.deltaTime;
		}
		if (base.isLocalPlayer)
		{
			if (ServerStatic.isDedicated)
			{
				CursorManager.isServerOnly = true;
			}
			if (base.isServer)
			{
				AllowContain();
			}
		}
		if (prevId != curClass)
		{
			RefreshPlyModel();
			prevId = curClass;
		}
		if (base.name == "Host")
		{
			Radio.roundStarted = roundStarted;
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSendToken(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSendToken called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdSendToken(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdRequestContactEmail(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestContactEmail called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestContactEmail();
		}
	}

	protected static void InvokeCmdCmdRequestServerConfig(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestServerConfig called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestServerConfig();
		}
	}

	protected static void InvokeCmdCmdRequestServerGroups(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestServerGroups called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestServerGroups();
		}
	}

	protected static void InvokeCmdCmdRequestHideTag(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestHideTag called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestHideTag();
		}
	}

	protected static void InvokeCmdCmdRequestShowTag(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestShowTag called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestShowTag(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdSuicide(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSuicide called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdSuicide(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	protected static void InvokeCmdCmdConfirmDisconnect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdConfirmDisconnect called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdConfirmDisconnect();
		}
	}

	protected static void InvokeCmdCmdRegisterEscape(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRegisterEscape called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRegisterEscape();
		}
	}

	protected static void InvokeCmdCmdRequestDeathScreen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestDeathScreen called on client.");
		}
		else
		{
			((CharacterClassManager)obj).CmdRequestDeathScreen();
		}
	}

	public void CallCmdSendToken(string token)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSendToken called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSendToken(token);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSendToken);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(token);
		SendCommandInternal(networkWriter, 2, "CmdSendToken");
	}

	public void CallCmdRequestContactEmail()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestContactEmail called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestContactEmail();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestContactEmail);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestContactEmail");
	}

	public void CallCmdRequestServerConfig()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestServerConfig called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestServerConfig();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestServerConfig);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestServerConfig");
	}

	public void CallCmdRequestServerGroups()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestServerGroups called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestServerGroups();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestServerGroups);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestServerGroups");
	}

	public void CallCmdRequestHideTag()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestHideTag called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestHideTag();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestHideTag);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRequestHideTag");
	}

	public void CallCmdRequestShowTag(bool global)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestShowTag called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestShowTag(global);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestShowTag);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(global);
		SendCommandInternal(networkWriter, 2, "CmdRequestShowTag");
	}

	public void CallCmdSuicide(PlayerStats.HitInfo hitInfo)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSuicide called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSuicide(hitInfo);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSuicide);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, hitInfo);
		SendCommandInternal(networkWriter, 0, "CmdSuicide");
	}

	public void CallCmdConfirmDisconnect()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdConfirmDisconnect called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdConfirmDisconnect();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdConfirmDisconnect);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdConfirmDisconnect");
	}

	public void CallCmdRegisterEscape()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRegisterEscape called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRegisterEscape();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRegisterEscape);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdRegisterEscape");
	}

	public void CallCmdRequestDeathScreen()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestDeathScreen called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestDeathScreen();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestDeathScreen);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 0, "CmdRequestDeathScreen");
	}

	protected static void InvokeRpcRpcPlaceBlood(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaceBlood called on server.");
		}
		else
		{
			((CharacterClassManager)obj).RpcPlaceBlood(reader.ReadVector3(), (int)reader.ReadPackedUInt32(), reader.ReadSingle());
		}
	}

	protected static void InvokeRpcTargetConsolePrint(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetConsolePrint called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetConsolePrint(ClientScene.readyConnection, reader.ReadString(), reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetSetDisconnectError(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetDisconnectError called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetSetDisconnectError(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetDeathScreen(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetDeathScreen called on server.");
		}
		else
		{
			((CharacterClassManager)obj).TargetDeathScreen(ClientScene.readyConnection, GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	public void CallRpcPlaceBlood(Vector3 pos, int type, float f)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaceBlood called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaceBlood);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		networkWriter.WritePackedUInt32((uint)type);
		networkWriter.Write(f);
		SendRPCInternal(networkWriter, 0, "RpcPlaceBlood");
	}

	public void CallTargetConsolePrint(NetworkConnection connection, string text, string color)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetConsolePrint called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetConsolePrint);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(text);
		networkWriter.Write(color);
		SendTargetRPCInternal(connection, networkWriter, 2, "TargetConsolePrint");
	}

	public void CallTargetSetDisconnectError(NetworkConnection conn, string message)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetDisconnectError called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetDisconnectError);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(message);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetSetDisconnectError");
	}

	public void CallTargetDeathScreen(NetworkConnection conn, PlayerStats.HitInfo hitinfo)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetDeathScreen called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetDeathScreen);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, hitinfo);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetDeathScreen");
	}

	static CharacterClassManager()
	{
		kCmdCmdSendToken = 970325235;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdSendToken, InvokeCmdCmdSendToken);
		kCmdCmdRequestContactEmail = 2054299309;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestContactEmail, InvokeCmdCmdRequestContactEmail);
		kCmdCmdRequestServerConfig = -2046741578;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestServerConfig, InvokeCmdCmdRequestServerConfig);
		kCmdCmdRequestServerGroups = -1929409976;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestServerGroups, InvokeCmdCmdRequestServerGroups);
		kCmdCmdRequestHideTag = -1886885625;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestHideTag, InvokeCmdCmdRequestHideTag);
		kCmdCmdRequestShowTag = -732213908;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestShowTag, InvokeCmdCmdRequestShowTag);
		kCmdCmdSuicide = -1051695024;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdSuicide, InvokeCmdCmdSuicide);
		kCmdCmdConfirmDisconnect = -1987348706;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdConfirmDisconnect, InvokeCmdCmdConfirmDisconnect);
		kCmdCmdRegisterEscape = -1826587486;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRegisterEscape, InvokeCmdCmdRegisterEscape);
		kCmdCmdRequestDeathScreen = -1840245105;
		NetworkBehaviour.RegisterCommandDelegate(typeof(CharacterClassManager), kCmdCmdRequestDeathScreen, InvokeCmdCmdRequestDeathScreen);
		kRpcRpcPlaceBlood = 1372291111;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kRpcRpcPlaceBlood, InvokeRpcRpcPlaceBlood);
		kTargetRpcTargetConsolePrint = -558403607;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetConsolePrint, InvokeRpcTargetConsolePrint);
		kTargetRpcTargetSetDisconnectError = -2047672291;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetSetDisconnectError, InvokeRpcTargetSetDisconnectError);
		kTargetRpcTargetDeathScreen = -520196787;
		NetworkBehaviour.RegisterRpcDelegate(typeof(CharacterClassManager), kTargetRpcTargetDeathScreen, InvokeRpcTargetDeathScreen);
		NetworkCRC.RegisterBehaviour("CharacterClassManager", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)ntfUnit);
			writer.WritePackedUInt32((uint)curClass);
			writer.Write(deathPosition);
			writer.Write(roundStarted);
			writer.Write(IsVerified);
			writer.Write(SteamId);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)ntfUnit);
		}
		if ((base.syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)curClass);
		}
		if ((base.syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(deathPosition);
		}
		if ((base.syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(roundStarted);
		}
		if ((base.syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(IsVerified);
		}
		if ((base.syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(SteamId);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			ntfUnit = (int)reader.ReadPackedUInt32();
			curClass = (int)reader.ReadPackedUInt32();
			deathPosition = reader.ReadVector3();
			roundStarted = reader.ReadBoolean();
			IsVerified = reader.ReadBoolean();
			SteamId = reader.ReadString();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetUnit((int)reader.ReadPackedUInt32());
		}
		if (((uint)num & 2u) != 0)
		{
			SetClassID((int)reader.ReadPackedUInt32());
		}
		if (((uint)num & 4u) != 0)
		{
			SyncDeathPos(reader.ReadVector3());
		}
		if (((uint)num & 8u) != 0)
		{
			SetRoundStart(reader.ReadBoolean());
		}
		if (((uint)num & 0x10u) != 0)
		{
			SetVerification(reader.ReadBoolean());
		}
		if (((uint)num & 0x20u) != 0)
		{
			SetSteamId(reader.ReadString());
		}
	}
}
