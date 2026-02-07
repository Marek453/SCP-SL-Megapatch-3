using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class Vector3ExtensionMethods
	{
		public static float DistanceXZ(this Vector3 PositionOne, Vector3 PositionTwo)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			zero.x = PositionOne.x;
			zero.y = PositionOne.z;
			zero2.x = PositionTwo.x;
			zero2.y = PositionTwo.z;
			return Vector2.Distance(zero, zero2);
		}

		public static Vector2 DirectionXZ(this Vector3 PositionOne, Vector3 PositionTwo)
		{
			Vector2 zero = Vector2.zero;
			Vector2 zero2 = Vector2.zero;
			zero.x = PositionOne.x;
			zero.y = PositionOne.z;
			zero2.x = PositionTwo.x;
			zero2.y = PositionTwo.z;
			return (zero2 - zero).normalized;
		}

		public static Vector2 xz(this Vector3 Vector3)
		{
			Vector2 zero = Vector2.zero;
			zero.x = Vector3.x;
			zero.y = Vector3.z;
			return zero;
		}

		public static Vector2 xy(this Vector3 Vector3)
		{
			Vector2 zero = Vector2.zero;
			zero.x = Vector3.x;
			zero.y = Vector3.y;
			return zero;
		}

		public static Vector2 yz(this Vector3 Vector3)
		{
			Vector2 zero = Vector2.zero;
			zero.x = Vector3.y;
			zero.y = Vector3.z;
			return zero;
		}
	}
}
