using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

public class YamlConfig
{
	public string[] RawData;

	public YamlConfig()
	{
		RawData = new string[0];
	}

	public YamlConfig(string path)
	{
		LoadConfigFile(path);
	}

	public void LoadConfigFile(string path)
	{
		RawData = FileManager.ReadAllLines(path);
	}

	public string GetString(string key, string def = "")
	{
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key + ": "))
			{
				return text.Substring(key.Length + 2);
			}
		}
		return def;
	}

	public int GetInt(string key, int def = 0)
	{
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key + ": "))
			{
				try
				{
					return Convert.ToInt32(text.Substring(key.Length + 2));
				}
				catch
				{
					return 0;
				}
			}
		}
		return def;
	}

	public float GetFloat(string key, float def = 0f)
	{
		string @string = GetString(key, string.Empty);
		if (@string == string.Empty)
		{
			return def;
		}
		@string = @string.Replace(',', '.');
		float result;
		return (!float.TryParse(@string, NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? def : result;
	}

	public bool GetBool(string key, bool def = false)
	{
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key + ": "))
			{
				return text.Substring(key.Length + 2) == "true";
			}
		}
		return def;
	}

	public List<string> GetStringList(string key)
	{
		bool flag = false;
		List<string> list = new List<string>();
		string[] rawData = RawData;
		foreach (string text in rawData)
		{
			if (text.StartsWith(key + ":"))
			{
				flag = true;
			}
			else if (flag)
			{
				if (text.StartsWith(" - "))
				{
					list.Add(text.Substring(3));
				}
				else if (!text.StartsWith("#"))
				{
					break;
				}
			}
		}
		return list;
	}

	public List<int> GetIntList(string key)
	{
		List<string> stringList = GetStringList(key);
		return ((IEnumerable<string>)stringList).Select((Func<string, int>)Convert.ToInt32).ToList();
	}

	public Dictionary<string, string> GetStringDictionary(string key)
	{
		List<string> stringList = GetStringList(key);
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (string item in stringList)
		{
			int num = item.IndexOf(": ", StringComparison.Ordinal);
			dictionary.Add(item.Substring(0, num), item.Substring(num + 2));
		}
		return dictionary;
	}

	public static string[] ParseCommaSeparatedString(string data)
	{
		if (!data.StartsWith("[") || !data.EndsWith("]"))
		{
			return null;
		}
		data = data.Substring(1, data.Length - 2);
		return data.Split(new string[1] { ", " }, StringSplitOptions.None);
	}
}
