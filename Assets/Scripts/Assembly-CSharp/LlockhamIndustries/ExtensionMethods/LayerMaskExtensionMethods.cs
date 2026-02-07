using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.ExtensionMethods
{
	public static class LayerMaskExtensionMethods
	{
		public static bool Contains(this LayerMask Mask, int Layer)
		{
			return (int)Mask == ((int)Mask | (1 << Layer));
		}

		public static LayerMask Remove(this LayerMask Mask, int Layer)
		{
			return (int)Mask & ~(1 << Layer);
		}

		public static LayerMask Remove(this LayerMask Mask, LayerMask Layers)
		{
			return (int)Mask & ~(int)Layers;
		}

		public static LayerMask Add(this LayerMask Mask, int Layer)
		{
			Mask = (int)Mask | (1 << Layer);
			return Mask;
		}

		public static LayerMask Add(this LayerMask Mask, LayerMask Layers)
		{
			Mask = (int)Mask | (int)Layers;
			return Mask;
		}

		public static int[] ContainedLayers(this LayerMask Mask)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < 32; i++)
			{
				if (Mask.Contains(i))
				{
					list.Add(i);
				}
			}
			return list.ToArray();
		}

		public static string[] ContainedLayerNames(this LayerMask Mask)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < 32; i++)
			{
				if (Mask.Contains(i))
				{
					list.Add(LayerMask.LayerToName(i));
				}
			}
			return list.ToArray();
		}

		public static void LogLayers(this LayerMask Mask)
		{
			int[] array = Mask.ContainedLayers();
			foreach (int num in array)
			{
				Debug.Log(num);
			}
		}

		public static void LogLayerNames(this LayerMask Mask)
		{
			string[] array = Mask.ContainedLayerNames();
			foreach (string message in array)
			{
				Debug.Log(message);
			}
		}
	}
}
