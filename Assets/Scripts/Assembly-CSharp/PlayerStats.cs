using System;
using System.Collections;
using System.Runtime.InteropServices;
using Dissonance.Integrations.UNet_HLAPI;
using RemoteAdmin;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerStats : NetworkBehaviour
{
	[Serializable]
	public struct HitInfo
	{
		public float amount;

		public string tool;

		public int time;

		public string attacker;

		public int plyID;

		public HitInfo(float amnt, string attackerName, string weapon, int attackerID)
		{
			amount = amnt;
			tool = weapon;
			attacker = attackerName;
			plyID = attackerID;
			time = ServerTime.time;
		}

		public GameObject GetPlayerObject()
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject in players)
			{
				if (gameObject.GetComponent<QueryProcessor>().PlayerId == plyID)
				{
					return gameObject;
				}
			}
			return null;
		}
	}

	public HitInfo lastHitInfo = new HitInfo(0f, "NONE", "NONE", 0);

	[SyncVar(hook = "SetHPAmount")]
	public int health;

	public int maxHP;

	public bool used914;

	private UserMainInterface ui;

	[HideInInspector]
	public CharacterClassManager ccm;

	private static Lift[] lifts;

	private bool pocket_cleanup;

	public Transform[] grenadePoints;

	private float killstreak_time;

	private int killstreak;

	private static int kCmdCmdSelfDeduct;

	private static int kCmdCmdTesla;

	private static int kTargetRpcTargetAchieve;

	private static int kTargetRpcTargetStats;

	private static int kTargetRpcTargetOofEffect;

	private static int kRpcRpcRoundrestart;

	public int Networkhealth
	{
		get
		{
			return health;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref health;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetHPAmount(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	private void Start()
	{
		pocket_cleanup = ConfigFile.ServerConfig.GetBool("SCP106_CLEANUP");
		ccm = GetComponent<CharacterClassManager>();
		ui = UserMainInterface.singleton;
		if (lifts.Length == 0)
		{
			lifts = UnityEngine.Object.FindObjectsOfType<Lift>();
		}
	}

	public float GetHealthPercent()
	{
		if (ccm.curClass < 0)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - (float)health / (float)ccm.klasy[ccm.curClass].maxHP);
	}

	[Command(channel = 2)]
	public void CmdSelfDeduct(HitInfo info)
	{
		HurtPlayer(info, base.gameObject);
	}

	public bool Explode(bool inWarhead)
	{
		bool flag = health > 0 && (inWarhead || base.transform.position.y < 900f);
		if (ccm.curClass == 3)
		{
			Scp106PlayerScript component = GetComponent<Scp106PlayerScript>();
			component.DeletePortal();
			if (component.goingViaThePortal)
			{
				flag = true;
			}
		}
		if (flag)
		{
			HurtPlayer(new HitInfo(999999f, "WORLD", "NUKE", 0), base.gameObject);
		}
		return flag;
	}

	private void Update()
	{
		if (base.isLocalPlayer && ccm.curClass != 2)
		{
			ui.SetHP(health, maxHP);
		}
		if (base.isLocalPlayer)
		{
			if (Input.GetKeyDown(KeyCode.B))
			{
				base.GetComponent<CharacterController>().enabled = false;
				base.transform.position = PlayerManager.singleton.players[1].transform.position;
				base.GetComponent<CharacterController>().enabled = true;
			}
			ui.hpOBJ.SetActive(ccm.curClass != 2);
		}
	}

	[Command(channel = 2)]
	public void CmdTesla()
	{
		HurtPlayer(new HitInfo(UnityEngine.Random.Range(100, 200), GetComponent<HlapiPlayer>().PlayerId, "TESLA", 0), base.gameObject);
	}
	[Command]
	public void CmdSTartDie()
	{
		StartCoroutine(StartDIE());
	}

	IEnumerator StartDIE()
	{
		while (true)
		{
			if (GetComponent<PlayerStats>().health >= 2f)
			{
				GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(1, "", "", 0), base.gameObject);
			}
			else
			{
				Vector3 vector3 = base.transform.position;
				CmdSetClass();
				GetComponent<PlyMovementSync>().SetPosition(vector3);
				GetComponent<PlayerStats>().SetHPAmount(GetComponent<PlayerStats>().ccm.klasy[GetComponent<PlayerStats>().ccm.curClass].maxHP);
				StopAllCoroutines();
				yield break;
			}
			yield return new WaitForSeconds(0.2f);
		}
	}
	[Command]
	void CmdSetClass()
	{
		base.GetComponent<CharacterClassManager>().SetPlayersClass(20, base.gameObject);
	}

	public void SetHPAmount(int hp)
	{
		Networkhealth = hp;
	}

	public bool HurtPlayer(HitInfo info, GameObject go)
	{
		bool flag = false;
		info.amount = Mathf.Abs(info.amount);
		if (info.amount > 999999f)
		{
			info.amount = 999999f;
		}
		PlayerStats component = go.GetComponent<PlayerStats>();
		CharacterClassManager component2 = go.GetComponent<CharacterClassManager>();
		component.Networkhealth = component.health - Mathf.CeilToInt(info.amount);
		component.lastHitInfo = info;
		if (component.health < 1 && component2.curClass != 2)
		{
			if (!flag && RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60)
			{
				CallTargetAchieve(component2.connectionToClient, "wowreally");
			}
			flag = true;
			if (component2.curClass == 9 && go.GetComponent<Scp096PlayerScript>().enraged == Scp096PlayerScript.RageState.Panic)
			{
				CallTargetAchieve(component2.connectionToClient, "unvoluntaryragequit");
			}
			else if (info.tool == "POCKET")
			{
				CallTargetAchieve(component2.connectionToClient, "newb");
			}
			else if (info.tool == "SCP:173")
			{
				CallTargetAchieve(component2.connectionToClient, "firsttime");
			}
			else if (info.tool == "FRAG" && info.plyID == go.GetComponent<QueryProcessor>().PlayerId)
			{
				CallTargetAchieve(component2.connectionToClient, "iwanttobearocket");
			}
			else if (info.tool.ToUpper().Contains("WEAPON"))
			{
				if (component2.curClass == 6 && component2.GetComponent<Inventory>().curItem >= 0 && component2.GetComponent<Inventory>().curItem <= 11)
				{
					GameObject playerObject = info.GetPlayerObject();
					if (playerObject != null && playerObject.GetComponent<CharacterClassManager>().curClass == 1)
					{
						CallTargetAchieve(component2.connectionToClient, "betrayal");
					}
				}
				if (Time.realtimeSinceStartup - killstreak_time > 30f || killstreak == 0)
				{
					killstreak = 0;
					killstreak_time = Time.realtimeSinceStartup;
				}
				if (GetComponent<WeaponManager>().GetShootPermission(component2, true))
				{
					killstreak++;
				}
				if (killstreak > 5)
				{
					CallTargetAchieve(base.connectionToClient, "pewpew");
				}
				if ((ccm.klasy[ccm.curClass].team == Team.MTF || ccm.klasy[ccm.curClass].team == Team.RSC) && component2.curClass == 1)
				{
					CallTargetStats(base.connectionToClient, "dboys_killed", "justresources", 50);
				}
				if (ccm.klasy[ccm.curClass].team == Team.RSC && ccm.klasy[component2.curClass].team == Team.SCP)
				{
					CallTargetAchieve(base.connectionToClient, "timetodoitmyself");
				}
			}
			ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Player " + go.GetComponent<NicknameSync>().myNick + " (" + go.GetComponent<CharacterClassManager>().SteamId + ") killed by " + info.attacker + " using " + info.tool + ".", ServerLogs.ServerLogType.KillLog);
			if (!pocket_cleanup || info.tool != "POCKET")
			{
				go.GetComponent<Inventory>().ServerDropAll();
				if (component2.curClass >= 0)
				{
					GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, component2.curClass, info, component2.klasy[component2.curClass].team != Team.SCP, go.GetComponent<HlapiPlayer>().PlayerId, go.GetComponent<NicknameSync>().myNick);
				}
			}
			else
			{
				go.GetComponent<Inventory>().Clear();
			}
			if (component2.curClass != -1)
			{
				if (component2.isLocalPlayer)
				{
					if (component2.klasy[component2.curClass].team == Team.SCP)
					{
						CmdAnnounce(component2.curClass);
					}
				}
			}
			component2.NetworkdeathPosition = go.transform.position;
			component.SetHPAmount(100);
			if (go.GetComponent<Sco008PlayerScript>().Infect)
			{
				Vector3 OldPos = go.transform.position;
				component2.SetClassID(20);
				go.GetComponent<PlyMovementSync>().SetPosition(OldPos);
				go.GetComponent<Sco008PlayerScript>().SetInfect(false);
			}
			else
			{
				component2.SetClassID(2);
			}
			if (TutorialManager.status)
			{
				PlayerManager.localPlayer.GetComponent<TutorialManager>().KillNPC();
			}
		}
		else
		{
			Vector3 vector = Vector3.zero;
			float num = 40f;
			if (info.tool.StartsWith("Weapon:"))
			{
				GameObject playerOfID = GetPlayerOfID(info.plyID);
				if (playerOfID != null)
				{
					vector = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
					Debug.Log(vector);
					num = 100f;
				}
			}
			if (component2.klasy[component2.curClass].fullName.Contains("939"))
			{
				component2.GetComponent<Scp939PlayerScript>().NetworkspeedMultiplier = 1.25f;
			}
			//TargetOofEffect(go.GetComponent<NetworkIdentity>().connectionToClient, vector, Mathf.Clamp01(info.amount / num));
		}
		return flag;
	}

	[Command]
	void CmdAnnounce(int id)
	{
		RpcAnnounce(id);
	}
	[ClientRpc]
	void RpcAnnounce(int id)
	{
		if (AnnounceManager.instance.StartAnnounce(ccm.klasy[id].fullName) != null)
		{
			NetworkServer.Spawn(AnnounceManager.instance.StartAnnounce(ccm.klasy[id].fullName));
		}
	}

	[TargetRpc]
	public void TargetAchieve(NetworkConnection conn, string key)
	{
		AchievementManager.Achieve(key);
	}

	[TargetRpc]
	public void TargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
		AchievementManager.StatsProgress(key, targetAchievement, maxValue);
	}

	private GameObject GetPlayerOfID(int id)
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<QueryProcessor>().PlayerId == id)
			{
				return gameObject;
			}
		}
		return null;
	}

	[Command]
	private void TargetOofEffect(NetworkConnection conn, Vector3 pos, float overall)
	{
		//OOF_Controller.singleton.AddBlood(pos, overall);
	}

	[ClientRpc(channel = 7)]
	private void RpcRoundrestart()
	{
		if (!base.isServer)
		{
			CustomNetworkManager customNetworkManager = UnityEngine.Object.FindObjectOfType<CustomNetworkManager>();
			customNetworkManager.reconnect = true;
			Invoke("ChangeLevel", 0.5f);
		}
	}

	public void Roundrestart()
	{
		CallRpcRoundrestart();
		Invoke("ChangeLevel", 2.5f);
	}

	private void ChangeLevel()
	{
		if (base.isServer)
		{
			GC.Collect();
			NetworkManager.singleton.ServerChangeScene(NetworkManager.singleton.onlineScene);
		}
		else
		{
			NetworkManager.singleton.StopClient();
		}
	}

	static PlayerStats()
	{
		lifts = new Lift[0];
		kCmdCmdSelfDeduct = -2147454163;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), kCmdCmdSelfDeduct, InvokeCmdCmdSelfDeduct);
		kCmdCmdTesla = -1109720487;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerStats), kCmdCmdTesla, InvokeCmdCmdTesla);
		kRpcRpcRoundrestart = 907411477;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kRpcRpcRoundrestart, InvokeRpcRpcRoundrestart);
		kTargetRpcTargetAchieve = 1310991230;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetAchieve, InvokeRpcTargetAchieve);
		kTargetRpcTargetStats = 662062348;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetStats, InvokeRpcTargetStats);
		kTargetRpcTargetOofEffect = -1463723612;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerStats), kTargetRpcTargetOofEffect, InvokeRpcTargetOofEffect);
		NetworkCRC.RegisterBehaviour("PlayerStats", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSelfDeduct(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSelfDeduct called on client.");
		}
		else
		{
			((PlayerStats)obj).CmdSelfDeduct(GeneratedNetworkCode._ReadHitInfo_PlayerStats(reader));
		}
	}

	protected static void InvokeCmdCmdTesla(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTesla called on client.");
		}
		else
		{
			((PlayerStats)obj).CmdTesla();
		}
	}

	public void CallCmdSelfDeduct(HitInfo info)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSelfDeduct called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSelfDeduct(info);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSelfDeduct);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WriteHitInfo_PlayerStats(networkWriter, info);
		SendCommandInternal(networkWriter, 2, "CmdSelfDeduct");
	}

	public void CallCmdTesla()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTesla called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTesla();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTesla);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdTesla");
	}

	protected static void InvokeRpcRpcRoundrestart(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcRoundrestart called on server.");
		}
		else
		{
			((PlayerStats)obj).RpcRoundrestart();
		}
	}

	protected static void InvokeRpcTargetAchieve(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetAchieve called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetAchieve(ClientScene.readyConnection, reader.ReadString());
		}
	}

	protected static void InvokeRpcTargetStats(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetStats called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetStats(ClientScene.readyConnection, reader.ReadString(), reader.ReadString(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcTargetOofEffect(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetOofEffect called on server.");
		}
		else
		{
			((PlayerStats)obj).TargetOofEffect(ClientScene.readyConnection, reader.ReadVector3(), reader.ReadSingle());
		}
	}

	public void CallRpcRoundrestart()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcRoundrestart called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcRoundrestart);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 7, "RpcRoundrestart");
	}

	public void CallTargetAchieve(NetworkConnection conn, string key)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetAchieve called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetAchieve);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(key);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetAchieve");
	}

	public void CallTargetStats(NetworkConnection conn, string key, string targetAchievement, int maxValue)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetStats called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetStats);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(key);
		networkWriter.Write(targetAchievement);
		networkWriter.WritePackedUInt32((uint)maxValue);
		SendTargetRPCInternal(conn, networkWriter, 0, "TargetStats");
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)health);
			return true;
		}
		bool flag = false;
		if ((base.syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)health);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(base.syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			health = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetHPAmount((int)reader.ReadPackedUInt32());
		}
	}
}
