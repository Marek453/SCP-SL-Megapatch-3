using System;
using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[Serializable]
	public struct ProjectionProperty
	{
		public string name;

		public int nameID;

		public PropertyType type;

		public Color color;

		public float value;

		public bool enabled;

		public ProjectionProperty(string Name, int ID, Color Color)
		{
			name = Name;
			nameID = ID;
			type = PropertyType.Color;
			color = Color;
			value = 0f;
			enabled = false;
		}

		public ProjectionProperty(string Name, int ID, float Value)
		{
			name = Name;
			nameID = ID;
			type = PropertyType.Float;
			color = Color.white;
			value = Value;
			enabled = false;
		}

		public ProjectionProperty(string Name, int ID, Color Color, float Value)
		{
			name = Name;
			nameID = ID;
			type = PropertyType.Combo;
			color = Color;
			value = Value;
			enabled = false;
		}
	}
}
