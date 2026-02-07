using System;
using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class Vector2ExtensionMethods
	{
		public static bool DirectionBetween(this Vector2 Direction, Vector2 CounterClockwiseLimit, Vector2 ClockwiseLimit)
		{
			return Direction.GetClockwiseAngleTo(ClockwiseLimit) < Direction.GetClockwiseAngleTo(CounterClockwiseLimit);
		}

		public static float GetClockwiseAngleTo(this Vector2 from, Vector2 to)
		{
			float num = Vector2.Angle(from, to);
			if (Vector2.Dot(from.RotateClockwise(90f), to) < 0f)
			{
				num = 360f - num;
			}
			return num;
		}

		public static float GetCounterClockwiseAngleTo(this Vector2 from, Vector2 to)
		{
			float num = Vector2.Angle(from, to);
			if (Vector2.Dot(from.RotateClockwise(90f), to) > 0f)
			{
				num = 360f - num;
			}
			return num;
		}

		public static Vector2 RotateClockwise(this Vector2 v, float angle)
		{
			float num = Mathf.Cos((float)Math.PI / 180f * (0f - angle));
			float num2 = Mathf.Sin((float)Math.PI / 180f * (0f - angle));
			return new Vector2(num * v.x - num2 * v.y, num2 * v.x + num * v.y);
		}

		public static Vector2 RotateCounterClockwise(this Vector2 v, float angle)
		{
			float num = Mathf.Cos((float)Math.PI / 180f * angle);
			float num2 = Mathf.Sin((float)Math.PI / 180f * angle);
			return new Vector2(num * v.x - num2 * v.y, num2 * v.x + num * v.y);
		}

		public static Vector2 RotateTowards(this Vector2 v, Vector2 GoalDirection, float angle)
		{
			angle = Mathf.Min(angle, Vector2.Angle(v, GoalDirection));
			if (v.GetClockwiseAngleTo(GoalDirection) <= 180f)
			{
				return v.RotateClockwise(angle);
			}
			return v.RotateCounterClockwise(angle);
		}

		public static Vector2 LerpDirectionTowards(this Vector2 from, Vector2 to, float amount)
		{
			float num = Vector2.Angle(from, to);
			Vector2 lhs = from.RotateClockwise(90f);
			if (Vector2.Dot(lhs, to) > 0f)
			{
				num *= -1f;
			}
			return from.RotateClockwise(num * amount);
		}

		public static Vector2 LerpClockwiseTowards(this Vector2 from, Vector2 to, float amount)
		{
			return from.RotateClockwise(from.GetClockwiseAngleTo(to) * amount);
		}

		public static Vector2 LerpCounterClockwiseTowards(this Vector2 from, Vector2 to, float amount)
		{
			return from.RotateCounterClockwise(from.GetCounterClockwiseAngleTo(to) * amount);
		}

		public static Vector2 AngleToVector2(this float Angle)
		{
			Angle *= (float)Math.PI / 180f;
			return new Vector2(Mathf.Sin(Angle), Mathf.Cos(Angle));
		}

		public static Vector2 RotatePointAroundPivot(this Vector2 Point, Vector2 Pivot, Quaternion Rotation)
		{
			Vector3 vector = new Vector3(Point.x, 0f, Point.y);
			Vector3 vector2 = new Vector3(Pivot.x, 0f, Pivot.y);
			Vector3 vector3 = vector - vector2;
			vector3 = Rotation * vector3;
			return (vector3 + vector2).xz();
		}
	}
}
