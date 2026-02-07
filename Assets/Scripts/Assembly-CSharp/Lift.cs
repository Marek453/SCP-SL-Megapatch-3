using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MEC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Lift : NetworkBehaviour
{
	[Serializable]
	public struct Elevator
	{
		public Transform target;

		public Animator door;

		public AudioSource musicSpeaker;

		private Vector3 pos;

		public void SetPosition()
		{
			pos = target.position;
		}

		public Vector3 GetPosition()
		{
			return pos;
		}
	}

	public enum Status
	{
		Up = 0,
		Down = 1,
		Moving = 2
	}

	[SyncVar(hook = "SetStatus")]
	public int statusID;

	public Elevator[] elevators;

	public Status status;

	public bool lockable;

	[SyncVar(hook = "SetLock")]
	private bool locked;

	public Text monitor;

	public float movingSpeed;

	public bool operative = true;

	public float maxDistance;

	private static int kRpcRpcPlayMusic;

	private static int kTargetRpcTargetBeingMoved;

	public int NetworkstatusID
	{
		get
		{
			return statusID;
		}
		[param: In]
		set
		{
			ref int fieldValue = ref statusID;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetStatus(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 1u);
		}
	}

	public bool Networklocked
	{
		get
		{
			return locked;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref locked;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetLock(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 2u);
		}
	}

	private void SetStatus(int i)
	{
		NetworkstatusID = i;
		status = (Status)i;
	}

	private void SetLock(bool b)
	{
		Networklocked = b;
		if (b && monitor != null)
		{
			monitor.text = TranslationReader.singleton.elements[4].values[34];
		}
	}

	public void Lock()
	{
		if (lockable)
		{
			SetLock(true);
			Timing.RunCoroutine(_LockdownUpdate(), Segment.Update);
		}
	}

	private void Awake()
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			elevator.target.tag = "LiftTarget";
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < elevators.Length; i++)
		{
			bool value = statusID == i && status != Status.Moving;
			elevators[i].door.SetBool("isOpen", value);
		}
	}

	public void UseLift()
	{
		if (operative && AlphaWarheadController.host.timeToDetonation != 0f && !locked)
		{
			Timing.RunCoroutine(_LiftAnimation(), Segment.Update);
			operative = false;
		}
	}

	private IEnumerator<float> _LiftAnimation()
	{
		Transform target = null;
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			if (!elevator.door.GetBool("isOpen"))
			{
				target = elevator.target;
			}
		}
		Status previousStatus = status;
		SetStatus(2);
		yield return Timing.WaitForSeconds(0.7f);
		CallRpcPlayMusic();
		yield return Timing.WaitForSeconds(2f);
		MovePlayers(target);
		yield return Timing.WaitForSeconds(movingSpeed - 2f);
		SetStatus((previousStatus != Status.Down) ? 1 : 0);
		yield return Timing.WaitForSeconds(2f);
		operative = true;
	}

	private IEnumerator<float> _LockdownUpdate()
	{
		while (status == Status.Moving || !operative)
		{
			yield return 0f;
		}
		if (status == Status.Down)
		{
			Timing.RunCoroutine(_LiftAnimation(), Segment.FixedUpdate);
		}
	}

	[ClientRpc(channel = 4)]
	private void RpcPlayMusic()
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			try
			{
				elevator.musicSpeaker.Play();
			}
			catch
			{
			}
		}
	}

	private void MovePlayers(Transform target)
	{
		GameObject[] players = PlayerManager.singleton.players;
		foreach (GameObject gameObject in players)
		{
			GameObject which = null;
			if (InRange(gameObject.transform.position, out which) && which.transform != target)
			{
				PlyMovementSync component = gameObject.GetComponent<PlyMovementSync>();
				gameObject.transform.parent = which.transform;
				Vector3 localPosition = gameObject.transform.localPosition;
				gameObject.transform.parent = target.transform;
				gameObject.transform.localPosition = localPosition;
				gameObject.transform.parent = null;
				component.SetPosition(gameObject.transform.position);
				component.SetRotation(target.transform.rotation.eulerAngles.y - which.transform.rotation.eulerAngles.y);
				CallTargetBeingMoved(gameObject.GetComponent<NetworkIdentity>().connectionToClient);
				gameObject.transform.parent = null;
			}
		}
	}

	[TargetRpc(channel = 4)]
	private void TargetBeingMoved(NetworkConnection target)
	{
		UnityEngine.Object.FindObjectOfType<ExplosionCameraShake>().Shake(0.15f);
	}

	public bool InRange(Vector3 pos, out GameObject which)
	{
		Elevator[] array = elevators;
		for (int i = 0; i < array.Length; i++)
		{
			Elevator elevator = array[i];
			bool flag = true;
			if (Mathf.Abs(elevator.target.position.x - pos.x) > maxDistance)
			{
				flag = false;
			}
			if (Mathf.Abs(elevator.target.position.y - pos.y) > maxDistance)
			{
				flag = false;
			}
			if (Mathf.Abs(elevator.target.position.z - pos.z) > maxDistance)
			{
				flag = false;
			}
			if (flag)
			{
				which = elevator.target.gameObject;
				return true;
			}
		}
		which = null;
		return false;
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcPlayMusic(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcPlayMusic called on server.");
		}
		else
		{
			((Lift)obj).RpcPlayMusic();
		}
	}

	protected static void InvokeRpcTargetBeingMoved(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("TargetRPC TargetBeingMoved called on server.");
		}
		else
		{
			((Lift)obj).TargetBeingMoved(ClientScene.readyConnection);
		}
	}

	public void CallRpcPlayMusic()
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("RPC Function RpcPlayMusic called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kRpcRpcPlayMusic);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendRPCInternal(networkWriter, 4, "RpcPlayMusic");
	}

	public void CallTargetBeingMoved(NetworkConnection target)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("TargetRPC Function TargetBeingMoved called on client.");
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)2);
		networkWriter.WritePackedUInt32((uint)kTargetRpcTargetBeingMoved);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		SendTargetRPCInternal(target, networkWriter, 4, "TargetBeingMoved");
	}

	static Lift()
	{
		kRpcRpcPlayMusic = 374858512;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Lift), kRpcRpcPlayMusic, InvokeRpcRpcPlayMusic);
		kTargetRpcTargetBeingMoved = -1324102726;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Lift), kTargetRpcTargetBeingMoved, InvokeRpcTargetBeingMoved);
		NetworkCRC.RegisterBehaviour("Lift", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)statusID);
			writer.Write(locked);
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
			writer.WritePackedUInt32((uint)statusID);
		}
		if ((base.syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(locked);
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
			statusID = (int)reader.ReadPackedUInt32();
			locked = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetStatus((int)reader.ReadPackedUInt32());
		}
		if (((uint)num & 2u) != 0)
		{
			SetLock(reader.ReadBoolean());
		}
	}
}
