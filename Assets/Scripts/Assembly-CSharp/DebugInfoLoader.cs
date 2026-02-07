using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugInfoLoader : MonoBehaviour
{
	public Text GPU;

	public Text GPUMemory;

	public Text ShaderLevel;

	public Text GraphicAPI;

	public Text Resolution;

	public Text Fullscreen;

	public Text CPU;

	public Text CPUThreadsAndFrequency;

	public Text RAM;

	public Text Audio;

	public Text OS;

	public Text Steam;

	public Text UnityVersion;

	public Text GameVersion;

	public Text Build;

	public Text GameLanguage;

	public Text GameScene;

	public Text Errors;

	private void OnEnable()
	{
		GPU.text = SystemInfo.graphicsDeviceName;
		GPUMemory.text = "VRAM: " + SystemInfo.graphicsMemorySize + "MB";
		ShaderLevel.text = "ShaderLevel " + SystemInfo.graphicsShaderLevel.ToString().Insert(1, ".");
		GraphicAPI.text = string.Concat(SystemInfo.graphicsDeviceType, " ", SystemInfo.graphicsDeviceVersion);
		Resolution.text = Screen.width + "x" + Screen.height + "  " + Application.targetFrameRate;
		Fullscreen.text = "Fullscreen: " + ResolutionManager.fullscreen;
		CPU.text = SystemInfo.processorType;
		CPUThreadsAndFrequency.text = "Threads: " + SystemInfo.processorCount + "   " + SystemInfo.processorFrequency + "MHz";
		RAM.text = "RAM: " + SystemInfo.systemMemorySize + "MB";
		Audio.text = "Audio Supported: " + SystemInfo.supportsAudio;
		OS.text = SystemInfo.operatingSystem.Replace("  ", " ");
		Steam.text = "Steam Loaded: " + SteamManager.Initialized;
		UnityVersion.text = "Unity " + Application.unityVersion;
		GameVersion.text = "Version: " + Object.FindObjectOfType<CustomNetworkManager>().CompatibleVersions[0];
		string text = Application.buildGUID;
		if (string.IsNullOrEmpty(text))
		{
			text = "Unity Editor";
		}
		Build.text = "Build: " + text;
		GameLanguage.text = "Language:" + PlayerPrefs.GetString("translation_path", "English (default)");
		GameScene.text = "Scene: " + SceneManager.GetActiveScene().name;
		Errors.text = "Asserts: " + DebugScreenController.asserts + " Errors: " + DebugScreenController.errors + " Exceptions: " + DebugScreenController.exceptions;
	}
}
