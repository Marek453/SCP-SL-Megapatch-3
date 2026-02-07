using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResolutionManager : MonoBehaviour
{
	[Serializable]
	public class ResolutionPreset
	{
		public int width;

		public int height;

		public ResolutionPreset(Resolution template)
		{
			width = template.width;
			height = template.height;
		}

		public void SetResolution()
		{
			Screen.SetResolution(width, height, fullscreen);
		}
	}

	public static int preset;

	public static bool fullscreen;

	public static List<ResolutionPreset> presets = new List<ResolutionPreset>();

	private bool FindResolution(Resolution res)
	{
		foreach (ResolutionPreset preset in presets)
		{
			if (preset.height == res.height && preset.width == res.width)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		presets.Clear();
		Resolution[] resolutions = Screen.resolutions;
		foreach (Resolution resolution in resolutions)
		{
			if (!FindResolution(resolution))
			{
				presets.Add(new ResolutionPreset(resolution));
			}
		}
		preset = Mathf.Clamp(PlayerPrefs.GetInt("SavedResolutionSet", presets.Count - 1), 0, presets.Count - 1);
		fullscreen = PlayerPrefs.GetInt("SavedFullscreen", 1) != 0;
		int @int = PlayerPrefs.GetInt("MaxFramerate", 969);
		if (@int == 969)
		{
			Application.targetFrameRate = -1;
		}
		else
		{
			Application.targetFrameRate = @int;
		}
		RefreshScreen();
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		RefreshScreen();
	}

	public static void RefreshScreen()
	{
		presets[Mathf.Clamp(preset, 0, presets.Count - 1)].SetResolution();
		try
		{
			UnityEngine.Object.FindObjectOfType<ResolutionText>().txt.text = presets[Mathf.Clamp(preset, 0, presets.Count - 1)].width + " × " + presets[Mathf.Clamp(preset, 0, presets.Count - 1)].height;
		}
		catch
		{
		}
	}

	public static void ChangeResolution(int id)
	{
		preset = Mathf.Clamp(preset + id, 0, presets.Count - 1);
		PlayerPrefs.SetInt("SavedResolutionSet", preset);
		RefreshScreen();
	}

	public static void ChangeFullscreen(bool isTrue)
	{
		fullscreen = isTrue;
		PlayerPrefs.SetInt("SavedFullscreen", isTrue ? 1 : 0);
		RefreshScreen();
	}
}
