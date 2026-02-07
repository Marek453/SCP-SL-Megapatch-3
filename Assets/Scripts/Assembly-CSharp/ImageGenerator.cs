using System;
using System.Collections.Generic;
using UnityEngine;

public class ImageGenerator : MonoBehaviour
{
	[Serializable]
	public class ColorMap
	{
		public Color color = Color.white;

		public RoomType type;

		public float rotationY;

		public Vector2 centerOffset;
	}

	[Serializable]
	public class RoomsOfType
	{
		public List<Room> roomsOfType = new List<Room>();

		public int amount;
	}

	[Serializable]
	public class Room
	{
		public List<GameObject> room = new List<GameObject>();

		public RoomType type;

		public bool required;

		public Room(Room r)
		{
			room = r.room;
			type = r.type;
			required = r.required;
		}
	}

	public enum RoomType
	{
		Straight = 0,
		Curve = 1,
		RoomT = 2,
		Cross = 3,
		Endoff = 4,
		Prison = 5,
        Checkpoint = 6
    }

	public int height;

	public Texture2D[] maps;

	private Texture2D map;

	private Color[] copy;

	public float gridSize;

	public List<ColorMap> colorMap = new List<ColorMap>();

	public List<Room> availableRooms = new List<Room>();

	public List<GameObject> doors = new List<GameObject>();

	private Vector3 offset;

	public float y_offset;

	private Transform entrRooms;

	public RoomsOfType[] roomsOfType;

	public bool GenerateMap(int seed)
	{
		foreach (Room availableRoom in availableRooms)
		{
			foreach (GameObject item in availableRoom.room)
			{
				if(item != null)
				{
				item.SetActive(false);
				}
			}
		}
		GetComponent<PocketDimensionGenerator>().GenerateMap(seed);
		UnityEngine.Random.InitState(seed);
		map = maps[UnityEngine.Random.Range(0, maps.Length)];
		InitEntrance();
		copy = map.GetPixels();
		GeneratorTask_CheckRooms();
		GeneratorTask_RemoveNotRequired();
		GeneratorTask_SetRooms();
		GeneratorTask_Cleanup();
		GeneratorTask_RemoveDoubledDoorPoints();
		map.SetPixels(copy);
		map.Apply();
		if (entrRooms != null)
		{
			entrRooms.parent = null;
		}
		return true;
	}

	private void InitEntrance()
	{
		if (height != -1001)
		{
			return;
		}
		Transform transform = GameObject.Find("Root_Checkpoint").transform;
		entrRooms = GameObject.Find("EntranceRooms").transform;
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				if (pixel == Color.white)
				{
					offset = -new Vector3((float)j * gridSize, 0f, (float)i * gridSize) / 3f;
				}
			}
		}
		offset += Vector3.up;
	}

	private void GeneratorTask_Cleanup()
	{
		RoomsOfType[] array = this.roomsOfType;
		foreach (RoomsOfType roomsOfType in array)
		{
			foreach (Room item in roomsOfType.roomsOfType)
			{
				foreach (GameObject item2 in item.room)
				{
					if (item.type != RoomType.Prison)
					{
						item2.SetActive(false);
					}
				}
			}
		}
	}

	private void GeneratorTask_RemoveDoubledDoorPoints()
	{
		if (doors.Count == 0)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		GameObject[] array = GameObject.FindGameObjectsWithTag("DoorPoint" + height);
		foreach (GameObject item in array)
		{
			list.Add(item);
		}
		foreach (GameObject item2 in list)
		{
			foreach (GameObject item3 in list)
			{
				if (Vector3.Distance(item2.transform.position, item3.transform.position) < 2f && item2 != item3)
				{
					UnityEngine.Object.DestroyImmediate(item3);
					GeneratorTask_RemoveDoubledDoorPoints();
					return;
				}
			}
		}
		List<SECTR_Portal> list2 = new List<SECTR_Portal>();
		for (int j = 0; j < doors.Count; j++)
		{
			try
			{
				if (j < list.Count)
				{
					doors[j].transform.position = list[j].transform.position;
					doors[j].transform.rotation = list[j].transform.rotation;
					SECTR_Portal component = list[j].GetComponent<SECTR_Portal>();
					if (component != null)
					{
						list2.Add(component);
						if (height % 2 == 0)
						{
							doors[j].GetComponent<Door>().SetPortal(component);
						}
					}
				}
				else
				{
					doors[j].SetActive(false);
				}
			}
			catch
			{
				Debug.LogError("Not enough doors!");
			}
		}
		foreach (SECTR_Portal item4 in list2)
		{
			item4.Setup();
		}
	}

	private void GeneratorTask_SetRooms()
	{
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				foreach (ColorMap item in colorMap)
				{
					if (item.color == pixel)
					{
						PlaceRoom(new Vector2(j, i) + item.centerOffset, item);
					}
				}
			}
		}
	}

	private void GeneratorTask_RemoveNotRequired()
	{
		foreach (ColorMap item in colorMap)
		{
			bool flag = false;
			while (!flag)
			{
				int num = 0;
				foreach (Room item2 in roomsOfType[(int)item.type].roomsOfType)
				{
					num += item2.room.Count;
				}
				if (num <= roomsOfType[(int)item.type].amount)
				{
					break;
				}
				flag = true;
				for (int i = 0; i < roomsOfType[(int)item.type].roomsOfType.Count; i++)
				{
					if (!roomsOfType[(int)item.type].roomsOfType[i].required && roomsOfType[(int)item.type].roomsOfType[i].room.Count > 0)
					{
						roomsOfType[(int)item.type].roomsOfType[i].room[0].SetActive(false);
						roomsOfType[(int)item.type].roomsOfType[i].room.RemoveAt(0);
						flag = false;
						break;
					}
				}
			}
		}
	}

	private void GeneratorTask_CheckRooms()
	{
		for (int i = 0; i < map.height; i++)
		{
			for (int j = 0; j < map.width; j++)
			{
				Color pixel = map.GetPixel(j, i);
				foreach (ColorMap item in colorMap)
				{
					if (!(item.color == pixel))
					{
						continue;
					}
					BlankSquare(new Vector2(j, i) + item.centerOffset);
					roomsOfType[(int)item.type].amount++;
					List<Room> list = new List<Room>();
					bool flag = false;
					for (int k = 0; k < availableRooms.Count; k++)
					{
						if (availableRooms[k].type == item.type && availableRooms[k].room.Count > 0 && availableRooms[k].required)
						{
							flag = true;
						}
					}
					bool flag2 = false;
					do
					{
						flag2 = false;
						for (int l = 0; l < availableRooms.Count; l++)
						{
							if (availableRooms[l].type == item.type && availableRooms[l].room.Count > 0 && (availableRooms[l].required || !flag))
							{
								list.Add(new Room(availableRooms[l]));
								availableRooms.RemoveAt(l);
								flag2 = true;
								break;
							}
						}
					}
					while (flag2);
					foreach (Room item2 in list)
					{
						roomsOfType[(int)item.type].roomsOfType.Add(new Room(item2));
					}
				}
			}
		}
		map.SetPixels(copy);
		map.Apply();
	}

	private void PlaceRoom(Vector2 pos, ColorMap type)
	{
		string message = string.Empty;
		try
		{
			message = "blanking";
			BlankSquare(pos);
			Room room = null;
			message = "do";
			do
			{
				message = "rand";
				int num = UnityEngine.Random.Range(0, roomsOfType[(int)type.type].roomsOfType.Count);
				message = "rset " + (int)type.type + "/" + roomsOfType.Length + num;
				room = roomsOfType[(int)type.type].roomsOfType[num];
				if (room.room.Count == 0)
				{
					message = "remove";
					roomsOfType[(int)type.type].roomsOfType.RemoveAt(num);
				}
			}
			while (room.room.Count == 0);
			message = "pos";
			room.room[0].transform.localPosition = new Vector3(pos.x * gridSize / 3f, height, pos.y * gridSize / 3f) + offset;
			message = "rot";
			room.room[0].transform.localRotation = Quaternion.Euler(Vector3.up * (type.rotationY + y_offset));
			message = "rev";
			room.room[0].SetActive(true);
			room.room.RemoveAt(0);
		}
		catch
		{
			MonoBehaviour.print(message);
		}
	}

	private void BlankSquare(Vector2 centerPoint)
	{
		centerPoint = new Vector2(centerPoint.x - 1f, centerPoint.y - 1f);
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 3; j++)
			{
				map.SetPixel((int)centerPoint.x + i, (int)centerPoint.y + j, new Color(0.3921f, 0.3921f, 0.3921f, 1f));
			}
		}
		map.Apply();
	}

	private void Awake()
	{
		foreach (GameObject door in doors)
		{
			door.GetComponent<Door>().SetZero();
		}
	}
}
