using UnityEngine;

public class FlashGrenade : Grenade
{
	public GameObject explosionEffects;

	public AnimationCurve shakeOverDistance;

	public AnimationCurve powerOverDistance;

	public AnimationCurve powerOverDot;

	public LayerMask viewLayerMask;

	private void Start()
	{
	}

	private void Update()
	{
		Transform transform = PlayerManager.localPlayer.GetComponent<Scp049PlayerScript>().plyCam.transform;
		Debug.Log(Vector3.Dot(transform.forward, (transform.position - base.transform.position).normalized));
		Debug.DrawRay(transform.position, -(transform.position - base.transform.position).normalized, Color.red, 10f);
	}

	public override void ClientsideExplosion()
	{
		Object.Destroy(Object.Instantiate(explosionEffects, base.transform.position, explosionEffects.transform.rotation), 10f);
		GrenadeManager.grenadesOnScene.Remove(this);
		ExplosionCameraShake.singleton.Shake(shakeOverDistance.Evaluate(Vector3.Distance(base.transform.position, PlayerManager.localPlayer.transform.position)));
		Transform transform = PlayerManager.localPlayer.GetComponent<Scp049PlayerScript>().plyCam.transform;
		RaycastHit hitInfo;
		if (Physics.Raycast(transform.position, -(transform.position - base.transform.position).normalized, out hitInfo, 1000f, viewLayerMask) && hitInfo.collider.gameObject.layer == 20)
		{
			PlayerManager.localPlayer.GetComponent<FlashEffect>().Play(powerOverDistance.Evaluate(Vector3.Distance(PlayerManager.localPlayer.transform.position, base.transform.position)) * powerOverDot.Evaluate(Vector3.Dot(transform.forward, (transform.position - base.transform.position).normalized)));
		}
		Object.Destroy(base.gameObject);
	}
}
