using System.Runtime.InteropServices;
using GameConsole;
using UnityEngine;
using UnityEngine.Networking;

public class RandomSeedSync : NetworkBehaviour
{
	[SyncVar(hook = "SetSeed")]
	public int seed = -1;

	private static int staticSeed;

	public static bool generated;

	private void Start()
	{
		if (!base.isLocalPlayer)
		{
			return;
		}
		if (NetworkServer.active)
		{
			WorkStation[] array = Object.FindObjectsOfType<WorkStation>();
			foreach (WorkStation workStation in array)
			{
				workStation.SetPosition(new Offset
				{
					position = workStation.transform.localPosition,
					rotation = workStation.transform.localRotation.eulerAngles,
					scale = Vector3.one
				});
			}
		}
		generated = false;
		seed = ConfigFile.ServerConfig.GetInt("map_seed", -1);
		while (NetworkServer.active && seed == -1)
		{
			seed = Random.Range(-999999999, 999999999);
		}
	}

	private void Update()
	{
		if (!generated && base.name == "Host" && seed != -1)
		{
			staticSeed = seed;
			generated = true;
			GenerateLevel();
		}
	}

	private void SetSeed(int i)
	{
		seed = i;
	}

	public static void GenerateLevel()
	{
		Console console = Object.FindObjectOfType<Console>();
		console.AddLog("Initializing generator...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
		ImageGenerator imageGenerator = null;
		ImageGenerator imageGenerator2 = null;
		ImageGenerator imageGenerator3 = null;
		ImageGenerator[] array = Object.FindObjectsOfType<ImageGenerator>();
		foreach (ImageGenerator imageGenerator4 in array)
		{
			if (imageGenerator4.height == 0)
			{
				imageGenerator = imageGenerator4;
			}
			if (imageGenerator4.height == -1000)
			{
				imageGenerator2 = imageGenerator4;
			}
			if (imageGenerator4.height == -1001)
			{
				imageGenerator3 = imageGenerator4;
			}
		}
		if (!TutorialManager.status)
		{
			imageGenerator.GenerateMap(staticSeed);
			imageGenerator2.GenerateMap(staticSeed + 1);
			imageGenerator3.GenerateMap(staticSeed + 2);
			Door[] array2 = Object.FindObjectsOfType<Door>();
			foreach (Door door in array2)
			{
				door.UpdatePos();
			}
			GateWay[] DD = Object.FindObjectsOfType<GateWay>();
			foreach (GateWay door in DD)
			{
				door.UpdatePos();
			}
		}
		GameObject[] array3 = GameObject.FindGameObjectsWithTag("DoorButton");
		foreach (GameObject gameObject in array3)
		{
			try
			{
				gameObject.GetComponent<ButtonWallAdjuster>().Adjust();
				ButtonWallAdjuster[] componentsInChildren = gameObject.GetComponentsInChildren<ButtonWallAdjuster>();
				foreach (ButtonWallAdjuster buttonWallAdjuster in componentsInChildren)
				{
					buttonWallAdjuster.Invoke("Adjust", 4f);
				}
			}
			catch
			{
			}
		}
		Lift[] array4 = Object.FindObjectsOfType<Lift>();
		foreach (Lift lift in array4)
		{
			Lift.Elevator[] elevators = lift.elevators;
			foreach (Lift.Elevator elevator in elevators)
			{
				elevator.SetPosition();
			}
		}
		console.AddLog("Spawning items...", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
		Door[] array5 = Object.FindObjectsOfType<Door>();
		foreach (Door door2 in array5)
		{
			if (door2.destroyed)
			{
				door2.DestroyDoor(true);
			}
			else
			{
				door2.SetActiveStatus(1);
				door2.SetActiveStatus(0);
			}
			door2.SetState(door2.isOpen);
		}
		if (NetworkServer.active)
		{
			PlayerManager.localPlayer.GetComponent<HostItemSpawner>().Spawn(staticSeed);
		}
		SECTR_Member[] array6 = Object.FindObjectsOfType<SECTR_Member>();
		foreach (SECTR_Member sECTR_Member in array6)
		{
			sECTR_Member.UpdateViaScript();
		}
		Pickup[] array7 = Object.FindObjectsOfType<Pickup>();
		foreach (Pickup pickup in array7)
		{
			pickup.transform.position = pickup.info.position;
			pickup.transform.rotation = pickup.info.rotation;
		}
		Object.FindObjectOfType<LCZ_LabelManager>().RefreshLabels();
		console.AddLog("The scene is ready! Good luck!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
	}
}
