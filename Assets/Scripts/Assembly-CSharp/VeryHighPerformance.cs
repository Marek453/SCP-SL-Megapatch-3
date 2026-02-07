using UnityEngine;

public class VeryHighPerformance : MonoBehaviour
{
	private Light[] lights;
	private Color origAmbientEquatorColor;
	private Color origambientGroundColor;
	private Color origambientSkyColor;
	private void Start()
	{
		lights = Object.FindObjectsOfType<Light>();
		origAmbientEquatorColor = RenderSettings.ambientEquatorColor;
		origambientGroundColor = RenderSettings.ambientGroundColor;
		origambientSkyColor = RenderSettings.ambientSkyColor;
		if (PlayerPrefs.GetInt("gfxsets_hp", 0) != 0)
		{
			Disable();
		}
	}
	public void Enable()
	{
		foreach (Light light in lights)
		{
			light.enabled = true;
		}
		RenderSettings.ambientEquatorColor = origAmbientEquatorColor;
		RenderSettings.ambientGroundColor = origambientGroundColor;
		RenderSettings.ambientSkyColor = origambientSkyColor;
	}
	public void Online()
	{
		RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
		RenderSettings.ambientGroundColor = new Color(0.5f, 0.5f, 0.5f);
		RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f);
	}

	public void OffLineLight()
	{
			RenderSettings.ambientEquatorColor = origAmbientEquatorColor;
		RenderSettings.ambientGroundColor = origambientGroundColor;
		RenderSettings.ambientSkyColor = origambientSkyColor;
	}

	public void Disable()
	{
		foreach (Light light in lights)
		{
			light.enabled = false;
		}
		RenderSettings.ambientEquatorColor = new Color(0.5f, 0.5f, 0.5f);
		RenderSettings.ambientGroundColor = new Color(0.5f, 0.5f, 0.5f);
		RenderSettings.ambientSkyColor = new Color(0.5f, 0.5f, 0.5f);
	}
}
