using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class AlphaWarheadNukesitePanel : NetworkBehaviour
{
	public Transform lever;

	[SyncVar(hook = "SetEnabled")]
	public new bool enabled;

	private float leverStatus;

	public BlastDoor blastDoor;

	public Door outsideDoor;

	public Material[] onOffMaterial;

	public Material led_blastdoors;

	public Material led_outsidedoor;

	public Material led_detonationinprogress;

	public Material led_cancel;

	public bool Networkenabled
	{
		get
		{
			return enabled;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref enabled;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetEnabled(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	private void FixedUpdate()
	{
		UpdateLeverStatus();
	}

	public bool AllowChangeLevelState()
	{
		return leverStatus == 0f || leverStatus == 1f;
	}

	private void UpdateLeverStatus()
	{
		if (!(AlphaWarheadController.host == null))
		{
			Color color = new Color(0.2f, 0.3f, 0.5f);
			led_detonationinprogress.SetColor("_EmissionColor", (!AlphaWarheadController.host.inProgress) ? Color.black : color);
			led_outsidedoor.SetColor("_EmissionColor", (!outsideDoor.isOpen) ? Color.black : color);
			led_blastdoors.SetColor("_EmissionColor", (!blastDoor.isClosed) ? Color.black : color);
			led_cancel.SetColor("_EmissionColor", (!(AlphaWarheadController.host.timeToDetonation > 10f) || !AlphaWarheadController.host.inProgress) ? Color.black : Color.red);
			leverStatus += ((!enabled) ? (-0.04f) : 0.04f);
			leverStatus = Mathf.Clamp01(leverStatus);
			for (int i = 0; i < 2; i++)
			{
				onOffMaterial[i].SetColor("_EmissionColor", (i != Mathf.RoundToInt(leverStatus)) ? Color.black : new Color(1.2f, 1.2f, 1.2f, 1f));
			}
			lever.localRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(10f, -170f, leverStatus), -90f, 90f));
		}
	}

	private void Awake()
	{
		AlphaWarheadOutsitePanel.nukeside = this;
	}

	public void SetEnabled(bool b)
	{
		Networkenabled = b;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(enabled);
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
			writer.Write(enabled);
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
			enabled = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetEnabled(reader.ReadBoolean());
		}
	}
}
