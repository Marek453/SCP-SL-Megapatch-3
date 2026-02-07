using UnityEngine;
using UnityEngine.Networking;

public class GameMenu : MonoBehaviour
{
    public GameObject background;

    public GameObject[] minors;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !CursorManager.Scp294PanelOpen && !CursorManager.eqOpen && !CursorManager.consoleOpen)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        GameObject[] array = minors;
        foreach (GameObject gameObject in array)
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
        background.SetActive(!background.activeSelf);
        CursorManager.pauseOpen = background.activeSelf;
        GameObject[] players = PlayerManager.singleton.players;
        GameObject[] array2 = players;
        foreach (GameObject gameObject2 in array2)
        {
            if (gameObject2.GetComponent<NetworkIdentity>().isLocalPlayer)
            {
                gameObject2.GetComponent<FirstPersonController>().isPaused = background.activeSelf;
            }
        }
    }

    public void SelectMinor(int id)
    {
        GameObject[] array = minors;
        foreach (GameObject gameObject in array)
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }
        array[id].SetActive(true);
    }


	public void Disconnect()
	{
		GameObject[] players = PlayerManager.singleton.players;
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				if (gameObject.GetComponent<NetworkIdentity>().isServer)
				{
					Object.FindObjectOfType<NetworkManager>().StopHost();
				}
				else
				{
					Object.FindObjectOfType<NetworkManager>().StopClient();
				}
			}
		}
	}
}
