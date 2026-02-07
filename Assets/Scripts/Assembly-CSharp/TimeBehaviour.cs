using System;
using UnityEngine;

public class TimeBehaviour : MonoBehaviour
{
	public static long CurrentTimestamp()
	{
		return DateTime.UtcNow.Ticks;
	}

	public static bool ValidateTimestamp(long timestampentry, long timestampexit, long limit)
	{
		return timestampexit - timestampentry < limit;
	}
}
