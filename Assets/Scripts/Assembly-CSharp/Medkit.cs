using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.PostProcessing;
using UnityStandardAssets.ImageEffects;

public class Medkit : NetworkBehaviour
{
	private float inventoryCooldown;

	private Inventory inv;

	private VignetteAndChromaticAberration blinkCtrl;

	private PlayerStats ps;

	private CharacterClassManager ccm;

	private float time;

	private static int kCmdCmdUseMedkit;

	private void Start()
	{
		blinkCtrl = GetComponentInChildren<VignetteAndChromaticAberration>();
		ccm = GetComponent<CharacterClassManager>();
		inv = GetComponent<Inventory>();
		ps = GetComponent<PlayerStats>();
	}

	private void Update()
	{
		if (time >= 0f)
		{
			time -= Time.deltaTime;
		}
		if (base.isLocalPlayer)
		{
			inventoryCooldown -= Time.deltaTime;
			if (Cursor.lockState != CursorLockMode.Locked)
			{
				inventoryCooldown = 0.2f;
			}
			if (inventoryCooldown <= 0f && Input.GetKeyDown(NewInput.GetKey("Shoot")) && inv.curItem == 14 && time < 0f)
			{
				blinkCtrl.chromaticAberration = 8;
				StartCoroutine(Use());
				CmdUseMedkit();
				time = 1f;
				inv.SetCurItem(-1);
			}
		}
	}

	[Command(channel = 2)]
	private void CmdUseMedkit()
	{
		for (int i = 0; i < inv.items.Count; i++)
		{
			if (inv.items[i].id == 14)
			{
				Team team = ccm.klasy[ccm.curClass].team;
				if (team != 0 && team != Team.RIP && time < 0f)
				{
					ps.Networkhealth = Mathf.Clamp(ps.health + Random.Range(50, 85), 0, ccm.klasy[ccm.curClass].maxHP);
				}
				time = 1f;
				inv.items.Remove(inv.items[i]);
				break;
			}
		}
	}

	IEnumerator Use()
	{
		if(blinkCtrl.chromaticAberration <= 0)
		{
			yield break;
		}
		yield return new WaitForSeconds(0.01f);
		blinkCtrl.chromaticAberration -= 2f * Time.deltaTime;
		StartCoroutine(Use());
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdUseMedkit(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdUseMedkit called on client.");
		}
		else
		{
			((Medkit)obj).CmdUseMedkit();
		}
	}

	public void CallCmdUseMedkit()
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdUseMedkit called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdUseMedkit();
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdUseMedkit);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendCommandInternal(networkWriter, 2, "CmdUseMedkit");
	}

	static Medkit()
	{
		kCmdCmdUseMedkit = -2049042393;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Medkit), kCmdCmdUseMedkit, InvokeCmdCmdUseMedkit);
		NetworkCRC.RegisterBehaviour("Medkit", 0);
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
