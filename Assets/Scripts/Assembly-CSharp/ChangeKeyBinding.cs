using System;
using System.Collections;
using System.Collections.Generic;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class ChangeKeyBinding : MonoBehaviour
{
	public Transform list_parent;

	public GameObject list_element;

	private List<GameObject> instances = new List<GameObject>();

	private bool working;

	private void Start()
	{
		RefreshList();
	}

	private void RefreshList()
	{
		working = false;
		foreach (GameObject instance in instances)
		{
			UnityEngine.Object.Destroy(instance);
		}
		NewInput.Load();
		for (int i = 0; i < NewInput.bindings.Count; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(list_element, list_parent);
			gameObject.transform.localScale = Vector3.one;
			gameObject.SetActive(true);
			instances.Add(gameObject);
			gameObject.GetComponentInChildren<Text>().text = NewInput.bindings[i].axis.ToString();
			Button componentInChildren = gameObject.GetComponentInChildren<Button>();
			componentInChildren.GetComponentInChildren<Text>().text = NewInput.bindings[i].key.ToString();
			componentInChildren.GetComponent<KeyBindElement>().axis = NewInput.bindings[i].axis;
		}
	}

	private IEnumerator<float> _AwaitPress(string axis)
	{
		if (!working)
		{
			working = true;
			KeyCode code = KeyCode.None;
			while (code == KeyCode.None || code == KeyCode.Escape)
			{
				code = GetCurrentKey();
				yield return 0f;
			}
			if (code != KeyCode.Escape)
			{
				NewInput.ChangeKey(axis, code);
			}
			RefreshList();
		}
	}

	public void ChangeKey(string axis)
	{
		Timing.RunCoroutine(_AwaitPress(axis), Segment.FixedUpdate);
	}

	public void Revent()
	{
		NewInput.Revent();
		RefreshList();
	}

	private KeyCode GetCurrentKey()
	{
		IEnumerator enumerator = Enum.GetValues(typeof(KeyCode)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				if (Input.GetKey((KeyCode)current))
				{
					return (KeyCode)current;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		return KeyCode.None;
	}
}
