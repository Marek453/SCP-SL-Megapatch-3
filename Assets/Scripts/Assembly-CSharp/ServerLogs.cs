using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ServerLogs : MonoBehaviour
{
	public enum ServerLogType
	{
		ConnectionUpdate = 0,
		RemoteAdminActivity_GameChanging = 1,
		RemoteAdminActivity_Misc = 2,
		KillLog = 3,
		GameEvent = 4,
		InternalMessage = 5
	}

	public enum Modules
	{
		Warhead = 0,
		Networking = 1,
		ClassChange = 2,
		Permissions = 3,
		Administrative = 4,
		Logger = 5
	}

	public class ServerLog
	{
		public string Content;

		public string Type;

		public string Module;

		public string Time;

		public bool Saved;
	}

	private static readonly string[] _txt = new string[6] { "Connection update", "Remote Admin", "Remote Admin - Misc", "Kill", "Game Event", "Internal" };

	private static readonly string[] _modulestxt = new string[6] { "Warhead", "Networking", "Class change", "Permissions", "Administrative", "Logger" };

	private readonly List<ServerLog> _logs = new List<ServerLog>();

	public static ServerLogs singleton;

	private int _port;

	private int _ready;

	private int _maxlen;

	private int _modulemaxlen;

	private bool _locked;

	private bool _queued;

	private string _roundStartTime;

	private void Awake()
	{
		singleton = this;
		_txt.ToList().ForEach(delegate(string txt)
		{
			_maxlen = Math.Max(_maxlen, txt.Length);
		});
		_modulestxt.ToList().ForEach(delegate(string txt)
		{
			_modulemaxlen = Math.Max(_modulemaxlen, txt.Length);
		});
		_ready++;
		AddLog(Modules.Logger, "Started logging.", ServerLogType.InternalMessage);
	}

	public static void AddLog(Modules module, string msg, ServerLogType type)
	{
		string time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");
		singleton._logs.Add(new ServerLog
		{
			Content = msg,
			Module = _modulestxt[(int)module],
			Type = _txt[(int)type],
			Time = time
		});
		if (NetworkServer.active)
		{
			singleton.StartCoroutine(singleton.AppendLog());
		}
	}

	private void Start()
	{
		_port = NetworkManager.singleton.networkPort;
		_roundStartTime = DateTime.Now.Year + "-" + ((DateTime.Now.Month >= 10) ? DateTime.Now.Month.ToString() : ("0" + DateTime.Now.Month)) + "-" + ((DateTime.Now.Day >= 10) ? DateTime.Now.Day.ToString() : ("0" + DateTime.Now.Day)) + " " + ((DateTime.Now.Hour >= 10) ? DateTime.Now.Hour.ToString() : ("0" + DateTime.Now.Hour)) + "." + ((DateTime.Now.Minute >= 10) ? DateTime.Now.Minute.ToString() : ("0" + DateTime.Now.Minute)) + "." + ((DateTime.Now.Second >= 10) ? DateTime.Now.Second.ToString() : ("0" + DateTime.Now.Second));
		_ready++;
	}

	private void OnDestroy()
	{
		if (NetworkServer.active)
		{
			AppendLog();
		}
	}

	private void Update()
	{
		if (_queued)
		{
			StartCoroutine(AppendLog());
		}
	}

	private IEnumerator AppendLog()
	{
		if (_locked || _ready < 2)
		{
			_queued = true;
			yield break;
		}
		_locked = true;
		_queued = false;
		if (!Directory.Exists(FileManager.AppFolder))
		{
			yield break;
		}
		if (!Directory.Exists(FileManager.AppFolder + "ServerLogs"))
		{
			Directory.CreateDirectory(FileManager.AppFolder + "ServerLogs");
		}
		if (!Directory.Exists(FileManager.AppFolder + "ServerLogs/" + _port))
		{
			Directory.CreateDirectory(FileManager.AppFolder + "ServerLogs/" + _port);
		}
		StreamWriter streamWriter = new StreamWriter(FileManager.AppFolder + "ServerLogs/" + _port + "/Round " + _roundStartTime + ".txt", true);
		string text = string.Empty;
		foreach (ServerLog log in _logs)
		{
			if (!log.Saved)
			{
				log.Saved = true;
				string text2 = text;
				text = text2 + log.Time + " | " + ToMax(log.Type, _maxlen) + " | " + ToMax(log.Module, _modulemaxlen) + " | " + log.Content + Environment.NewLine;
			}
		}
		streamWriter.Write(text);
		streamWriter.Close();
		_locked = false;
	}

	private static string ToMax(string text, int max)
	{
		while (text.Length < max)
		{
			text += " ";
		}
		return text;
	}
}
