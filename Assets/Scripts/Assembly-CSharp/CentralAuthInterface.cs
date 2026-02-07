using GameConsole;
using UnityEngine;

public class CentralAuthInterface : ICentralAuth
{
	private readonly CharacterClassManager _s;

	private readonly bool _is;

	public CentralAuthInterface(CharacterClassManager sync, bool server)
	{
		_s = sync;
		_is = server;
	}

	public CharacterClassManager GetCcm()
	{
		return _s;
	}

	public void TokenGenerated(string token)
	{
		Console.singleton.AddLog("Authentication token obtained from central server.", Color.green);
		_s.CallCmdSendToken(token);
	}

	public void RequestBadge(string token)
	{
		_s.GetComponent<ServerRoles>().RequestBadge(token);
	}

	public void Fail()
	{
		if (_is)
		{
			ServerConsole.AddLog("Failed to validate authentication token.");
			ServerConsole.Disconnect(_s.connectionToClient, "Failed to validate authentication token.");
		}
		else
		{
			Console.singleton.AddLog("Failed to obtain authentication token from central server.", Color.red);
			_s.connectionToServer.Disconnect();
			_s.connectionToServer.Dispose();
		}
	}

	public void Ok(string steamId, string nickname, string ban, string steamban, string server)
	{
		ServerConsole.AddLog("Accepted authentication token of user " + steamId + " with global ban status " + ban + " signed by " + server + " server.");
		_s.CallTargetConsolePrint(_s.connectionToClient, "Accepted your authentication token (your steam id " + steamId + ") with global ban status " + ban + " signed by " + server + " server.", "green");
		if (BanHandler.QueryBan(steamId, null).Key != null)
		{
			_s.CallTargetConsolePrint(_s.connectionToClient, "You are banned from this server.", "red");
			ServerConsole.AddLog("Player kicked due to local SteamID ban.");
			ServerConsole.Disconnect(_s.connectionToClient, "You are banned from this server.");
		}
		else if (!WhiteList.IsWhitelisted(steamId))
		{
			_s.CallTargetConsolePrint(_s.connectionToClient, "You are not on the whitelist!", "red");
			ServerConsole.AddLog("Player kicked due to whitelist enabled.");
			ServerConsole.Disconnect(_s.connectionToClient, "You are not on the whitelist for this server.");
		}
		else if ((ConfigFile.ServerConfig.GetBool("use_vac", true) || ServerStatic.PermissionsHandler.IsVerified) && steamban != "0")
		{
			_s.CallTargetConsolePrint(_s.connectionToClient, "You have active steam ban (" + steamban + " ban).", "red");
			ServerConsole.AddLog("Player kicked due to steam ban (" + steamban + " ban).");
			ServerConsole.Disconnect(_s.connectionToClient, "You have active steam ban (" + steamban + " ban).");
		}
		else if ((ConfigFile.ServerConfig.GetBool("global_bans_cheating", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "1")
		{
			_s.CallTargetConsolePrint(_s.connectionToClient, "You have been globally banned for cheating.", "red");
			ServerConsole.AddLog("Player kicked due to global ban for cheating.");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned for cheating.");
		}
		else if ((ConfigFile.ServerConfig.GetBool("global_bans_griefing", true) || ServerStatic.PermissionsHandler.IsVerified) && ban == "2")
		{
			_s.CallTargetConsolePrint(_s.connectionToClient, "You have been globally banned for griefing.", "red");
			ServerConsole.AddLog("Player kicked due to global ban for griefing.");
			ServerConsole.Disconnect(_s.connectionToClient, "You have been globally banned for griefing.");
		}
		else
		{
			_s.GetComponent<ServerRoles>().StartServerChallenge(0);
		}
	}

	public void FailToken()
	{
		_s.CallTargetConsolePrint(_s.connectionToClient, "Your authentication token is invalid.", "red");
		ServerConsole.AddLog("Rejected invalid authentication token.");
		ServerConsole.Disconnect(_s.connectionToClient, "Your authentication token is invalid.");
	}
}
