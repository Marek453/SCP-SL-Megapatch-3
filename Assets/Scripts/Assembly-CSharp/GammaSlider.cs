using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class GammaSlider : MonoBehaviour
{
	public PostProcessingProfile profile;

	public Slider slider;

	public Text warningText;

	private void Start()
	{
		if (slider != null)
		{
			slider.value = PlayerPrefs.GetFloat("gamma", 0f);
			SetValue(slider.value);
		}
	}

	public void SetValue(float f)
	{
		warningText.enabled = f > 0.5f;
		PlayerPrefs.SetFloat("gamma", f);
		ColorGradingModel.Settings settings = default(ColorGradingModel.Settings);
		settings = profile.colorGrading.settings;
		settings.basic.postExposure = f;
		profile.colorGrading.settings = settings;
	}
}
