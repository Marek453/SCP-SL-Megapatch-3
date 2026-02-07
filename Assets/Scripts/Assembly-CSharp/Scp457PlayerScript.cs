using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Scp457PlayerScript : NetworkBehaviour
{
	[Header("Player Properties")]
	public Camera plyCam;

	public bool iAm457;

	public bool sameClass;

	public float ultimatePoints;

	public float burnTime = 5f;

	private float curBurn;

	private GameObject[] players;

	public void Init(int classID, Class c)
	{
		sameClass = c.team == Team.SCP || c.team == Team.SH;
		if (classID == 19)
		{
			iAm457 = true;
		}
		else
		{
			iAm457 = false;
		}
	}

	private void Start()
	{
		StartCoroutine(DeductFireHP());
		InvokeRepeating("DetectPlayersInRange", 1f, 0.2f);
		InvokeRepeating("RefreshPlayerList", 1f, 5f);
	}

	private void RefreshPlayerList()
	{
		players = PlayerManager.singleton.players;
	}

	private void DetectPlayersInRange()
	{
		if (!base.isLocalPlayer || !iAm457)
		{
			return;
		}
		GameObject[] array = players;
		foreach (GameObject gameObject in array)
		{
			if (gameObject != null && !gameObject.GetComponent<Scp457PlayerScript>().sameClass && Vector3.Distance(base.transform.position, gameObject.transform.position) < 2f)
			{
				print("dasdas");
				CmdBurnPlayer(gameObject.transform.gameObject);
			}
		}
	}

	private IEnumerator DeductFireHP()
	{
		if (!base.isLocalPlayer)
		{
			yield break;
		}
		while (true)
		{
			if (curBurn > 0f)
			{
				curBurn -= 0.2f;
				CmdSelfDeduct(base.gameObject, 1f);
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

	public void Burn()
	{
		curBurn = burnTime;
	}

	[Command]
	private void CmdSelfDeduct(GameObject go, float am)
	{
		go.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(am,"SCP-457", "FIRE", 0),go);
	}

	[ClientRpc]
	private void RpcBurnPlayer(GameObject go)
	{
		go.GetComponent<Scp457PlayerScript>().Burn();
	}

	[Command]
	private void CmdBurnPlayer(GameObject go)
	{
		RpcBurnPlayer(go);
	}
}
