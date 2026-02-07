using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cryptography;
using MEC;
using Org.BouncyCastle.Crypto;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameConsole
{
	public class Console : MonoBehaviour
	{
		[Serializable]
		public class CommandHint
		{
			public string name;

			public string shortDesc;

			[Multiline]
			public string fullDesc;
		}

		[Serializable]
		public class Value
		{
			public string key;

			public string value;

			public Value(string k, string v)
			{
				key = k;
				value = v;
			}
		}

		[Serializable]
		public class Log
		{
			public string text;

			public Color32 color;

			public bool nospace;

			public Log(string t, Color32 c, bool b)
			{
				text = t;
				color = c;
				nospace = b;
			}
		}

		private bool allwaysRefreshing;

		public static AsymmetricKeyParameter Publickey;

		internal static AsymmetricCipherKeyPair SessionKeys;

		private List<Log> logs = new List<Log>();

		private List<Value> values = new List<Value>();

		public CommandHint[] hints;

		public Text txt;

		public InputField cmdField;

		public GameObject console;

		public static Console singleton;

		private int scrollup;

		private int previous_scrlup;

		private string loadedLevel;

		private string content;

		private bool change;

		private string response = string.Empty;

		public List<Log> GetAllLogs()
		{
			return logs;
		}

		private void Awake()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (singleton == null)
			{
				singleton = this;
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
			}
		}

		private void Start()
		{
			AddLog("Hi there! Initializing console...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			AddLog("Done! Type 'help' to print the list of available commands.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			Timing.RunCoroutine(_RefreshPublicKey(), Segment.FixedUpdate);
			AddLog("Generatig session keys...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
			SessionKeys = ECDSA.GenerateKeys();
			AddLog("Session keys generated!", Color.green);
		}

		private void Update()
		{
			if (change)
			{
				txt.text = content;
				change = false;
			}
		}

		private void RefreshConsoleScreen()
		{
			change = true;
			bool flag = false;
			if (txt.text.Length > 15000)
			{
				logs.RemoveAt(0);
				flag = true;
			}
			if (txt == null)
			{
				return;
			}
			content = string.Empty;
			if (logs.Count > 0)
			{
				for (int i = 0; i < logs.Count - scrollup; i++)
				{
					string text = ((!logs[i].nospace) ? "\n\n" : "\n") + "<color=" + ColorToHex(logs[i].color) + ">" + logs[i].text + "</color>";
					if (text.Contains("@#{["))
					{
						string text2 = text.Remove(text.IndexOf("@#{["));
						string text3 = text.Remove(0, text.IndexOf("@#{[") + 4);
						text3 = text3.Remove(text3.Length - 12);
						foreach (Value value in values)
						{
							if (value.key == text3)
							{
								text = text2 + value.value + "</color>";
							}
						}
					}
					content += text;
				}
			}
			if (flag)
			{
				RefreshConsoleScreen();
			}
		}

		public void AddLog(string text, Color32 c, bool nospace = false)
		{
			response = response + text + Environment.NewLine;
			if (!nospace)
			{
				response += Environment.NewLine;
			}
			scrollup = 0;
			logs.Add(new Log(text, c, nospace));
			RefreshConsoleScreen();
		}

		private string ColorToHex(Color32 color)
		{
			string text = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
			return "#" + text;
		}

		public static GameObject FindConnectedRoot(NetworkConnection conn)
		{
			try
			{
				foreach (PlayerController playerController in conn.playerControllers)
				{
					if (playerController.gameObject.tag == "Player")
					{
						return playerController.gameObject;
					}
				}
			}
			catch
			{
				return null;
			}
			return null;
		}

		public string TypeCommand(string cmd)
		{
			response = string.Empty;
			string[] array = cmd.ToUpper().Split(' ');
			cmd = array[0];
			switch (cmd)
			{
			case "HELLO":
				AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			case "LENNY":
				AddLog("<size=450>( \u0361° \u035cʖ \u0361°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue));
				break;
			case "CONTACT":
			{
				GameObject[] array12 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array13 = array12;
				foreach (GameObject gameObject6 in array13)
				{
					if (gameObject6.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting contact email to server owner...", Color.yellow);
						gameObject6.GetComponent<CharacterClassManager>().CallCmdRequestContactEmail();
					}
				}
				break;
			}
			case "SRVCFG":
			{
				GameObject[] array8 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array9 = array8;
				foreach (GameObject gameObject4 in array9)
				{
					if (gameObject4.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting server config...", Color.yellow);
						gameObject4.GetComponent<CharacterClassManager>().CallCmdRequestServerConfig();
					}
				}
				break;
			}
			case "GROUPS":
			{
				GameObject[] array10 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array11 = array10;
				foreach (GameObject gameObject5 in array11)
				{
					if (gameObject5.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting server groups...", Color.yellow);
						gameObject5.GetComponent<CharacterClassManager>().CallCmdRequestServerGroups();
					}
				}
				break;
			}
			case "HIDETAG":
			{
				GameObject[] array27 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array28 = array27;
				foreach (GameObject gameObject14 in array28)
				{
					if (gameObject14.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Hidding your tag...", Color.yellow);
						gameObject14.GetComponent<CharacterClassManager>().CallCmdRequestHideTag();
					}
				}
				break;
			}
			case "SHOWTAG":
			case "TAG":
			{
				GameObject[] array31 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array32 = array31;
				foreach (GameObject gameObject18 in array32)
				{
					if (gameObject18.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting your local tag...", Color.yellow);
						gameObject18.GetComponent<CharacterClassManager>().CallCmdRequestShowTag(false);
					}
				}
				break;
			}
			case "GLOBALTAG":
			case "GTAG":
			{
				GameObject[] array23 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array24 = array23;
				foreach (GameObject gameObject12 in array24)
				{
					if (gameObject12.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting your global tag...", Color.yellow);
						gameObject12.GetComponent<CharacterClassManager>().CallCmdRequestShowTag(true);
					}
				}
				break;
			}
			case "GLOBALBAN":
			case "GBAN":
			case "SUPERBAN":
			{
				if (array.Length < 3 || (array[1].ToLower() != "nick" && array[1].ToLower() != "id"))
				{
					AddLog("Syntax: globalban <nick/id> <player to ban>", Color.red);
					break;
				}
				if (!File.Exists(FileManager.AppFolder + "StaffAPI.txt"))
				{
					AddLog("Staff API token not found on your computer!", Color.red);
					break;
				}
				GameObject[] array25 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array26 = array25;
				foreach (GameObject gameObject13 in array26)
				{
					if (gameObject13.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						AddLog("Requesting your global ban...", Color.yellow);
						gameObject13.GetComponent<QueryProcessor>().RequestGlobalBan(array[2], (!(array[1].ToLower() == "id")) ? 1 : 0);
					}
				}
				break;
			}
			case "CONFIRM":
			{
				GameObject[] array20 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array21 = array20;
				foreach (GameObject gameObject10 in array21)
				{
					if (gameObject10.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						gameObject10.GetComponent<QueryProcessor>().ConfirmGlobalBanning();
					}
				}
				break;
			}
			case "OVERWATCH":
			case "OVR":
			{
				GameObject[] array14 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array15 = array14;
				foreach (GameObject gameObject7 in array15)
				{
					if (gameObject7.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						if (array.Length == 1)
						{
							gameObject7.GetComponent<ServerRoles>().CallCmdToggleOverwatch();
						}
						else if (array[1] == "1" || array[1].ToLower() == "true" || array[1].ToLower() == "enable" || array[1].ToLower() == "on")
						{
							gameObject7.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(true);
						}
						else if (array[1] == "0" || array[1].ToLower() == "false" || array[1].ToLower() == "disable" || array[1].ToLower() == "off")
						{
							gameObject7.GetComponent<ServerRoles>().CallCmdSetOverwatchStatus(false);
						}
						else
						{
							AddLog("Unknown status: " + array[1], Color.red);
						}
					}
				}
				break;
			}
			case "GIVE":
			{
				if (!(from player in GameObject.FindGameObjectsWithTag("Player")
					select player.GetComponent<PlayerStats>()).Any((PlayerStats nid) => nid.isLocalPlayer && nid.isServer))
				{
					AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
				int result4 = 0;
				if (array.Length >= 2 && int.TryParse(array[1], out result4))
				{
					string text9 = "offline";
					GameObject[] array33 = GameObject.FindGameObjectsWithTag("Player");
					GameObject[] array34 = array33;
					foreach (GameObject gameObject19 in array34)
					{
						if (!gameObject19.GetComponent<NetworkIdentity>().isLocalPlayer)
						{
							continue;
						}
						text9 = "online";
						Inventory component6 = gameObject19.GetComponent<Inventory>();
						if (component6 != null)
						{
							if (component6.availableItems.Length > result4)
							{
								component6.AddNewItem(result4);
								text9 = "none";
							}
							else
							{
								AddLog("Failed to add ITEM#" + result4.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
						}
					}
					if (text9 == "offline" || text9 == "online")
					{
						AddLog((!(text9 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("ITEM#" + result4.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "ROUNDRESTART":
			{
				bool flag4 = false;
				GameObject[] array22 = GameObject.FindGameObjectsWithTag("Player");
				foreach (GameObject gameObject11 in array22)
				{
					PlayerStats component5 = gameObject11.GetComponent<PlayerStats>();
					if (component5.isLocalPlayer && component5.isServer)
					{
						flag4 = true;
						AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						component5.Roundrestart();
					}
				}
				if (!flag4)
				{
					AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "ITEMLIST":
			{
				string text2 = "offline";
				GameObject[] array4 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array5 = array4;
				foreach (GameObject gameObject2 in array5)
				{
					int result = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out result))
					{
						AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						return response;
					}
					if (!gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						continue;
					}
					text2 = "online";
					Inventory component = gameObject2.GetComponent<Inventory>();
					if (!(component != null))
					{
						continue;
					}
					text2 = "none";
					if (result < 1)
					{
						AddLog("Page '" + result + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						RefreshConsoleScreen();
						return response;
					}
					Item[] availableItems = component.availableItems;
					for (int l = 10 * (result - 1); l < 10 * result; l++)
					{
						if (10 * (result - 1) > availableItems.Length)
						{
							AddLog("Page '" + result + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							break;
						}
						if (l >= availableItems.Length)
						{
							break;
						}
						AddLog("ITEM#" + l.ToString("000") + " : " + availableItems[l].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
				}
				if (text2 != "none")
				{
					AddLog((!(text2 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "BAN":
			{
				if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
				{
					break;
				}
				if (array.Length < 3)
				{
					AddLog("Syntax: BAN [player kick / ip] [minutes]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					foreach (NetworkConnection connection in NetworkServer.connections)
					{
						string text8 = string.Empty;
						GameObject gameObject15 = FindConnectedRoot(connection);
						if (gameObject15 != null)
						{
							text8 = gameObject15.GetComponent<NicknameSync>().myNick;
						}
						if (text8 == string.Empty)
						{
							AddLog("Player :: " + connection.address, new Color32(160, 128, 128, byte.MaxValue), true);
						}
						else
						{
							AddLog("Player :: " + text8 + " :: " + connection.address, new Color32(128, 160, 128, byte.MaxValue), true);
						}
					}
					break;
				}
				int result3 = 0;
				if (int.TryParse(array[2], out result3))
				{
					bool flag5 = false;
					foreach (NetworkConnection connection2 in NetworkServer.connections)
					{
						GameObject gameObject16 = FindConnectedRoot(connection2);
						if (connection2.address.ToUpper().Contains(array[1]) || (gameObject16 != null && gameObject16.GetComponent<NicknameSync>().myNick.ToUpper().Contains(array[1])))
						{
							flag5 = true;
							PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(gameObject16, result3, string.Empty);
							AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						}
					}
					if (!flag5)
					{
						AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
				}
				else
				{
					AddLog("Parse error: [minutes] - has to be an integer.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			}
			case "CLS":
			case "CLEAR":
				logs.Clear();
				RefreshConsoleScreen();
				break;
			case "QUIT":
			case "EXIT":
				logs.Clear();
				RefreshConsoleScreen();
				AddLog("<size=50>GOODBYE!</size>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				RefreshConsoleScreen();
				Invoke("QuitGame", 1f);
				break;
			case "HELP":
			{
				if (array.Length > 1)
				{
					string text = array[1];
					CommandHint[] array2 = hints;
					foreach (CommandHint commandHint in array2)
					{
						if (commandHint.name == text)
						{
							AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
							RefreshConsoleScreen();
							return response;
						}
					}
					AddLog("Help for command '" + array[1] + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					RefreshConsoleScreen();
					return response;
				}
				AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				CommandHint[] array3 = hints;
				foreach (CommandHint commandHint2 in array3)
				{
					AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), true);
				}
				AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
				RefreshConsoleScreen();
				break;
			}
			case "REFRESHFIX":
				allwaysRefreshing = !allwaysRefreshing;
				AddLog("Console log refresh mode: " + ((!allwaysRefreshing) ? "OPTIMIZED" : "FIXED"), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			case "COLOR":
			case "COLORS":
			{
				bool flag2 = array.Length > 1 && array[1].ToUpper() == "LIST";
				bool flag3 = (array.Length > 1 && array[1].ToUpper() == "ALL") || (array.Length > 2 && array[2].ToUpper() == "ALL");
				GameObject[] array16 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array17 = array16;
				foreach (GameObject gameObject8 in array17)
				{
					ServerRoles component3 = gameObject8.GetComponent<ServerRoles>();
					if (!component3.isLocalPlayer)
					{
						continue;
					}
					AddLog("Available colors:", Color.gray);
					string text5 = string.Empty;
					ServerRoles.NamedColor[] namedColors = component3.NamedColors;
					foreach (ServerRoles.NamedColor namedColor in namedColors)
					{
						if (!namedColor.Restricted || flag3)
						{
							if (flag2)
							{
								AddLog("<color=#" + namedColor.ColorHex + ">" + namedColor.Name + " - #" + namedColor.ColorHex + "</color>", Color.white);
							}
							else
							{
								string text6 = text5;
								text5 = text6 + "<color=#" + namedColor.ColorHex + ">" + namedColor.Name + "</color>    ";
							}
						}
					}
					if (!flag2)
					{
						AddLog(text5, Color.white);
					}
				}
				break;
			}
			case "VALUE":
			{
				if (array.Length < 2)
				{
					AddLog("The second argument cannot be <i>null</i>!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
				bool flag = false;
				string text4 = array[1];
				foreach (Value value in values)
				{
					if (value.key == text4)
					{
						flag = true;
						AddLog("The value of " + text4 + " is: @#{[" + text4 + "}]#@", new Color32(50, 70, 100, byte.MaxValue));
					}
				}
				if (!flag)
				{
					AddLog("Key " + text4 + " not found!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "SEED":
			{
				GameObject gameObject = GameObject.Find("Host");
				AddLog("Map seed is: <b>" + ((!(gameObject == null)) ? gameObject.GetComponent<RandomSeedSync>().seed.ToString() : "NONE") + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				break;
			}
			case "SHOWRIDS":
			{
				GameObject[] array29 = GameObject.FindGameObjectsWithTag("RoomID");
				GameObject[] array30 = array29;
				foreach (GameObject gameObject17 in array30)
				{
					gameObject17.GetComponentsInChildren<MeshRenderer>()[0].enabled = !gameObject17.GetComponentsInChildren<MeshRenderer>()[0].enabled;
					gameObject17.GetComponentsInChildren<MeshRenderer>()[1].enabled = !gameObject17.GetComponentsInChildren<MeshRenderer>()[1].enabled;
				}
				if (array29.Length > 0)
				{
					AddLog("Show RIDS: " + array29[0].GetComponentInChildren<MeshRenderer>().enabled, new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				else
				{
					AddLog("There are no RIDS!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "CLASSLIST":
			{
				string text7 = "offline";
				GameObject[] array18 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array19 = array18;
				foreach (GameObject gameObject9 in array19)
				{
					int result2 = 1;
					if (array.Length >= 2 && !int.TryParse(array[1], out result2))
					{
						AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						return response;
					}
					if (!gameObject9.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						continue;
					}
					text7 = "online";
					CharacterClassManager component4 = gameObject9.GetComponent<CharacterClassManager>();
					if (!(component4 != null))
					{
						continue;
					}
					text7 = "none";
					if (result2 < 1)
					{
						AddLog("Page '" + result2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
						RefreshConsoleScreen();
						return response;
					}
					Class[] klasy = component4.klasy;
					for (int num7 = 10 * (result2 - 1); num7 < 10 * result2; num7++)
					{
						if (10 * (result2 - 1) > klasy.Length)
						{
							AddLog("Page '" + result2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							break;
						}
						if (num7 >= klasy.Length)
						{
							break;
						}
						AddLog("CLASS#" + num7.ToString("000") + " : " + klasy[num7].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
				}
				if (text7 != "none")
				{
					AddLog((!(text7 == "offline")) ? "Player inventory script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				break;
			}
			case "RANGE":
			{
				string text3 = "offline";
				GameObject[] array6 = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array7 = array6;
				foreach (GameObject gameObject3 in array7)
				{
					if (gameObject3.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						text3 = "online";
						ShootingRange component2 = gameObject3.GetComponent<ShootingRange>();
						if (component2 != null)
						{
							text3 = "none";
							component2.isOnRange = true;
						}
					}
				}
				if (text3 == "offline" || text3 == "online")
				{
					AddLog((!(text3 == "offline")) ? "Player range script couldn't be find!" : "You cannot use that command if you are not playing on any server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				}
				else
				{
					AddLog("<b>Shooting range</b> is now available!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				}
				break;
			}
			case "WARHEAD":
			{
				AlphaWarheadController host = AlphaWarheadController.host;
				if (array.Length == 1)
				{
					AddLog("Synax: warhead (status|detonate|cancel|enable|disable)", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
				switch (array[1].ToLower())
				{
				case "status":
					if (host.detonated || host.timeToDetonation == 0f)
					{
						AddLog("Warhead has been detonated.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (host.inProgress)
					{
						AddLog("Detonation is in progress.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (!AlphaWarheadOutsitePanel.nukeside.enabled)
					{
						AddLog("Warhead is disabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else if (host.timeToDetonation > AlphaWarheadController.host.RealDetonationTime())
					{
						AddLog("Warhead is restarting.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					else
					{
						AddLog("Warhead is ready to detonation.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					}
					break;
				case "detonate":
					AlphaWarheadController.host.StartDetonation();
					AddLog("Detonation sequence started.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "cancel":
					AlphaWarheadController.host.CancelDetonation(null);
					AddLog("Detonation has been canceled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "enable":
					AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
					AddLog("Warhead has been enabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				case "disable":
					AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
					AddLog("Warhead has been disabled.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				default:
					AddLog("WARHEAD: Unknown subcommand.", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
					break;
				}
				break;
			}
			case "CONFIG":
				if (array.Length < 2)
				{
					TypeCommand("HELP CONFIG");
					break;
				}
				switch (array[1])
				{
				case "RELOAD":
				case "R":
				case "RLD":
					ConfigFile.ReloadGameConfig();
					ServerStatic.RolesConfig = new YamlConfig(ServerStatic.RolesConfigPath);
					ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig);
					AddLog("Configuration file <b>successfully reloaded</b>. New settings will be applied on <b>your</b> server in <b>next</b> round.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					break;
				case "PATH":
					AddLog("Configuration file path: <i>" + ConfigFile.ConfigPath + "</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					AddLog("<i>No visible drive letter means the root game directory.</i>", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					break;
				case "VALUE":
					if (array.Length < 3)
					{
						AddLog("Please enter key name in the third argument. (CONFIG VALUE <i>KEYNAME</i>)", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
					}
					else
					{
						AddLog("The value of <i>'" + array[2] + "'</i> is: " + ConfigFile.ServerConfig.GetString(array[2], "<color=ff0>DENIED: Entered key does not exists</color>"), new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
					}
					break;
				}
				break;
			default:
				AddLog("Command " + cmd + " does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
				break;
			}
			return response;
		}

		public void ProceedButton()
		{
			if (cmdField.text != string.Empty)
			{
				TypeCommand(cmdField.text);
			}
			cmdField.text = string.Empty;
			EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
		}

		private void LateUpdate()
		{
			if (Input.GetKeyDown(KeyCode.Return))
			{
				ProceedButton();
			}
			else if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				ToggleConsole();
			}
			else if (Input.GetKey(KeyCode.Escape) && console.activeSelf)
			{
				ToggleConsole();
			}
			scrollup += Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel") * 10f);
			scrollup = ((logs.Count > 0) ? Mathf.Clamp(scrollup, 0, logs.Count - 1) : 0);
			if (previous_scrlup != scrollup)
			{
				previous_scrlup = scrollup;
				RefreshConsoleScreen();
			}
			Scene activeScene = SceneManager.GetActiveScene();
			if (activeScene.name != loadedLevel)
			{
				loadedLevel = activeScene.name;
				AddLog("Scene Manager: Loaded scene '" + activeScene.name + "' [" + activeScene.path + "]", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
				RefreshConsoleScreen();
			}
			if (allwaysRefreshing)
			{
				RefreshConsoleScreen();
			}
		}

		public void ToggleConsole()
		{
			CursorManager.consoleOpen = !console.activeSelf;
			cmdField.text = string.Empty;
			console.SetActive(!console.activeSelf);
			if (PlayerManager.singleton != null)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
				GameObject[] array2 = array;
				foreach (GameObject gameObject in array2)
				{
					if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
					{
						FirstPersonController component = gameObject.GetComponent<FirstPersonController>();
						if (component != null)
						{
							component.usingConsole = console.activeSelf;
						}
					}
				}
			}
			if (console.activeSelf)
			{
				EventSystem.current.SetSelectedGameObject(cmdField.gameObject);
			}
		}

		private IEnumerator<float> _RefreshPublicKey()
		{
			WWW www = new WWW(CentralServer.URL + "publickey.php");
			yield return Timing.WaitUntilDone(www);
			try
			{
				Publickey = ECDSA.PublicKeyFromString(www.text);
				ServerConsole.Publickey = Publickey;
				AddLog("Downloaded public key from central server.\nSHA256 of public key: " + Sha.HashToString(Sha.Sha256(www.text)), Color.green);
			}
			catch
			{
				AddLog("Can't refresh central server public key!", Color.red);
			}
		}

		private void QuitGame()
		{
			Application.Quit();
		}
	}
}
