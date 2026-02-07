using System.Collections.Generic;
using UnityEngine;

public class PocketDimensionGenerator : MonoBehaviour
{
	private List<PocketDimensionTeleport> pdtps = new List<PocketDimensionTeleport>();

	public void GenerateMap(int seed)
	{
		Random.InitState(seed);
		PocketDimensionTeleport[] array = Object.FindObjectsOfType<PocketDimensionTeleport>();
		foreach (PocketDimensionTeleport item in array)
		{
			pdtps.Add(item);
		}
		foreach (PocketDimensionTeleport pdtp in pdtps)
		{
			pdtp.SetType(PocketDimensionTeleport.PDTeleportType.Killer);
		}
		for (int j = 0; j < 2; j++)
		{
			SetRandomTeleport(PocketDimensionTeleport.PDTeleportType.Exit);
		}
	}

	private void SetRandomTeleport(PocketDimensionTeleport.PDTeleportType type)
	{
		int index = Random.Range(0, pdtps.Count);
		pdtps[index].SetType(type);
		pdtps.RemoveAt(index);
	}
}
