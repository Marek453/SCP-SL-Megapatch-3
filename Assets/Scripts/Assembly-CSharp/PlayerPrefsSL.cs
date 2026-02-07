using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

public class PlayerPrefsSL : MonoBehaviour
{
	public enum DataTypes
	{
		Float = 0,
		Int = 1,
		String = 2,
		Bool = 3
	}

	private static bool isRegistryAllowed;

	private static string[] registry;

	private static string floatheader = "float_";

	private static string intheader = "int_";

	private static string stringheader = "string_";

	private static string boolheader = "bool_";

	private static string path = "registry.txt";

	private void Awake()
	{
		isRegistryAllowed = SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows && (!File.Exists("useregistry.txt") || !(File.ReadAllText("useregistry.txt").Trim().ToLower() == "useregistry: false".Trim().ToLower()));
		Refresh();
	}

	private static bool Refresh()
	{
		if (!isRegistryAllowed)
		{
			if (!File.Exists(path))
			{
				File.Create(path).Close();
			}
			registry = File.ReadAllLines(path);
			return true;
		}
		return false;
	}

	private static string[] RemoveElement(string element, string[] myArray)
	{
		return myArray.Where((string w) => !w.StartsWith(element)).ToArray();
	}

	public static void SetFloat(string key, float value)
	{
		if (Refresh())
		{
			bool flag = false;
			for (int i = 0; i < registry.Length - 1; i++)
			{
				if (registry[i].StartsWith(floatheader + key + ":"))
				{
					registry[i] = floatheader + key + ": " + value;
					flag = true;
				}
			}
			if (!flag)
			{
				using (StreamWriter streamWriter = File.AppendText(path))
				{
					streamWriter.WriteLine(floatheader + key + ": " + value);
					return;
				}
			}
			File.WriteAllLines(path, registry);
		}
		else
		{
			PlayerPrefs.SetFloat(floatheader + key, value);
		}
	}

	public static void SetInt(string key, int value)
	{
		if (Refresh())
		{
			bool flag = false;
			for (int i = 0; i < registry.Length - 1; i++)
			{
				if (registry[i].StartsWith(intheader + key + ":"))
				{
					registry[i] = intheader + key + ": " + value;
					flag = true;
				}
			}
			if (!flag)
			{
				using (StreamWriter streamWriter = File.AppendText(path))
				{
					streamWriter.WriteLine(intheader + key + ": " + value);
					return;
				}
			}
			File.WriteAllLines(path, registry);
		}
		else
		{
			PlayerPrefs.SetInt(intheader + key, value);
		}
	}

	public static void SetString(string key, string value)
	{
		if (Refresh())
		{
			bool flag = false;
			for (int i = 0; i < registry.Length - 1; i++)
			{
				if (registry[i].StartsWith(stringheader + key + ":"))
				{
					registry[i] = stringheader + key + ": " + value;
					flag = true;
				}
			}
			if (!flag)
			{
				using (StreamWriter streamWriter = File.AppendText(path))
				{
					streamWriter.WriteLine(stringheader + key + ": " + value);
					return;
				}
			}
			File.WriteAllLines(path, registry);
		}
		else
		{
			PlayerPrefs.SetString(stringheader + key, value);
		}
	}

	public static void SetBool(string key, bool value)
	{
		if (Refresh())
		{
			bool flag = false;
			for (int i = 0; i < registry.Length - 1; i++)
			{
				if (registry[i].StartsWith(boolheader + key + ":"))
				{
					registry[i] = boolheader + key + ": " + value;
					flag = true;
				}
			}
			if (!flag)
			{
				using (StreamWriter streamWriter = File.AppendText(path))
				{
					streamWriter.WriteLine(boolheader + key + ": " + value);
					return;
				}
			}
			File.WriteAllLines(path, registry);
		}
		else
		{
			PlayerPrefs.SetInt(boolheader + key, value ? 1 : 0);
		}
	}

	public static void DeleteKey(string key, DataTypes type)
	{
		string text = key;
		switch (type)
		{
		case DataTypes.Bool:
			text = boolheader + key;
			break;
		case DataTypes.Float:
			text = floatheader + key;
			break;
		case DataTypes.Int:
			text = intheader + key;
			break;
		case DataTypes.String:
			text = stringheader + key;
			break;
		}
		if (Refresh())
		{
			string[] myArray = RemoveElement(key, registry);
			myArray = RemoveElement(text, myArray);
			File.WriteAllLines(path, myArray);
		}
		else
		{
			PlayerPrefs.DeleteKey(key);
			PlayerPrefs.DeleteKey(text);
		}
	}

	public static void DeleteKey(string key)
	{
		if (Refresh())
		{
			string[] myArray = RemoveElement(key, registry);
			myArray = RemoveElement(floatheader + key, myArray);
			myArray = RemoveElement(intheader + key, myArray);
			myArray = RemoveElement(boolheader + key, myArray);
			myArray = RemoveElement(stringheader + key, myArray);
			File.WriteAllLines(path, myArray);
		}
		else
		{
			PlayerPrefs.DeleteKey(key);
			PlayerPrefs.DeleteKey(floatheader + key);
			PlayerPrefs.DeleteKey(intheader + key);
			PlayerPrefs.DeleteKey(boolheader + key);
			PlayerPrefs.DeleteKey(stringheader + key);
		}
	}

	public static void DeleteAll()
	{
		if (Refresh())
		{
			File.WriteAllText(path, string.Empty);
		}
		else
		{
			PlayerPrefs.DeleteAll();
		}
	}

	public static float GetFloat(string key, float defaultValue, bool forcedefault = false)
	{
		try
		{
			if (forcedefault)
			{
				return defaultValue;
			}
			if (Refresh())
			{
				string[] array = registry;
				foreach (string text in array)
				{
					float result;
					if (text.StartsWith(intheader + key + ":") && float.TryParse(text.Replace(intheader + key + ":", string.Empty).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return result;
					}
				}
				string[] array2 = registry;
				foreach (string text2 in array2)
				{
					float result2;
					if (text2.StartsWith(key + ":") && float.TryParse(text2.Replace(key + ":", string.Empty).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
					{
						return result2;
					}
				}
				return defaultValue;
			}
			return PlayerPrefs.GetFloat(key, defaultValue);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static int GetInt(string key, int defaultValue, bool forcedefault = false)
	{
		try
		{
			if (forcedefault)
			{
				return defaultValue;
			}
			if (Refresh())
			{
				string[] array = registry;
				foreach (string text in array)
				{
					int result;
					if (text.StartsWith(intheader + key + ":") && int.TryParse(text.Replace(intheader + key + ":", string.Empty).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
					{
						return result;
					}
				}
				string[] array2 = registry;
				foreach (string text2 in array2)
				{
					int result2;
					if (text2.StartsWith(key + ":") && int.TryParse(text2.Replace(key + ":", string.Empty).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
					{
						return result2;
					}
				}
				return defaultValue;
			}
			return PlayerPrefs.GetInt(key, defaultValue);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static string GetString(string key, string defaultValue, bool forcedefault = false)
	{
		try
		{
			if (forcedefault)
			{
				return defaultValue;
			}
			if (Refresh())
			{
				string[] array = registry;
				foreach (string text in array)
				{
					if (text.StartsWith(stringheader + key + ":"))
					{
						return text.Replace(stringheader + key + ":", string.Empty).Trim();
					}
				}
				string[] array2 = registry;
				foreach (string text2 in array2)
				{
					if (text2.StartsWith(key + ":"))
					{
						return text2.Replace(key + ":", string.Empty).Trim();
					}
				}
				return defaultValue;
			}
			return PlayerPrefs.GetString(key, defaultValue);
		}
		catch
		{
			return defaultValue;
		}
	}

	public static bool GetBool(string key, bool defaultValue, bool forcedefault = false)
	{
		try
		{
			if (forcedefault)
			{
				return defaultValue;
			}
			if (Refresh())
			{
				string[] array = registry;
				foreach (string text in array)
				{
					if (text.StartsWith(boolheader + key + ":"))
					{
						return text.Replace(boolheader + key + ":", string.Empty).Trim() == "true";
					}
				}
				string[] array2 = registry;
				foreach (string text2 in array2)
				{
					if (text2.StartsWith(key + ":"))
					{
						return text2.Replace(key + ":", string.Empty).Trim() == "true";
					}
				}
				return defaultValue;
			}
			return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
		}
		catch
		{
			return defaultValue;
		}
	}

	public static bool HasKey(string key, DataTypes type)
	{
		string text = key;
		switch (type)
		{
		case DataTypes.Bool:
			text = boolheader + key;
			break;
		case DataTypes.Float:
			text = floatheader + key;
			break;
		case DataTypes.Int:
			text = intheader + key;
			break;
		case DataTypes.String:
			text = stringheader + key;
			break;
		}
		if (Refresh())
		{
			string[] array = registry;
			foreach (string text2 in array)
			{
				if (text2.StartsWith(text + ":") || text2.StartsWith(key + ":"))
				{
					return true;
				}
			}
			return false;
		}
		return PlayerPrefs.HasKey(text) || PlayerPrefs.HasKey(key);
	}

	public static bool HasKey(string key)
	{
		if (Refresh())
		{
			string[] array = registry;
			foreach (string text in array)
			{
				if (text.StartsWith(key + ":") || text.StartsWith(floatheader + key + ":") || text.StartsWith(intheader + key + ":") || text.StartsWith(boolheader + key + ":") || text.StartsWith(stringheader + key + ":"))
				{
					return true;
				}
			}
			return false;
		}
		return PlayerPrefs.HasKey(key) || PlayerPrefs.HasKey(floatheader + key) || PlayerPrefs.HasKey(intheader + key) || PlayerPrefs.HasKey(boolheader + key) || PlayerPrefs.HasKey(stringheader + key);
	}

	public static bool HasKeyWithName(string key)
	{
		if (Refresh())
		{
			string[] array = registry;
			foreach (string text in array)
			{
				if (text.StartsWith(key + ":"))
				{
					return true;
				}
			}
			return false;
		}
		return PlayerPrefs.HasKey(key);
	}
}
