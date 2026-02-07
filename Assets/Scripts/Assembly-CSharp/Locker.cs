using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class Locker : NetworkBehaviour
{
	public Vector3 localPos;

	public float searchTime;

	public int[] ids;

	[SyncVar]
	public bool isTaken;

	public bool NetworkisTaken
	{
		get
		{
			return isTaken;
		}
		[param: In]
		set
		{
			SetSyncVar(value, ref isTaken, 1u);
		}
	}

	public int GetItem()
	{
		return (!isTaken) ? ids[Random.Range(0, ids.Length)] : (-1);
	}

	public void SetTaken(bool b)
	{
		NetworkisTaken = b;
	}

	public void SetupPos()
	{
		localPos = base.transform.localPosition;
	}

	public void Update()
	{
		base.transform.localPosition = localPos;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(isTaken);
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
			writer.Write(isTaken);
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
			isTaken = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			isTaken = reader.ReadBoolean();
		}
	}
}
