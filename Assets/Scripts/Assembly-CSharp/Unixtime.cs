using System;
using UnityEngine;

public class Unixtime : MonoBehaviour
{
	private DiscordController controller;

	private void Start()
	{
		ResetTime();
	}

	public void ResetTime()
	{
		controller = GetComponent<DiscordController>();
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
		long startTimestamp = (long)(DateTime.UtcNow - dateTime).TotalSeconds;
		controller.presence.startTimestamp = startTimestamp;
	}
}
