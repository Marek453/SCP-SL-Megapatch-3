using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Handcuffs : NetworkBehaviour
{
	public TextMeshProUGUI distanceText;

	private Transform plyCam;

	private CharacterClassManager ccm;

	private Inventory inv;

	public LayerMask mask;

	public float maxDistance;

	private Image uncuffProgress;

	[SyncVar(hook = "SetTarget")]
	public GameObject cuffTarget;

	private float progress;

	private float lostCooldown;

	private NetworkInstanceId ___cuffTargetNetId;

	private static int kCmdCmdTarget;

	private static int kCmdCmdResetTarget;

	public GameObject NetworkcuffTarget
	{
		get
		{
			return cuffTarget;
		}
		[param: In]
		set
		{
			ref GameObject gameObjectField = ref cuffTarget;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetTarget(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVarGameObject(value, ref gameObjectField, 1u, ref ___cuffTargetNetId);
		}
	}

	private void Start()
	{
		uncuffProgress = GameObject.Find("UncuffProgress").GetComponent<Image>();
		inv = GetComponent<Inventory>();
		plyCam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			CheckForInput();
			UpdateText();
		}
		if (cuffTarget != null)
		{
			cuffTarget.GetComponent<AnimationController>().cuffed = true;
		}
	}

	private void CheckForInput()
	{
		if (cuffTarget != null)
		{
			bool flag = false;
			foreach (Inventory.SyncItemInfo item in inv.items)
			{
				if (item.id == 27)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				CallCmdTarget(null);
			}
		}
		if (!(Inventory.inventoryCooldown <= 0f))
		{
			return;
		}
		if (inv.curItem == 27)
		{
			if (Input.GetKeyDown(NewInput.GetKey("Shoot")) && cuffTarget == null)
			{
				CuffPlayer();
			}
			else if (Input.GetKeyDown(NewInput.GetKey("Zoom")) && cuffTarget != null)
			{
				CallCmdTarget(null);
			}
		}
		if (ccm.curClass >= 0 && ccm.klasy[ccm.curClass].team != 0 && Input.GetKey(NewInput.GetKey("Interact")))
		{
			RaycastHit hitInfo;
			if (Physics.Raycast(plyCam.position, plyCam.forward, out hitInfo, maxDistance, GetComponent<PlayerInteract>().mask))
			{
				Handcuffs componentInParent = hitInfo.collider.GetComponentInParent<Handcuffs>();
				if (componentInParent != null && componentInParent.GetComponent<AnimationController>().handAnimator != null && componentInParent.GetComponent<AnimationController>().handAnimator.GetBool("Cuffed"))
				{
					progress += Time.deltaTime;
					if (progress >= 1.5f)
					{
						progress = 0f;
						GameObject[] players = PlayerManager.singleton.players;
						foreach (GameObject gameObject in players)
						{
							if (gameObject.GetComponent<Handcuffs>().cuffTarget == componentInParent.gameObject)
							{
								CallCmdResetTarget(gameObject);
							}
						}
					}
				}
				else
				{
					progress = 0f;
				}
			}
			else
			{
				progress = 0f;
			}
		}
		else
		{
			progress = 0f;
		}
		if (ccm.curClass != 3)
		{
			uncuffProgress.fillAmount = Mathf.Clamp01(progress / 1.5f);
		}
	}

	private void CuffPlayer()
	{
		Ray ray = new Ray(plyCam.position, plyCam.forward);
		RaycastHit hitInfo;
		if (!Physics.Raycast(ray, out hitInfo, maxDistance, mask))
		{
			return;
		}
		CharacterClassManager componentInParent = hitInfo.collider.GetComponentInParent<CharacterClassManager>();
		if (!(componentInParent != null))
		{
			return;
		}
		Class @class = ccm.klasy[componentInParent.curClass];
		if (@class.team != 0 && (@class.team == Team.CDP || @class.team == Team.CHI) != (ccm.klasy[ccm.curClass].team == Team.CDP || ccm.klasy[ccm.curClass].team == Team.CHI) && componentInParent.GetComponent<AnimationController>().curAnim == 0 && componentInParent.GetComponent<AnimationController>().speed == Vector2.zero)
		{
			if (ccm.klasy[ccm.curClass].team == Team.CDP && @class.team == Team.MTF)
			{
				AchievementManager.Achieve("tableshaveturned");
			}
			CallCmdTarget(componentInParent.gameObject);
		}
	}

	[Command(channel = 2)]
	public void CmdTarget(GameObject t)
	{
		if (t == null || (Vector3.Distance(base.transform.position, t.transform.position) < 3f && inv.curItem == 27))
		{
			SetTarget(t);
			if (t != null)
			{
				t.GetComponent<Inventory>().ServerDropAll();
			}
		}
	}

	[Command(channel = 2)]
	public void CmdResetTarget(GameObject t)
	{
		t.GetComponent<Handcuffs>().SetTarget(null);
	}

	private void SetTarget(GameObject t)
	{
		NetworkcuffTarget = t;
	}

	private void UpdateText()
	{
		if (cuffTarget != null)
		{
			float num = Vector3.Distance(base.transform.position, cuffTarget.transform.position);
			if (num > 200f)
			{
				num = 200f;
				lostCooldown += Time.deltaTime;
				if (lostCooldown > 1f)
				{
					CallCmdTarget(null);
				}
			}
			else
			{
				lostCooldown = 0f;
			}
			distanceText.text = (num * 1.5f).ToString("0 m");
		}
		else
		{
			distanceText.text = "NONE";
		}
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdTarget called on client.");
		}
		else
		{
			((Handcuffs)obj).CmdTarget(reader.ReadGameObject());
		}
	}

	protected static void InvokeCmdCmdResetTarget(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdResetTarget called on client.");
		}
		else
		{
			((Handcuffs)obj).CmdResetTarget(reader.ReadGameObject());
		}
	}

	public void CallCmdTarget(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdTarget(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdTarget");
	}

	public void CallCmdResetTarget(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdResetTarget called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdResetTarget(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdResetTarget);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdResetTarget");
	}

	static Handcuffs()
	{
		kCmdCmdTarget = 624996931;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Handcuffs), kCmdCmdTarget, InvokeCmdCmdTarget);
		kCmdCmdResetTarget = -1476369842;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Handcuffs), kCmdCmdResetTarget, InvokeCmdCmdResetTarget);
		NetworkCRC.RegisterBehaviour("Handcuffs", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(cuffTarget);
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
			writer.Write(cuffTarget);
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
			___cuffTargetNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetTarget(reader.ReadGameObject());
		}
	}

	public override void PreStartClient()
	{
		if (!___cuffTargetNetId.IsEmpty())
		{
			NetworkcuffTarget = ClientScene.FindLocalObject(___cuffTargetNetId);
		}
	}
}
