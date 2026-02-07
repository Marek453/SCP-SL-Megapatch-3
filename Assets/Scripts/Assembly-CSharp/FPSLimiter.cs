using UnityEngine;
using UnityEngine.UI;

public class FPSLimiter : MonoBehaviour
{
	public GameObject warning;

	private void OnEnable()
	{
		if (QualitySettings.vSyncCount != 0)
		{
			warning.SetActive(true);
		}
		else
		{
			warning.SetActive(false);
		}
		int @int = PlayerPrefs.GetInt("MaxFramerate", 969);
		if (@int == 969)
		{
			Application.targetFrameRate = -1;
		}
		else
		{
			Application.targetFrameRate = @int;
		}
		if (Application.targetFrameRate == -1)
		{
			base.gameObject.GetComponent<Dropdown>().value = 0;
			return;
		}
		bool flag = false;
		for (int i = 1; i < base.gameObject.GetComponent<Dropdown>().options.Count; i++)
		{
			int result = 0;
			if (!flag && int.TryParse(base.gameObject.GetComponent<Dropdown>().options[i].text, out result) && result == Application.targetFrameRate)
			{
				base.gameObject.GetComponent<Dropdown>().value = i;
				flag = true;
			}
		}
		if (!flag)
		{
			base.gameObject.GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(Application.targetFrameRate.ToString()));
			base.gameObject.GetComponent<Dropdown>().RefreshShownValue();
			base.gameObject.GetComponent<Dropdown>().value = base.gameObject.GetComponent<Dropdown>().options.Count - 1;
		}
	}

	public void OnValueChange()
	{
		ChangeLimit(base.gameObject.GetComponent<Dropdown>().options[base.gameObject.GetComponent<Dropdown>().value].text);
	}

	private void ChangeLimit(string limit)
	{
		int result;
		if (int.TryParse(base.gameObject.GetComponent<Dropdown>().options[base.gameObject.GetComponent<Dropdown>().value].text, out result))
		{
			Application.targetFrameRate = Mathf.Clamp(result, 15, 999);
			PlayerPrefs.SetInt("MaxFramerate", Mathf.Clamp(result, 15, 999));
		}
		else
		{
			Application.targetFrameRate = -1;
			PlayerPrefs.SetInt("MaxFramerate", 969);
		}
	}
}
