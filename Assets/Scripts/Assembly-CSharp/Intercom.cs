using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Intercom : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Transform area;

	public float triggerDistance;

	private float speechTime;

	private float cooldownAfter;

	public float speechRemainingTime;

	public float remainingCooldown;

	public Text txt;

	[SyncVar(hook = "SetSpeaker")]
	public GameObject speaker;

	public static Intercom host;

	public GameObject start_sound;

	public GameObject stop_sound;

	private string content = string.Empty;

	private bool inUse;

	private bool isTransmitting;

	private NetworkInstanceId ___speakerNetId;

	private static int kRpcRpcPlaySound;

	private static int kRpcRpcUpdateText;

	private static int kCmdCmdSetTransmit;

	public GameObject Networkspeaker
	{
		get
		{
			return speaker;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref speaker;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetSpeaker(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 1u, ref ___speakerNetId);
		}
	}

	private void SetSpeaker(GameObject go)
	{
		Networkspeaker = go;
	}

	private void Log(string s)
	{
	}

	private IEnumerator<float> _StartTransmitting(GameObject sp)
	{
		CallRpcPlaySound(true, sp.GetComponent<QueryProcessor>().PlayerId);
		Log("Beep beep!");
		yield return Timing.WaitForSeconds(2f);
		SetSpeaker(sp);
		Log("Speaker set!");
		speechRemainingTime = speechTime;
		Log("Timer set! IsNull: " + (speaker == null) + " AllowSpeak:" + ServerAllowToSpeak());
		while (speechRemainingTime > 0f && speaker != null && sp.GetComponent<Intercom>().ServerAllowToSpeak())
		{
			speechRemainingTime -= Timing.DeltaTime;
			yield return 0f;
		}
		Log("Unlinking the current speaker!");
		if (speaker != null)
		{
			SetSpeaker(null);
		}
		Log("Beeeeep!");
		CallRpcPlaySound(false, 0);
		remainingCooldown = cooldownAfter;
		while (remainingCooldown >= 0f)
		{
			remainingCooldown -= Time.deltaTime;
			yield return 0f;
		}
		inUse = false;
	}

	private void Start()
	{
		if (!TutorialManager.status)
		{
			txt = GameObject.Find("IntercomMonitor").GetComponent<Text>();
			ccm = GetComponent<CharacterClassManager>();
			area = GameObject.Find("IntercomSpeakingZone").transform;
			speechTime = ConfigFile.ServerConfig.GetInt("intercom_max_speech_time", 20);
			cooldownAfter = ConfigFile.ServerConfig.GetInt("intercom_cooldown", 180);
			Timing.RunCoroutine(_FindHost());
			Timing.RunCoroutine(_CheckForInput());
			if (base.isLocalPlayer && base.isServer)
			{
				InvokeRepeating("RefreshText", 5f, 7f);
			}
		}
	}

	private void RefreshText()
	{
		CallRpcUpdateText(content);
	}

	private IEnumerator<float> _FindHost()
	{
		while (host == null)
		{
			GameObject h = GameObject.Find("Host");
			if (h != null)
			{
				host = h.GetComponent<Intercom>();
			}
			yield return 0f;
		}
	}

	[ClientRpc]
	public void RpcPlaySound(bool start, int transmitterID)
	{
		if (PlayerManager.localPlayer.GetComponent<QueryProcessor>().PlayerId == transmitterID)
		{
			AchievementManager.Achieve("isthisthingon");
		}
		GameObject obj = Object.Instantiate((!start) ? stop_sound : start_sound);
		Object.Destroy(obj, 10f);
	}

	private void Update()
	{
		if (!TutorialManager.status && base.isLocalPlayer && base.isServer)
		{
			UpdateText();
		}
	}

	private void UpdateText()
	{
		if (remainingCooldown > 0f)
		{
			content = "RESTARTING\n" + Mathf.CeilToInt(remainingCooldown);
		}
		else if (speaker != null)
		{
			content = "TRANSMITTING...\nTIME LEFT - " + Mathf.CeilToInt(speechRemainingTime);
		}
		else
		{
			content = "READY";
		}
		if (content != txt.text)
		{
			CallRpcUpdateText(content);
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcUpdateText(string t)
	{
		try
		{
			txt.text = t;
		}
		catch
		{
		}
	}

	public void RequestTransmission(GameObject spk)
	{
		if (spk == null)
		{
			SetSpeaker(null);
		}
		else if (remainingCooldown <= 0f && !inUse)
		{
			inUse = true;
			Timing.RunCoroutine(_StartTransmitting(spk), Segment.Update);
		}
	}

	private IEnumerator<float> _CheckForInput()
	{
		if (base.isLocalPlayer)
		{
			while (true)
			{
				if (host != null)
				{
					if (ClientAllowToSpeak() && host.speaker == null)
					{
						CallCmdSetTransmit(true);
					}
					if (!ClientAllowToSpeak() && host.speaker == base.gameObject)
					{
						yield return Timing.WaitForSeconds(1f);
						if (!ClientAllowToSpeak())
						{
							CallCmdSetTransmit(false);
						}
					}
				}
				yield return 0f;
			}
		}
		yield return 0f;
	}

	private bool ClientAllowToSpeak()
	{
		return Vector3.Distance(base.transform.position, area.position) < triggerDistance && Input.GetKey(NewInput.GetKey("Voice Chat")) && ccm.klasy[ccm.curClass].team != Team.SCP;
	}

	private bool ServerAllowToSpeak()
	{
		return Vector3.Distance(base.transform.position, area.position) < triggerDistance && ccm.klasy[ccm.curClass].team != Team.SCP;
	}

	[Command(channel = 2)]
	private void CmdSetTransmit(bool player)
	{
		if (player)
		{
			if (ServerAllowToSpeak())
			{
				host.RequestTransmission(base.gameObject);
			}
		}
		else if (host.speaker == base.gameObject)
		{
			host.RequestTransmission(null);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdSetTransmit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSetTransmit called on client.");
		}
		else
		{
			((Intercom)obj).CmdSetTransmit(reader.ReadBoolean());
		}
	}

	public void CallCmdSetTransmit(bool player)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSetTransmit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSetTransmit(player);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSetTransmit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(player);
		SendCommandInternal(networkWriter, 2, "CmdSetTransmit");
	}

	protected static void InvokeRpcRpcPlaySound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlaySound called on server.");
		}
		else
		{
			((Intercom)obj).RpcPlaySound(reader.ReadBoolean(), (int)reader.ReadPackedUInt32());
		}
	}

	protected static void InvokeRpcRpcUpdateText(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUpdateText called on server.");
		}
		else
		{
			((Intercom)obj).RpcUpdateText(reader.ReadString());
		}
	}

	public void CallRpcPlaySound(bool start, int transmitterID)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlaySound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlaySound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(start);
		networkWriter.WritePackedUInt32((uint)transmitterID);
		SendRPCInternal(networkWriter, 0, "RpcPlaySound");
	}

	public void CallRpcUpdateText(string t)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUpdateText called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUpdateText);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendRPCInternal(networkWriter, 2, "RpcUpdateText");
	}

	static Intercom()
	{
		kCmdCmdSetTransmit = 1248049261;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Intercom), kCmdCmdSetTransmit, InvokeCmdCmdSetTransmit);
		kRpcRpcPlaySound = 239129888;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Intercom), kRpcRpcPlaySound, InvokeRpcRpcPlaySound);
		kRpcRpcUpdateText = 1243388753;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Intercom), kRpcRpcUpdateText, InvokeRpcRpcUpdateText);
		NetworkCRC.RegisterBehaviour("Intercom", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(speaker);
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
			writer.Write(speaker);
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
			___speakerNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetSpeaker(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		if (!___speakerNetId.IsEmpty())
		{
			Networkspeaker = ClientScene.FindLocalObject(___speakerNetId);
		}
	}
}
