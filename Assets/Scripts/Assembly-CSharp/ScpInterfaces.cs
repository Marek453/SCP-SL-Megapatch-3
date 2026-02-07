using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ScpInterfaces : MonoBehaviour
{
	public GameObject Scp106_eq;

	public TextMeshProUGUI Scp106_ability_highlight;

	public Text Scp106_ability_points;

	public GameObject Scp049_eq;

	public Image Scp049_loading;

	public Image Scp008_Hud;

	private GameObject FindLocalPlayer()
	{
		GameObject[] array = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject gameObject in array)
		{
			if (gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
			{
				return gameObject;
			}
		}
		return null;
	}

	public void CreatePortal()
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().CreatePortalInCurrentPosition();
	}

	public void Update106Highlight(int id)
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().highlightID = id;
	}

	public void Use106Portal()
	{
		FindLocalPlayer().GetComponent<Scp106PlayerScript>().UseTeleport();
	}
}
