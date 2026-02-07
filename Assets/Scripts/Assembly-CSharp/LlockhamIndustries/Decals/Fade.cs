using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[RequireComponent(typeof(ProjectionRenderer))]
	public class Fade : Modifier
	{
		public FadeType type;

		public FadeWrapMode wrapMode;

		public AnimationCurve fade = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		public float fadeLength = 1f;

		private ProjectionRenderer projection;

		private float timeElapsed;

		private bool executing;

		private float originalAlpha;

		private Vector3 originalScale;

		private void Awake()
		{
			projection = GetComponent<ProjectionRenderer>();
		}

		protected override void Begin()
		{
			timeElapsed = 0f;
			executing = true;
			originalAlpha = GetAlpha(projection);
			originalScale = projection.transform.localScale;
			switch (wrapMode)
			{
			case FadeWrapMode.Loop:
				fade.postWrapMode = WrapMode.Loop;
				break;
			case FadeWrapMode.PingPong:
				fade.postWrapMode = WrapMode.PingPong;
				break;
			case FadeWrapMode.Once:
			case FadeWrapMode.Clamp:
				fade.postWrapMode = WrapMode.ClampForever;
				break;
			}
			ApplyFade(projection, 0f);
		}

		public override void Perform()
		{
			if (!executing || fade == null)
			{
				return;
			}
			if ((wrapMode != FadeWrapMode.Clamp && wrapMode != 0) || timeElapsed <= fadeLength)
			{
				timeElapsed += base.UpdateRate;
				ApplyFade(projection, timeElapsed / fadeLength);
				return;
			}
			if (wrapMode == FadeWrapMode.Clamp)
			{
				ApplyFade(projection, 1f);
			}
			if (wrapMode == FadeWrapMode.Once)
			{
				projection.Destroy();
			}
			executing = false;
		}

		private void ApplyFade(ProjectionRenderer Projection, float Time)
		{
			float num = fade.Evaluate(Time);
			switch (type)
			{
			case FadeType.Alpha:
				SetAlpha(Projection, originalAlpha * num);
				break;
			case FadeType.Scale:
				SetScale(Projection, originalScale * num);
				break;
			case FadeType.Both:
				SetAlpha(Projection, originalAlpha * num);
				SetScale(Projection, originalScale * num);
				break;
			}
		}

		private float GetAlpha(ProjectionRenderer Projection)
		{
			switch (Projection.Properties[0].type)
			{
			case PropertyType.Float:
				return Projection.Properties[0].value;
			case PropertyType.Color:
				return Projection.Properties[0].color.a;
			case PropertyType.Combo:
				return Projection.Properties[0].value;
			default:
				return 1f;
			}
		}

		private void SetAlpha(ProjectionRenderer Projection, float Alpha)
		{
			switch (Projection.Properties[0].type)
			{
			case PropertyType.Float:
			case PropertyType.Combo:
				Projection.SetFloat(0, Alpha);
				break;
			case PropertyType.Color:
			{
				Color color = Projection.Properties[0].color;
				color.a = Alpha;
				Projection.SetColor(0, color);
				break;
			}
			}
			Projection.UpdateProperties();
		}

		private void SetScale(ProjectionRenderer Projection, Vector3 Scale)
		{
			Projection.transform.localScale = Scale;
		}
	}
}
