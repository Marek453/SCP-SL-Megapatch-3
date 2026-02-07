using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class DestructableManager : MonoBehaviour
	{
		private static DestructableManager singleton;

		public List<Destructable> Destructables = new List<Destructable>();

		private static DestructableManager Singleton
		{
			get
			{
				if (singleton == null)
				{
					GameObject gameObject = new GameObject("Destructable Manager");
					Object.DontDestroyOnLoad(gameObject);
					gameObject.AddComponent<DestructableManager>();
				}
				return singleton;
			}
		}

		private void Awake()
		{
			if (singleton == null)
			{
				singleton = this;
			}
			else if (singleton != this)
			{
				Object.Destroy(base.gameObject);
			}
		}

		public static void Register(Destructable Destructable)
		{
			Singleton.Destructables.Add(Destructable);
		}

		public static void Deregister(Destructable Destructable)
		{
			singleton.Destructables.Remove(Destructable);
		}

		public static void Restore()
		{
			if (!(singleton != null))
			{
				return;
			}
			foreach (Destructable destructable in singleton.Destructables)
			{
				destructable.Restore();
			}
		}
	}
}
