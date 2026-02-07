using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfigFile : MonoBehaviour
{
	public static YamlConfig ServerConfig;

	public static YamlConfig HosterPolicy;

	internal static string ConfigPath;

	public static Dictionary<string, int[]> smBalancedPicker;

	private void Start()
	{
		if (!Directory.Exists(FileManager.AppFolder))
		{
			Directory.CreateDirectory(FileManager.AppFolder);
		}
		if (File.Exists(FileManager.AppFolder + "config.txt") && !File.Exists(FileManager.AppFolder + "LEGANCY CONFIG BACKUP - NOT WORKING.txt"))
		{
			File.Move(FileManager.AppFolder + "config.txt", FileManager.AppFolder + "LEGANCY CONFIG BACKUP - NOT WORKING.txt");
		}
	}

	public static void ReloadGameConfig()
	{
		if (ServerConfig == null)
		{
			throw new IOException("Please use ReloadGameConfig() with arguments first!");
		}
		ServerConfig = ReloadGameConfig(ConfigPath, true);
	}

	public static YamlConfig ReloadGameConfig(string path, bool notSet = false)
	{
		if (!notSet)
		{
			ConfigPath = path;
		}
		if (!Directory.Exists(FileManager.AppFolder))
		{
			Directory.CreateDirectory(FileManager.AppFolder);
		}
		if (File.Exists(path))
		{
			return new YamlConfig(path);
		}
		try
		{
			File.Copy("MiscData/gameconfig_template.txt", path);
		}
		catch
		{
			ServerConsole.AddLog("Error during copying config file!");
			return null;
		}
		return new YamlConfig(path);
	}
}
