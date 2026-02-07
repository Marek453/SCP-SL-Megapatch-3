using Dissonance.Integrations.UNet_HLAPI;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Scp049PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public GameObject plyCam;

	public bool iAm049;

	public bool sameClass;

	public GameObject scpInstance;

	[Header("Infection")]
	public float currentInfection;

	[Header("Attack & Recall")]
	public float distance = 2.4f;

	public float recallDistance = 3.5f;

	public float recallProgress;

	private GameObject recallingObject;

	private Ragdoll recallingRagdoll;

	private ScpInterfaces interfaces;

	private Image loadingCircle;

	private FirstPersonController fpc;

	[Header("Boosts")]
	public AnimationCurve boost_recallTime;

	public AnimationCurve boost_infectTime;

	private static int kCmdCmdInfectPlayer;

	private static int kRpcRpcInfectPlayer;

	private static int kCmdCmdRecallPlayer;

	private void Start()
	{
		interfaces = Object.FindObjectOfType<ScpInterfaces>();
		loadingCircle = interfaces.Scp049_loading;
		if (base.isLocalPlayer)
		{
			fpc = GetComponent<FirstPersonController>();
		}
	}

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP || c.team == Team.SH;
		iAm049 = classID == 5;
		if (base.isLocalPlayer)
		{
			interfaces.Scp049_eq.SetActive(iAm049);
		}
	}

	private void Update()
	{
		DeductInfection();
		UpdateInput();
	}

	private void DeductInfection()
	{
		if (currentInfection > 0f)
		{
			currentInfection -= Time.deltaTime;
		}
		if (currentInfection < 0f)
		{
			currentInfection = 0f;
		}
	}

	private void UpdateInput()
	{
		if (base.isLocalPlayer)
		{
			if (Input.GetKeyDown(NewInput.GetKey("Shoot")))
			{
				Attack();
			}
			if (Input.GetKeyDown(NewInput.GetKey("Interact")))
			{
				Surgery();
			}
			Recalling();
		}
	}

	private void Attack()
	{
		RaycastHit hitInfo;
		if (iAm049 && Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, distance))
		{
			Scp049PlayerScript component = hitInfo.transform.GetComponent<Scp049PlayerScript>();
			if (component != null && !component.sameClass)
			{
				InfectPlayer(component.gameObject, GetComponent<HlapiPlayer>().PlayerId);
			}
		}
	}

	private void Surgery()
	{
		RaycastHit hitInfo;
		if (!iAm049 || !Physics.Raycast(plyCam.transform.position, plyCam.transform.forward, out hitInfo, recallDistance))
		{
			return;
		}
		Ragdoll componentInParent = hitInfo.transform.GetComponentInParent<Ragdoll>();
		if (!(componentInParent != null) || !componentInParent.allowRecall)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<HlapiPlayer>().PlayerId == componentInParent.owner.ownerHLAPI_id && gameObject.GetComponent<Scp049PlayerScript>().currentInfection > 0f && componentInParent.allowRecall)
			{
				recallingObject = gameObject;
				recallingRagdoll = componentInParent;
			}
		}
	}

	private void DestroyPlayer(GameObject recallingRagdoll)
	{
		if (recallingRagdoll.CompareTag("Ragdoll"))
		{
			NetworkServer.Destroy(recallingRagdoll);
		}
	}

	private void Recalling()
	{
		if (iAm049 && Input.GetKey(NewInput.GetKey("Interact")) && recallingObject != null)
		{
			fpc.lookingAtMe = true;
			recallProgress += Time.deltaTime / boost_recallTime.Evaluate(GetComponent<PlayerStats>().GetHealthPercent());
			if (recallProgress >= 1f)
			{
				AchievementManager.Achieve("turnthemall");
				CallCmdRecallPlayer(recallingObject, recallingRagdoll.gameObject);
				recallProgress = 0f;
				recallingObject = null;
			}
		}
		else
		{
			recallingObject = null;
			recallProgress = 0f;
			if (iAm049)
			{
				fpc.lookingAtMe = false;
			}
		}
		loadingCircle.fillAmount = recallProgress;
	}

	private void InfectPlayer(GameObject target, string id)
	{
		CallCmdInfectPlayer(target, id);
		Hitmarker.Hit();
	}

	[Command(channel = 2)]
	private void CmdInfectPlayer(GameObject target, string id)
	{
		if (GetComponent<CharacterClassManager>().curClass == 5 && Vector3.Distance(target.transform.position, GetComponent<PlyMovementSync>().position) < distance * 1.3f)
		{
			GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(4949f, id, "SCP:049", GetComponent<QueryProcessor>().PlayerId), target);
			CallRpcInfectPlayer(target, boost_infectTime.Evaluate(GetComponent<PlayerStats>().GetHealthPercent()));
		}
	}

	[ClientRpc(channel = 2)]
	private void RpcInfectPlayer(GameObject target, float infTime)
	{
		target.GetComponent<Scp049PlayerScript>().currentInfection = infTime;
	}

	[Command(channel = 2)]
	private void CmdRecallPlayer(GameObject target, GameObject ragdoll)
	{
		CharacterClassManager component = target.GetComponent<CharacterClassManager>();
		Ragdoll component2 = ragdoll.GetComponent<Ragdoll>();
		if (component2 != null && component != null && component.curClass == 2 && GetComponent<CharacterClassManager>().curClass == 5 && component2.owner.deathCause.tool == "SCP:049")
		{
			RoundSummary.changed_into_zombies++;
			component.SetClassID(10);
			target.GetComponent<PlayerStats>().Networkhealth = component.klasy[10].maxHP;
			DestroyPlayer(ragdoll);
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdInfectPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdInfectPlayer called on client.");
		}
		else
		{
			((Scp049PlayerScript)obj).CmdInfectPlayer(reader.ReadGameObject(), reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdRecallPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdRecallPlayer called on client.");
		}
		else
		{
			((Scp049PlayerScript)obj).CmdRecallPlayer(reader.ReadGameObject(), reader.ReadGameObject());
		}
	}

	public void CallCmdInfectPlayer(GameObject target, string id)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdInfectPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdInfectPlayer(target, id);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdInfectPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		networkWriter.Write(id);
		SendCommandInternal(networkWriter, 2, "CmdInfectPlayer");
	}

	public void CallCmdRecallPlayer(GameObject target, GameObject ragdoll)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdRecallPlayer called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdRecallPlayer(target, ragdoll);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdRecallPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		networkWriter.Write(ragdoll);
		SendCommandInternal(networkWriter, 2, "CmdRecallPlayer");
	}

	protected static void InvokeRpcRpcInfectPlayer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcInfectPlayer called on server.");
		}
		else
		{
			((Scp049PlayerScript)obj).RpcInfectPlayer(reader.ReadGameObject(), reader.ReadSingle());
		}
	}

	public void CallRpcInfectPlayer(GameObject target, float infTime)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcInfectPlayer called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcInfectPlayer);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(target);
		networkWriter.Write(infTime);
		SendRPCInternal(networkWriter, 2, "RpcInfectPlayer");
	}

	static Scp049PlayerScript()
	{
		kCmdCmdInfectPlayer = -2004090729;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp049PlayerScript), kCmdCmdInfectPlayer, InvokeCmdCmdInfectPlayer);
		kCmdCmdRecallPlayer = 1670066835;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Scp049PlayerScript), kCmdCmdRecallPlayer, InvokeCmdCmdRecallPlayer);
		kRpcRpcInfectPlayer = -1920658579;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Scp049PlayerScript), kRpcRpcInfectPlayer, InvokeRpcRpcInfectPlayer);
		NetworkCRC.RegisterBehaviour("Scp049PlayerScript", 0);
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
