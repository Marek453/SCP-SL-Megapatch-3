using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadController : NetworkBehaviour
{
	[Serializable]
	public class DetonationScenario
	{
		public AudioClip clip;

		public int tMinusTime;

		public float additionalTime;

		public float SumTime()
		{
			return (float)tMinusTime + additionalTime;
		}
	}

	[SyncVar(hook = "SetTime")]
	public float timeToDetonation;

	[SyncVar(hook = "SetStartScenario")]
	public int sync_startScenario;

	[SyncVar(hook = "SetResumeScenario")]
	public int sync_resumeScenario = -1;

	private static int startScenario;

	private static int resumeScenario;

	[SyncVar(hook = "SetProgress")]
	public bool inProgress;

	public int cooldown = 30;

	public static AudioSource alarmSource;

	public static AlphaWarheadController host;

	public DetonationScenario[] scenarios_start;

	public DetonationScenario[] scenarios_resume;

	public AudioClip sound_canceled;

	public bool doorsClosed;

	public bool doorsOpen;

	internal BlastDoor[] blastDoors;

	public bool detonated;

	public int warheadKills;

	private float shake;

	private static int kRpcRpcShake;

	public float NetworktimeToDetonation
	{
		get
		{
			return timeToDetonation;
		}
		[param: In]
		set
		{
			ref float fieldValue = ref timeToDetonation;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTime(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public int Networksync_startScenario
	{
		get
		{
			return sync_startScenario;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref sync_startScenario;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStartScenario(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public int Networksync_resumeScenario
	{
		get
		{
			return sync_resumeScenario;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref sync_resumeScenario;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetResumeScenario(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public bool NetworkinProgress
	{
		get
		{
			return inProgress;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref inProgress;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetProgress(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 8u);
		}
	}

	private void SetTime(float f)
	{
		NetworktimeToDetonation = f;
	}

	private void SetStartScenario(int i)
	{
		Networksync_startScenario = i;
	}

	private void SetResumeScenario(int i)
	{
		Networksync_resumeScenario = i;
	}

	private void SetProgress(bool b)
	{
		NetworkinProgress = b;
	}

	public void StartDetonation()
	{
		doorsOpen = false;
		ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Countdown started.", ServerLogs.ServerLogType.GameEvent);
		if ((resumeScenario == -1 && scenarios_start[startScenario].SumTime() == timeToDetonation) || (resumeScenario != -1 && scenarios_resume[resumeScenario].SumTime() == timeToDetonation))
		{
			SetProgress(true);
		}
	}

	public void InstantPrepare()
	{
		if (resumeScenario == -1)
		{
			NetworktimeToDetonation = scenarios_start[startScenario].SumTime();
		}
		else
		{
			NetworktimeToDetonation = scenarios_resume[resumeScenario].SumTime();
		}
	}

	private IEnumerator<float> _ReadCustomTranslations()
	{
		DetonationScenario[] array = scenarios_resume;
		foreach (DetonationScenario asource in array)
		{
			string path = TranslationReader.path + "/Custom Audio/" + asource.clip.name + ".ogg";
			if (File.Exists(path))
			{
				WWW www = new WWW("file://" + path);
				asource.clip = www.GetAudioClip(false);
				while (asource.clip.loadState != AudioDataLoadState.Loaded)
				{
					yield return Timing.WaitUntilDone(www);
				}
				asource.clip.name = Path.GetFileName(path);
				continue;
			}
			yield break;
		}
		DetonationScenario[] array2 = scenarios_start;
		foreach (DetonationScenario asource2 in array2)
		{
			string path2 = TranslationReader.path + "/Custom Audio/" + asource2.clip.name + ".ogg";
			if (File.Exists(path2))
			{
				WWW www2 = new WWW("file://" + path2);
				asource2.clip = www2.GetAudioClip(false);
				while (asource2.clip.loadState != AudioDataLoadState.Loaded)
				{
					yield return Timing.WaitUntilDone(www2);
				}
				asource2.clip.name = Path.GetFileName(path2);
				continue;
			}
			break;
		}
	}

	public void CancelDetonation(GameObject disabler)
	{
		ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent);
		if (!inProgress || !(timeToDetonation > 10f))
		{
			return;
		}
		if (timeToDetonation <= 15f && disabler != null)
		{
			GetComponent<PlayerStats>().CallTargetAchieve(disabler.GetComponent<NetworkIdentity>().connectionToClient, "thatwasclose");
		}
		for (int i = 0; i < scenarios_resume.Length; i++)
		{
			if (scenarios_resume[i].SumTime() > timeToDetonation && scenarios_resume[i].SumTime() < scenarios_start[startScenario].SumTime())
			{
				Networksync_resumeScenario = i;
			}
		}
		SetTime(((resumeScenario >= 0) ? scenarios_resume[resumeScenario].SumTime() : scenarios_start[startScenario].SumTime()) + (float)cooldown);
		SetProgress(false);
		Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
		foreach (Door door in array)
		{
			door.warheadlock = false;
			door.UpdateLock();
		}
	}

	internal void Detonate()
	{
		ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent);
		detonated = true;
		CallRpcShake();
		GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject2 in array2)
			{
				if (gameObject.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject2.transform.position, gameObject.transform.position) < 3.5f))
				{
					warheadKills++;
				}
			}
		}
		Door[] array3 = UnityEngine.Object.FindObjectsOfType<Door>();
		foreach (Door door in array3)
		{
			if (door.blockAfterDetonation)
			{
				door.Networklocked = true;
				door.OpenWarhead(true);
			}
		}
	}

	[ClientRpc]
	private void RpcShake()
	{
		ExplosionCameraShake.singleton.Shake(1f);
		if (base.isLocalPlayer && base.transform.position.y > 900f)
		{
			AchievementManager.Achieve("tminus");
		}
	}

	private void FixedUpdate()
	{
		if (base.name == "Host")
		{
			host = this;
			startScenario = sync_startScenario;
			resumeScenario = sync_resumeScenario;
		}
		else
		{
			host = GameObject.Find("Host").GetComponent<AlphaWarheadController>();

        }
		if (!(host == null) && base.isLocalPlayer)
		{
			UpdateSourceState();
			if (base.isServer)
			{
				ServerCountdown();
			}
		}
	}

	private void UpdateSourceState()
	{
		if (TutorialManager.status)
		{
			return;
		}
		if (host.inProgress)
		{
			if (host.timeToDetonation != 0f)
			{
				if (!alarmSource.isPlaying)
				{
					alarmSource.volume = 1f;
					alarmSource.clip = ((resumeScenario >= 0) ? scenarios_resume[resumeScenario].clip : scenarios_start[startScenario].clip);
					alarmSource.Play();
					return;
				}
				float num = RealDetonationTime();
				float num2 = num - host.timeToDetonation;
				if (Mathf.Abs(alarmSource.time - num2) > 0.5f)
				{
					alarmSource.time = Mathf.Clamp(num2, 0f, num);
				}
			}
			if (host.timeToDetonation < 5f && host.timeToDetonation != 0f)
			{
				shake += Time.fixedDeltaTime / 20f;
				shake = Mathf.Clamp(shake, 0f, 0.5f);
				if (Vector3.Distance(base.transform.position, AlphaWarheadOutsitePanel.nukeside.transform.position) < 100f)
				{
					ExplosionCameraShake.singleton.Shake(shake);
				}
			}
		}
		else if (alarmSource.isPlaying && alarmSource.clip != null)
		{
			alarmSource.Stop();
			alarmSource.clip = null;
			alarmSource.PlayOneShot(sound_canceled);
		}
	}

	public float RealDetonationTime()
	{
		return (resumeScenario < 0) ? scenarios_start[startScenario].SumTime() : scenarios_resume[resumeScenario].SumTime();
	}

	[ServerCallback]
	private void ServerCountdown()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		float num = RealDetonationTime();
		float num2 = timeToDetonation;
		if (timeToDetonation != 0f)
		{
			if (inProgress)
			{
				num2 -= Time.fixedDeltaTime;
				if (num2 < 2f && !doorsClosed)
				{
					doorsClosed = true;
					BlastDoor[] array = blastDoors;
					foreach (BlastDoor blastDoor in array)
					{
						blastDoor.SetClosed(true);
					}
				}
				if (!doorsOpen && num2 < num - ((resumeScenario < 0) ? scenarios_start[startScenario].additionalTime : scenarios_resume[resumeScenario].additionalTime))
				{
					doorsOpen = true;
					Door[] array2 = UnityEngine.Object.FindObjectsOfType<Door>();
					foreach (Door door in array2)
					{
						door.OpenWarhead();
					}
				}
				if (num2 <= 0f)
				{
					Detonate();
				}
				num2 = Mathf.Clamp(num2, 0f, num);
			}
			else
			{
				if (num2 > num)
				{
					num2 -= Time.fixedDeltaTime;
				}
				num2 = Mathf.Clamp(num2, num, (float)cooldown + num);
			}
		}
		if (num2 != timeToDetonation)
		{
			SetTime(num2);
		}
	}

	private void Start()
	{
		if (!base.isLocalPlayer || TutorialManager.status)
		{
			return;
		}
		Timing.RunCoroutine(_ReadCustomTranslations(), Segment.FixedUpdate);
		alarmSource = GameObject.Find("GameManager").GetComponent<AudioSource>();
		blastDoors = UnityEngine.Object.FindObjectsOfType<BlastDoor>();
		if (!base.isServer)
		{
			return;
		}
		int @int = ConfigFile.ServerConfig.GetInt("warhead_tminus_start_duration", 90);
		@int = Mathf.Clamp(@int, 80, 120);
		float f = @int / 10;
		@int = Mathf.RoundToInt(f);
		@int *= 10;
		Networksync_startScenario = 3;
		for (int i = 0; i < scenarios_start.Length; i++)
		{
			if (scenarios_start[i].tMinusTime == @int)
			{
				Networksync_startScenario = i;
			}
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcShake(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcShake called on server.");
		}
		else
		{
			((AlphaWarheadController)obj).RpcShake();
		}
	}

	public void CallRpcShake()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcShake called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcShake);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 0, "RpcShake");
	}

	static AlphaWarheadController()
	{
		kRpcRpcShake = -737840022;
		NetworkBehaviour.RegisterRpcDelegate(typeof(AlphaWarheadController), kRpcRpcShake, InvokeRpcRpcShake);
		NetworkCRC.RegisterBehaviour("AlphaWarheadController", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(timeToDetonation);
			writer.WritePackedUInt32((uint)sync_startScenario);
			writer.WritePackedUInt32((uint)sync_resumeScenario);
			writer.Write(inProgress);
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
			writer.Write(timeToDetonation);
		}
		if ((base.syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)sync_startScenario);
		}
		if ((base.syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)sync_resumeScenario);
		}
		if ((base.syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(inProgress);
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
			timeToDetonation = reader.ReadSingle();
			sync_startScenario = (int)reader.ReadPackedUInt32();
			sync_resumeScenario = (int)reader.ReadPackedUInt32();
			inProgress = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetTime(reader.ReadSingle());
		}
		if (((uint)num & 2u) != 0)
		{
			SetStartScenario((int)reader.ReadPackedUInt32());
		}
		if (((uint)num & 4u) != 0)
		{
			SetResumeScenario((int)reader.ReadPackedUInt32());
		}
		if (((uint)num & 8u) != 0)
		{
			SetProgress(reader.ReadBoolean());
		}
	}
}
