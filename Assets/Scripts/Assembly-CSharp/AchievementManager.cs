using Steamworks;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
	public static void Achieve(string key)
	{
		if (SteamManager.Initialized && !ServerStatic.isDedicated)
		{
			SteamUserStats.SetAchievement(key);
			Debug.Log("Achievement get! " + key);
			SteamUserStats.RequestCurrentStats();
		}
	}

	public static void StatsProgress(string key, string completeAchievement, int maxValue)
	{
		if (SteamManager.Initialized && !ServerStatic.isDedicated)
		{
			int pData;
			SteamUserStats.GetStat(key, out pData);
			pData++;
			pData = Mathf.Clamp(pData, 0, maxValue);
			SteamUserStats.SetStat(key, pData);
			SteamUserStats.IndicateAchievementProgress(completeAchievement, (uint)pData, (uint)maxValue);
			Debug.Log("Stats Progress! " + key + " " + pData + "/" + maxValue);
		}
	}
}
