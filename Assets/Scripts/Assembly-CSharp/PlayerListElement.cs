using Dissonance;
using Dissonance.Integrations.UNet_HLAPI;
using UnityEngine;

public class PlayerListElement : MonoBehaviour
{
	public GameObject instance;

	public void Use(bool b)
	{
		Object.FindObjectOfType<DissonanceComms>().FindPlayer(instance.GetComponent<HlapiPlayer>().PlayerId).IsLocallyMuted = b;
	}
}
