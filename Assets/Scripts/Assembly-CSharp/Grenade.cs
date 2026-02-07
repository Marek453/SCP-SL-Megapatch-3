using UnityEngine;
using UnityEngine.Networking;

public class Grenade : MonoBehaviour
{
	public AudioClip[] collisionSounds;

	public float collisionSpeedToSound;

	public string id;

	public void Explode(int playerID)
	{
		if (NetworkServer.active)
		{
			ServersideExplosion(playerID);
		}
		ClientsideExplosion();
	}

	public virtual void ServersideExplosion(int grenadeOwnerPlayerID)
	{
	}

	public virtual void ClientsideExplosion()
	{
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.relativeVelocity.magnitude > collisionSpeedToSound)
		{
			GetComponent<AudioSource>().PlayOneShot(collisionSounds[Random.Range(0, collisionSounds.Length)]);
		}
	}

	public void SyncMovement(Vector3 pos, Vector3 vel, Quaternion rot, Vector3 angularSpeed)
	{
		if (Vector3.Distance(pos, base.transform.position) > 1f)
		{
			GetComponent<Rigidbody>().velocity = vel;
			GetComponent<Rigidbody>().angularVelocity = angularSpeed;
			base.transform.position = pos;
			base.transform.rotation = rot;
		}
	}
}
