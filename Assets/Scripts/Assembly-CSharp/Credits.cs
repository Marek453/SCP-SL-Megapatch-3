using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour
{
	[Serializable]
	public class CreditLogType
	{
		public float preTime;

		public float postTime;

		public GameObject preset;
	}

	[Serializable]
	public class CreditLog
	{
		public string text1_en;

		public string text2_en;

		public int type;
	}

	public Transform maskPosition;
    private CoroutineHandle coroutineHandle;
    [Range(0.2f, 2.5f)]
	public float speed = 1f;

	public CreditLogType[] logTypes;

	public CreditLog[] logQueue;

	private List<GameObject> spawnedLogs = new List<GameObject>();

	private void SpawnType(CreditLogType l, string txt1, string txt2)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(l.preset, maskPosition);
		Text[] componentsInChildren = gameObject.GetComponentsInChildren<Text>();
		componentsInChildren[0].text = txt1;
		if (componentsInChildren.Length > 1)
		{
			componentsInChildren[1].text = txt2;
		}
		UnityEngine.Object.Destroy(gameObject, 12f / speed);
		spawnedLogs.Add(gameObject);
		CreditText component = gameObject.GetComponent<CreditText>();
		component.move = true;
		component.speed *= speed;
	}

	private void OnEnable()
	{
        StartCoroutine(_Play());
	}

	private void OnDisable()
	{
        StopAllCoroutines();
		foreach (GameObject spawnedLog in spawnedLogs)
		{
			if (spawnedLog != null)
			{
				Destroy(spawnedLog);
			}
		}
		spawnedLogs.Clear();
	}

	private IEnumerator _Play()
	{
		bool first = true;
		CreditLog[] array = logQueue;
		foreach (CreditLog item in array)
		{
			CreditLogType type = logTypes[item.type];
			if (first)
			{
				first = false;
			}
			else
			{
				for (int j = 0; (float)j < type.preTime / speed; j++)
				{
					yield return new WaitForSeconds(0.02f / speed);
				}
			}
			SpawnType(type, item.text1_en, item.text2_en);
			for (int i = 0; (float)i < type.postTime; i++)
			{
				yield return new WaitForSeconds(0.02f / speed);
			}
		}
		yield return new WaitForSeconds(8f / speed);
		GetComponentInParent<MainMenuScript>().ChangeMenu(0);
	}
}
