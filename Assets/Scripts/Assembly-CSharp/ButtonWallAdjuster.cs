using UnityEngine;

public class ButtonWallAdjuster : MonoBehaviour
{
	public float offset = 0.1f;

	private bool adjusted;

	public bool onAwake;

	private void Start()
	{
		if (onAwake)
		{
			Adjust();
		}
	}

	public void Adjust()
	{
		if (!adjusted || onAwake)
		{
			adjusted = true;
			base.transform.position += base.transform.up;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(base.transform.position, -base.transform.up), out hitInfo, 2.5f))
			{
				base.transform.position = hitInfo.point;
				base.transform.position -= base.transform.up * offset * 0.1f;
			}
		}
	}
}
