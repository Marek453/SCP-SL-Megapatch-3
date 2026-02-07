using UnityEngine;

public class DebugLogScreen : MonoBehaviour
{
	public GameObject log;

	public GameObject info;

	private void OnEnable()
	{
		info.SetActive(true);
		CursorManager.debuglogopen = log.activeSelf;
	}

	private void OnDisable()
	{
		info.SetActive(false);
		CursorManager.debuglogopen = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F4))
		{
			log.SetActive(!log.activeSelf);
			CursorManager.debuglogopen = log.activeSelf;
		}
	}
}
