using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
	public static bool eqOpen;

	public static bool pauseOpen;

	public static bool Scp294PanelOpen;

	public static bool isServerOnly;

	public static bool consoleOpen;

	public static bool is079;

	public static bool scp106;

	public static bool roundStarted;

	public static bool raOp;

	public static bool plOp;

	public static bool debuglogopen;

	public static bool isNotFacility;

	public static bool isApplicationNotFocused;

	private void LateUpdate()
	{
		bool flag = eqOpen | pauseOpen | isServerOnly | consoleOpen | is079 | scp106 | roundStarted | raOp | plOp | isNotFacility | isApplicationNotFocused | Scp294PanelOpen;
		Cursor.lockState = ((!flag) ? CursorLockMode.Locked : CursorLockMode.None);
		Cursor.visible = flag;
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded += OnSceneWasLoaded;
	}

	private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
	{
		UnsetAll();
		isNotFacility = SceneManager.GetActiveScene().name != "Facility";
	}

	private void OnApplicationFocus(bool focus)
	{
		isApplicationNotFocused = !focus;
	}

	public static void UnsetAll()
	{
		eqOpen = false;
		pauseOpen = false;
		isServerOnly = false;
		consoleOpen = false;
		is079 = false;
		scp106 = false;
		roundStarted = false;
		raOp = false;
		plOp = false;
		debuglogopen = false;
		Scp294PanelOpen = false;
	}
}
