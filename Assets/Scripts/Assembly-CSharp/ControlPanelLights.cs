using System.Collections.Generic;
using MEC;
using UnityEngine;

public class ControlPanelLights : MonoBehaviour
{
	public Texture[] emissions;

	public Material targetMat;

	private void Start()
	{
		Timing.RunCoroutine(_Animate(), Segment.FixedUpdate);
	}

	private IEnumerator<float> _Animate()
	{
		int i = emissions.Length;
		while (true)
		{
			targetMat.SetTexture("_EmissionMap", emissions[Random.Range(0, i)]);
			yield return Timing.WaitForSeconds(Random.Range(0.2f, 0.8f));
		}
	}
}
