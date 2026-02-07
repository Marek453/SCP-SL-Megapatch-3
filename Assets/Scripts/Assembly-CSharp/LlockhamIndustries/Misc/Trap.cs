using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public abstract class Trap : MonoBehaviour
	{
		[Header("General")]
		public bool autoRearm;

		private TrapState state;

		public TrapState State
		{
			get
			{
				return state;
			}
		}

		public void Trigger()
		{
			if (state == TrapState.Rearming)
			{
				StopCoroutine("OnRearm");
			}
			if (state != TrapState.Triggering)
			{
				state = TrapState.Triggering;
				StartCoroutine("OnTrigger");
			}
		}

		public void Rearm()
		{
			if (state == TrapState.Triggering)
			{
				StopCoroutine("OnTrigger");
			}
			if (state != TrapState.Rearming)
			{
				state = TrapState.Rearming;
				StartCoroutine("OnRearm");
			}
		}

		protected abstract IEnumerator OnTrigger();

		protected abstract IEnumerator OnRearm();

		protected void TriggerComplete()
		{
			state = TrapState.Idle;
			if (autoRearm)
			{
				Rearm();
			}
		}

		protected void RearmComplete()
		{
			state = TrapState.Idle;
		}
	}
}
