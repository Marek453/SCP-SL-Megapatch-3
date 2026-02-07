using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class DebugManager : MonoBehaviour
	{
		private static DebugManager singleton;

		public Font font;

		public RectTransform logRect;

		public float logHeight = 40f;

		private List<DebugEntry> Entries = new List<DebugEntry>();

		private void Awake()
		{
			if (singleton == null)
			{
				singleton = this;
			}
			else if (singleton != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		private void Start()
		{
			CreateEntries();
		}

		private void OnEnable()
		{
			Application.logMessageReceived += OnLog;
		}

		private void OnDisable()
		{
			Application.logMessageReceived -= OnLog;
		}

		public static void Log(string Title, string Log)
		{
			if (!(singleton != null))
			{
				return;
			}
			List<DebugEntry> entries = singleton.Entries;
			foreach (DebugEntry item in entries)
			{
				if (item.title == Title)
				{
					item.Update(Title, Log);
					return;
				}
			}
			for (int num = entries.Count - 1; num > 0; num--)
			{
				entries[num].Update(entries[num - 1].title, entries[num - 1].log);
			}
			entries[0].Update(Title, Log);
		}

		private void OnLog(string logString, string stackTrace, LogType type)
		{
			Log(type.ToString(), logString);
		}

		private void CreateEntries()
		{
			int num = Mathf.FloorToInt(logRect.rect.height / logHeight);
			for (int i = 0; i < num; i++)
			{
				Entries.Add(new DebugEntry(font, logRect, logHeight, i));
			}
		}
	}
}
