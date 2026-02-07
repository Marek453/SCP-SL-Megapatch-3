using UnityEngine;
using UnityEngine.Networking;

public class FallDamage : NetworkBehaviour
{
	public bool isGrounded = true;

	public LayerMask groundMask;

	[SerializeField]
	private float groundMaxDistance = 1.3f;

	public AudioClip sound;

	public AudioSource sfxsrc;

	private float previousHeight;

	public AnimationCurve damageOverDistance;

	private CharacterClassManager ccm;

	public string zone;

	private static int kCmdCmdFall;

	private static int kRpcRpcDoSound;

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			CalculateGround();
		}
	}

	private void CalculateGround()
	{
		if (TutorialManager.status)
		{
			return;
		}
		RaycastHit hitInfo;
		bool flag = Physics.Raycast(new Ray(base.transform.position, Vector3.down), out hitInfo, groundMaxDistance, groundMask);
		if (flag && zone != hitInfo.transform.root.name)
		{
			zone = hitInfo.transform.root.name;
			if (zone.Contains("Heavy"))
			{
				SoundtrackManager.singleton.mainIndex = 1;
			}
			else if (zone.Contains("Out"))
			{
				SoundtrackManager.singleton.mainIndex = 2;
			}
			else
			{
				SoundtrackManager.singleton.mainIndex = 0;
			}
		}
		if (flag != isGrounded)
		{
			isGrounded = flag;
			if (isGrounded)
			{
				OnTouchdown();
			}
			else
			{
				OnLoseContactWithGround();
			}
		}
	}

	private void OnLoseContactWithGround()
	{
		previousHeight = base.transform.position.y;
	}

	private void OnTouchdown()
	{
		float num = damageOverDistance.Evaluate(previousHeight - base.transform.position.y);
		if (num > 5f && ccm.klasy[ccm.curClass].team != 0)
		{
			if ((float)GetComponent<PlayerStats>().health - num <= 0f)
			{
				AchievementManager.Achieve("gravity");
			}
			CallCmdFall(num);
		}
	}

	[Command(channel = 2)]
	private void CmdFall(float dmg)
	{
		CallRpcDoSound(base.transform.position, dmg);
		GetComponent<CharacterClassManager>().CallRpcPlaceBlood(base.transform.position, 0, Mathf.Clamp(dmg / 30f, 0.8f, 2f));
		GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(Mathf.Abs(dmg), "WORLD", "FALLDOWN", 0), base.gameObject);
	}

	[ClientRpc]
	private void RpcDoSound(Vector3 pos, float dmg)
	{
		sfxsrc.PlayOneShot(sound);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdFall(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdFall called on client.");
		}
		else
		{
			((FallDamage)obj).CmdFall(reader.ReadSingle());
		}
	}

	public void CallCmdFall(float dmg)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdFall called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdFall(dmg);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdFall);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(dmg);
		SendCommandInternal(networkWriter, 2, "CmdFall");
	}

	protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoSound called on server.");
		}
		else
		{
			((FallDamage)obj).RpcDoSound(reader.ReadVector3(), reader.ReadSingle());
		}
	}

	public void CallRpcDoSound(Vector3 pos, float dmg)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDoSound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDoSound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(pos);
		networkWriter.Write(dmg);
		SendRPCInternal(networkWriter, 0, "RpcDoSound");
	}

	static FallDamage()
	{
		kCmdCmdFall = -1476756283;
		NetworkBehaviour.RegisterCommandDelegate(typeof(FallDamage), kCmdCmdFall, InvokeCmdCmdFall);
		kRpcRpcDoSound = 675793188;
		NetworkBehaviour.RegisterRpcDelegate(typeof(FallDamage), kRpcRpcDoSound, InvokeRpcRpcDoSound);
		NetworkCRC.RegisterBehaviour("FallDamage", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}
}
