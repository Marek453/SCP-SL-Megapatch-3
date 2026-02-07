using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class MTFRespawn : NetworkBehaviour
{
	public GameObject ciTheme;

	public GameObject MtfTheme;

	private ChopperAutostart mtf_a;

	[Range(30f, 1000f)]
	public int minMtfTimeToRespawn = 200;

	[Range(40f, 1200f)]
	public int maxMtfTimeToRespawn = 400;

	public float CI_Time_Multiplier = 2f;

	public float CI_Percent = 20f;

	[Range(2f, 15f)]
	[Space(10f)]
	public int maxRespawnAmount = 15;

	public float timeToNextRespawn;

	public bool nextWaveIsCI;

	public List<GameObject> playersToNTF = new List<GameObject>();

	private bool loaded;

	private bool chopperStarted;
	private CharacterClassManager characterClassManager;
	[SyncVar]
	public bool isSH;

	[HideInInspector]
	public float respawnCooldown;

	private void Start()
	{
		minMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("minimum_MTF_time_to_spawn", 200);
		maxMtfTimeToRespawn = ConfigFile.ServerConfig.GetInt("maximum_MTF_time_to_spawn", 400);
		CI_Percent = ConfigFile.ServerConfig.GetInt("ci_respawn_percent", 35);
		characterClassManager = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		if (TutorialManager.status)
		{
			return;
		}
		if (respawnCooldown >= 0f)
		{
			respawnCooldown -= Time.deltaTime;
		}
		if (base.name != "Host" || !base.isLocalPlayer)
		{
			return;
		}
		if (mtf_a == null)
		{
			mtf_a = Object.FindObjectOfType<ChopperAutostart>();
		}
		timeToNextRespawn -= Time.deltaTime;
		if (timeToNextRespawn < ((!nextWaveIsCI) ? 18f : 13.5f) && !loaded)
		{
			loaded = true;
			GameObject[] players = PlayerManager.singleton.players;
			GameObject[] array = players;
			foreach (GameObject gameObject in array)
			{
				if (gameObject.GetComponent<CharacterClassManager>().curClass == 2)
				{
					chopperStarted = true;
					if (nextWaveIsCI && !AlphaWarheadController.host.detonated)
					{
						SummonVan();
					}
					else
					{
						SummonChopper(true);
					}
					break;
				}
			}
		}
		if (timeToNextRespawn < 0f)
		{
			loaded = false;
			if (characterClassManager.roundStarted)
			{
				SummonChopper(false);
			}
			if (chopperStarted)
			{
				respawnCooldown = 30f;
				RespawnDeadPlayers();
			}
			nextWaveIsCI = (float)Random.Range(0, 100) <= CI_Percent;
			timeToNextRespawn = (float)Random.Range(minMtfTimeToRespawn, maxMtfTimeToRespawn) * ((!nextWaveIsCI) ? 1f : (1f / CI_Time_Multiplier));
			chopperStarted = false;
		}
	}

	private void RespawnDeadPlayers()
	{
		int num = 0;
		List<GameObject> list = PlayerManager.singleton.players.Where((GameObject item) => item.GetComponent<CharacterClassManager>().curClass == 2 && !item.GetComponent<ServerRoles>().OverwatchEnabled).ToList();
		while (list.Count > maxRespawnAmount)
		{
			list.RemoveAt(Random.Range(0, list.Count));
		}
		if (nextWaveIsCI && AlphaWarheadController.host.detonated)
		{
			nextWaveIsCI = false;
		}
		int i = Random.Range(0, 100);
		foreach (GameObject item in list)
		{
			if (!(item == null))
			{
				num++;
				if (nextWaveIsCI)
				{
					if (i > 70)
					{
						isSH = true;
						characterClassManager.SetPlayersClass(18, item);
					}
					else
					{
						characterClassManager.SetPlayersClass(8, item);
					}

				}
				else
				{
					playersToNTF.Add(item);
				}
			}
		}
		if (num > 0)
		{
			ServerLogs.AddLog(ServerLogs.Modules.ClassChange, ((!nextWaveIsCI) ? "MTF" : "Chaos Insurgency") + " respawned!", ServerLogs.ServerLogType.GameEvent);
			if (nextWaveIsCI)
			{
				Invoke("CmdDelayCIAnnounc", 1f);
			}
			else
			{
				PlayAnnonc();
				Invoke("CmdDelayMTFAnnounc", 1f);
			}
		}
		SummonNTF();
	}
	public void SummonNTF()
	{
		if (playersToNTF.Count <= 0)
		{
			return;
		}
		SetUnit(playersToNTF.ToArray());
		for (int i = 0; i < playersToNTF.Count; i++)
		{
			if (i == 0)
			{
				characterClassManager.SetPlayersClass(12, playersToNTF[i]);
			}
			else if (i <= 3)
			{
				characterClassManager.SetPlayersClass(11, playersToNTF[i]);
			}
			else
			{
				characterClassManager.SetPlayersClass(13, playersToNTF[i]);
			}
		}
		playersToNTF.Clear();
	}

	[ServerCallback]
	private void SetUnit(GameObject[] ply)
	{
		if (NetworkServer.active)
		{
			int unit = GetComponent<NineTailedFoxUnits>().NewName();
			foreach (GameObject gameObject in ply)
			{
				gameObject.GetComponent<CharacterClassManager>().SetUnit(unit);
			}
		}
	}

	[ServerCallback]
	private void SummonChopper(bool state)
	{
		if (NetworkServer.active)
		{
			mtf_a.SetState(state);
		}
	}

	[ServerCallback]
	private void SummonVan()
	{
		if (NetworkServer.active)
		{
			RpcVan();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcVan()
	{
		GameObject.Find("CIVanArrive").GetComponent<Animator>().SetTrigger("Arrive");
	}

	private void CmdDelayCIAnnounc()
	{
		PlayAnnoncCI();
	}

	private void CmdDelayMTFAnnounc()
	{
		PlayAnnoncMTF();
	}

	[ServerCallback]
	private void PlayAnnonc()
	{
		if (NetworkServer.active)
		{
			RpcAnnounc();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnounc()
	{
		GameObject.Find("MTF_Announc").GetComponent<AudioSource>().Play();
	}

	[ServerCallback]
	private void PlayAnnoncCI()
	{
		if (NetworkServer.active)
		{
			RpcAnnouncCI();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnouncCI()
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer)
			{
				Team team = component.klasy[component.curClass].team;
				if (team == Team.CDP || team == Team.CHI || component.GetComponent<ServerRoles>().OverwatchEnabled)
				{
					Instantiate(ciTheme);
				}
			}
		}
	}

	[ServerCallback]
	private void PlayAnnoncMTF()
	{
		if (NetworkServer.active)
		{
			RpcAnnouncMTF();
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcAnnouncMTF()
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.isLocalPlayer)
			{
				Team team = component.klasy[component.curClass].team;
				if (team == Team.MTF || component.GetComponent<ServerRoles>().OverwatchEnabled)
				{
					Object.Instantiate(MtfTheme);
				}
			}
		}
	}
}
