using UnityEngine;

namespace LlockhamIndustries.Decals
{
	public abstract class Modifier : MonoBehaviour
	{
		[SerializeField]
		private Frequency frequency;

		public Frequency Frequency
		{
			get
			{
				return frequency;
			}
			set
			{
				if (value != frequency)
				{
					Deregister();
					frequency = value;
					Register();
				}
			}
		}

		protected float UpdateRate
		{
			get
			{
				switch (frequency)
				{
				case Frequency.TenPerSec:
					return 0.1f;
				case Frequency.OncePerSec:
					return 1f;
				default:
					return Time.deltaTime;
				}
			}
		}

		protected virtual void OnEnable()
		{
			Begin();
			Register();
		}

		protected virtual void OnDisable()
		{
			Deregister();
		}

		private void Register()
		{
			if (Application.isPlaying && base.gameObject.activeInHierarchy)
			{
				ModifierManager.Register(this);
			}
		}

		private void Deregister()
		{
			if (Application.isPlaying)
			{
				ModifierManager.Deregister(this);
			}
		}

		protected abstract void Begin();

		public abstract void Perform();
	}
}
