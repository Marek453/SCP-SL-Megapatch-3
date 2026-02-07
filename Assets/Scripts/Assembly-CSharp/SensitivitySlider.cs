using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class SensitivitySlider : MonoBehaviour
{
	public Slider slider;

	private void Start()
	{
		OnValueChanged(PlayerPrefs.GetFloat("Sens", 1f));
		slider.value = PlayerPrefs.GetFloat("Sens", 1f);
	}

	public void OnValueChanged(float vol)
	{
		PlayerPrefs.SetFloat("Sens", vol);
		Sensitivity.sens = slider.value;
	}
}
