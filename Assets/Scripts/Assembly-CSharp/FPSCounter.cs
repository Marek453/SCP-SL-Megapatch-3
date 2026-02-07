using System;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
	public Text text;

	private ushort k = 24;

	private double framerate;

	private double frametime;

	private void Update()
	{
		float deltaTime = Time.deltaTime;
		framerate = Math.Round(1f / deltaTime, 1);
		frametime = Math.Round(deltaTime * 1000f, 1);
	}

	private void FixedUpdate()
	{
		k++;
		if (k == 25)
		{
			k = 0;
			text.text = "Framerate: " + framerate + "   " + frametime + "ms";
		}
	}
}
