using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cryptography;
using GameConsole;
using MEC;
using Steamworks;
using UnityEngine;

public class CentralAuth : MonoBehaviour
{
	private byte[] m_Ticket;

	private uint m_pcbTicket;

	private string hexticket;

	private string _roleToRequest;

	private HAuthTicket m_HAuthTicket;

	private ICentralAuth _ica;

	private bool _responded;

	private Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;

	public static CentralAuth singleton;

	private void Awake()
	{
		singleton = this;
	}

	public void GenerateToken(ICentralAuth icaa)
	{
		if (SteamManager.Initialized)
		{
			GameConsole.Console.singleton.AddLog("Obtaining ticket from Steam...", Color.blue);
			_ica = icaa;
			if (m_GetAuthSessionTicketResponse == null)
			{
				m_GetAuthSessionTicketResponse = Callback<GetAuthSessionTicketResponse_t>.Create(OnGetAuthSessionTicketResponse);
			}
			m_Ticket = new byte[1024];
			m_HAuthTicket = SteamUser.GetAuthSessionTicket(m_Ticket, 1024, out m_pcbTicket);
		}
	}

	public void OnGetAuthSessionTicketResponse(GetAuthSessionTicketResponse_t pCallback)
	{
		Array.Resize(ref m_Ticket, (int)m_pcbTicket);
		hexticket = BitConverter.ToString(m_Ticket).Replace("-", string.Empty);
		_responded = true;
	}

	private void Update()
	{
		if (_responded)
		{
			_responded = false;
			Timing.RunCoroutine(_RequestToken(), Segment.FixedUpdate);
		}
		if (!string.IsNullOrEmpty(_roleToRequest) && PlayerManager.localPlayer != null && !string.IsNullOrEmpty(PlayerManager.localPlayer.GetComponent<NicknameSync>().myNick))
		{
			GameConsole.Console.singleton.AddLog("Requesting your global badge...", Color.yellow);
			_ica.RequestBadge(_roleToRequest);
			_roleToRequest = string.Empty;
		}
	}

	private IEnumerator<float> _RequestToken()
	{
		GameConsole.Console.singleton.AddLog("Requesting signature from central servers...", Color.blue);
		WWWForm form = new WWWForm();
		form.AddField("publickey", Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(GameConsole.Console.SessionKeys.Public))));
		form.AddField("ticket", hexticket);
		WWW www = new WWW(CentralServer.URL + "requestsignature.php", form);
		yield return Timing.WaitUntilDone(www);
		if (string.IsNullOrEmpty(www.error))
		{
			try
			{
				if (File.Exists(FileManager.AppFolder + "EnableDebug.txt"))
				{
					string[] array = www.text.Replace("<br>", "\n").Split('\n');
					string[] array2 = array;
					foreach (string text in array2)
					{
						GameConsole.Console.singleton.AddLog("[AUTH DEBUG] " + text, Color.cyan);
					}
				}
				GameConsole.Console.singleton.AddLog("Sending your authentication token to game server...", Color.green);
				string[] array3 = www.text.Split(new string[1] { "=== SECTION ===<br>" }, StringSplitOptions.None);
				_ica.TokenGenerated(array3[0]);
				if (array3[1] != "-")
				{
					_roleToRequest = array3[1];
				}
				else
				{
					GameConsole.Console.singleton.AddLog("No global badge has been issued for your account.", Color.cyan);
				}
				yield break;
			}
			catch (Exception ex)
			{
				GameConsole.Console.singleton.AddLog("Error during authentication: " + ex.Message + ". StackTrace: " + ex.StackTrace, Color.red);
				yield break;
			}
		}
		GameConsole.Console.singleton.AddLog("Could not request token - " + www.error, Color.red);
		Debug.LogError("Could not request token - " + www.error);
	}

	public void StartValidateToken(ICentralAuth icaa, string token)
	{
		Timing.RunCoroutine(_ValidateToken(icaa, token), Segment.FixedUpdate);
	}

	private IEnumerator<float> _ValidateToken(ICentralAuth icaa, string token)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				ServerConsole.AddLog("Authentication token signature mismatch.");
				icaa.GetCcm().CallTargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to signature mismatch.", "red");
				icaa.FailToken();
			}
			else
			{
				string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
				Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
				if (dictionary["Usage"] != "Authentication")
				{
					ServerConsole.AddLog("Player tried to use token not issued to authentication purposes.");
					icaa.GetCcm().CallTargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to invalid purpose of signature.", "red");
					_ica.FailToken();
				}
				else if (dictionary["Test signature"] != "NO")
				{
					ServerConsole.AddLog("Player tried to use authentication token issued only for testing. Server: " + dictionary["Issued by"] + ".");
					icaa.GetCcm().CallTargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to testing signature.", "red");
					_ica.FailToken();
				}
				else
				{
					DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
					DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
					if (dateTime < DateTime.UtcNow)
					{
						ServerConsole.AddLog("Player tried to use expired authentication token. Server: " + dictionary["Issued by"] + ".");
						ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
						icaa.GetCcm().CallTargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to expired signature.", "red");
						_ica.FailToken();
					}
					else if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
					{
						ServerConsole.AddLog("Player tried to use non-issued authentication token. Server: " + dictionary["Issued by"] + ".");
						ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
						icaa.GetCcm().CallTargetConsolePrint(icaa.GetCcm().connectionToClient, "Authentication token rejected due to non-issued signature.", "red");
						_ica.FailToken();
					}
					else
					{
						icaa.GetCcm().GetComponent<ServerRoles>().firstVerResult = dictionary;
						icaa.Ok(dictionary["Steam ID"], dictionary["Nickname"], dictionary["Global ban"], dictionary["Steam ban"], dictionary["Issued by"]);
					}
				}
			}
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Error during authentication token verification: " + ex.Message);
			icaa.Fail();
		}
		yield return 0f;
	}

	internal static string ValidateForGlobalBanning(string token, string nickname)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to signature mismatch.", Color.red);
				return "-1";
			}
			string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
			Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
			if (dictionary["Usage"] != "Authentication")
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to usage mismatch.", Color.red);
				return "-1";
			}
			if (dictionary["Test signature"] != "NO")
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to test flag.", Color.red);
				return "-1";
			}
			if (ServerRoles.Base64Decode(dictionary["Nickname"]) != nickname)
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to nickname mismatch (token issued for " + ServerRoles.Base64Decode(dictionary["Nickname"]) + ").", Color.red);
				return "-1";
			}
			DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
			DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
			if (dateTime < DateTime.UtcNow.AddMinutes(-45.0))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to expiration date.", Color.red);
				return "-1";
			}
			if (dateTime2 > DateTime.UtcNow.AddMinutes(45.0))
			{
				GameConsole.Console.singleton.AddLog("Authentication token rejected due to issuance date.", Color.red);
				return "-1";
			}
			GameConsole.Console.singleton.AddLog("Accepted verification token of user " + dictionary["Steam ID"] + " - " + ServerRoles.Base64Decode(dictionary["Nickname"]) + " signed by " + dictionary["Issued by"] + ".", Color.green);
			return dictionary["Steam ID"];
		}
		catch (Exception ex)
		{
			GameConsole.Console.singleton.AddLog("Error during authentication token verification: " + ex.Message, Color.red);
			return "-1";
		}
	}

	internal static Dictionary<string, string> ValidateBadgeRequest(string token, string steamid, string nickname)
	{
		try
		{
			string text = token.Substring(0, token.IndexOf("<br>Signature: ", StringComparison.Ordinal));
			string text2 = token.Substring(token.IndexOf("<br>Signature: ", StringComparison.Ordinal) + 15);
			text2 = text2.Replace("<br>", string.Empty);
			if (!ECDSA.Verify(text, text2, ServerConsole.Publickey))
			{
				ServerConsole.AddLog("Badge request signature mismatch.");
				return null;
			}
			string[] source = text.Split(new string[1] { "<br>" }, StringSplitOptions.None);
			Dictionary<string, string> dictionary = source.Select((string rwr) => rwr.Split(new string[1] { ": " }, StringSplitOptions.None)).ToDictionary((string[] split) => split[0], (string[] split) => split[1]);
			if (dictionary["Usage"] != "Badge request")
			{
				ServerConsole.AddLog("Player tried to use token not issued to request a badge.");
				return null;
			}
			if (dictionary["Test signature"] != "NO")
			{
				ServerConsole.AddLog("Player tried to use badge request token issued only for testing. Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			if (dictionary["Steam ID"] != steamid && !string.IsNullOrEmpty(steamid))
			{
				ServerConsole.AddLog("Player tried to use badge request token issued for different user (Steam ID mismatch). Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			if (ServerRoles.Base64Decode(dictionary["Nickname"]) != nickname)
			{
				ServerConsole.AddLog("Player tried to use badge request token issued for different user (nickname mismatch). Server: " + dictionary["Issued by"] + ".");
				return null;
			}
			DateTime dateTime = DateTime.ParseExact(dictionary["Expiration time"], "yyyy-MM-dd HH:mm:ss", null);
			DateTime dateTime2 = DateTime.ParseExact(dictionary["Issuence time"], "yyyy-MM-dd HH:mm:ss", null);
			if (dateTime < DateTime.UtcNow)
			{
				ServerConsole.AddLog("Player tried to use expired badge request token. Server: " + dictionary["Issued by"] + ".");
				ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
				return null;
			}
			if (dateTime2 > DateTime.UtcNow.AddMinutes(20.0))
			{
				ServerConsole.AddLog("Player tried to use non-issued badge request token. Server: " + dictionary["Issued by"] + ".");
				ServerConsole.AddLog("Make sure that time and timezone set on server is correct. We recommend synchronizing the time.");
				return null;
			}
			return dictionary;
		}
		catch (Exception ex)
		{
			ServerConsole.AddLog("Error during badge request token verification: " + ex.Message);
			Debug.Log("Error during badge request token verification: " + ex.Message + " StackTrace: " + ex.StackTrace);
			return null;
		}
	}
}
