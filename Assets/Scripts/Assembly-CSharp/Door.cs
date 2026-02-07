using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Door : NetworkBehaviour, IComparable
{
	public string permissionLevel;

	[SyncVar(hook = "SetState")]
	public bool isOpen;

	private bool buffedStatus;

	private bool _wasLocked;

	public GameObject OpenTrigger;

	public bool dontOpenOnWarhead;

	public bool blockAfterDetonation;

	[SyncVar(hook = "SetLock")]
	public bool locked;

	public float curCooldown;

	public float cooldown;

	public bool lockdown;

	public bool warheadlock;

	public bool commandlock;

	public string DoorName;

	public Animator[] parts;

	public AudioSource soundsource;

	public AudioClip[] sound_open;

	public AudioClip[] sound_close;

	public AudioClip sound_checkpointWarning;

	public AudioClip sound_denied;

	public MovingStatus moving;

	[HideInInspector]
	public List<GameObject> buttons = new List<GameObject>();

	public Vector3 localPos;

	public Quaternion localRot;

	public GameObject destroyedPrefab;

	public bool isGateway;
	public GameObject Effect;

	private Rigidbody[] destoryedRb;

	[SyncVar(hook = "DestroyDoor")]
	public bool destroyed;

	private bool prevDestroyed;

	private SECTR_Portal portal;

	public int doorType;

	private int status = -1;

	private bool deniedInProgress;

	private static int kRpcRpcDoSound;

	public bool NetworkisOpen
	{
		get
		{
			return isOpen;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref isOpen;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				SetState(value);
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

	public bool Networkdestroyed
	{
		get
		{
			return destroyed;
		}
		[param: In]
		set
		{
			ref bool fieldValue = ref destroyed;
			if (NetworkServer.localClientActive && !base.syncVarHookGuard)
			{
				base.syncVarHookGuard = true;
				DestroyDoor(value);
				base.syncVarHookGuard = false;
			}
			SetSyncVar(value, ref fieldValue, 4u);
		}
	}

	public int CompareTo(object obj)
	{
		return string.CompareOrdinal(DoorName, ((Door)obj).DoorName);
	}

	private void SetLock(bool l)
	{
		Networklocked = l;
	}

	public void UpdateLock()
	{
		Networklocked = commandlock | lockdown | warheadlock;
	}

	public void SetPortal(SECTR_Portal p)
	{
		portal = p;
	}

	public void SetLocalPos()
	{
		localPos = base.transform.localPosition;
		localRot = base.transform.localRotation;
	}

	private IEnumerator _UpdatePosition()
	{
		Animator[] array = parts;
		foreach (Animator animator in array)
		{
			animator.SetBool("isOpen", isOpen);
		}
		if (!(sound_checkpointWarning != null) || !isOpen)
		{
			yield break;
		}
		deniedInProgress = true;
		moving.moving = true;
		SetActiveStatus(2);
		float t = 0f;
		while (t < 5f)
		{
			t += 0.1f;
			yield return new WaitForSeconds(0.1f);
			if (curCooldown < 0f)
			{
				SetActiveStatus(1);
			}
		}
		if (locked)
		{
			moving.moving = false;
			deniedInProgress = false;
			yield break;
		}
		soundsource.PlayOneShot(sound_checkpointWarning);
		SetActiveStatus(5);
		yield return new WaitForSeconds(2f);
		SetActiveStatus(0);
		moving.moving = false;
		deniedInProgress = false;
		SetState(false);
		soundsource.PlayOneShot(sound_close[UnityEngine.Random.Range(0, sound_close.Length)]);
	}

	public void SetState(bool open)
	{
		NetworkisOpen = open;
		ForceCooldown(cooldown);
	}

	public void DestroyDoor(bool b)
	{
		if (b && destroyedPrefab != null)
		{
			Networkdestroyed = true;
		}
		else
		{
			Networkdestroyed = false;
		}
	}

	private IEnumerator _RefreshDestroyAnimation()
	{
		Animator[] array = parts;
		foreach (Animator animator in array)
		{
			if (animator.gameObject.activeSelf)
			{
				animator.gameObject.SetActive(false);
				GameObject gameObject = UnityEngine.Object.Instantiate(destroyedPrefab, animator.transform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
				gameObject.transform.localScale = Vector3.one;
				gameObject.transform.parent = null;
				int num = 0;
				destoryedRb = gameObject.GetComponentsInChildren<Rigidbody>();
				Vector3 vector = ((!(portal == null)) ? portal.GetRandomSectorPos() : Vector3.one);
				Rigidbody[] array2 = destoryedRb;
				foreach (Rigidbody rigidbody in array2)
				{
					rigidbody.GetComponent<Collider>().isTrigger = true;
					rigidbody.transform.parent = null;
					Vector3 vector2 = vector - base.transform.position;
					vector2.y = 0f;
					vector2 = vector2.normalized;
					rigidbody.velocity = ((num != 1 && num != 2) ? vector2 : (-vector2)) * UnityEngine.Random.Range(7, 9);
					num++;
				}
			}
		}
		yield return new WaitForSeconds(0.15f);
		Rigidbody[] array3 = destoryedRb;
		foreach (Rigidbody rigidbody2 in array3)
		{
			rigidbody2.GetComponent<Collider>().isTrigger = false;
		}
		yield return new WaitForSeconds(5f);
		Rigidbody[] array4 = destoryedRb;
		foreach (Rigidbody rigidbody3 in array4)
		{
			rigidbody3.isKinematic = true;
			rigidbody3.GetComponent<Collider>().enabled = false;
		}
	}

	private void Start()
	{
		StartCoroutine(_Start());
	}

	private IEnumerator _Start()
	{
		Component[] componentsInChildren = GetComponentsInChildren(typeof(Renderer));
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Renderer renderer = (Renderer)componentsInChildren[i];
			if (renderer.tag == "DoorButton")
			{
				buttons.Add(renderer.gameObject);
			}
		}
		SetActiveStatus(0);
		float time = 0f;
		while (time < 10f)
		{
			time += 0.02f;
			if (buffedStatus != isOpen)
			{
				buffedStatus = isOpen;
				ForceCooldown(cooldown);
				break;
			}
			yield return 0f;
		}
	}

	public void UpdatePos()
	{
		if (!(localPos == Vector3.zero))
		{
			base.transform.localPosition = localPos;
			base.transform.localRotation = localRot;
		}
	}

	public void SetZero()
	{
		localPos = Vector3.zero;
	}

	public void ChangeState()
	{
		if (curCooldown < 0f && !moving.moving && !deniedInProgress && !locked)
		{
			moving.moving = true;
			SetState(!isOpen);
			CallRpcDoSound();
		}
	}

	public void OpenDecontamination()
	{
		moving.moving = true;
		SetState(true);
		CallRpcDoSound();
		Networklocked = true;
		lockdown = false;
	}

	public void OpenWarhead(bool force = false)
	{
		if (dontOpenOnWarhead && !force)
		{
			return;
		}
		warheadlock = true;
		if (!locked || force)
		{
			UpdateLock();
			if (force || (!moving.moving && permissionLevel != "CONT_LVL_3" && permissionLevel != "UNACCESSIBLE"))
			{
				moving.moving = true;
				SetState(true);
				CallRpcDoSound();
			}
		}
	}

	[ClientRpc(channel = 14)]
	public void RpcDoSound()
	{
		soundsource.PlayOneShot((!isOpen) ? sound_close[UnityEngine.Random.Range(0, sound_close.Length)] : sound_open[UnityEngine.Random.Range(0, sound_open.Length)]);
	}

	public void SetActiveStatus(int s)
	{
		if (status == s)
		{
			return;
		}
		status = s;
		foreach (GameObject button in buttons)
		{
			MeshRenderer component = button.GetComponent<MeshRenderer>();
			Text componentInChildren = button.GetComponentInChildren<Text>();
			Image componentInChildren2 = button.GetComponentInChildren<Image>();
			if (component != null)
			{
				component.material = ButtonStages.types[doorType].stages[s].mat;
			}
			if (componentInChildren != null)
			{
				componentInChildren.text = ButtonStages.types[doorType].stages[s].info;
			}
			if (componentInChildren2 != null)
			{
				componentInChildren2.color = ((!(ButtonStages.types[doorType].stages[s].texture == null)) ? Color.white : Color.clear);
				componentInChildren2.sprite = ButtonStages.types[doorType].stages[s].texture;
			}
		}
	}

	private void LateUpdate()
	{
		if (prevDestroyed != destroyed)
		{
			GameObject gameObject = GameObject.Find("Host");
			if (gameObject != null && RandomSeedSync.generated)
			{
				StartCoroutine(_RefreshDestroyAnimation());
			}
		}
		if (curCooldown >= 0f)
		{
			curCooldown -= Time.deltaTime;
		}
		if (!deniedInProgress && !locked)
		{
			if (curCooldown >= 0f && status != 3)
			{
				if (sound_checkpointWarning == null)
				{
					if (portal != null)
					{
						portal.Flags = (SECTR_Portal.PortalFlags)0;
					}
					SetActiveStatus(2);
				}
			}
			else
			{
				if (portal != null)
				{
					portal.Flags = ((!(isOpen | destroyed)) ? SECTR_Portal.PortalFlags.Closed : ((SECTR_Portal.PortalFlags)0));
				}
				SetActiveStatus(isOpen ? 1 : 0);
			}
		}
		if (locked)
		{
			if (portal != null)
			{
				portal.Flags = ((!(isOpen | destroyed)) ? SECTR_Portal.PortalFlags.Closed : ((SECTR_Portal.PortalFlags)0));
			}
			if (_wasLocked)
			{
				return;
			}
			_wasLocked = true;
			SetActiveStatus(4);
		}
		else if (_wasLocked)
		{
			_wasLocked = false;
		}
		if (doorType == 3)
		{
			if (locked && !_wasLocked)
			{
				_wasLocked = true;
			}
			else if (!locked && _wasLocked)
			{
				_wasLocked = false;
				SetState(false);
				CallRpcDoSound();
			}
		}
	}

	public IEnumerator _Denied()
	{
		if (curCooldown < 0f && !moving.moving && !deniedInProgress)
		{
			deniedInProgress = true;
			soundsource.PlayOneShot(sound_denied);
			if (!locked)
			{
				SetActiveStatus(3);
			}
			yield return new WaitForSeconds(1f);
			deniedInProgress = false;
		}
	}

	public void ForceCooldown(float cd)
	{
		curCooldown = cd;
		StartCoroutine(_UpdatePosition());
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcDoSound(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("RPC RpcDoSound called on server.");
		}
		else
		{
			((Door)obj).RpcDoSound();
		}
	}

	public void CallRpcDoSound()
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
		SendRPCInternal(networkWriter, 14, "RpcDoSound");
	}

	static Door()
	{
		kRpcRpcDoSound = 630763456;
		NetworkBehaviour.RegisterRpcDelegate(typeof(Door), kRpcRpcDoSound, InvokeRpcRpcDoSound);
		NetworkCRC.RegisterBehaviour("Door", 0);
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(isOpen);
			writer.Write(locked);
			writer.Write(destroyed);
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
			writer.Write(isOpen);
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
		if ((base.syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(base.syncVarDirtyBits);
				flag = true;
			}
			writer.Write(destroyed);
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
			isOpen = reader.ReadBoolean();
			locked = reader.ReadBoolean();
			destroyed = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetState(reader.ReadBoolean());
		}
		if (((uint)num & 2u) != 0)
		{
			SetLock(reader.ReadBoolean());
		}
		if (((uint)num & 4u) != 0)
		{
			DestroyDoor(reader.ReadBoolean());
		}
	}
}
