using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Networking;

public class MuteHandler : NetworkBehaviour
{
	private static string _path;

	private static List<string> mutes;

	private void Start()
	{
		_path = FileManager.AppFolder + "mutes.txt";
		try
		{
			if (!Directory.Exists(FileManager.AppFolder))
			{
				Directory.CreateDirectory(FileManager.AppFolder);
			}
			if (!File.Exists(_path))
			{
				File.Create(_path).Close();
			}
		}
		catch
		{
			ServerConsole.AddLog("Can't create mute file!");
		}
	}

	public static void IssuePersistantMute(string steamId)
	{
		steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
		string[] source = FileManager.ReadAllLines(_path);
		if (!source.Where((string b) => b == steamId).Any())
		{
			FileManager.AppendFile(steamId, _path);
			return;
		}
		RevokePersistantMute(steamId);
		IssuePersistantMute(steamId);
	}

	public static void RevokePersistantMute(string steamId)
	{
		steamId = steamId.Replace(";", ":").Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty);
		string[] data = (from l in FileManager.ReadAllLines(_path)
			where l != steamId
			select l).ToArray();
		FileManager.WriteToFile(data, _path);
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
