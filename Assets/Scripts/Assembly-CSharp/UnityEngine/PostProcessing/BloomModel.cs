using System;

namespace UnityEngine.PostProcessing
{
	[Serializable]
	public class BloomModel : PostProcessingModel
	{
		[Serializable]
		public struct BloomSettings
		{
			[Tooltip("Strength of the bloom filter.")]
			[Min(0f)]
			public float intensity;

			[Tooltip("Filters out pixels under this level of brightness.")]
			[Min(0f)]
			public float threshold;

			[Tooltip("Makes transition between under/over-threshold gradual (0 = hard threshold, 1 = soft threshold).")]
			[Range(0f, 1f)]
			public float softKnee;

			[Tooltip("Changes extent of veiling effects in a screen resolution-independent fashion.")]
			[Range(1f, 7f)]
			public float radius;

			[Tooltip("Reduces flashing noise with an additional filter.")]
			public bool antiFlicker;

			public float thresholdLinear
			{
				get
				{
					return Mathf.GammaToLinearSpace(threshold);
				}
				set
				{
					threshold = Mathf.LinearToGammaSpace(value);
				}
			}

			public static BloomSettings defaultSettings
			{
				get
				{
					BloomSettings result = default(BloomSettings);
					result.intensity = 0.5f;
					result.threshold = 1.1f;
					result.softKnee = 0.5f;
					result.radius = 4f;
					result.antiFlicker = false;
					return result;
				}
			}
		}

		[Serializable]
		public struct LensDirtSettings
		{
			[Tooltip("Dirtiness texture to add smudges or dust to the lens.")]
			public Texture texture;

			[Min(0f)]
			[Tooltip("Amount of lens dirtiness.")]
			public float intensity;

			public static LensDirtSettings defaultSettings
			{
				get
				{
					LensDirtSettings result = default(LensDirtSettings);
					result.texture = null;
					result.intensity = 3f;
					return result;
				}
			}
		}

		[Serializable]
		public struct Settings
		{
			public BloomSettings bloom;

			public LensDirtSettings lensDirt;

			public static Settings defaultSettings
			{
				get
				{
					Settings result = default(Settings);
					result.bloom = BloomSettings.defaultSettings;
					result.lensDirt = LensDirtSettings.defaultSettings;
					return result;
				}
			}
		}

		[SerializeField]
		private Settings m_Settings = Settings.defaultSettings;

		public Settings settings
		{
			get
			{
				return m_Settings;
			}
			set
			{
				m_Settings = value;
			}
		}

		public override void Reset()
		{
			m_Settings = Settings.defaultSettings;
		}
	}
}
