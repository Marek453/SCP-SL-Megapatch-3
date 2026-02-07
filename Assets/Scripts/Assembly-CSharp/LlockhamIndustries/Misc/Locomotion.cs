using System;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Rigidbody))]
	public class Locomotion : MonoBehaviour
	{
		public float acceleration = 0.2f;

		public float rotationSpeed = 0.2f;

		private Animator anim;

		private float rotation;

		private float rotationVelocity;

		private Vector3 rotationDirection;

		private Vector2 locomotion;

		private Vector3 movement;

		private float previousMoveSpeed;

		private float moveSpeedVelocity;

		private Vector3 previousMovement;

		private Vector3 moveDirectionVelocity;

		private float locomotionStrength;

		private float previousLocomotionStrength;

		private float locomotionStrengthVelocity;

		public Vector3 Movement
		{
			set
			{
				movement = value;
			}
		}

		public Vector3 Direction
		{
			set
			{
				rotationDirection = value;
			}
		}

		protected void Awake()
		{
			anim = GetComponent<Animator>();
			rotation = Quaternion.LookRotation(base.transform.forward).eulerAngles.y;
		}

		protected void FixedUpdate()
		{
			UpdateRotation();
			UpdateLocomotionDirection();
			UpdateLocomotionStrength();
		}

		private void UpdateRotation()
		{
			float num = rotation;
			if (rotationDirection != Vector3.zero)
			{
				num = Quaternion.LookRotation(rotationDirection).eulerAngles.y;
				float num2 = 1f + Mathf.Clamp01(locomotion.magnitude - 1f);
				float num3 = 1f / rotationSpeed * num2;
				rotation = Mathf.SmoothDampAngle(rotation, num, ref rotationVelocity, num3 * 0.02f);
			}
			else
			{
				rotationVelocity = 0f;
			}
			base.transform.rotation = Quaternion.Euler(new Vector3(0f, rotation, 0f));
		}

		private void UpdateLocomotionDirection()
		{
			Vector3 vector = (previousMovement = Vector3.SmoothDamp(previousMovement, movement, ref moveDirectionVelocity, 1f / acceleration * 0.02f));
			float num = Vector3.Angle(base.transform.forward, vector.normalized);
			if (Mathf.Sign(Vector3.Dot(base.transform.right, vector.normalized)) < 0f)
			{
				num = 360f - num;
			}
			num *= (float)Math.PI / 180f;
			locomotion.x = Mathf.Sin(num);
			locomotion.y = Mathf.Cos(num);
			locomotion *= vector.magnitude;
			if (locomotion.x > 0.5f)
			{
				float num2 = 0.5f / locomotion.x;
				locomotion *= num2;
			}
			anim.SetFloat("X", locomotion.x);
			anim.SetFloat("Y", locomotion.y);
		}

		private void UpdateLocomotionStrength()
		{
			float a = Mathf.Clamp01(movement.magnitude * 2f);
			float b = Mathf.Clamp(Mathf.Abs(rotationVelocity / 400f), 0f, 1f);
			locomotionStrength = Mathf.Max(a, b);
			locomotionStrength = Mathf.SmoothDamp(previousLocomotionStrength, locomotionStrength, ref locomotionStrengthVelocity, 1f / acceleration * 0.02f);
			previousLocomotionStrength = locomotionStrength;
			anim.SetFloat("Locomote", locomotionStrength);
		}
	}
}
