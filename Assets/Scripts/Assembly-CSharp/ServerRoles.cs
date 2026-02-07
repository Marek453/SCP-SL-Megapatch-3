using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Cryptography;
using GameConsole;
using MEC;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class ServerRoles : NetworkBehaviour
{
	[Serializable]
	public class NamedColor
	{
		public string Name;

		public Gradient SpeakingColorIn;

		public Gradient SpeakingColorOut;

		public string ColorHex;

		public bool Restricted;
	}

	[Serializable]
	public enum AccessMode
	{
		LocalAccess = 1,
		GlobalAccess = 2,
		PasswordOverride = 3
	}

	public NamedColor[] NamedColors;

	[SyncVar(hook = "SetColor")]
	public string MyColor;

	[SyncVar(hook = "SetText")]
	public string MyText;

	[SyncVar(hook = "SetBadgeUpdate")]
	public string GlobalBadge;

	public string _bgc;

	public string _bgt;

	public NamedColor CurrentColor;

	public bool AuthroizeBadge;

	private string _globalBadgeUnconfirmed;

	private string _prevColor;

	private string _prevText;

	private string _prevBadge;

	private bool _requested;

	private bool _badgeRequested;

	private bool _authRequested;

	internal AsymmetricKeyParameter PublicKey;

	internal string PrevBadge;

	internal bool OverwatchPermitted;

	internal bool OverwatchEnabled;

	internal bool AmIInOverwatch;

	[SyncVar]
	public bool GlobalSet;

	[SyncVar]
	public bool RemoteAdmin;

	[SyncVar]
	public int Permissions;

	[SyncVar]
	public bool Staff;

	[SyncVar]
	public AccessMode RemoteAdminMode;

	private string _badgeUserChallenge;

	private string _authChallenge;

	private string _badgeChallenge;

	public Dictionary<string, string> firstVerResult;

	private static int kCmdCmdRequestBadge;

	private static int kTargetRpcTargetSignServerChallenge;

	private static int kCmdCmdServerSignatureComplete;

	private static int kTargetRpcTargetOpenRemoteAdmin;

	private static int kCmdCmdSetOverwatchStatus;

	private static int kCmdCmdToggleOverwatch;

	private static int kTargetRpcTargetSetOverwatch;

	public string NetworkMyColor
	{
		get
		{
			return MyColor;
		}
		[param: In]
		set
		{
			ref string myColor = ref MyColor;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetColor(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref myColor, 1u);
		}
	}

	public string NetworkMyText
	{
		get
		{
			return MyText;
		}
		[param: In]
		set
		{
			ref string myText = ref MyText;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetText(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref myText, 2u);
		}
	}

	public string NetworkGlobalBadge
	{
		get
		{
			return GlobalBadge;
		}
		[param: In]
		set
		{
			ref string globalBadge = ref GlobalBadge;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetBadgeUpdate(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref globalBadge, 4u);
		}
	}

	public bool NetworkGlobalSet
	{
		get
		{
			return GlobalSet;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref GlobalSet, 8u);
		}
	}

	public bool NetworkRemoteAdmin
	{
		get
		{
			return RemoteAdmin;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref RemoteAdmin, 16u);
		}
	}

	public int NetworkPermissions
	{
		get
		{
			return Permissions;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Permissions, 32u);
		}
	}

	public bool NetworkStaff
	{
		get
		{
			return Staff;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref Staff, 64u);
		}
	}

	public AccessMode NetworkRemoteAdminMode
	{
		get
		{
			return RemoteAdminMode;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref RemoteAdminMode, 128u);
		}
	}

	[Command(channel = 2)]
	public void CmdRequestBadge(string token)
	{
		if (!_requested)
		{
			_requested = true;
			Timing.RunCoroutine(_RequestRoleFromServer(token), Segment.FixedUpdate);
		}
	}

	[ServerCallback]
	public void RefreshPermissions()
	{
		if (NetworkServer.active)
		{
			UserGroup userGroup = ServerStatic.PermissionsHandler.GetUserGroup(GetComponent<CharacterClassManager>().SteamId);
			SetGroup(userGroup, false);
		}
	}

	[ServerCallback]
	public void SetGroup(UserGroup group, bool ovr, bool byAdmin = false)
	{
		if (!NetworkServer.active || group == null)
		{
			return;
		}
		GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, byAdmin ? "Updating your group on server (set by server administrator)..." : "Updating your group on server (local permissions)...", "cyan");
		if (!OverwatchPermitted && ServerStatic.PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
		{
			OverwatchPermitted = true;
		}
		if (group.Permissions > 0 && Permissions != ServerStatic.PermissionsHandler.FullPerm && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions))
		{
			NetworkRemoteAdmin = true;
			NetworkPermissions = group.Permissions;
			NetworkRemoteAdminMode = ((!ovr) ? AccessMode.LocalAccess : AccessMode.PasswordOverride);
			GetComponent<QueryProcessor>().PasswordTries = 0;
			CallTargetOpenRemoteAdmin(base.connectionToClient);
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, byAdmin ? "Your remote admin access has been granted (set by server administrator)." : "Your remote admin access has been granted (local permissions).", "cyan");
		}
		ServerLogs.AddLog(ServerLogs.Modules.Permissions, "User with nickname " + GetComponent<NicknameSync>().myNick + " and SteamID " + GetComponent<CharacterClassManager>().SteamId + " has been assigned to group " + group.BadgeText + " (local permissions).", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
		if (!(group.BadgeColor == "hidden"))
		{
			NetworkMyText = group.BadgeText;
			NetworkMyColor = group.BadgeColor;
			if (!byAdmin)
			{
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (local permissions).", "cyan");
			}
			else
			{
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your role \"" + group.BadgeText + "\" with color " + group.BadgeColor + " has been granted to you (set by server administrator).", "cyan");
			}
		}
	}

	private IEnumerator<float> _RequestRoleFromServer(string token)
	{
		Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(token, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
		if (dictionary != null)
		{
			_globalBadgeUnconfirmed = token;
			StartServerChallenge(1);
		}
		yield break;
	}

	public string GetColoredRoleString(bool newLine = false)
	{
		if (string.IsNullOrEmpty(MyColor) || string.IsNullOrEmpty(MyText) || CurrentColor == null)
		{
			return string.Empty;
		}
		if ((CurrentColor.Restricted || MyText.Contains("[") || MyText.Contains("]") || MyText.Contains("<") || MyText.Contains(">")) && !AuthroizeBadge)
		{
			return string.Empty;
		}
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			return ((!newLine) ? string.Empty : "\n") + "<color=#" + namedColor.ColorHex + ">" + MyText + "</color>";
		}
		return string.Empty;
	}

	public Color GetColor()
	{
		if (string.IsNullOrEmpty(MyColor) || MyColor == "default" || CurrentColor == null)
		{
			return Color.white;
		}
		if ((CurrentColor.Restricted || MyText.Contains("[") || MyText.Contains("]") || MyText.Contains("<") || MyText.Contains(">")) && !AuthroizeBadge)
		{
			return Color.white;
		}
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		return (namedColor != null) ? namedColor.SpeakingColorIn.Evaluate(1f) : Color.white;
	}

	public Gradient[] GetGradient()
	{
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		return new Gradient[2] { namedColor.SpeakingColorIn, namedColor.SpeakingColorOut };
	}

	private void Update()
	{
		if (CurrentColor == null)
		{
			return;
		}
		if (GlobalBadge != _prevBadge)
		{
			_prevBadge = GlobalBadge;
			if (string.IsNullOrEmpty(GlobalBadge))
			{
				_bgc = string.Empty;
				_bgt = string.Empty;
				AuthroizeBadge = false;
				_prevColor += ".";
				_prevText += ".";
				return;
			}
			GameConsole.Console.singleton.AddLog("Validating global badge of user " + GetComponent<NicknameSync>().myNick, Color.gray);
			Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(GlobalBadge, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
			if (dictionary == null)
			{
				GameConsole.Console.singleton.AddLog("Validation of global badge of user " + GetComponent<NicknameSync>().myNick + " failed - invalid digital signature.", Color.red);
				_bgc = string.Empty;
				_bgt = string.Empty;
				AuthroizeBadge = false;
				_prevColor += ".";
				_prevText += ".";
				return;
			}
			GameConsole.Console.singleton.AddLog("Validation of global badge of user " + GetComponent<NicknameSync>().myNick + " complete - badge signed by central server " + dictionary["Issued by"] + ".", Color.grey);
			_bgc = dictionary["Badge color"];
			_bgt = dictionary["Badge text"];
			NetworkMyColor = dictionary["Badge color"];
			NetworkMyText = dictionary["Badge text"];
			AuthroizeBadge = true;
		}
		if (!(_prevColor == MyColor) || !(_prevText == MyText))
		{
			if (CurrentColor.Restricted && (MyText != _bgt || MyColor != _bgc))
			{
				GameConsole.Console.singleton.AddLog("TAG FAIL 1 - " + MyText + " - " + _bgt + " /-/ " + MyColor + " - " + _bgc, Color.gray);
				AuthroizeBadge = false;
				NetworkMyColor = string.Empty;
				NetworkMyText = string.Empty;
				_prevColor = string.Empty;
				_prevText = string.Empty;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
			else if ((MyText != _bgt && (MyText.Contains("[") || MyText.Contains("]"))) || MyText.Contains("<") || MyText.Contains(">"))
			{
				GameConsole.Console.singleton.AddLog("TAG FAIL 2 - " + MyText + " - " + _bgt + " /-/ " + MyColor + " - " + _bgc, Color.gray);
				AuthroizeBadge = false;
				NetworkMyColor = string.Empty;
				NetworkMyText = string.Empty;
				_prevColor = string.Empty;
				_prevText = string.Empty;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
			else
			{
				_prevColor = MyColor;
				_prevText = MyText;
				_prevBadge = GlobalBadge;
				PlayerList.UpdatePlayerRole(base.gameObject);
			}
		}
	}

	public void SetColor(string i)
	{
		NetworkMyColor = i;
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			CurrentColor = namedColor;
		}
	}

	public void SetText(string i)
	{
		NetworkMyText = i;
		NamedColor namedColor = NamedColors.FirstOrDefault((NamedColor row) => row.Name == MyColor);
		if (namedColor != null)
		{
			CurrentColor = namedColor;
		}
	}

	public void SetBadgeUpdate(string i)
	{
		NetworkGlobalBadge = i;
	}

	[ServerCallback]
	public void StartServerChallenge(int selector)
	{
		if (NetworkServer.active && (selector != 0 || string.IsNullOrEmpty(_authChallenge)) && (selector != 1 || string.IsNullOrEmpty(_badgeChallenge)) && selector <= 1 && selector >= 0)
		{
			RandomNumberGenerator randomNumberGenerator = new RNGCryptoServiceProvider();
			byte[] array = new byte[32];
			randomNumberGenerator.GetBytes(array);
			string text = Convert.ToBase64String(array);
			if (selector == 0)
			{
				_authChallenge = "auth-" + text;
				CallTargetSignServerChallenge(base.connectionToClient, _authChallenge);
			}
			else
			{
				_badgeChallenge = "badge-server-" + text;
				CallTargetSignServerChallenge(base.connectionToClient, _badgeChallenge);
			}
		}
	}

	[TargetRpc(channel = 2)]
	public void TargetSignServerChallenge(NetworkConnection target, string challenge)
	{
		if (challenge.StartsWith("auth-"))
		{
			if (_authRequested)
			{
				return;
			}
			_authRequested = true;
		}
		else
		{
			if (!challenge.StartsWith("badge-server-") || _badgeRequested)
			{
				return;
			}
			_badgeRequested = true;
		}
		string response = ECDSA.Sign(challenge, GameConsole.Console.SessionKeys.Private);
		GameConsole.Console.singleton.AddLog("Signed " + challenge + " for server.", Color.cyan);
		CallCmdServerSignatureComplete(challenge, response, ECDSA.KeyToString(GameConsole.Console.SessionKeys.Public));
	}

	[Command(channel = 2)]
	public void CmdServerSignatureComplete(string challenge, string response, string publickey)
	{
		if (firstVerResult == null)
		{
			firstVerResult = CentralAuth.ValidateBadgeRequest(_globalBadgeUnconfirmed, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
		}
		if (firstVerResult == null)
		{
			return;
		}
		if (firstVerResult["Public key"] != Base64Encode(Sha.HashToString(Sha.Sha256(publickey))))
		{
			GameConsole.Console.singleton.AddLog("Rejected signature of challenge " + challenge + " due to public key hash mismatch.", Color.red);
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to public key mismatch.", "red");
			return;
		}
		if (PublicKey == null)
		{
			PublicKey = ECDSA.PublicKeyFromString(publickey);
		}
		if (!ECDSA.Verify(challenge, response, PublicKey))
		{
			GameConsole.Console.singleton.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.", Color.red);
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
		}
		else if (challenge.StartsWith("auth-") && challenge == _authChallenge)
		{
			GetComponent<CharacterClassManager>().NetworkSteamId = firstVerResult["Steam ID"];
			GetComponent<NicknameSync>().UpdateNickname(Base64Decode(firstVerResult["Nickname"]));
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Hi " + Base64Decode(firstVerResult["Nickname"]) + "! Your challenge signature has been accepted.", "green");
			RefreshPermissions();
			_authChallenge = string.Empty;
		}
		else
		{
			if (!challenge.StartsWith("badge-server-") || !(challenge == _badgeChallenge))
			{
				return;
			}
			Dictionary<string, string> dictionary = CentralAuth.ValidateBadgeRequest(_globalBadgeUnconfirmed, GetComponent<CharacterClassManager>().SteamId, GetComponent<NicknameSync>().myNick);
			if (dictionary == null)
			{
				ServerConsole.AddLog("Rejected signature of challenge " + challenge + " due to signature mismatch.");
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Challenge signature rejected due to signature mismatch.", "red");
				return;
			}
			SetBadgeUpdate(_globalBadgeUnconfirmed);
			PrevBadge = _globalBadgeUnconfirmed;
			_globalBadgeUnconfirmed = string.Empty;
			if (dictionary["Remote admin"] == "YES" || dictionary["Management"] == "YES" || dictionary["Global banning"] == "YES")
			{
				NetworkStaff = true;
			}
			if (dictionary["Overwatch mode"] == "YES")
			{
				OverwatchPermitted = true;
			}
			if (dictionary["Remote admin"] == "YES" && ServerStatic.PermissionsHandler.StaffAccess)
			{
				NetworkRemoteAdmin = true;
				NetworkPermissions = ServerStatic.PermissionsHandler.FullPerm;
				NetworkRemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient);
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - staff).", "cyan");
			}
			else if (dictionary["Management"] == "YES" && ServerStatic.PermissionsHandler.ManagersAccess)
			{
				NetworkRemoteAdmin = true;
				NetworkPermissions = ServerStatic.PermissionsHandler.FullPerm;
				NetworkRemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient);
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - management).", "cyan");
			}
			else if (dictionary["Global banning"] == "YES" && ServerStatic.PermissionsHandler.BanningTeamAccess)
			{
				NetworkRemoteAdmin = true;
				NetworkPermissions = ServerStatic.PermissionsHandler.FullPerm;
				NetworkRemoteAdminMode = AccessMode.GlobalAccess;
				GetComponent<QueryProcessor>().PasswordTries = 0;
				CallTargetOpenRemoteAdmin(base.connectionToClient);
				GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your remote admin access has been granted (global permissions - banning team).", "cyan");
			}
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "Your global badge has been granted.", "cyan");
			_badgeChallenge = string.Empty;
		}
	}

	[TargetRpc]
	private void TargetOpenRemoteAdmin(NetworkConnection connection)
	{
		UnityEngine.Object.FindObjectOfType<UIController>().ActivateRemoteAdmin();
	}

	[Command(channel = 2)]
	public void CmdSetOverwatchStatus(bool status)
	{
		if (!OverwatchPermitted && status)
		{
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
		}
		else
		{
			SetOverwatchStatus(status);
		}
	}

	[Command(channel = 2)]
	public void CmdToggleOverwatch()
	{
		if (!OverwatchPermitted && !OverwatchEnabled)
		{
			GetComponent<CharacterClassManager>().CallTargetConsolePrint(base.connectionToClient, "You don't have permissions to enable overwatch mode!", "red");
		}
		else
		{
			SetOverwatchStatus(!OverwatchEnabled);
		}
	}

	public void SetOverwatchStatus(bool status)
	{
		OverwatchEnabled = status;
		if (status && GetComponent<CharacterClassManager>().curClass != 2)
		{
			GetComponent<CharacterClassManager>().SetClassID(2);
		}
		CallTargetSetOverwatch(base.connectionToClient, OverwatchEnabled);
	}

	public void RequestBadge(string token)
	{
		CallCmdRequestBadge(token);
	}

	public static string Base64Encode(string plainText)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(plainText);
		return Convert.ToBase64String(bytes);
	}

	public static string Base64Decode(string base64EncodedData)
	{
		byte[] bytes = Convert.FromBase64String(base64EncodedData);
		return Encoding.UTF8.GetString(bytes);
	}

	[TargetRpc(channel = 2)]
	public void TargetSetOverwatch(NetworkConnection conn, bool s)
	{
		GameConsole.Console.singleton.AddLog("Overwatch status: " + ((!s) ? "DISABLED" : "ENABLED"), Color.green);
		AmIInOverwatch = s;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdRequestBadge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRequestBadge called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdRequestBadge(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdServerSignatureComplete(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdServerSignatureComplete called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdServerSignatureComplete(reader.ReadString(), reader.ReadString(), reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdSetOverwatchStatus(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetOverwatchStatus called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdSetOverwatchStatus(reader.ReadBoolean());
		}
	}

	protected static void InvokeCmdCmdToggleOverwatch(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdToggleOverwatch called on client.");
		}
		else
		{
			((ServerRoles)obj).CmdToggleOverwatch();
		}
	}

	public void CallCmdRequestBadge(string token)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRequestBadge called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRequestBadge(token);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRequestBadge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(token);
		SendCommandInternal(networkWriter, 2, "CmdRequestBadge");
	}

	public void CallCmdServerSignatureComplete(string challenge, string response, string publickey)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdServerSignatureComplete called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdServerSignatureComplete(challenge, response, publickey);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdServerSignatureComplete);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(challenge);
		networkWriter.Write(response);
		networkWriter.Write(publickey);
		SendCommandInternal(networkWriter, 2, "CmdServerSignatureComplete");
	}

	public void CallCmdSetOverwatchStatus(bool status)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetOverwatchStatus called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetOverwatchStatus(status);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetOverwatchStatus);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(status);
		SendCommandInternal(networkWriter, 2, "CmdSetOverwatchStatus");
	}

	public void CallCmdToggleOverwatch()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdToggleOverwatch called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdToggleOverwatch();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdToggleOverwatch);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdToggleOverwatch");
	}

	protected static void InvokeRpcTargetSignServerChallenge(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSignServerChallenge called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetSignServerChallenge(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetOpenRemoteAdmin(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOpenRemoteAdmin called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetOpenRemoteAdmin(ClientScene.readyConnection);
		}
	}

	protected static void InvokeRpcTargetSetOverwatch(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetSetOverwatch called on server.");
		}
		else
		{
			((ServerRoles)obj).TargetSetOverwatch(ClientScene.readyConnection, reader.ReadBoolean());
		}
	}

	public void CallTargetSignServerChallenge(NetworkConnection target, string challenge)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSignServerChallenge called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSignServerChallenge);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(challenge);
		SendTargetRPCInternal(target, networkWriter, 2, "TargetSignServerChallenge");
	}

	public void CallTargetOpenRemoteAdmin(NetworkConnection connection)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetOpenRemoteAdmin called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetOpenRemoteAdmin);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendTargetRPCInternal(connection, networkWriter, 0, "TargetOpenRemoteAdmin");
	}

	public void CallTargetSetOverwatch(NetworkConnection conn, bool s)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetSetOverwatch called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetSetOverwatch);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(s);
		SendTargetRPCInternal(conn, networkWriter, 2, "TargetSetOverwatch");
	}

	static ServerRoles()
	{
		kCmdCmdRequestBadge = 1417446350;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdRequestBadge, InvokeCmdCmdRequestBadge);
		kCmdCmdServerSignatureComplete = -834487468;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdServerSignatureComplete, InvokeCmdCmdServerSignatureComplete);
		kCmdCmdSetOverwatchStatus = 200610181;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdSetOverwatchStatus, InvokeCmdCmdSetOverwatchStatus);
		kCmdCmdToggleOverwatch = -571630643;
		NetworkBehaviour.RegisterCommandDelegate(typeof(ServerRoles), kCmdCmdToggleOverwatch, InvokeCmdCmdToggleOverwatch);
		kTargetRpcTargetSignServerChallenge = 1367769996;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetSignServerChallenge, InvokeRpcTargetSignServerChallenge);
		kTargetRpcTargetOpenRemoteAdmin = 1449538856;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetOpenRemoteAdmin, InvokeRpcTargetOpenRemoteAdmin);
		kTargetRpcTargetSetOverwatch = -1052391504;
		NetworkBehaviour.RegisterRpcDelegate(typeof(ServerRoles), kTargetRpcTargetSetOverwatch, InvokeRpcTargetSetOverwatch);
		NetworkCRC.RegisterBehaviour("ServerRoles", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(MyColor);
			writer.Write(MyText);
			writer.Write(GlobalBadge);
			writer.Write(GlobalSet);
			writer.Write(RemoteAdmin);
			writer.WritePackedUInt32((uint)Permissions);
			writer.Write(Staff);
			writer.Write((int)RemoteAdminMode);
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
			writer.Write(MyColor);
		}
		if ((base.syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(MyText);
		}
		if ((base.syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(GlobalBadge);
		}
		if ((base.syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(GlobalSet);
		}
		if ((base.syncVarDirtyBits & 0x10u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(RemoteAdmin);
		}
		if ((base.syncVarDirtyBits & 0x20u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)Permissions);
		}
		if ((base.syncVarDirtyBits & 0x40u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(Staff);
		}
		if ((base.syncVarDirtyBits & 0x80u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write((int)RemoteAdminMode);
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
			MyColor = reader.ReadString();
			MyText = reader.ReadString();
			GlobalBadge = reader.ReadString();
			GlobalSet = reader.ReadBoolean();
			RemoteAdmin = reader.ReadBoolean();
			Permissions = (int)reader.ReadPackedUInt32();
			Staff = reader.ReadBoolean();
			RemoteAdminMode = (AccessMode)reader.ReadInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetColor(reader.ReadString());
		}
		if (((uint)num & 2u) != 0)
		{
			SetText(reader.ReadString());
		}
		if (((uint)num & 4u) != 0)
		{
			SetBadgeUpdate(reader.ReadString());
		}
		if (((uint)num & 8u) != 0)
		{
			GlobalSet = reader.ReadBoolean();
		}
		if (((uint)num & 0x10u) != 0)
		{
			RemoteAdmin = reader.ReadBoolean();
		}
		if (((uint)num & 0x20u) != 0)
		{
			Permissions = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 0x40u) != 0)
		{
			Staff = reader.ReadBoolean();
		}
		if (((uint)num & 0x80u) != 0)
		{
			RemoteAdminMode = (AccessMode)reader.ReadInt32();
		}
	}
}
