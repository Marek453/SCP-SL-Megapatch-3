using System;
using UnityEngine;
using UnityEngine.Networking;

public class BanPlayer : NetworkBehaviour
{
	public string BanUser(GameObject user, int duration, string reason)
	{
		string text = "good";
		string text2 = "nothing";
		string text3 = "nothing";
		try
		{
			if (duration > 0)
			{
				string text4 = "Missing Nick";
				text = "Setting nick";
				text4 = user.GetComponent<NicknameSync>().myNick;
				text = "Online ban";
				if (ConfigFile.ServerConfig.GetBool("online_mode"))
				{
					BanDetails banDetails = new BanDetails();
					banDetails.OriginalName = text4;
					banDetails.Id = user.GetComponent<CharacterClassManager>().SteamId;
					banDetails.IssuanceTime = TimeBehaviour.CurrentTimestamp();
					banDetails.Expires = DateTime.UtcNow.AddMinutes(duration).Ticks;
					banDetails.Reason = reason;
					banDetails.Issuer = "ADMIN";
					BanDetails ban = banDetails;
					text2 = BanHandler.IssueBan(ban, 0);
				}
				else
				{
					text2 = "good";
				}
				text = "IP ban";
				if (ConfigFile.ServerConfig.GetBool("ip_banning"))
				{
					BanDetails banDetails = new BanDetails();
					banDetails.OriginalName = text4;
					banDetails.Id = user.GetComponent<NetworkIdentity>().connectionToClient.address;
					banDetails.IssuanceTime = TimeBehaviour.CurrentTimestamp();
					banDetails.Expires = DateTime.UtcNow.AddMinutes(duration).Ticks;
					banDetails.Reason = reason;
					banDetails.Issuer = "ADMIN";
					BanDetails ban2 = banDetails;
					text3 = BanHandler.IssueBan(ban2, 1);
				}
				else
				{
					text3 = "good";
				}
			}
			else
			{
				text2 = "good";
				text3 = "good";
			}
			text = "good";
		}
		catch
		{
		}
		text = ((!(text2 == "good") || !(text3 == "good")) ? ("Online ban: " + text2 + ", IP ban: " + text3) : "good");
		ServerConsole.Disconnect(user, (duration <= 0) ? "You have been kicked." : "You have been banned.");
		return text;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
