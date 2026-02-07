using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PlayerInteract : NetworkBehaviour
{
	public GameObject playerCamera;

	public LayerMask mask;

	public float raycastMaxDistance;

	private CharacterClassManager ccm;

	private Inventory inv;

	private static int kCmdCmdUse914;

	private static int kCmdCmdChange914knob;

	private static int kRpcRpcUse914;

	private static int kCmdCmdUseWorkStation_Place;

	private static int kCmdCmdUseWorkStation_Take;

	private static int kCmdCmdUsePanel;

	private static int kRpcRpcLeverSound;

	private static int kCmdCmdUseElevator;

	private static int kCmdCmdSwitchAWButton;

	private static int kCmdCmdDetonateWarhead;

	private static int kCmdCmdOpenDoor;

	private static int kRpcRpcDenied;

	private static int kCmdCmdContain106;

	private static int kRpcRpcContain106;

	private void Update()
	{
		RaycastHit hitInfo;
		if (!base.isLocalPlayer || !Input.GetKeyDown(NewInput.GetKey("Interact")) || GetComponent<CharacterClassManager>().curClass == 2 || !Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hitInfo, raycastMaxDistance, mask))
		{
			return;
		}
		if (hitInfo.transform.GetComponentInParent<Door>() != null)
		{
			CallCmdOpenDoor(hitInfo.transform.GetComponentInParent<Door>().gameObject);
		}
		else if (hitInfo.transform.CompareTag("AW_Button"))
		{
			if (inv.curItem != 0)
			{
				string[] permissions = inv.availableItems[Mathf.Clamp(inv.curItem, 0, inv.availableItems.Length - 1)].permissions;
				foreach (string text in permissions)
				{
					if (text == "CONT_LVL_3")
					{
						CallCmdSwitchAWButton();
						return;
					}
				}
			}
			GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = true;
			Invoke("DisableDeniedText", 1f);
		}
		else if (hitInfo.transform.CompareTag("AW_Detonation"))
		{
			if (AlphaWarheadOutsitePanel.nukeside.enabled && !AlphaWarheadController.host.inProgress)
			{
				CallCmdDetonateWarhead();
			}
		}
		else if (hitInfo.transform.CompareTag("AW_Panel"))
		{
			CallCmdUsePanel(hitInfo.transform.name);
		}
		else if (hitInfo.transform.CompareTag("GateWayButton"))
		{
			CmdSetLockState(hitInfo.transform.GetComponentInParent<GateWay>().gameObject);
		}
		else if (hitInfo.transform.CompareTag("GateWayOpenButton"))
		{
			if(!hitInfo.transform.GetComponentInParent<GateWay>().isMoveing)
			{
				if(!hitInfo.transform.GetComponentInParent<GateWay>().Lock)
				{
					CmdOpenGate(hitInfo.transform.GetComponentInParent<GateWay>().gameObject);
				}
			}
		}
		else if (hitInfo.transform.CompareTag("294"))
		{
			if(inv.curItem == 17 && !hitInfo.collider.gameObject.GetComponent<Scp294>().isUsed)
			{
				CmdUse294(hitInfo.collider.gameObject);
			CursorManager.Scp294PanelOpen = true;
			UserMainInterface.singleton.Scp294Panel.SetActive(true);
			base.GetComponent<FirstPersonController>().using294 = true;
			GetComponent<Scp457PlayerScript>().plyCam.gameObject.SetActive(false);
			for (int i = 0; i < inv.items.Count; i++)
			{
				if(inv.items[i].id == 17)
				{
				inv.items.RemoveAt(i);
			inv.SetCurItem(-1);
			break;
				}
			}
			}
		}
		else if (hitInfo.transform.CompareTag("914_use"))
		{
			CallCmdUse914();
		}
		else if (hitInfo.transform.CompareTag("914_knob"))
		{
			CallCmdChange914knob();
		}
		else if (hitInfo.transform.CompareTag("ElevatorButton"))
		{
			Lift[] array = Object.FindObjectsOfType<Lift>();
			foreach (Lift lift in array)
			{
				Lift.Elevator[] elevators = lift.elevators;
				for (int k = 0; k < elevators.Length; k++)
				{
					Lift.Elevator elevator = elevators[k];
					if (ChckDis(elevator.door.transform.position))
					{
						CallCmdUseElevator(lift.transform.gameObject);
					}
				}
			}
		}
		else if (hitInfo.transform.CompareTag("FemurBreaker"))
		{
			CallCmdContain106();
		}
		else if (hitInfo.collider.CompareTag("WS"))
		{
			hitInfo.collider.GetComponentInParent<WorkStation>().UseButton(hitInfo.collider.GetComponent<Button>());
		}
	}
	[Command]
	void CmdSetLockState(GameObject GateWay)
	{
		GateWay.GetComponent<GateWay>().LockState(!GateWay.GetComponent<GateWay>().Lock);
	}

	[Command]
	void CmdOpenGate(GameObject GateWay)
	{
		GateWay.GetComponent<GateWay>().Open();
	}

	[Command(channel = 4)]
	private void CmdUse914()
	{
		if (!Scp914.singleton.working && ChckDis(GameObject.FindGameObjectWithTag("914_use").transform.position))
		{
			CallRpcUse914();
		}
	}

	[Command(channel = 4)]
	private void CmdUse294(GameObject _294)
	{
		_294.GetComponent<Scp294>().isUsed = true;
	}

	[Command(channel = 4)]
	private void CmdChange914knob()
	{
		if (!Scp914.singleton.working && ChckDis(GameObject.FindGameObjectWithTag("914_use").transform.position))
		{
			Scp914.singleton.ChangeKnobStatus();
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcUse914()
	{
		Scp914.singleton.StartRefining();
	}

	[Command(channel = 4)]
	public void CmdUseWorkStation_Place(GameObject station)
	{
		if (ChckDis(station.transform.position))
		{
			station.GetComponent<WorkStation>().ConnectTablet(base.gameObject);
		}
	}

	[Command(channel = 4)]
	public void CmdUseWorkStation_Take(GameObject station)
	{
		if (ChckDis(station.transform.position))
		{
			station.GetComponent<WorkStation>().UnconnectTablet(base.gameObject);
		}
	}

	[Command(channel = 4)]
	private void CmdUsePanel(string n)
	{
		AlphaWarheadNukesitePanel nukeside = AlphaWarheadOutsitePanel.nukeside;
		if (ChckDis(nukeside.transform.position))
		{
			if (n.Contains("cancel"))
			{
				AlphaWarheadController.host.CancelDetonation(base.gameObject);
			}
			else if (n.Contains("lever") && nukeside.AllowChangeLevelState())
			{
				nukeside.Networkenabled = !nukeside.enabled;
				CallRpcLeverSound();
			}
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcLeverSound()
	{
		AlphaWarheadOutsitePanel.nukeside.lever.GetComponent<AudioSource>().Play();
	}

	[Command(channel = 4)]
	private void CmdUseElevator(GameObject elevator)
	{
		Lift.Elevator[] elevators = elevator.GetComponent<Lift>().elevators;
		for (int i = 0; i < elevators.Length; i++)
		{
			Lift.Elevator elevator2 = elevators[i];
			if (ChckDis(elevator2.door.transform.position))
			{
				elevator.GetComponent<Lift>().UseLift();
			}
		}
	}

	[Command(channel = 4)]
	private void CmdSwitchAWButton()
	{
		GameObject gameObject = GameObject.Find("OutsitePanelScript");
		if (!ChckDis(gameObject.transform.position))
		{
			return;
		}
		string[] permissions = inv.availableItems[inv.curItem].permissions;
		foreach (string text in permissions)
		{
			if (text == "CONT_LVL_3")
			{
				gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>().SetKeycardState(true);
				break;
			}
		}
	}

	[Command(channel = 4)]
	private void CmdDetonateWarhead()
	{
		GameObject gameObject = GameObject.Find("OutsitePanelScript");
		if (ChckDis(gameObject.transform.position) && AlphaWarheadOutsitePanel.nukeside.enabled && gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
		{
			AlphaWarheadController.host.StartDetonation();
		}
	}

	[Command(channel = 14)]
	private void CmdOpenDoor(GameObject doorID)
	{
		bool flag = false;
		Door component = doorID.GetComponent<Door>();
		if (component.buttons.Count == 0)
		{
			flag = ChckDis(doorID.transform.position);
		}
		if (!flag)
		{
			foreach (GameObject button in component.buttons)
			{
				if (flag = ChckDis(button.transform.position))
				{
					break;
				}
			}
		}
		if (!flag)
		{
			return;
		}
		Scp096PlayerScript component2 = GetComponent<Scp096PlayerScript>();
		if (component.destroyedPrefab != null && (!component.isOpen || component.curCooldown > 0f) && component2.iAm096 && component2.enraged == Scp096PlayerScript.RageState.Enraged)
		{
			if (!component.locked)
			{
				component.DestroyDoor(true);
			}
			return;
		}
		if (GetComponent<CharacterClassManager>().klasy[GetComponent<CharacterClassManager>().curClass].team == Team.SH)
		{
			component.ChangeState();
			return;
		}
		if (component.permissionLevel.ToUpper() == "CHCKPOINT_ACC" && GetComponent<CharacterClassManager>().klasy[GetComponent<CharacterClassManager>().curClass].team == Team.SCP)
		{
			component.ChangeState();
			return;
		}
		try
		{
			if (string.IsNullOrEmpty(component.permissionLevel))
			{
				if (!component.locked)
				{
					component.ChangeState();
				}
				return;
			}
			string[] permissions = inv.availableItems[inv.curItem].permissions;
			foreach (string text in permissions)
			{
				if (!(text != component.permissionLevel))
				{
					if (!component.locked)
					{
						component.ChangeState();
					}
					else
					{
						CallRpcDenied(doorID);
					}
					return;
				}
			}
			CallRpcDenied(doorID);
		}
		catch
		{
			CallRpcDenied(doorID);
		}
	}

	[ClientRpc(channel = 14)]
	private void RpcDenied(GameObject door)
	{
		StartCoroutine(door.GetComponent<Door>()._Denied());
	}

	private bool ChckDis(Vector3 pos, float distanceMultiplier = 1f)
	{
		if (TutorialManager.status)
		{
			return true;
		}
		return Vector3.Distance(GetComponent<PlyMovementSync>().position, pos) < raycastMaxDistance * 1.5f;
	}

	[Command(channel = 4)]
	private void CmdContain106()
	{
		if (!Object.FindObjectOfType<LureSubjectContainer>().allowContain || (ccm.klasy[ccm.curClass].team == Team.SCP && ccm.curClass != 3) || !ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) || Object.FindObjectOfType<OneOhSixContainer>().used || ccm.klasy[ccm.curClass].team == Team.RIP)
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<CharacterClassManager>().curClass == 3)
			{
				gameObject.GetComponent<Scp106PlayerScript>().Contain();
			}
		}
		CallRpcContain106(base.gameObject);
		Object.FindObjectOfType<OneOhSixContainer>().SetState(true);
		StartCoroutine(_Kill106());
	}

	private IEnumerator _Kill106()
	{
		yield return Timing.WaitForSeconds(20f);
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			CharacterClassManager component = gameObject.GetComponent<CharacterClassManager>();
			if (component.curClass == 3)
			{
				component.SetPlayersClass(2, gameObject);
				gameObject.GetComponent<Scp106PlayerScript>().RpcAnnounceContaining();
			}
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcContain106(GameObject executor)
	{
		Object.Instantiate(GetComponent<Scp106PlayerScript>().screamsPrefab);
		if (!(executor == base.gameObject))
		{
			return;
		}
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			if (gameObject.GetComponent<CharacterClassManager>().curClass == 3)
			{
				AchievementManager.Achieve("securecontainprotect");
			}
		}
	}

	private void Start()
	{
		ccm = GetComponent<CharacterClassManager>();
		inv = GetComponent<Inventory>();
	}

	private void DisableDeniedText()
	{
		GameObject.Find("Keycard Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(1);
	}

	private void DisableAlphaText()
	{
		GameObject.Find("Alpha Denied Text").GetComponent<Text>().enabled = false;
		HintManager.singleton.AddHint(2);
	}

	private void DisableLockText()
	{
		GameObject.Find("Lock Denied Text").GetComponent<Text>().enabled = false;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdUse914(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUse914 called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUse914();
		}
	}

	protected static void InvokeCmdCmdChange914knob(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdChange914knob called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdChange914knob();
		}
	}

	protected static void InvokeCmdCmdUseWorkStation_Place(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseWorkStation_Place called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseWorkStation_Place(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUseWorkStation_Take(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseWorkStation_Take called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseWorkStation_Take(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdUsePanel(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUsePanel called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUsePanel(reader.ReadString());
		}
	}

	protected static void InvokeCmdCmdUseElevator(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseElevator called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdUseElevator(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdSwitchAWButton(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdSwitchAWButton called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdSwitchAWButton();
		}
	}

	protected static void InvokeCmdCmdDetonateWarhead(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdDetonateWarhead called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdDetonateWarhead();
		}
	}

	protected static void InvokeCmdCmdOpenDoor(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdOpenDoor called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdOpenDoor(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdContain106(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdContain106 called on client.");
		}
		else
		{
			((PlayerInteract)obj).CmdContain106();
		}
	}

	public void CallCmdUse914()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUse914 called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUse914();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUse914);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdUse914");
	}

	public void CallCmdChange914knob()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdChange914knob called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdChange914knob();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdChange914knob);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdChange914knob");
	}

	public void CallCmdUseWorkStation_Place(GameObject station)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseWorkStation_Place called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseWorkStation_Place(station);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseWorkStation_Place);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(station);
		SendCommandInternal(networkWriter, 4, "CmdUseWorkStation_Place");
	}

	public void CallCmdUseWorkStation_Take(GameObject station)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseWorkStation_Take called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseWorkStation_Take(station);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseWorkStation_Take);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(station);
		SendCommandInternal(networkWriter, 4, "CmdUseWorkStation_Take");
	}

	public void CallCmdUsePanel(string n)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUsePanel called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUsePanel(n);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUsePanel);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(n);
		SendCommandInternal(networkWriter, 4, "CmdUsePanel");
	}

	public void CallCmdUseElevator(GameObject elevator)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseElevator called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseElevator(elevator);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseElevator);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(elevator);
		SendCommandInternal(networkWriter, 4, "CmdUseElevator");
	}

	public void CallCmdSwitchAWButton()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdSwitchAWButton called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdSwitchAWButton();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdSwitchAWButton);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdSwitchAWButton");
	}

	public void CallCmdDetonateWarhead()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdDetonateWarhead called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdDetonateWarhead();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdDetonateWarhead);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdDetonateWarhead");
	}

	public void CallCmdOpenDoor(GameObject doorID)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdOpenDoor called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdOpenDoor(doorID);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdOpenDoor);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(doorID);
		SendCommandInternal(networkWriter, 14, "CmdOpenDoor");
	}

	public void CallCmdContain106()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdContain106 called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdContain106();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdContain106);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 4, "CmdContain106");
	}

	protected static void InvokeRpcRpcUse914(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcUse914 called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcUse914();
		}
	}

	protected static void InvokeRpcRpcLeverSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcLeverSound called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcLeverSound();
		}
	}

	protected static void InvokeRpcRpcDenied(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDenied called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcDenied(reader.ReadGameObject());
		}
	}

	protected static void InvokeRpcRpcContain106(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcContain106 called on server.");
		}
		else
		{
			((PlayerInteract)obj).RpcContain106(reader.ReadGameObject());
		}
	}

	public void CallRpcUse914()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcUse914 called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcUse914);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcUse914");
	}

	public void CallRpcLeverSound()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcLeverSound called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcLeverSound);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcLeverSound");
	}

	public void CallRpcDenied(GameObject door)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcDenied called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcDenied);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(door);
		SendRPCInternal(networkWriter, 14, "RpcDenied");
	}

	public void CallRpcContain106(GameObject executor)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcContain106 called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcContain106);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(executor);
		SendRPCInternal(networkWriter, 4, "RpcContain106");
	}

	static PlayerInteract()
	{
		kCmdCmdUse914 = -1419322708;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUse914, InvokeCmdCmdUse914);
		kCmdCmdChange914knob = -845424245;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdChange914knob, InvokeCmdCmdChange914knob);
		kCmdCmdUseWorkStation_Place = 1646281979;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseWorkStation_Place, InvokeCmdCmdUseWorkStation_Place);
		kCmdCmdUseWorkStation_Take = -1055163885;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseWorkStation_Take, InvokeCmdCmdUseWorkStation_Take);
		kCmdCmdUsePanel = 1853207668;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUsePanel, InvokeCmdCmdUsePanel);
		kCmdCmdUseElevator = 339400830;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdUseElevator, InvokeCmdCmdUseElevator);
		kCmdCmdSwitchAWButton = -710673229;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdSwitchAWButton, InvokeCmdCmdSwitchAWButton);
		kCmdCmdDetonateWarhead = -151679759;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdDetonateWarhead, InvokeCmdCmdDetonateWarhead);
		kCmdCmdOpenDoor = 1645579471;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdOpenDoor, InvokeCmdCmdOpenDoor);
		kCmdCmdContain106 = 1084648090;
		NetworkBehaviour.RegisterCommandDelegate(typeof(PlayerInteract), kCmdCmdContain106, InvokeCmdCmdContain106);
		kRpcRpcUse914 = -637254142;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcUse914, InvokeRpcRpcUse914);
		kRpcRpcLeverSound = -829118990;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcLeverSound, InvokeRpcRpcLeverSound);
		kRpcRpcDenied = -1136563096;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcDenied, InvokeRpcRpcDenied);
		kRpcRpcContain106 = -1051575568;
		NetworkBehaviour.RegisterRpcDelegate(typeof(PlayerInteract), kRpcRpcContain106, InvokeRpcRpcContain106);
		NetworkCRC.RegisterBehaviour("PlayerInteract", 0);
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
