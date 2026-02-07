using System;
using UnityEngine;

namespace LlockhamIndustries.VR
{
	public abstract class VRController : MonoBehaviour
	{
		private string grip = string.Empty;

		private string trigger = string.Empty;

		private string trackpad = string.Empty;

		private string trackpadHorizontal = string.Empty;

		private string trackpadVertical = string.Empty;

		public Hand hand;

		protected bool Grip
		{
			get
			{
				if (grip == string.Empty)
				{
					return false;
				}
				return Input.GetAxis(grip) != 0f;
			}
		}

		protected bool Trigger
		{
			get
			{
				if (trigger == string.Empty)
				{
					return false;
				}
				return Input.GetAxis(trigger) != 0f;
			}
		}

		protected bool TrackPad
		{
			get
			{
				if (trackpad == string.Empty)
				{
					return false;
				}
				return Input.GetAxis(trackpad) != 0f;
			}
		}

		protected Vector2 TrackPadPosition
		{
			get
			{
				if (trackpadHorizontal == string.Empty)
				{
					return Vector2.zero;
				}
				return new Vector2(Input.GetAxis(trackpadHorizontal), Input.GetAxis(trackpadVertical));
			}
		}

		protected void OnEnable()
		{
		}

		private void Start()
		{
			TestAxis();
		}

		private void TestAxis()
		{
			switch (hand)
			{
			case Hand.Left:
				try
				{
					Input.GetAxis("axis 11");
					grip = "axis 11";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 14");
					trigger = "axis 14";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 16");
					trigger = "axis 16";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 1");
					trackpadHorizontal = "axis 1";
					Input.GetAxis("axis 2");
					trackpadVertical = "axis 2";
					break;
				}
				catch (ArgumentException)
				{
					break;
				}
			case Hand.Right:
				try
				{
					Input.GetAxis("axis 12");
					grip = "axis 12";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 15");
					trigger = "axis 15";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 16");
					trigger = "axis 17";
				}
				catch (ArgumentException)
				{
				}
				try
				{
					Input.GetAxis("axis 4");
					trackpadHorizontal = "axis 4";
					Input.GetAxis("axis 5");
					trackpadVertical = "axis 5";
					break;
				}
				catch (ArgumentException)
				{
					break;
				}
			}
		}
	}
}
