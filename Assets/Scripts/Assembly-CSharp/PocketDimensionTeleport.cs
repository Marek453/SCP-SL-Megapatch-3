using UnityEngine;
using UnityEngine.Networking;

public class PocketDimensionTeleport : NetworkBehaviour
{
	public enum PDTeleportType
	{
		Killer = 0,
		Exit = 1
	}

	private PDTeleportType type;

	public void SetType(PDTeleportType t)
	{
		type = t;
	}

	[ServerCallback]
	private void OnTriggerEnter(Collider other)
	{
		if (!NetworkServer.active)
		{
			return;
		}
		NetworkIdentity component = other.GetComponent<NetworkIdentity>();
		if (component != null)
		{
			if (type == PDTeleportType.Killer || Object.FindObjectOfType<BlastDoor>().isClosed)
			{
				component.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", "POCKET", 0), other.gameObject);
			}
			else if (type == PDTeleportType.Exit)
			{
				GameObject[] array = GameObject.FindGameObjectsWithTag("PD_EXIT");
				other.GetComponent<PlyMovementSync>().SetPosition(array[Random.Range(0, array.Length)].transform.position);
			}
		}
	}

	private void UNetVersion()
	{
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
