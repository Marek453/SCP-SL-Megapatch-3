using System;
using System.Reflection;
using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class GameObjectExtensionMethods
	{
		public static T AddComponent<T>(this GameObject GameObject, T Source) where T : MonoBehaviour
		{
			return GameObject.AddComponent<T>().GetCopyOf(Source);
		}

		public static MonoBehaviour AddComponent(this GameObject GameObject, MonoBehaviour Source)
		{
			Type type = Source.GetType();
			if (type.IsSubclassOf(typeof(MonoBehaviour)))
			{
				return ((MonoBehaviour)GameObject.AddComponent(type)).GetCopyOf(Source);
			}
			return null;
		}

		public static T GetCopyOf<T>(this MonoBehaviour Monobehaviour, T Source) where T : MonoBehaviour
		{
			Type type = Monobehaviour.GetType();
			if (type != Source.GetType())
			{
				return (T)null;
			}
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			while (type != typeof(MonoBehaviour))
			{
				FieldInfo[] fields = type.GetFields(bindingAttr);
				FieldInfo[] array = fields;
				foreach (FieldInfo fieldInfo in array)
				{
					fieldInfo.SetValue(Monobehaviour, fieldInfo.GetValue(Source));
				}
				type = type.BaseType;
			}
			return Monobehaviour as T;
		}
	}
}
