using System;
using System.IO;
using System.Linq;
using System.Text;

public class FileManager
{
	public static string AppFolder
	{
		get
		{
			if (ConfigFile.HosterPolicy != null && ConfigFile.HosterPolicy.GetBool("gamedir_for_configs"))
			{
				return "AppData/";
			}
			return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/SCP Secret Laboratory/";
		}
	}

	public static string[] ReadAllLines(string path)
	{
		return File.ReadAllLines(path, Encoding.UTF8);
	}

	public static void WriteToFile(string[] data, string path)
	{
		File.WriteAllLines(path, data, Encoding.UTF8);
	}

	public static void WriteStringToFile(string data, string path)
	{
		File.WriteAllText(path, data, Encoding.UTF8);
	}

	public static void AppendFile(string data, string path, bool newLine = true)
	{
		string[] array = ReadAllLines(path);
		if (!newLine || array.Length == 0 || array[array.Length - 1].EndsWith(Environment.NewLine) || array[array.Length - 1].EndsWith("\n"))
		{
			File.AppendAllText(path, data, Encoding.UTF8);
		}
		else
		{
			File.AppendAllText(path, Environment.NewLine + data, Encoding.UTF8);
		}
	}

	public static void RenameFile(string path, string newpath)
	{
		File.Move(path, newpath);
	}

	public static void DeleteFile(string path)
	{
		File.Delete(path);
	}

	public static void ReplaceLine(int line, string text, string path)
	{
		string[] array = ReadAllLines(path);
		array[line] = text;
		WriteToFile(array, path);
	}

	public static void RemoveEmptyLines(string path)
	{
		string[] data = (from s in ReadAllLines(path)
			where !string.IsNullOrEmpty(s.Replace(Environment.NewLine, string.Empty).Replace("\n", string.Empty).Replace(" ", string.Empty))
			select s).ToArray();
		WriteToFile(data, path);
	}
}
