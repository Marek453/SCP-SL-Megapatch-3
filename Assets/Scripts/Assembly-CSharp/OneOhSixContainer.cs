using System.Runtime.InteropServices;
using UnityEngine.Networking;

public class OneOhSixContainer : NetworkBehaviour
{
	[SyncVar(hook = "SetState")]
	public bool used;

	public bool Networkused
	{
		get
		{
			return used;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref used;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetState(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public void SetState(bool b)
	{
		Networkused = b;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(used);
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
			writer.Write(used);
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
			used = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetState(reader.ReadBoolean());
		}
	}
}
