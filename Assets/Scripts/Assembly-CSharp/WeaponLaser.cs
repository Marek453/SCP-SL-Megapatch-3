using UnityEngine;

public class WeaponLaser : MonoBehaviour
{
	public bool isenabled;

	public Light l;

	private void Update()
	{
		l.enabled = isenabled;
	}
}
