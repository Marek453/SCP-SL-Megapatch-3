using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Searching : NetworkBehaviour
{
	private CharacterClassManager ccm;

	private Inventory inv;

	private bool isHuman;

	private GameObject pickup;

	private Transform cam;

	private FirstPersonController fpc;

	private AmmoBox ammobox;

	private float timeToPickUp;

	private float errorMsgDur;

	private GameObject overloaderror;

	private Slider progress;

	private GameObject progressGO;

	public float rayDistance;

	private static int kCmdCmdPickupItem;

	private void Start()
	{
		fpc = GetComponent<FirstPersonController>();
		cam = GetComponent<Scp049PlayerScript>().plyCam.transform;
		ccm = GetComponent<CharacterClassManager>();
		inv = GetComponent<Inventory>();
		overloaderror = UserMainInterface.singleton.overloadMsg;
		progress = UserMainInterface.singleton.searchProgress;
		progressGO = UserMainInterface.singleton.searchOBJ;
		ammobox = GetComponent<AmmoBox>();
	}

	public void Init(bool isNotHuman)
	{
		isHuman = !isNotHuman;
	}

	private void Update()
	{
		if (base.isLocalPlayer)
		{
			Raycast();
			ContinuePickup();
			ErrorMessage();
		}
	}

	public void ShowErrorMessage()
	{
		errorMsgDur = 2f;
	}

	private void ErrorMessage()
	{
		if (errorMsgDur > 0f)
		{
			errorMsgDur -= Time.deltaTime;
		}
		overloaderror.SetActive(errorMsgDur > 0f);
	}

	private void ContinuePickup()
	{
		if (pickup != null)
		{
			if (!Input.GetKey(NewInput.GetKey("Interact")))
			{
				pickup = null;
				fpc.isSearching = false;
				progressGO.SetActive(false);
				return;
			}
			timeToPickUp -= Time.deltaTime;
			progressGO.SetActive(true);
			progress.value = progress.maxValue - timeToPickUp;
			if (!(timeToPickUp <= 0f))
			{
				return;
			}
			if (pickup.GetComponent<Pickup>() != null)
			{
				WeaponManager.Weapon[] weapons = GetComponent<WeaponManager>().weapons;
				foreach (WeaponManager.Weapon weapon in weapons)
				{
					if (weapon.inventoryID == pickup.GetComponent<Pickup>().info.itemId)
					{
						AchievementManager.Achieve("thatcanbeusefull");
					}
				}
			}
			progressGO.SetActive(false);
			CallCmdPickupItem(pickup);
			fpc.isSearching = false;
			pickup = null;
		}
		else
		{
			fpc.isSearching = false;
			progressGO.SetActive(false);
		}
	}

	private void Raycast()
	{
		RaycastHit hitInfo;
		if (!Input.GetKeyDown(NewInput.GetKey("Interact")) || !AllowPickup() || !Physics.Raycast(new Ray(cam.position, cam.forward), out hitInfo, rayDistance, GetComponent<PlayerInteract>().mask))
		{
			return;
		}
		Pickup componentInParent = hitInfo.transform.GetComponentInParent<Pickup>();
		Locker componentInParent2 = hitInfo.transform.GetComponentInParent<Locker>();
		if (componentInParent != null)
		{
			if (inv.items.Count < 8 || inv.availableItems[componentInParent.info.itemId].noEquipable)
			{
				timeToPickUp = componentInParent.searchTime;
				progress.maxValue = componentInParent.searchTime;
				fpc.isSearching = true;
				pickup = componentInParent.gameObject;
			}
			else
			{
				ShowErrorMessage();
			}
		}
		if (componentInParent2 != null)
		{
			if (inv.items.Count < 8)
			{
				timeToPickUp = componentInParent2.searchTime;
				progress.maxValue = componentInParent2.searchTime;
				fpc.isSearching = true;
				pickup = componentInParent2.gameObject;
			}
			else
			{
				ShowErrorMessage();
			}
		}
	}

	private bool AllowPickup()
	{
		if (!isHuman)
		{
			return false;
		}
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<Handcuffs>().cuffTarget == base.gameObject)
			{
				return false;
			}
		}
		return true;
	}

	[Command(channel = 2)]
	private void CmdPickupItem(GameObject t)
	{
		if (!(t == null) && ccm.IsHuman() && !(Vector3.Distance(GetComponent<PlyMovementSync>().position, t.transform.position) > 3.5f))
		{
			int num = -1;
			Pickup component = t.GetComponent<Pickup>();
			if (component != null)
			{
				num = component.info.itemId;
				component.Delete();
			}
			Locker component2 = t.GetComponent<Locker>();
			if (component2 != null && !component2.isTaken)
			{
				num = component2.GetItem();
				component2.SetTaken(true);
			}
			if (num != -1)
			{
				AddItem(num, (!(t.GetComponent<Pickup>() == null)) ? component.info.durability : (-1f));
			}
		}
	}

	public void AddItem(int id, float dur)
	{
		if (id == -1)
		{
			return;
		}
		if (!inv.availableItems[id].noEquipable)
		{
			inv.AddNewItem(id, (dur != -1f) ? dur : inv.availableItems[id].durability);
			return;
		}
		string[] array = ammobox.amount.Split(':');
		for (int i = 0; i < 3; i++)
		{
			if (ammobox.types[i].inventoryID == id)
			{
				array[i] = ((float)ammobox.GetAmmo(i) + dur).ToString();
			}
		}
		ammobox.Networkamount = array[0] + ":" + array[1] + ":" + array[2];
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeCmdCmdPickupItem(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkServer.active)
		{
			Debug.LogError("Command CmdPickupItem called on client.");
		}
		else
		{
			((Searching)obj).CmdPickupItem(reader.ReadGameObject());
		}
	}

	public void CallCmdPickupItem(GameObject t)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError("Command function CmdPickupItem called on server.");
			return;
		}
		if (base.isServer)
		{
			CmdPickupItem(t);
			return;
		}
		NetworkWriter networkWriter = new NetworkWriter();
		networkWriter.Write((short)0);
		networkWriter.Write((short)5);
		networkWriter.WritePackedUInt32((uint)kCmdCmdPickupItem);
		networkWriter.Write(GetComponent<NetworkIdentity>().netId);
		networkWriter.Write(t);
		SendCommandInternal(networkWriter, 2, "CmdPickupItem");
	}

	static Searching()
	{
		kCmdCmdPickupItem = 2021286825;
		NetworkBehaviour.RegisterCommandDelegate(typeof(Searching), kCmdCmdPickupItem, InvokeCmdCmdPickupItem);
		NetworkCRC.RegisterBehaviour("Searching", 0);
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
