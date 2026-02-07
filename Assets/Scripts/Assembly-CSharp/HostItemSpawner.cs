// HostItemSpawner
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HostItemSpawner : NetworkBehaviour
{
	private RandomItemSpawner ris;

	private Item[] avItems;

	private void Start()
	{
		avItems = Object.FindObjectOfType<Inventory>().availableItems;
	}

	public void Spawn(int seed)
	{
		Random.InitState(seed);
		string text = string.Empty;
		try
		{
			ris = Object.FindObjectOfType<RandomItemSpawner>();
			RandomItemSpawner.PickupPositionRelation[] pickups = ris.pickups;
			List<RandomItemSpawner.PositionPosIdRelation> list = new List<RandomItemSpawner.PositionPosIdRelation>();
			text = "Starting";
			RandomItemSpawner.PositionPosIdRelation[] posIds = ris.posIds;
			foreach (RandomItemSpawner.PositionPosIdRelation item in posIds)
			{
				list.Add(item);
			}
			int num = 0;
			RandomItemSpawner.PickupPositionRelation[] array = pickups;
			foreach (RandomItemSpawner.PickupPositionRelation pickupPositionRelation in array)
			{
				for (int k = 0; k < list.Count; k++)
				{
					list[k].index = k;
				}
				List<RandomItemSpawner.PositionPosIdRelation> list2 = new List<RandomItemSpawner.PositionPosIdRelation>();
				foreach (RandomItemSpawner.PositionPosIdRelation item2 in list)
				{
					if (item2.posID == pickupPositionRelation.posID)
					{
						list2.Add(item2);
					}
				}
				text = "Setting items: " + num;
				int index = Random.Range(0, list2.Count);
				RandomItemSpawner.PositionPosIdRelation positionPosIdRelation = list2[index];
				int index2 = positionPosIdRelation.index;
				if(isServer)
				{
				RpcSetPos(pickupPositionRelation.pickup.gameObject, positionPosIdRelation.position.position, pickupPositionRelation.itemID, positionPosIdRelation.position.rotation.eulerAngles, (pickupPositionRelation.duration != -1) ? ((float)pickupPositionRelation.duration) : avItems[pickupPositionRelation.itemID].durability);
				}
				list.RemoveAt(index2);
				num++;
			}
		}
		catch
		{
			Debug.LogError("Something is wrong, lol: " + text);
		}
	}

	[ClientRpc]
	private void RpcSetPos(GameObject obj, Vector3 pos, int item, Vector3 rot, float dur)
	{
		obj.GetComponent<Pickup>().SetupPickup(new Pickup.PickupInfo
			{
				position = pos,
				rotation = Quaternion.Euler(rot),
				itemId = item,
				durability = dur,
				ownerPlayerID = 0
			});
	}
}
