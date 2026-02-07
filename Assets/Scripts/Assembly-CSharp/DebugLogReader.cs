using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DebugLogReader : MonoBehaviour
{
	public ScrollRect scroll;

	public GameObject parent;

	public GameObject prefab;

	private List<GameObject> lines = new List<GameObject>();

	private void OnEnable()
	{
		try
		{
			string text;
			if (SystemInfo.operatingSystemFamily == OperatingSystemFamily.Windows)
			{
				text = "file://c:/Users/" + Environment.UserName + "/AppData/LocalLow/Hubert Moszka/SCP_ Secret Laboratory/output_log.txt";
			}
			else
			{
				if (SystemInfo.operatingSystemFamily != OperatingSystemFamily.Linux)
				{
					return;
				}
				text = "file://" + Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/unity3d/Hubert Moszka/SCP_ Secret Laboratory/Player.log";
			}
			if (File.Exists(text.Replace("file://", string.Empty)))
			{
				using (WWW wWW = new WWW(text))
				{
					string[] array = wWW.text.Trim().Split(Environment.NewLine.ToCharArray());
					foreach (GameObject line in lines)
					{
						UnityEngine.Object.Destroy(line);
					}
					lines.Clear();
					string[] array2 = array;
					foreach (string text2 in array2)
					{
						if (!string.IsNullOrEmpty(text2))
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(prefab, parent.transform);
							Text component = gameObject.GetComponent<Text>();
							component.text = text2.Trim();
							lines.Add(gameObject);
						}
					}
				}
			}
			scroll.velocity = new Vector2(0f, 99999f);
		}
		catch
		{
		}
	}
}
