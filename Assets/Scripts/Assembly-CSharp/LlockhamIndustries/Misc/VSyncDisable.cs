using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class VSyncDisable : MonoBehaviour
	{
		private void Start()
		{
			QualitySettings.vSyncCount = 0;
		}
	}
}
