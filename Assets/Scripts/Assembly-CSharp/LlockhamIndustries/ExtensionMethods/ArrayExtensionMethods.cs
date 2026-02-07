using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class ArrayExtensionMethods
	{
		public static T[] Insert<T>(this T[] Array, T Item, int Index)
		{
			if (Item != null)
			{
				if (Array != null)
				{
					Index = Mathf.Clamp(Index, 0, Array.Length);
					T[] array = Array;
					Array = new T[Array.Length + 1];
					int num = 0;
					for (int i = 0; i < Array.Length; i++)
					{
						if (i != Index)
						{
							Array[i] = array[num];
							num++;
						}
						else
						{
							Array[i] = Item;
						}
					}
				}
				else
				{
					Array = new T[1] { Item };
				}
			}
			return Array;
		}

		public static T[] Add<T>(this T[] Array, T Item)
		{
			if (Item != null)
			{
				Array = ((Array == null) ? new T[1] { Item } : Array.Insert(Item, Array.Length));
			}
			return Array;
		}

		public static bool Contains<T>(this T[] Array, T Item)
		{
			if (Array != null && Item != null && Array.Length > 0)
			{
				for (int i = 0; i < Array.Length; i++)
				{
					if (Array[i].Equals(Item))
					{
						return true;
					}
				}
			}
			return false;
		}

		public static T[] Remove<T>(this T[] Array, T Item)
		{
			if (Array != null && Item != null && Array.Length > 0)
			{
				T[] array = new T[Array.Length - 1];
				bool flag = false;
				for (int i = 0; i < Array.Length; i++)
				{
					if (!flag && Array[i] != null && Array[i].Equals(Item))
					{
						flag = true;
					}
					else
					{
						array[(!flag) ? i : (i - 1)] = Array[i];
					}
				}
				return (!flag) ? Array : array;
			}
			return Array;
		}

		public static T[] RemoveAt<T>(this T[] Array, int Index)
		{
			if (Array != null && Array.Length > 0)
			{
				if (Index >= 0 && Index < Array.Length)
				{
					T[] array = Array;
					Array = new T[Array.Length - 1];
					int num = 0;
					for (int i = 0; i < array.Length; i++)
					{
						if (i != Index)
						{
							Array[num] = array[i];
							num++;
						}
					}
				}
				else
				{
					Debug.LogError("Index out of Bounds");
				}
			}
			return Array;
		}

		public static T[] Resize<T>(this T[] Array, int Size)
		{
			if (Array != null)
			{
				T[] array = Array;
				Array = new T[Size];
				for (int i = 0; i < Mathf.Min(Array.Length, array.Length); i++)
				{
					Array[i] = array[i];
				}
			}
			return Array;
		}
	}
}
