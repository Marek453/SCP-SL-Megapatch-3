using UnityEngine;

namespace LlockhamIndustries.Decals
{
	internal struct CollisionData
	{
		public Vector3 position;

		public Quaternion rotation;

		public Transform surface;

		public int layer;

		public CollisionData(Vector3 Position, Quaternion Rotation, Transform Surface, int Layer)
		{
			position = Position;
			rotation = Rotation;
			surface = Surface;
			layer = Layer;
		}
	}
}
