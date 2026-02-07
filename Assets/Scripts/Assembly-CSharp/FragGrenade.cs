using System.Linq;
using RemoteAdmin;
using UnityEngine;
using UnityEngine.Networking;

public class FragGrenade : Grenade
{
	public GameObject explosionEffects;

	public AnimationCurve shakeOverDistance;

	public AnimationCurve damageOverDistance;

	public LayerMask layerMask;

	public LayerMask triggerMask;

	public float triggerOtherNadesDistance = 12f;

	private static int thrownFrags;

	public override void ClientsideExplosion()
	{
		Object.Destroy(Object.Instantiate(explosionEffects, base.transform.position, explosionEffects.transform.rotation), 10f);
		GrenadeManager.grenadesOnScene.Remove(this);
		ExplosionCameraShake.singleton.Shake(shakeOverDistance.Evaluate(Vector3.Distance(base.transform.position, PlayerManager.localPlayer.transform.position)));
		Object.Destroy(base.gameObject);
	}

	public override void ServersideExplosion(int grenadeOwnerPlayerID)
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, triggerOtherNadesDistance, triggerMask);
		if (NetworkServer.active)
		{
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				Pickup componentInChildren = collider.GetComponentInChildren<Pickup>();
				if (componentInChildren != null && componentInChildren.info.itemId == 25)
				{
					thrownFrags++;
					PlayerManager.localPlayer.GetComponent<GrenadeManager>().ChangeIntoGrenade(componentInChildren, 0, grenadeOwnerPlayerID, thrownFrags, ((componentInChildren.transform.position - base.transform.position).normalized + Vector3.up / 3f).normalized * 16f, componentInChildren.transform.position);
				}
			}
		}
		bool @bool = ConfigFile.ServerConfig.GetBool("friendly_fire");
		GameObject gameObject = null;
		if (!@bool)
		{
			GameObject[] players = PlayerManager.singleton.players;
			foreach (GameObject gameObject2 in players)
			{
				if (gameObject2.GetComponent<QueryProcessor>().PlayerId == grenadeOwnerPlayerID)
				{
					gameObject = gameObject2;
				}
			}
		}
		GameObject[] players2 = PlayerManager.singleton.players;
		foreach(var door in GameObject.FindObjectsOfType<Door>().ToList().Where(door => Vector3.Distance(door.transform.position, base.transform.position) < triggerOtherNadesDistance))
        {
			if(door.destroyedPrefab != null)
			{
				door.DestroyDoor(true);	
			}
        }
		foreach (GameObject gameObject3 in players2)
		{
			PlayerStats component = gameObject3.GetComponent<PlayerStats>();
			if (component == null || component.ccm.curClass == 2)
			{
				continue;
			}
			float num = damageOverDistance.Evaluate(Vector3.Distance(base.transform.position, component.transform.position));
			num = ((!component.ccm.IsHuman()) ? (num * ConfigFile.ServerConfig.GetFloat("scp_grenade_multiplier", 1f)) : (num * ConfigFile.ServerConfig.GetFloat("human_grenade_multiplier", 0.7f)));
			if (!(num > 5f) || (!@bool && gameObject3.GetComponent<QueryProcessor>().PlayerId != grenadeOwnerPlayerID && (gameObject == null || !gameObject.GetComponent<WeaponManager>().GetShootPermission(component.ccm))))
			{
				continue;
			}
			Transform[] grenadePoints = component.grenadePoints;
			foreach (Transform transform in grenadePoints)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(new Ray(base.transform.position, (transform.position - base.transform.position).normalized), out hitInfo, 100f, layerMask) && hitInfo.collider.GetComponentInParent<PlayerStats>() == component)
				{
					component.HurtPlayer(new PlayerStats.HitInfo(num, "GRENADE", "FRAG", grenadeOwnerPlayerID), gameObject3);
					break;
				}
			}
		}
	}
}
