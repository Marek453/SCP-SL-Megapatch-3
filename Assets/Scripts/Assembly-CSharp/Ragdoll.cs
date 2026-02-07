using System;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class Ragdoll : NetworkBehaviour
{
	[Serializable]
	public struct Info
	{
		public string ownerHLAPI_id;

		public string steamClientName;

		public PlayerStats.HitInfo deathCause;

		public int charclass;

		public Info(string owner, string nick, PlayerStats.HitInfo info, int cc)
		{
			ownerHLAPI_id = owner;
			steamClientName = nick;
			charclass = cc;
			deathCause = info;
		}
	}

	[SyncVar(hook = "SetOwner")]
	public Info owner;

	[SyncVar(hook = "SetRecall")]
	public bool allowRecall;

	public Info Networkowner
	{
		get
		{
			return owner;
		}
		[param: In]
		set
		{
			ref Info fieldValue = ref owner;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetOwner(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public bool NetworkallowRecall
	{
		get
		{
			return allowRecall;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref allowRecall;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetRecall(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	public void SetOwner(Info s)
	{
		Networkowner = s;
	}

	private void Start()
	{
		Invoke("Unfr", 0.1f);
		Invoke("Refreeze", 7f);
	}

	public void SetRecall(bool b)
	{
		NetworkallowRecall = b;
	}

	private void Refreeze()
	{
		CharacterJoint[] componentsInChildren = GetComponentsInChildren<CharacterJoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			UnityEngine.Object.Destroy(componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = GetComponentsInChildren<Rigidbody>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			UnityEngine.Object.Destroy(componentsInChildren2[j]);
		}
	}

	private void Unfr()
	{
		Rigidbody[] componentsInChildren = GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody rigidbody in componentsInChildren)
		{
			rigidbody.isKinematic = false;
		}
		Collider[] componentsInChildren2 = GetComponentsInChildren<Collider>();
		foreach (Collider collider in componentsInChildren2)
		{
			collider.enabled = true;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			GeneratedNetworkCode._WriteInfo_Ragdoll(writer, owner);
			writer.Write(allowRecall);
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
			GeneratedNetworkCode._WriteInfo_Ragdoll(writer, owner);
		}
		if ((base.syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(allowRecall);
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
			owner = GeneratedNetworkCode._ReadInfo_Ragdoll(reader);
			allowRecall = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetOwner(GeneratedNetworkCode._ReadInfo_Ragdoll(reader));
		}
		if (((uint)num & 2u) != 0)
		{
			SetRecall(reader.ReadBoolean());
		}
	}
}
