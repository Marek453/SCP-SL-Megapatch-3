using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class AnimatedScale : MonoBehaviour
	{
		public float desiredScale = 2f;

		public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		public float speed = 1f;

		private Vector3 initialScale;

		private float sampleTime;

		private void OnEnable()
		{
			StartCoroutine(Scale());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private IEnumerator Scale()
		{
			yield return new WaitForFixedUpdate();
			initialScale = base.transform.localScale;
			while (sampleTime < 1f)
			{
				sampleTime = Mathf.MoveTowards(sampleTime, 1f, Time.fixedDeltaTime / speed);
				float scaleModifier = Mathf.Lerp(1f, desiredScale, curve.Evaluate(sampleTime));
				base.transform.localScale = initialScale * scaleModifier;
				yield return new WaitForFixedUpdate();
			}
		}
	}
}
