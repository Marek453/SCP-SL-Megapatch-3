using UnityEngine;

public class PhysicsUpdateRate : MonoBehaviour
{
	public int updatesPerSecond = 60;

	private void Start()
	{
		Time.fixedDeltaTime = 1f / (float)updatesPerSecond;
	}
}
