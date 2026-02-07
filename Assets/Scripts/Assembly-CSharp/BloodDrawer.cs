using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BloodDrawer : NetworkBehaviour
{
	[Serializable]
	public class BloodType
	{
		public GameObject[] prefabs;
	}

	public LayerMask mask;

	private static List<Transform> instances = new List<Transform>();

	public int maxBlood = 500;

	public BloodType[] bloodTypes;

	private static int iteration;

	private void Start()
	{
		if (base.isLocalPlayer)
		{
			instances = new List<Transform>();
		}
		maxBlood = PlayerPrefs.GetInt("gfxsets_blood", 500);
	}

	public void DrawBlood(Vector3 pos, Quaternion rot, int bloodType)
	{
	}

	public void PlaceUnderneath(Transform obj, int type, float amountMultiplier = 1f)
	{
		PlaceUnderneath(obj.position, type, amountMultiplier);
	}

	public void PlaceUnderneath(Vector3 pos, int type, float amountMultiplier = 1f)
	{
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
