using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Cryptography;
using GameConsole;
using LlockhamIndustries.ExtensionMethods;
using UnityEngine;
using UnityEngine.Networking;

namespace RemoteAdmin
{
	public class QueryProcessor : NetworkBehaviour
	{
		private static int _idIterator;

		internal int PasswordTries;

		internal int SignaturesCounter;

		private int _signaturesCounter;

		private ServerRoles _roles;

		internal byte[] Key;

		internal byte[] Salt;

		internal byte[] ClientSalt;

		private byte[] _key;

		private byte[] _salt;

		private byte[] _clientSalt;

		public static QueryProcessor Localplayer;

		private float _lastPlayerlistRequest;

		public const int HashIterations = 250;

		private string _toBan;

		private string _toBanNick;

		private string _toBanSteamID;

		private int _toBanType;

		public static bool Lockdown;

		[SyncVar(hook = "SetId")]
		public int PlayerId;

		[SyncVar(hook = "SetOverridePasswordEnabled")]
		public bool OverridePasswordEnabled;

		[SyncVar]
		public bool GameplayData;

		private string ipAddress;

		private NetworkConnection conns;

		private static int kCmdCmdRequestSalt;

		private static int kTargetRpcTargetSaltGenerated;

		private static int kCmdCmdSendPassword;

		private static int kTargetRpcTargetReplyPassword;

		private static int kTargetRpcTargetReply;

		private static int kCmdCmdSendQuery;

		private static int kTargetRpcTargetStaffPlayerListResponse;

		private static int kTargetRpcTargetStaffAuthTokenResponse;

		public int NetworkPlayerId
		{
			get
			{
				return PlayerId;
			}
			[param: In]
			set
			{
				ref int playerId = ref PlayerId;
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					SetId(value);
					base.syncVarHookGuard = false;
				}
				SetSyncVar(value, ref playerId, 1u);
			}
		}

		public bool NetworkOverridePasswordEnabled
		{
			get
			{
				return OverridePasswordEnabled;
			}
			[param: In]
			set
			{
				ref bool overridePasswordEnabled = ref OverridePasswordEnabled;
				if (NetworkServer.localClientActive && !base.syncVarHookGuard)
				{
					base.syncVarHookGuard = true;
					SetOverridePasswordEnabled(value);
					base.syncVarHookGuard = false;
				}
				SetSyncVar(value, ref overridePasswordEnabled, 2u);
			}
		}

		public bool NetworkGameplayData
		{
			get
			{
				return GameplayData;
			}
			[param: In]
			set
			{
				SetSyncVar(value, ref GameplayData, 4u);
			}
		}

		private void SetOverridePasswordEnabled(bool b)
		{
			NetworkOverridePasswordEnabled = b;
		}

		private void SetId(int id)
		{
			NetworkPlayerId = id;
		}

		private void Start()
		{
			_roles = GetComponent<ServerRoles>();
			SignaturesCounter = 0;
			_signaturesCounter = 0;
			if (NetworkServer.active)
			{
				conns = base.connectionToClient;
				ipAddress = conns.address;
				NetworkOverridePasswordEnabled = ServerStatic.PermissionsHandler.OverrideEnabled;
				_idIterator++;
				SetId(_idIterator);
			}
			if (base.isLocalPlayer)
			{
				Localplayer = this;
				InvokeRepeating("RefreshPlayerList", 2f, 2f);
			}
		}

		public void RefreshPlayerList()
		{
			if (base.isLocalPlayer && _roles.RemoteAdmin && _lastPlayerlistRequest > 0.2f)
			{
				_lastPlayerlistRequest = 0f;
				CmdSendQuery("REQUEST_DATA PLAYER_LIST SILENT");
			}
		}

		public static void StaticRefreshPlayerList()
		{
			if (Localplayer != null)
			{
				Localplayer.RefreshPlayerList();
			}
		}

		private void Update()
		{
			if (base.isLocalPlayer && _lastPlayerlistRequest < 1f)
			{
				_lastPlayerlistRequest += Time.deltaTime;
			}
		}

		[Command(channel = 2)]
		public void CmdRequestSalt(byte[] clSalt)
		{
			if (!ServerStatic.PermissionsHandler.OverrideEnabled)
			{
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Password authentication is disabled on this server!", "magenta");
				return;
			}
			if (_clientSalt == null)
			{
				if (clSalt == null)
				{
					GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Please generate and send your salt!", "red");
					return;
				}
				if (clSalt.Length < 16)
				{
					GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Generated salt is too short. Please generate longer salt and try again!", "red");
					return;
				}
				_clientSalt = clSalt;
				if (_key == null && _salt != null)
				{
					_key = ServerStatic.PermissionsHandler.DerivePassword(_salt, _clientSalt);
				}
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your salt " + Convert.ToBase64String(clSalt) + " has been accepted by the server.", "cyan");
			}
			if (_salt != null)
			{
				CallTargetSaltGenerated(base.connectionToClient, _salt);
				return;
			}
			RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider();
			byte[] array = new byte[16];
			randomNumberGenerator.GetBytes(array);
			_salt = array;
			_key = ServerStatic.PermissionsHandler.DerivePassword(_salt, _clientSalt);
			CallTargetSaltGenerated(base.connectionToClient, _salt);
		}

		[TargetRpc(channel = 2)]
		public void TargetSaltGenerated(NetworkConnection conn, byte[] salt)
		{
			if (salt.Length < 16)
			{
				GameConsole.Console.singleton.AddLog(string.Concat("Rejected salt ", salt, " because it's too short!"), Color.red);
				return;
			}
			GameConsole.Console.singleton.AddLog("Obtained server's salt " + Convert.ToBase64String(salt) + " from server.", Color.cyan);
			Salt = salt;
		}

		[Command(channel = 15)]
		public void CmdSendPassword(byte[] authSignature)
		{
			bool b = false;
			if (_roles.RemoteAdmin)
			{
				b = true;
				PasswordTries = 0;
			}
			else
			{
				if (_salt == null || _clientSalt == null)
				{
					GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Can't verify your remote admin password - please generate salt first!", "red");
					return;
				}
				if (_clientSalt.Length < 16)
				{
					GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Generated salt is too short. Please rejoin the server and try again!", "red");
					return;
				}
				if (VerifyHmacSignature("Login", -1, authSignature, false))
				{
					PasswordTries = 0;
					UserGroup overrideGroup = ServerStatic.PermissionsHandler.OverrideGroup;
					if (overrideGroup != null)
					{
						ServerConsole.AddLog("Assigned group " + overrideGroup.BadgeText + " to " + GetComponent<NicknameSync>().myNick + " - override password.");
						_roles.SetGroup(overrideGroup, true);
						b = true;
					}
					else
					{
						GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Non-existing group is assigned for override password!", "red");
					}
				}
				else
				{
					PasswordTries++;
					ServerConsole.AddLog("Rejected override password sent by " + GetComponent<NicknameSync>().myNick + ".");
				}
			}
			if (PasswordTries >= 3)
			{
				ServerConsole.Disconnect(base.connectionToClient, "You have been kicked for too many Remote Admin login attempts.");
			}
			else
			{
				CallTargetReplyPassword(base.connectionToClient, b);
			}
		}

		[TargetRpc(channel = 14)]
		private void TargetReplyPassword(NetworkConnection conn, bool b)
		{
			UnityEngine.Object.FindObjectOfType<UIController>().awaitingLogin = (b ? 2 : 0);
		}

		[TargetRpc(channel = 15)]
		private void TargetReply(NetworkConnection conn, string content, bool isSuccess, bool logInConsole, string overrideDisplay)
		{
			string text = content.Remove(content.IndexOf("#", StringComparison.Ordinal));
			content = content.Remove(0, content.IndexOf("#", StringComparison.Ordinal) + 1);
			if (logInConsole)
			{
				TextBasedRemoteAdmin.AddLog(((!isSuccess) ? "<color=orange>" : "<color=white>") + "(" + text + ") " + content + "</color>");
			}
			if (overrideDisplay == string.Empty)
			{
				switch (text)
				{
				case "HELP":
					Application.OpenURL("https://docs.google.com/document/d/1nj6fNULwc7Kx3fNnt5Gh2YTIqg8jS5d_Z0fDXpTimAw/edit?usp=sharing");
					return;
				case "REQUEST_DATA:PLAYER_LIST":
					PlayerRequest.singleton.ResponsePlayerList(content, isSuccess, GameplayData);
					return;
				case "REQUEST_DATA:PLAYER":
					PlayerRequest.singleton.ResponsePlayerSpecific(content, isSuccess);
					return;
				case "LOGOUT":
				{
					UIController uIController = UnityEngine.Object.FindObjectOfType<UIController>();
					if (uIController.root_root.activeSelf)
					{
						uIController.ChangeConsoleStage();
					}
					uIController.loggedIn = false;
					return;
				}
				}
				int num = 0;
				SubmenuSelector.SubMenu[] menus = SubmenuSelector.singleton.menus;
				foreach (SubmenuSelector.SubMenu subMenu in menus)
				{
					if (subMenu.commandTemplate.StartsWith(text))
					{
						DisplayDataOnScreen.singleton.Show(num, ((!isSuccess) ? "<color=red>" : "<color=green>") + content + "</color>");
					}
					num++;
				}
				return;
			}
			int num2 = 0;
			SubmenuSelector.SubMenu[] menus2 = SubmenuSelector.singleton.menus;
			foreach (SubmenuSelector.SubMenu subMenu2 in menus2)
			{
				if (subMenu2.commandTemplate == overrideDisplay)
				{
					DisplayDataOnScreen.singleton.Show(num2, ((!isSuccess) ? "<color=red>" : "<color=green>") + content + "</color>");
				}
				num2++;
			}
		}

		[Client]
		public void CmdSendQuery(string query)
		{
			if (!NetworkClient.active)
			{
				Debug.LogWarning("[Client] function 'System.Void RemoteAdmin.QueryProcessor::CmdSendQuery(System.String)' called on server");
				return;
			}
			SignaturesCounter++;
			CallCmdSendQuery(query, SignaturesCounter, SignRequest(query));
		}

		[Command(channel = 15)]
		public void CmdSendQuery(string query, int counter, byte[] signature)
		{
			if (_roles.RemoteAdmin)
			{
				if (VerifyRequestSignature(query, counter, signature))
				{
					ProcessQuery(query);
				}
				else
				{
					GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Signature verification of request \"" + query + "\" failed!", "magenta");
				}
			}
			else
			{
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "You are not logged in to remote admin panel!", "red");
			}
		}

		[ServerCallback]
		private void ProcessQuery(string q)
		{
			if (!NetworkServer.active)
			{
				return;
			}
			if (!q.Contains("SILENT"))
			{
				TextBasedRemoteAdmin.AddLog("<color=purple>(USER-INPUT) " + q + "</color>");
			}
			string[] array = q.Split(' ');
			string myNick = GetComponent<NicknameSync>().myNick;
			int failures;
			int successes;
			string error;
			bool replySent;
			switch (array[0].ToUpper())
			{
			case "HELLO":
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Hello World!", true, true, string.Empty);
				break;
			case "HELP":
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#This should be useful!", true, true, string.Empty);
				break;
			case "BAN":
				if (array.Length >= 3)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " ran the ban command (duration: " + array[2] + " min) on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					StandardizedQueryModel1(array[0], array[1], array[2], out failures, out successes, out error, out replySent);
					if (replySent)
					{
						break;
					}
					if (failures == 0)
					{
						string text8 = "Banned";
						int result;
						if (int.TryParse(array[2], out result))
						{
							text8 = ((result <= 0) ? "Kicked" : "Banned");
						}
						CallTargetReply(base.connectionToClient, array[0] + "#Done! " + text8 + " " + successes + " player(s)!", true, true, string.Empty);
					}
					else
					{
						CallTargetReply(base.connectionToClient, array[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, false, true, string.Empty);
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "SETGROUP":
				if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.SetGroup))
				{
					break;
				}
				if (array.Length >= 3)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Permissions, myNick + " ran the setgroup command (new group: " + array[2] + " min) on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					StandardizedQueryModel1(array[0], array[1], array[2], out failures, out successes, out error, out replySent);
					if (!replySent)
					{
						if (failures == 0)
						{
							CallTargetReply(base.connectionToClient, array[0] + "#Done! The request affedted " + successes + " player(s)!", true, true, string.Empty);
						}
						else
						{
							CallTargetReply(base.connectionToClient, array[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, false, true, string.Empty);
						}
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "GROUPS":
			{
				string text5 = "Groups defined on this server:";
				Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
				ServerRoles.NamedColor[] namedColors = GetComponent<ServerRoles>().NamedColors;
				foreach (KeyValuePair<string, UserGroup> permentry in allGroups)
				{
					try
					{
						string text3 = text5;
						text5 = text3 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - <color=#" + namedColors.FirstOrDefault((ServerRoles.NamedColor x) => x.Name == permentry.Value.BadgeColor).ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
					}
					catch
					{
						string text3 = text5;
						text5 = text3 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
					{
						text5 += " K";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
					{
						text5 += " B1";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
					{
						text5 += " B2";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
					{
						text5 += " FSE";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
					{
						text5 += " FSP";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
					{
						text5 += " FC";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
					{
						text5 += " G";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
					{
						text5 += " EW";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
					{
						text5 += " ERS";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
					{
						text5 += " ERD";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
					{
						text5 += " SG";
					}
					if (ServerStatic.PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
					{
						text5 += " FM";
					}
				}
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#" + text5, true, true, string.Empty);
				break;
			}
		    case "HP":
		    case "SETHP":
			if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.ForceclassToSpectator, false))
			{
				break;
			}
			if (array.Length >= 3)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " ran the sethp command on " + array[1] + " players (HP: " + array[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1(array[0], array[1], array[2], out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						TargetReply(base.connectionToClient, array[0] + "#Done! The request affected " + successes + " player(s)!", isSuccess: true, logInConsole: true, string.Empty);
					}
					else
					{
						TargetReply(base.connectionToClient, array[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, isSuccess: false, logInConsole: true, string.Empty);
					}
				}
			}
			else
			{
				TargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", isSuccess: false, logInConsole: true, string.Empty);
			}
			break;
			case "BRING":
			if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.ForceclassToSpectator, false))
			{
				break;
			}
				if (GetComponent<CharacterClassManager>().curClass == 2 || GetComponent<CharacterClassManager>().curClass < 0)
				{
					TargetReply(base.connectionToClient, "BRING#Command disabled when you are spectator!", isSuccess: false, logInConsole: true, "AdminTools");
					break;
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " ran the bring command on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				StandardizedQueryModel1("BRING", array[1], string.Empty, out failures, out successes, out error, out replySent);
				if (!replySent)
				{
					if (failures == 0)
					{
						TargetReply(base.connectionToClient, "BRING#Done! The request affected " + successes + " player(s)!", isSuccess: true, logInConsole: true, "AdminTools");
						break;
					}
					TargetReply(base.connectionToClient, "BRING#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, isSuccess: false, logInConsole: true, "AdminTools");
				}
			break;
			case "FORCECLASS":
				if (array.Length >= 3)
				{
					if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.ForceclassWithoutRestrictions, false) || (array[2] == "2" && CheckPermissions(array[0].ToUpper(), PlayerPermissions.ForceclassToSpectator, false)) || (array[1] == PlayerId + "." && CheckPermissions(array[0].ToUpper(), PlayerPermissions.ForceclassSelf, false)))
					{
						ServerLogs.AddLog(ServerLogs.Modules.ClassChange, myNick + " ran the forceclass command (ID:" + array[2] + ") on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
						StandardizedQueryModel1(array[0], array[1], array[2], out failures, out successes, out error, out replySent);
						if (!replySent)
						{
							if (failures == 0)
							{
								CallTargetReply(base.connectionToClient, array[0] + "#Done! The request affected " + successes + " player(s)!", true, true, string.Empty);
							}
							else
							{
								CallTargetReply(base.connectionToClient, array[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, false, true, string.Empty);
							}
						}
					}
					else
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#You don't have permissions to execute this command.", false, true, string.Empty);
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "OVR":
			case "OVERWATCH":
				if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.Overwatch))
				{
					break;
				}
				if (array.Length >= 2)
				{
					if (array.Length == 2)
					{
						array = array.Resize(3);
						array[2] = string.Empty;
					}
					ServerLogs.AddLog(ServerLogs.Modules.ClassChange, myNick + " ran the overwatch command (new status: " + ((!(array[2] == string.Empty)) ? array[2] : "TOGGLE") + ") on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					StandardizedQueryModel1("OVERWATCH", array[1], array[2], out failures, out successes, out error, out replySent);
					if (!replySent)
					{
						if (failures == 0)
						{
							CallTargetReply(base.connectionToClient, "OVERWATCH#Done! The request affected " + successes + " player(s)!", true, true, "AdminTools");
							break;
						}
						CallTargetReply(base.connectionToClient, "OVERWATCH#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, false, true, "AdminTools");
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, "AdminTools");
				}
				break;
			case "LD":
			case "LOCKDOWN":
			{
				if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					break;
				}
				if (!Lockdown)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " enabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					Door[] array3 = UnityEngine.Object.FindObjectsOfType<Door>();
					foreach (Door door in array3)
					{
						if (!door.locked)
						{
							door.lockdown = true;
							door.UpdateLock();
						}
					}
					Lockdown = true;
					CallTargetReply(base.connectionToClient, array[0] + "#Lockdown enabled!", true, true, string.Empty);
					break;
				}
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " disabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
				Door[] array4 = UnityEngine.Object.FindObjectsOfType<Door>();
				foreach (Door door2 in array4)
				{
					if (door2.lockdown)
					{
						door2.lockdown = false;
						door2.UpdateLock();
					}
				}
				Lockdown = false;
				CallTargetReply(base.connectionToClient, array[0] + "#Lockdown disabled!", true, true, string.Empty);
				break;
			}
			case "O":
			case "OPEN":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					if (array.Length != 2)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Syntax of this program: " + array[0].ToUpper() + " DoorName", false, true, string.Empty);
					}
					else
					{
						ProcessDoorQuery("OPEN", array[1]);
					}
				}
				break;
			case "C":
			case "CLOSE":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					if (array.Length != 2)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Syntax of this program: " + array[0].ToUpper() + " DoorName", false, true, string.Empty);
					}
					else
					{
						ProcessDoorQuery("CLOSE", array[1]);
					}
				}
				break;
			case "L":
			case "LOCK":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					if (array.Length != 2)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Syntax of this program: " + array[0].ToUpper() + " DoorName", false, true, string.Empty);
					}
					else
					{
						ProcessDoorQuery("LOCK", array[1]);
					}
				}
				break;
			case "UL":
			case "UNLOCK":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					if (array.Length != 2)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Syntax of this program: " + array[0].ToUpper() + " DoorName", false, true, string.Empty);
					}
					else
					{
						ProcessDoorQuery("UNLOCK", array[1]);
					}
				}
				break;
			case "DESTROY":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					if (array.Length != 2)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Syntax of this program: " + array[0].ToUpper() + " DoorName", false, true, string.Empty);
					}
					else
					{
						ProcessDoorQuery("DESTROY", array[1]);
					}
				}
				break;
			case "DL":
			case "DOORS":
			case "DOORLIST":
				if (CheckPermissions(array[0].ToUpper(), PlayerPermissions.FacilityManagement))
				{
					string text4 = "List of named doors in the facility:\n";
					Door[] source = UnityEngine.Object.FindObjectsOfType<Door>();
					List<string> list = (from item in source
						where !string.IsNullOrEmpty(item.DoorName)
						select item.DoorName + " - " + ((!item.isOpen) ? "<color=orange>CLOSED</color>" : "<color=green>OPENED</color>") + ((!item.locked) ? string.Empty : " <color=red>[LOCKED]</color>") + ((!string.IsNullOrEmpty(item.permissionLevel)) ? " <color=blue>[CARD REQUIRED]</color>" : string.Empty)).ToList();
					list.Sort();
					text4 += list.Aggregate((string current, string adding) => current + "\n" + adding);
					CallTargetReply(base.connectionToClient, array[0] + "#" + text4, true, true, string.Empty);
				}
				break;
			case "GIVE":
				if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.GivingItems))
				{
					break;
				}
				if (array.Length >= 3)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " ran the give command (ID:" + array[2] + ") on " + array[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					StandardizedQueryModel1(array[0], array[1], array[2], out failures, out successes, out error, out replySent);
					if (!replySent)
					{
						if (failures == 0)
						{
							CallTargetReply(base.connectionToClient, array[0] + "#Done! The request affected " + successes + " player(s)!", true, true, string.Empty);
						}
						else
						{
							CallTargetReply(base.connectionToClient, array[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, false, true, string.Empty);
						}
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "REQUEST_DATA":
				if (array.Length >= 2)
				{
					if (array[1].ToUpper() == "PLAYER_LIST")
					{
						try
						{
							string text6 = "\n";
							NetworkGameplayData = CheckPermissions(array[0].ToUpper(), PlayerPermissions.GameplayData, false);
							foreach (NetworkConnection connection in NetworkServer.connections)
							{
								GameObject gameObject3 = GameConsole.Console.FindConnectedRoot(connection);
								if (gameObject3 != null)
								{
									if (!q.ToUpper().Contains("STAFF"))
									{
										string text3 = text6;
										text6 = text3 + "(" + gameObject3.GetComponent<QueryProcessor>().PlayerId + ") " + gameObject3.GetComponent<NicknameSync>().myNick.Replace("\n", string.Empty).Replace("<", string.Empty).Replace(">", string.Empty) + ((!gameObject3.GetComponent<ServerRoles>().OverwatchEnabled) ? string.Empty : "<OVRM>");
									}
									else
									{
										string text3 = text6;
										text6 = text3 + gameObject3.GetComponent<QueryProcessor>().PlayerId + ";" + gameObject3.GetComponent<NicknameSync>().myNick;
									}
								}
								text6 += "\n";
							}
							if (!q.ToUpper().Contains("STAFF"))
							{
								CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER_LIST#" + text6, true, array.Length < 3 || array[2].ToUpper() != "SILENT", string.Empty);
							}
							else
							{
								CallTargetStaffPlayerListResponse(base.connectionToClient, text6);
							}
							break;
						}
						catch (Exception ex)
						{
							CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, false, true, string.Empty);
							throw;
						}
					}
					if (array[1].ToUpper() == "PLAYER")
					{
						if (array.Length >= 3)
						{
							try
							{
								GameObject gameObject4 = null;
								NetworkConnection networkConnection = null;
								foreach (NetworkConnection connection2 in NetworkServer.connections)
								{
									GameObject gameObject5 = GameConsole.Console.FindConnectedRoot(connection2);
									if (array[2].Contains("."))
									{
										array[2] = array[2].Split('.')[0];
									}
									if (gameObject5 != null && gameObject5.GetComponent<QueryProcessor>().PlayerId.ToString() == array[2])
									{
										gameObject4 = gameObject5;
										networkConnection = connection2;
									}
								}
								if (gameObject4 == null)
								{
									CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#Player with id " + ((!string.IsNullOrEmpty(array[2])) ? array[2] : "[null]") + " not found!", false, true, string.Empty);
								}
								else
								{
									bool flag2 = CheckPermissions(array[0].ToUpper(), PlayerPermissions.GameplayData, false);
									CharacterClassManager component2 = gameObject4.GetComponent<CharacterClassManager>();
									string empty = string.Empty;
									empty = empty + "Nickname: " + gameObject4.GetComponent<NicknameSync>().myNick;
									empty = empty + "\nPlayer ID: " + gameObject4.GetComponent<QueryProcessor>().PlayerId;
									empty = empty + "\nIP: " + ((networkConnection == null) ? "null" : networkConnection.address);
									empty = empty + "\nSteam ID: " + ((!string.IsNullOrEmpty(component2.SteamId)) ? component2.SteamId : "(none)");
									empty = empty + "\nServer role: " + gameObject4.GetComponent<ServerRoles>().GetColoredRoleString();
									if (gameObject4.GetComponent<ServerRoles>().OverwatchEnabled)
									{
										empty += "\n<color=#008080>OVERWATCH MODE</color>";
									}
									else
									{
										empty = empty + "\nClass: " + ((!flag2) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : ((component2.curClass < 0 || component2.curClass >= component2.klasy.Length) ? "None" : component2.klasy[component2.curClass].fullName));
										empty = empty + "\nHP: " + ((!flag2) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : gameObject4.GetComponent<PlayerStats>().health.ToString());
										if (!flag2)
										{
											empty += "\n<color=#D4AF37>* GameplayData permission required</color>";
										}
									}
									CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#" + empty, true, true, string.Empty);
								}
								break;
							}
							catch (Exception ex2)
							{
								CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source, false, true, string.Empty);
								throw;
							}
						}
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, string.Empty);
					}
					else if (array[1].ToUpper() == "AUTH")
					{
						if (!GetComponent<ServerRoles>().Staff && !CheckPermissions(array[0].ToUpper(), PlayerPermissions.LongTermBanning))
						{
							break;
						}
						if (array.Length >= 3)
						{
							try
							{
								GameObject gameObject6 = null;
								foreach (NetworkConnection connection3 in NetworkServer.connections)
								{
									GameObject gameObject7 = GameConsole.Console.FindConnectedRoot(connection3);
									if (array[2].Contains("."))
									{
										array[2] = array[2].Split('.')[0];
									}
									if (gameObject7 != null && gameObject7.GetComponent<QueryProcessor>().PlayerId.ToString() == array[2])
									{
										gameObject6 = gameObject7;
									}
								}
								if (gameObject6 == null)
								{
									CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#Player with id " + ((!string.IsNullOrEmpty(array[2])) ? array[2] : "[null]") + " not found!", false, true, string.Empty);
								}
								else if (!q.ToUpper().Contains("STAFF"))
								{
									string text7 = "Authentication token of player " + gameObject6.GetComponent<NicknameSync>().myNick + "(" + gameObject6.GetComponent<QueryProcessor>().PlayerId + "):\n" + gameObject6.GetComponent<CharacterClassManager>().AuthToken;
									CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#" + text7, true, true, string.Empty);
								}
								else
								{
									CallTargetStaffAuthTokenResponse(base.connectionToClient, gameObject6.GetComponent<CharacterClassManager>().AuthToken);
								}
								break;
							}
							catch (Exception ex3)
							{
								CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex3.Message + "\nStackTrace: " + ex3.StackTrace + "\nAt: " + ex3.Source, false, true, string.Empty);
								throw;
							}
						}
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", false, true, string.Empty);
					}
					else
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", false, true, string.Empty);
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "CONTACT":
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email", string.Empty), false, true, string.Empty);
				break;
			case "LOGOUT":
				if (_roles.RemoteAdminMode == ServerRoles.AccessMode.PasswordOverride)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " logged out from the Remote Admin.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
					_roles.NetworkRemoteAdmin = false;
					if (!_roles.GlobalSet)
					{
						_roles.SetText(string.Empty);
						_roles.SetColor("default");
					}
					PasswordTries = 0;
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Logged out!", true, true, string.Empty);
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#You can't log out, when you are not using override password!", true, true, string.Empty);
				}
				break;
			case "SERVER_EVENT":
				if (array.Length >= 2)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Administrative, myNick + " forced a server event: " + array[1].ToUpper(), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
					GameObject gameObject = GameObject.Find("Host");
					bool flag = true;
					switch (array[1].ToUpper())
					{
					case "FORCE_CI_RESPAWN":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.RespawnEvents))
						{
							return;
						}
						gameObject.GetComponent<MTFRespawn>().nextWaveIsCI = true;
						gameObject.GetComponent<MTFRespawn>().timeToNextRespawn = 0.1f;
						break;
					case "FORCE_MTF_RESPAWN":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.RespawnEvents))
						{
							return;
						}
						gameObject.GetComponent<MTFRespawn>().nextWaveIsCI = false;
						gameObject.GetComponent<MTFRespawn>().timeToNextRespawn = 0.1f;
						break;
					case "DETONATION_START":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.WarheadEvents))
						{
							return;
						}
						gameObject.GetComponent<AlphaWarheadController>().InstantPrepare();
						gameObject.GetComponent<AlphaWarheadController>().StartDetonation();
						break;
					case "DETONATION_CANCEL":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.WarheadEvents))
						{
							return;
						}
						gameObject.GetComponent<AlphaWarheadController>().CancelDetonation(null);
						break;
					case "DETONATION_INSTANT":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.WarheadEvents))
						{
							return;
						}
						gameObject.GetComponent<AlphaWarheadController>().InstantPrepare();
						gameObject.GetComponent<AlphaWarheadController>().StartDetonation();
						gameObject.GetComponent<AlphaWarheadController>().NetworktimeToDetonation = 5f;
						break;
					case "TERMINATE_UNCONN":
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.RoundEvents))
						{
							return;
						}
						foreach (NetworkConnection connection4 in NetworkServer.connections)
						{
							if (GameConsole.Console.FindConnectedRoot(connection4) == null)
							{
								connection4.Disconnect();
								connection4.Dispose();
							}
						}
						break;
					case "ROUND_RESTART":
					{
						if (!CheckPermissions(array[0].ToUpper(), PlayerPermissions.RoundEvents))
						{
							return;
						}
						GameObject[] array2 = GameObject.FindGameObjectsWithTag("Player");
						foreach (GameObject gameObject2 in array2)
						{
							PlayerStats component = gameObject2.GetComponent<PlayerStats>();
							if (component.isLocalPlayer && component.isServer)
							{
								component.Roundrestart();
							}
						}
						break;
					}
					default:
						flag = false;
						break;
					}
					if (flag)
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Started event: " + array[1].ToUpper(), true, true, string.Empty);
					}
					else
					{
						CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Incorrect event! (Doesn't exist)", false, true, string.Empty);
					}
				}
				else
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", false, true, string.Empty);
				}
				break;
			case "HIDETAG":
				_roles.SetText(string.Empty);
				_roles.SetColor("default");
				_roles.SetBadgeUpdate(string.Empty);
				_roles.NetworkGlobalSet = false;
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Tag hidden!", true, true, string.Empty);
				PasswordTries = 0;
				break;
			case "SHOWTAG":
				_roles.RefreshPermissions();
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Local tag refreshed!", true, true, string.Empty);
				break;
			case "GTAG":
			case "GLOBALTAG":
				if (string.IsNullOrEmpty(_roles.PrevBadge))
				{
					CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#You don't have global tag.", false, true, string.Empty);
					break;
				}
				_roles.SetBadgeUpdate(_roles.PrevBadge);
				_roles.NetworkGlobalSet = true;
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Global tag refreshed!", true, true, string.Empty);
				break;
			case "SRVCFG":
			{
				YamlConfig serverConfig = ConfigFile.ServerConfig;
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#Server name: " + serverConfig.GetString("server_name", string.Empty) + "\nServer IP: " + serverConfig.GetString("server_ip", string.Empty) + "\nCurrent Server IP:: " + CustomNetworkManager.Ip + "\nServer pastebin ID: " + serverConfig.GetString("serverinfo_pastebin_id", string.Empty) + "\nServer max players: " + serverConfig.GetInt("max_players") + "\nOnline mode: " + serverConfig.GetBool("online_mode") + "\nIP banning: " + serverConfig.GetBool("ip_banning") + "\nWhitelist: " + serverConfig.GetBool("enable_whitelist") + "\nQuery status: " + serverConfig.GetBool("enable_query") + " with port shift " + serverConfig.GetInt("query_port_shift") + "\nFriendly fire: " + serverConfig.GetBool("friendly_fire") + "\nMap seed: " + serverConfig.GetInt("map_seed"), true, true, string.Empty);
				break;
			}
			case "PERM":
			{
				int permissions = GetComponent<ServerRoles>().Permissions;
				string text = "Your permissions:";
				List<string> allPermissions = ServerStatic.PermissionsHandler.GetAllPermissions();
				foreach (string item in allPermissions)
				{
					string text2 = ((!ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler.GetPermissionValue(item))) ? string.Empty : "*");
					string text3 = text;
					text = text3 + "\n" + item + text2 + " (" + ServerStatic.PermissionsHandler.GetPermissionValue(item) + "): " + ((!ServerStatic.PermissionsHandler.IsPermitted(permissions, item)) ? "NO" : "YES");
				}
				CallTargetReply(base.connectionToClient, array[0].ToUpper() + "#" + text, true, true, string.Empty);
				break;
			}
			default:
				CallTargetReply(base.connectionToClient, "SYSTEM#Unknown command!", false, true, string.Empty);
				break;
			}
		}

		private void ProcessDoorQuery(string command, string door)
		{
			if (!CheckPermissions(command.ToUpper(), PlayerPermissions.FacilityManagement))
			{
				return;
			}
			bool flag = false;
			door = door.ToUpper();
			int num = 0;
			switch (command)
			{
			case "OPEN":
				num = 1;
				break;
			case "LOCK":
				num = 2;
				break;
			case "UNLOCK":
				num = 3;
				break;
			case "DESTROY":
				num = 4;
				break;
			default:
				num = 0;
				break;
			}
			Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
			foreach (Door door2 in array)
			{
				if (!(door2.DoorName.ToUpper() != door) || !(door != "**") || (!(door != "!*") && string.IsNullOrEmpty(door2.DoorName)) || (!(door != "*") && !string.IsNullOrEmpty(door2.DoorName)))
				{
					switch (num)
					{
					case 0:
						door2.SetState(false);
						door2.CallRpcDoSound();
						break;
					case 1:
						door2.SetState(true);
						door2.CallRpcDoSound();
						break;
					case 2:
						door2.commandlock = true;
						door2.UpdateLock();
						break;
					case 3:
						door2.commandlock = false;
						door2.UpdateLock();
						break;
					case 4:
						door2.DestroyDoor(true);
						break;
					}
					flag = true;
				}
			}
			CallTargetReply(base.connectionToClient, command + "#" + ((!flag) ? ("Can't find door " + door + ".") : ("Door " + door + " " + command.ToLower() + "ed.")), flag, true, "DoorsManagement");
			if (flag)
			{
				ServerLogs.AddLog(ServerLogs.Modules.Administrative, GetComponent<NicknameSync>().myNick + " " + command.ToLower() + "ed door " + door + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
			}
		}

		private void StandardizedQueryModel1(string programName, string playerIds, string xValue, out int failures, out int successes, out string error, out bool replySent)
		{
			error = string.Empty;
			failures = 0;
			successes = 0;
			replySent = false;
			programName = programName.ToUpper();
			int result;
			if (int.TryParse(xValue, out result) || programName == "SETGROUP" || programName == "OVERWATCH")
			{
				List<int> list = new List<int>();
				try
				{
					string[] source = playerIds.Split('.');
					list.AddRange(source.Where((string item) => !string.IsNullOrEmpty(item)).Select(int.Parse));
					UserGroup userGroup = null;
					if (programName == "BAN")
					{
						replySent = true;
						if ((result <= 60 && !CheckPermissions(programName, PlayerPermissions.KickingAndShortTermBanning)) || (result > 60 && result <= 1440 && !CheckPermissions(programName, PlayerPermissions.BanningUpToDay)) || (result > 1440 && !CheckPermissions(programName, PlayerPermissions.LongTermBanning)))
						{
							return;
						}
						replySent = false;
					}
					else if (programName == "SETGROUP")
					{
						userGroup = ServerStatic.PermissionsHandler.GetGroup(xValue);
						if (userGroup == null)
						{
							replySent = true;
							CallTargetReply(base.connectionToClient, programName + "#Requested group doesn't exist!", false, true, string.Empty);
							return;
						}
					}
					foreach (int item in list)
					{
						try
						{
							GameObject[] players = PlayerManager.singleton.players;
							Vector3 position = GetComponent<PlyMovementSync>().position;
							foreach (GameObject gameObject in players)
							{
								if (item != gameObject.GetComponent<QueryProcessor>().PlayerId)
								{
									continue;
								}
								switch (programName)
								{
								case "BAN":
								{
									string text = "Error Code: ";
									error = text + GetComponent<BanPlayer>().BanUser(gameObject, result, string.Empty);
									if (error != text + "good")
									{
										failures++;
									}
									break;
								}
								case "FORCECLASS":
									GetComponent<CharacterClassManager>().SetPlayersClass(result, gameObject);
									break;
								case "HP":
								case "SETHP":
									gameObject.GetComponent<PlayerStats>().SetHPAmount(result);
									break;
								case "GIVE":
									gameObject.GetComponent<Inventory>().AddNewItem(result);
									break;
								case "BRING":
									if (gameObject.GetComponent<CharacterClassManager>().curClass == 2 || gameObject.GetComponent<CharacterClassManager>().curClass == -1)
									{
										failures++;
										continue;
									}
									gameObject.GetComponent<PlyMovementSync>().SetPosition(transform.position);
									break;
								case "SETGROUP":
									gameObject.GetComponent<ServerRoles>().SetGroup(userGroup, false, true);
									break;
								case "OVERWATCH":
									if (string.IsNullOrEmpty(xValue))
									{
										gameObject.GetComponent<ServerRoles>().CallCmdToggleOverwatch();
										break;
									}
									if (xValue == "1" || xValue.ToLower() == "true" || xValue.ToLower() == "enable" || xValue.ToLower() == "on")
									{
										gameObject.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(true);
										break;
									}
									if (xValue == "0" || xValue.ToLower() == "false" || xValue.ToLower() == "disable" || xValue.ToLower() == "off")
									{
										gameObject.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(false);
										break;
									}
									replySent = true;
									CallTargetReply(base.connectionToClient, programName + "#Invalid option " + xValue + " - leave null for toggle or use 1/0, true/false, enable/disable or on/off.", false, true, "AdminTools");
									return;
								}
								successes++;
							}
						}
						catch (Exception ex)
						{
							failures++;
							error = ex.Message + "\nStackTrace:\n" + ex.StackTrace;
						}
					}
					return;
				}
				catch (Exception ex2)
				{
					replySent = true;
					CallTargetReply(base.connectionToClient, programName + "#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source + "\nMost likely the PlayerId array was not in the correct format.", false, true, string.Empty);
					throw;
				}
			}
			replySent = true;
			CallTargetReply(base.connectionToClient, programName + "#The third parameter has to be an integer!", false, true, string.Empty);
		}

		internal bool CheckPermissions(string queryZero, PlayerPermissions perm, bool reply = true)
		{
			if (ServerStatic.PermissionsHandler.IsPermitted(GetComponent<ServerRoles>().Permissions, perm))
			{
				return true;
			}
			if (reply)
			{
				CallTargetReply(base.connectionToClient, queryZero + "#You don't have permissions to execute this command.\nMissing permission: " + perm, false, true, string.Empty);
			}
			return false;
		}

		public bool VerifyRequestSignature(string message, int counter, byte[] signature, bool validateCounter = true)
		{
			return (GetComponent<ServerRoles>().RemoteAdminMode != ServerRoles.AccessMode.PasswordOverride) ? VerifyEcdsaSignature(message, counter, signature, validateCounter) : VerifyHmacSignature(message, counter, signature, validateCounter);
		}

		public byte[] SignRequest(string message, int counter = -2)
		{
			return (GetComponent<ServerRoles>().RemoteAdminMode != ServerRoles.AccessMode.PasswordOverride) ? EcdsaSign(message, counter) : HmacSign(message, counter);
		}

		public bool VerifyHmacSignature(string message, int counter, byte[] signature, bool validateCounter = true)
		{
			if (counter <= _signaturesCounter)
			{
				if (validateCounter)
				{
					return false;
				}
			}
			else
			{
				_signaturesCounter = counter;
			}
			return OverridePasswordEnabled && Sha.Sha512Hmac(Utf8.GetBytes(message + ":[:COUNTER:]:" + counter), _key).SequenceEqual(signature);
		}

		public bool VerifyEcdsaSignature(string message, int counter, byte[] signature, bool validateCounter = true)
		{
			if (counter <= _signaturesCounter)
			{
				if (validateCounter)
				{
					return false;
				}
			}
			else
			{
				_signaturesCounter = counter;
			}
			return ECDSA.VerifyBytes(message + ":[:COUNTER:]:" + counter, signature, GetComponent<ServerRoles>().PublicKey);
		}

		public byte[] EcdsaSign(string message, int counter = -2)
		{
			if (counter == -2)
			{
				counter = SignaturesCounter;
			}
			return ECDSA.SignBytes(message + ":[:COUNTER:]:" + counter, GameConsole.Console.SessionKeys.Private);
		}

		public byte[] HmacSign(string message, int counter = -2)
		{
			if (counter == -2)
			{
				counter = SignaturesCounter;
			}
			return Sha.Sha512Hmac(Utf8.GetBytes(message + ":[:COUNTER:]:" + counter), Key);
		}

		public static byte[] DerivePassword(string password, byte[] serversalt, byte[] clientsalt)
		{
			byte[] salt = Sha.Sha512(Convert.ToBase64String(serversalt) + Convert.ToBase64String(clientsalt));
			return PBKDF2.Pbkdf2HashBytes(password, salt, 250, 512);
		}

		internal void RequestGlobalBan(string key, int keytype)
		{
			_toBan = key;
			_toBanType = keytype;
			CmdSendQuery("REQUEST_DATA PLAYER_LIST STAFF");
		}

		[TargetRpc(channel = 2)]
		internal void TargetStaffPlayerListResponse(NetworkConnection conn, string data)
		{
			if (string.IsNullOrEmpty(_toBan) || !string.IsNullOrEmpty(_toBanNick))
			{
				return;
			}
			string[] array = data.Split('\n');
			string text = "-1";
			string text2 = string.Empty;
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				try
				{
					int num = text3.IndexOf(";", StringComparison.Ordinal);
					if (num != -1)
					{
						string text4 = text3.Substring(0, num);
						string text5 = text3.Substring(num + 1);
						if (_toBanType == 0 && text4 == _toBan)
						{
							text = text4;
							text2 = text5;
							break;
						}
						if (_toBanType == 1 && string.Equals(text5, _toBan, StringComparison.CurrentCultureIgnoreCase))
						{
							text = text4;
							text2 = text5;
							break;
						}
					}
				}
				catch (Exception ex)
				{
					GameConsole.Console.singleton.AddLog("Error while processing online list for global banning: " + ex.GetType().FullName, Color.red);
				}
			}
			if (text == "-1")
			{
				GameConsole.Console.singleton.AddLog("Requested player can't be found!", Color.red);
			}
			else
			{
				GameConsole.Console.singleton.AddLog("Requesting authentication token of player " + text2 + "(" + text + ").", Color.cyan);
				_toBan = text;
				_toBanNick = text2;
				CmdSendQuery("REQUEST_DATA AUTH " + text + " STAFF");
			}
			_toBanType = 0;
		}

		[TargetRpc(channel = 2)]
		internal void TargetStaffAuthTokenResponse(NetworkConnection conn, string auth)
		{
			if (!string.IsNullOrEmpty(_toBan) && !string.IsNullOrEmpty(_toBanNick))
			{
				string text = CentralAuth.ValidateForGlobalBanning(auth, _toBanNick);
				if (text == "-1")
				{
					GameConsole.Console.singleton.AddLog("Aborting global banning....", Color.red);
					_toBan = string.Empty;
					_toBanNick = string.Empty;
					_toBanSteamID = string.Empty;
					_toBanType = 0;
				}
				else
				{
					_toBanSteamID = text;
					GameConsole.Console.singleton.AddLog("==== GLOBAL BANNING FINAL STEP ====", Color.cyan);
					GameConsole.Console.singleton.AddLog("Nick: " + _toBanNick, Color.cyan);
					GameConsole.Console.singleton.AddLog("ID on this server: " + _toBan, Color.cyan);
					GameConsole.Console.singleton.AddLog("SteamID64: " + _toBanSteamID, Color.cyan);
					GameConsole.Console.singleton.AddLog(string.Empty, Color.cyan);
					GameConsole.Console.singleton.AddLog("To confirm ban please execute \"CONFIRM\" command.", Color.cyan);
					GameConsole.Console.singleton.AddLog("==== GLOBAL BANNING FINAL STEP ====", Color.cyan);
					_toBanNick = string.Empty;
				}
			}
		}

		internal void ConfirmGlobalBanning()
		{
			StartCoroutine(IssueGlobalBan());
		}

		private IEnumerator IssueGlobalBan()
		{
			if (string.IsNullOrEmpty(_toBanSteamID))
			{
				GameConsole.Console.singleton.AddLog("You don't have any pending global ban request to confirm.", Color.yellow);
				yield break;
			}
			GameConsole.Console.singleton.AddLog("Issuing global ban for " + _toBanSteamID, Color.cyan);
			WWWForm form = new WWWForm();
			form.AddField("token", FileManager.ReadAllLines(FileManager.AppFolder + "StaffAPI.txt")[0]);
			form.AddField("action", "ban");
			form.AddField("steamid", _toBanSteamID);
			WWW www = new WWW(CentralServer.URL + "globalbanning.php", form);
			yield return www;
			if (!string.IsNullOrEmpty(www.error))
			{
				GameConsole.Console.singleton.AddLog("Error during global ban issuance: " + www.error, Color.red);
			}
			else if (www.text == "Banned")
			{
				GameConsole.Console.singleton.AddLog("Global ban issued, kicking player from server...", Color.cyan);
				CmdSendQuery("BAN " + _toBan + ". 0");
				GameConsole.Console.singleton.AddLog("==== GLOBAL BANNING CONFIRMATION ====", Color.green);
				GameConsole.Console.singleton.AddLog("ID on this server: " + _toBan, Color.green);
				GameConsole.Console.singleton.AddLog("SteamID64: " + _toBanSteamID, Color.green);
				GameConsole.Console.singleton.AddLog(string.Empty, Color.green);
				GameConsole.Console.singleton.AddLog("Player has been globally banned.", Color.green);
				GameConsole.Console.singleton.AddLog("Request to kick this player has been sent to game server.", Color.green);
				GameConsole.Console.singleton.AddLog("==== GLOBAL BANNING CONFIRMATION ====", Color.green);
				_toBanSteamID = string.Empty;
				_toBan = string.Empty;
				_toBanNick = string.Empty;
				_toBanType = 0;
			}
			else
			{
				GameConsole.Console.singleton.AddLog("Server error during global ban issuance: " + www.text, Color.red);
			}
			yield return null;
		}

		private void OnDestroy()
		{
			if (NetworkServer.active)
			{
				CustomNetworkManager.PlayerDisconnect(conns);
				if (ServerLogs.singleton != null)
				{
					ServerLogs.AddLog(ServerLogs.Modules.Networking, "Player ID " + PlayerId + " disconnected from IP " + ipAddress + " with SteamID " + ((!string.IsNullOrEmpty(GetComponent<CharacterClassManager>().SteamId)) ? GetComponent<CharacterClassManager>().SteamId : "(unavailable)") + " and nickname " + GetComponent<NicknameSync>().myNick + ". His last class was " + GetComponent<CharacterClassManager>().curClass, ServerLogs.ServerLogType.ConnectionUpdate);
				}
			}
		}

		private void UNetVersion()
		{
		}

		protected static void InvokeCmdCmdRequestSalt(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdRequestSalt called on client.");
			}
			else
			{
				((QueryProcessor)obj).CmdRequestSalt(reader.ReadBytesAndSize());
			}
		}

		protected static void InvokeCmdCmdSendPassword(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendPassword called on client.");
			}
			else
			{
				((QueryProcessor)obj).CmdSendPassword(reader.ReadBytesAndSize());
			}
		}

		protected static void InvokeCmdCmdSendQuery(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("Command CmdSendQuery called on client.");
			}
			else
			{
				((QueryProcessor)obj).CmdSendQuery(reader.ReadString(), (int)reader.ReadPackedUInt32(), reader.ReadBytesAndSize());
			}
		}

		public void CallCmdRequestSalt(byte[] clSalt)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdRequestSalt called on server.");
				return;
			}
			if (base.isServer)
			{
				CmdRequestSalt(clSalt);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)5);
			networkWriter.WritePackedUInt32((uint)kCmdCmdRequestSalt);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.WriteBytesFull(clSalt);
			SendCommandInternal(networkWriter, 2, "CmdRequestSalt");
		}

		public void CallCmdSendPassword(byte[] authSignature)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdSendPassword called on server.");
				return;
			}
			if (base.isServer)
			{
				CmdSendPassword(authSignature);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)5);
			networkWriter.WritePackedUInt32((uint)kCmdCmdSendPassword);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.WriteBytesFull(authSignature);
			SendCommandInternal(networkWriter, 15, "CmdSendPassword");
		}

		public void CallCmdSendQuery(string query, int counter, byte[] signature)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("Command function CmdSendQuery called on server.");
				return;
			}
			if (base.isServer)
			{
				CmdSendQuery(query, counter, signature);
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)5);
			networkWriter.WritePackedUInt32((uint)kCmdCmdSendQuery);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(query);
			networkWriter.WritePackedUInt32((uint)counter);
			networkWriter.WriteBytesFull(signature);
			SendCommandInternal(networkWriter, 15, "CmdSendQuery");
		}

		protected static void InvokeRpcTargetSaltGenerated(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetSaltGenerated called on server.");
			}
			else
			{
				((QueryProcessor)obj).TargetSaltGenerated(ClientScene.readyConnection, reader.ReadBytesAndSize());
			}
		}

		protected static void InvokeRpcTargetReplyPassword(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetReplyPassword called on server.");
			}
			else
			{
				((QueryProcessor)obj).TargetReplyPassword(ClientScene.readyConnection, reader.ReadBoolean());
			}
		}

		protected static void InvokeRpcTargetReply(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetReply called on server.");
			}
			else
			{
				((QueryProcessor)obj).TargetReply(ClientScene.readyConnection, reader.ReadString(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadString());
			}
		}

		protected static void InvokeRpcTargetStaffPlayerListResponse(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetStaffPlayerListResponse called on server.");
			}
			else
			{
				((QueryProcessor)obj).TargetStaffPlayerListResponse(ClientScene.readyConnection, reader.ReadString());
			}
		}

		protected static void InvokeRpcTargetStaffAuthTokenResponse(NetworkBehaviour obj, NetworkReader reader)
		{
			if (!NetworkClient.active)
			{
				Debug.LogError("TargetRPC TargetStaffAuthTokenResponse called on server.");
			}
			else
			{
				((QueryProcessor)obj).TargetStaffAuthTokenResponse(ClientScene.readyConnection, reader.ReadString());
			}
		}

		public void CallTargetSaltGenerated(NetworkConnection conn, byte[] salt)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetSaltGenerated called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSaltGenerated);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.WriteBytesFull(salt);
			SendTargetRPCInternal(conn, networkWriter, 2, "TargetSaltGenerated");
		}

		public void CallTargetReplyPassword(NetworkConnection conn, bool b)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetReplyPassword called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetReplyPassword);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(b);
			SendTargetRPCInternal(conn, networkWriter, 14, "TargetReplyPassword");
		}

		public void CallTargetReply(NetworkConnection conn, string content, bool isSuccess, bool logInConsole, string overrideDisplay)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetReply called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetReply);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(content);
			networkWriter.Write(isSuccess);
			networkWriter.Write(logInConsole);
			networkWriter.Write(overrideDisplay);
			SendTargetRPCInternal(conn, networkWriter, 15, "TargetReply");
		}

		public void CallTargetStaffPlayerListResponse(NetworkConnection conn, string data)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetStaffPlayerListResponse called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetStaffPlayerListResponse);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(data);
			SendTargetRPCInternal(conn, networkWriter, 2, "TargetStaffPlayerListResponse");
		}

		public void CallTargetStaffAuthTokenResponse(NetworkConnection conn, string auth)
		{
			if (!NetworkServer.active)
			{
				Debug.LogError("TargetRPC Function TargetStaffAuthTokenResponse called on client.");
				return;
			}
			NetworkWriter networkWriter = new NetworkWriter();
			networkWriter.Write((short)0);
			networkWriter.Write((short)2);
			networkWriter.WritePackedUInt32((uint)kTargetRpcTargetStaffAuthTokenResponse);
			networkWriter.Write(GetComponent<NetworkIdentity>().netId);
			networkWriter.Write(auth);
			SendTargetRPCInternal(conn, networkWriter, 2, "TargetStaffAuthTokenResponse");
		}

		static QueryProcessor()
		{
			kCmdCmdRequestSalt = -780447461;
			NetworkBehaviour.RegisterCommandDelegate(typeof(QueryProcessor), kCmdCmdRequestSalt, InvokeCmdCmdRequestSalt);
			kCmdCmdSendPassword = 1923616621;
			NetworkBehaviour.RegisterCommandDelegate(typeof(QueryProcessor), kCmdCmdSendPassword, InvokeCmdCmdSendPassword);
			kCmdCmdSendQuery = -1744616138;
			NetworkBehaviour.RegisterCommandDelegate(typeof(QueryProcessor), kCmdCmdSendQuery, InvokeCmdCmdSendQuery);
			kTargetRpcTargetSaltGenerated = -59915534;
			NetworkBehaviour.RegisterRpcDelegate(typeof(QueryProcessor), kTargetRpcTargetSaltGenerated, InvokeRpcTargetSaltGenerated);
			kTargetRpcTargetReplyPassword = -1238863682;
			NetworkBehaviour.RegisterRpcDelegate(typeof(QueryProcessor), kTargetRpcTargetReplyPassword, InvokeRpcTargetReplyPassword);
			kTargetRpcTargetReply = -489945853;
			NetworkBehaviour.RegisterRpcDelegate(typeof(QueryProcessor), kTargetRpcTargetReply, InvokeRpcTargetReply);
			kTargetRpcTargetStaffPlayerListResponse = -1316694695;
			NetworkBehaviour.RegisterRpcDelegate(typeof(QueryProcessor), kTargetRpcTargetStaffPlayerListResponse, InvokeRpcTargetStaffPlayerListResponse);
			kTargetRpcTargetStaffAuthTokenResponse = -454891367;
			NetworkBehaviour.RegisterRpcDelegate(typeof(QueryProcessor), kTargetRpcTargetStaffAuthTokenResponse, InvokeRpcTargetStaffAuthTokenResponse);
			NetworkCRC.RegisterBehaviour("QueryProcessor", 0);
		}

		public override bool OnSerialize(NetworkWriter writer, bool forceAll)
		{
			if (forceAll)
			{
				writer.WritePackedUInt32((uint)PlayerId);
				writer.Write(OverridePasswordEnabled);
				writer.Write(GameplayData);
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
				writer.WritePackedUInt32((uint)PlayerId);
			}
			if ((base.syncVarDirtyBits & 2u) != 0)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(OverridePasswordEnabled);
			}
			if ((base.syncVarDirtyBits & 4u) != 0)
			{
				if (!flag)
				{
					writer.WritePackedUInt32(base.syncVarDirtyBits);
					flag = true;
				}
				writer.Write(GameplayData);
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
				PlayerId = (int)reader.ReadPackedUInt32();
				OverridePasswordEnabled = reader.ReadBoolean();
				GameplayData = reader.ReadBoolean();
				return;
			}
			int num = (int)reader.ReadPackedUInt32();
			if (((uint)num & (true ? 1u : 0u)) != 0)
			{
				SetId((int)reader.ReadPackedUInt32());
			}
			if (((uint)num & 2u) != 0)
			{
				SetOverridePasswordEnabled(reader.ReadBoolean());
			}
			if (((uint)num & 4u) != 0)
			{
				GameplayData = reader.ReadBoolean();
			}
		}
	}
}
