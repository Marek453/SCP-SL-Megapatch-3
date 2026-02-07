using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BanHandler : MonoBehaviour
{
	private void Start()
	{
		try
		{
			if (!Directory.Exists(FileManager.AppFolder))
			{
				Directory.CreateDirectory(FileManager.AppFolder);
			}
			if (!File.Exists(GetPath(0)))
			{
				File.Create(GetPath(0)).Close();
			}
			else
			{
				FileManager.RemoveEmptyLines(GetPath(0));
			}
			if (!File.Exists(GetPath(1)))
			{
				File.Create(GetPath(1)).Close();
			}
			else
			{
				FileManager.RemoveEmptyLines(GetPath(1));
			}
		}
		catch
		{
			ServerConsole.AddLog("Can't create ban files!");
		}
		ValidateBans();
	}

	public static string IssueBan(BanDetails ban, int selector)
	{
		string result = "good";
		try
		{
			result = "1";
			ban.OriginalName = ban.OriginalName.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			result = "2";
			List<BanDetails> bans = GetBans(selector);
			result = "3";
			bool flag = bans.Where((BanDetails b) => b.Id == ban.Id).Any();
			result = "4";
			if (!flag)
			{
				FileManager.AppendFile(ban.ToString(), GetPath(selector));
				FileManager.RemoveEmptyLines(GetPath(selector));
			}
			else
			{
				result = "5";
				RemoveBan(ban.Id, selector);
				result = "6";
				IssueBan(ban, selector);
			}
			result = "good";
			return result;
		}
		catch
		{
			return result;
		}
	}

	public static void ValidateBans()
	{
		ValidateBans(0);
		ValidateBans(1);
	}

	public static void ValidateBans(int selector)
	{
		List<string> list = FileManager.ReadAllLines(GetPath(selector)).ToList();
		List<int> list2 = new List<int>();
		for (int num = list.Count - 1; num >= 0; num--)
		{
			string ban = list[num];
			if (ProcessBanItem(ban) == null || !CheckExpiration(ProcessBanItem(ban), -1))
			{
				list2.Add(num);
			}
		}
		List<int> list3 = new List<int>();
		foreach (int item in list2)
		{
			if (!list3.Contains(item))
			{
				list3.Add(item);
			}
		}
		foreach (int item2 in list3.OrderByDescending((int index) => index))
		{
			list.RemoveAt(item2);
		}
		FileManager.WriteToFile(list.ToArray(), GetPath(selector));
	}

	public static bool CheckExpiration(BanDetails ban, int selector)
	{
		if (ban == null)
		{
			return false;
		}
		if (TimeBehaviour.ValidateTimestamp(ban.Expires, TimeBehaviour.CurrentTimestamp(), 0L))
		{
			return true;
		}
		if (selector >= 0)
		{
			RemoveBan(ban.Id, selector);
		}
		return false;
	}

	public static BanDetails ReturnChecks(BanDetails ban, int selector)
	{
		return (!CheckExpiration(ban, selector)) ? null : ban;
	}

	public static void RemoveBan(string id, int selector)
	{
		id = id.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
		string[] data = (from l in FileManager.ReadAllLines(GetPath(selector))
			where ProcessBanItem(l) != null && ProcessBanItem(l).Id != id
			select l).ToArray();
		FileManager.WriteToFile(data, GetPath(selector));
	}

	public static List<BanDetails> GetBans(int selector)
	{
		string[] source = FileManager.ReadAllLines(GetPath(selector));
		return (from b in source.Select(ProcessBanItem)
			where b != null
			select b).ToList();
	}

	public static KeyValuePair<BanDetails, BanDetails> QueryBan(string steamId, string ip)
	{
		string ban = null;
		string ban2 = null;
		if (!string.IsNullOrEmpty(steamId))
		{
			steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			string[] source = FileManager.ReadAllLines(GetPath(0));
			ban = source.Where((string b) => ProcessBanItem(b) != null && ProcessBanItem(b).Id == steamId).FirstOrDefault();
		}
		if (!string.IsNullOrEmpty(ip))
		{
			ip = ip.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
			string[] source2 = FileManager.ReadAllLines(GetPath(1));
			ban2 = source2.Where((string b) => ProcessBanItem(b) != null && ProcessBanItem(b).Id == ip).FirstOrDefault();
		}
		return new KeyValuePair<BanDetails, BanDetails>(ReturnChecks(ProcessBanItem(ban), 0), ReturnChecks(ProcessBanItem(ban2), 1));
	}

	public static BanDetails ProcessBanItem(string ban)
	{
		if (string.IsNullOrEmpty(ban) || !ban.Contains(";"))
		{
			return null;
		}
		string[] array = ban.Split(';');
		if (array.Length != 6)
		{
			return null;
		}
		BanDetails banDetails = new BanDetails();
		banDetails.OriginalName = array[0];
		banDetails.Id = array[1].Trim();
		banDetails.Expires = Convert.ToInt64(array[2].Trim());
		banDetails.Reason = array[3];
		banDetails.Issuer = array[4];
		banDetails.IssuanceTime = Convert.ToInt64(array[5].Trim());
		return banDetails;
	}

	public static string GetPath(int selector)
	{
		switch (selector)
		{
		case 0:
			return FileManager.AppFolder + "SteamIdBans.txt";
		case 1:
			return FileManager.AppFolder + "IpBans.txt";
		default:
			return FileManager.AppFolder + "SteamIdBans.txt";
		}
	}
}
