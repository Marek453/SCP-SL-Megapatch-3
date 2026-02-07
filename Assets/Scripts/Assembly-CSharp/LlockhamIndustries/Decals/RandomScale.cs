using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class RandomScale : MonoBehaviour
	{
		public float minSize = 0.5f;

		public float maxSize = 0.8f;

		private void Awake()
		{
			float num = Random.Range(minSize, maxSize);
			base.transform.localScale = new Vector3(num, num, num);
		}
	}
}
