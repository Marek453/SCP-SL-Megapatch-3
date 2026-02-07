using System;
using System.Collections;
using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class TransformExtensionMethods
	{
		public static void LocalZero(this Transform Transform)
		{
			Transform.localPosition = Vector3.zero;
			Transform.localRotation = Quaternion.identity;
			Transform.localScale = Vector3.one;
		}

		public static void WorldZero(this Transform Transform)
		{
			Transform.position = Vector3.zero;
			Transform.rotation = Quaternion.identity;
			Transform.localScale = Vector3.one;
		}

		public static void Copy(this Transform Transform, Transform Target)
		{
			Transform.position = Target.position;
			Transform.rotation = Target.rotation;
			Transform.localScale = Target.localScale;
		}

		public static Transform FindRecursively(this Transform Transform, string Name)
		{
			if (Transform.name == Name)
			{
				return Transform;
			}
			IEnumerator enumerator = Transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					Transform transform2 = transform.FindRecursively(Name);
					if (transform2 != null)
					{
						return transform2;
					}
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			return null;
		}

		public static Transform FindParentRecursively(this Transform Transform, string Name)
		{
			if (Transform.name == Name)
			{
				return Transform;
			}
			Transform transform = Transform.parent.FindParentRecursively(Name);
			if (transform != null)
			{
				return transform;
			}
			return null;
		}
	}
}
