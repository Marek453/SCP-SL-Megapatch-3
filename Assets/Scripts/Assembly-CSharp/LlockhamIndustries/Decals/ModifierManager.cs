using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public class ModifierManager : MonoBehaviour
	{
		private static ModifierManager singleton;

		private List<Modifier> perFrameModifiers;

		private List<Modifier> tenModifiers;

		private List<Modifier> oneModifiers;

		private WaitForSeconds tenthOfASecond = new WaitForSeconds(0.1f);

		private WaitForSeconds second = new WaitForSeconds(1f);

		public static bool Initialized
		{
			get
			{
				return singleton != null;
			}
		}

		public static ModifierManager Singleton
		{
			get
			{
				if (singleton == null)
				{
					GameObject gameObject = new GameObject("Dynamic Decals");
					gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
					gameObject.AddComponent<ModifierManager>();
				}
				return singleton;
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		private void OnEnable()
		{
			if (singleton == null)
			{
				singleton = this;
			}
			else if (singleton != this)
			{
				if (Application.isPlaying)
				{
					Object.Destroy(base.gameObject);
				}
				else
				{
					Object.DestroyImmediate(base.gameObject, true);
				}
				return;
			}
			perFrameModifiers = new List<Modifier>();
			tenModifiers = new List<Modifier>();
			oneModifiers = new List<Modifier>();
			StartCoroutine(TenTimesPerSecond());
			StartCoroutine(OncePerSecond());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private static List<Modifier> GetModifiers(Frequency p_Frequency)
		{
			switch (p_Frequency)
			{
			case Frequency.PerFrame:
				return Singleton.perFrameModifiers;
			case Frequency.TenPerSec:
				return Singleton.tenModifiers;
			case Frequency.OncePerSec:
				return Singleton.oneModifiers;
			default:
				return null;
			}
		}

		public static void Register(Modifier p_Modifier)
		{
			List<Modifier> modifiers = GetModifiers(p_Modifier.Frequency);
			if (!modifiers.Contains(p_Modifier))
			{
				modifiers.Add(p_Modifier);
			}
		}

		public static void Deregister(Modifier p_Modifier)
		{
			List<Modifier> modifiers = GetModifiers(p_Modifier.Frequency);
			modifiers.Remove(p_Modifier);
		}

		private void Update()
		{
			for (int i = 0; i < perFrameModifiers.Count; i++)
			{
				perFrameModifiers[i].Perform();
			}
		}

		private IEnumerator TenTimesPerSecond()
		{
			while (true)
			{
				for (int i = 0; i < tenModifiers.Count; i++)
				{
					tenModifiers[i].Perform();
				}
				yield return tenthOfASecond;
			}
		}

		private IEnumerator OncePerSecond()
		{
			while (true)
			{
				for (int i = 0; i < oneModifiers.Count; i++)
				{
					oneModifiers[i].Perform();
				}
				yield return second;
			}
		}
	}
}
