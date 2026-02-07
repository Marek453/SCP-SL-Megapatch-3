using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

public class Pickup : NetworkBehaviour
{
	[Serializable]
	public struct PickupInfo
	{
		public Vector3 position;

		public Quaternion rotation;

		public int itemId;

		public float durability;

		public int ownerPlayerID;
	}

	public float searchTime;

	[SyncVar(hook = "SyncPickup")]
	public PickupInfo info;

	public static Inventory inv;

	public static List<Pickup> instances;

	private int previousId = -1;

	private GameObject model;

	private void SyncPickup(PickupInfo pickupInfo)
	{
		info = pickupInfo;
	}

	public void SetupPickup(PickupInfo pickupInfo)
	{
		info = pickupInfo;
		base.transform.position = info.position;
		base.transform.rotation = info.rotation;
		RefreshModel();
		UpdatePosition();
	}

	[ServerCallback]
	private void UpdatePosition()
	{
		if (NetworkServer.active)
		{
			PickupInfo pickupInfo = info;
			pickupInfo.position = base.transform.position;
			pickupInfo.rotation = base.transform.rotation;
			SyncPickup(pickupInfo);
		}
	}

	public void CheckForRefresh()
	{
		UpdatePosition();
		if (previousId != info.itemId || model == null)
		{
			previousId = info.itemId;
			RefreshModel();
		}
	}

	private void RefreshModel()
	{
		if (model != null)
		{
			UnityEngine.Object.Destroy(model.gameObject);
		}
		model = UnityEngine.Object.Instantiate(inv.availableItems[info.itemId].prefab, base.transform);
		model.transform.localPosition = Vector3.zero;
		searchTime = inv.availableItems[info.itemId].pickingtime;
		base.transform.position = info.position;
		base.transform.rotation = info.rotation;
	}

	public void Delete()
	{
		NetworkServer.Destroy(base.gameObject);
	}

	private IEnumerator Start()
	{
		Inventory.collectionModified = true;
		if (!NetworkServer.active)
		{
			GetComponent<Rigidbody>().isKinematic = true;
		}
		yield return new WaitForEndOfFrame();
		if (instances == null)
		{
			instances = new List<Pickup>();
		}
		instances.Add(this);
	}

	private void OnDestroy()
	{
		if (instances != null)
		{
			instances.Remove(this);
			Inventory.collectionModified = true;
		}
	}
}
