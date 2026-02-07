using UnityEngine;
using UnityEngine.Rendering;

namespace LlockhamIndustries.VR
{
	public class VRPlayspace : MonoBehaviour
	{
		public LayerMask obstructions;

		public Vector3 baseDimensions = new Vector3(2f, 2f, 2f);

		public Vector3 nodeDimensions = new Vector3(2.4f, 2.4f, 2.4f);

		public float boundsThickness = 0.1f;

		public Material boundsMaterial;

		public Vector3 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		private void Start()
		{
			GenerateBounds();
		}

		public Vector3 TrialPosition(Vector3 TargetPosition)
		{
			Vector3 normalized = (TargetPosition - base.transform.position).normalized;
			float num = Vector3.Distance(TargetPosition, base.transform.position);
			Vector3 center = base.transform.position + new Vector3(0f, baseDimensions.y / 2f, 0f);
			RaycastHit hitInfo;
			if (Physics.BoxCast(center, baseDimensions * 0.4f, normalized, out hitInfo, base.transform.rotation, num, obstructions))
			{
				num = hitInfo.distance;
			}
			while (num > 0f)
			{
				Vector3 vector = base.transform.position + normalized * num;
				if (TrialGrounded(vector))
				{
					return vector;
				}
				num -= 0.01f;
			}
			return base.transform.position;
		}

		private bool TrialGrounded(Vector3 Point)
		{
			Vector3 origin = Point + base.transform.forward * baseDimensions.z / 2f + -base.transform.right * baseDimensions.x / 2f + Vector3.up * baseDimensions.y;
			if (!Physics.Raycast(origin, Vector3.down, baseDimensions.y + 0.2f, obstructions))
			{
				return false;
			}
			Vector3 origin2 = Point + base.transform.forward * baseDimensions.z / 2f + base.transform.right * baseDimensions.x / 2f + Vector3.up * baseDimensions.y;
			if (!Physics.Raycast(origin2, Vector3.down, baseDimensions.y + 0.2f, obstructions))
			{
				return false;
			}
			Vector3 origin3 = Point + -base.transform.forward * baseDimensions.z / 2f + -base.transform.right * baseDimensions.x / 2f + Vector3.up * baseDimensions.y;
			if (!Physics.Raycast(origin3, Vector3.down, baseDimensions.y + 0.2f, obstructions))
			{
				return false;
			}
			Vector3 origin4 = Point + -base.transform.forward * baseDimensions.z / 2f + base.transform.right * baseDimensions.x / 2f + Vector3.up * baseDimensions.y;
			if (!Physics.Raycast(origin4, Vector3.down, baseDimensions.y + 0.2f, obstructions))
			{
				return false;
			}
			return true;
		}

		public Vector3 ClampNode(Vector3 Point)
		{
			Point = base.transform.InverseTransformPoint(Point);
			Point.y = Mathf.Clamp(Point.y, 0f, nodeDimensions.y);
			Point.x = Mathf.Clamp(Point.x, (0f - nodeDimensions.x) / 2f, nodeDimensions.x / 2f);
			Point.z = Mathf.Clamp(Point.z, (0f - nodeDimensions.z) / 2f, nodeDimensions.z / 2f);
			Point = base.transform.TransformPoint(Point);
			return Point;
		}

		private void GenerateBounds()
		{
			GameObject gameObject = new GameObject("Bounds");
			gameObject.transform.SetParent(base.transform);
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
			meshRenderer.sharedMaterial = boundsMaterial;
			meshFilter.sharedMesh = GenerateBoundsMesh();
		}

		private Mesh GenerateBoundsMesh()
		{
			float y = 0.01f;
			Mesh mesh = new Mesh();
			Vector3[] array = new Vector3[8];
			Vector3[] normals = new Vector3[8]
			{
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up,
				Vector3.up
			};
			Vector2[] array2 = new Vector2[8];
			int[] array3 = new int[24];
			array[0] = new Vector3(baseDimensions.x / 2f, y, baseDimensions.z / 2f);
			array[1] = new Vector3(baseDimensions.x / 2f + boundsThickness, y, baseDimensions.z / 2f + boundsThickness);
			array[2] = new Vector3((0f - baseDimensions.x) / 2f, y, baseDimensions.z / 2f);
			array[3] = new Vector3((0f - baseDimensions.x) / 2f - boundsThickness, y, baseDimensions.z / 2f + boundsThickness);
			array[4] = new Vector3((0f - baseDimensions.x) / 2f, y, (0f - baseDimensions.z) / 2f);
			array[5] = new Vector3((0f - baseDimensions.x) / 2f - boundsThickness, y, (0f - baseDimensions.z) / 2f - boundsThickness);
			array[6] = new Vector3(baseDimensions.x / 2f, y, (0f - baseDimensions.z) / 2f);
			array[7] = new Vector3(baseDimensions.x / 2f + boundsThickness, y, (0f - baseDimensions.z) / 2f - boundsThickness);
			array2[0] = new Vector2(0f, 0f);
			array2[2] = new Vector2(0f, 0f);
			array2[4] = new Vector2(0f, 0f);
			array2[6] = new Vector2(0f, 0f);
			array2[1] = new Vector2(1f, 0f);
			array2[3] = new Vector2(1f, 0f);
			array2[5] = new Vector2(1f, 0f);
			array2[7] = new Vector2(1f, 0f);
			array3[0] = 0;
			array3[1] = 2;
			array3[2] = 1;
			array3[3] = 1;
			array3[4] = 2;
			array3[5] = 3;
			array3[6] = 2;
			array3[7] = 4;
			array3[8] = 3;
			array3[9] = 3;
			array3[10] = 4;
			array3[11] = 5;
			array3[12] = 4;
			array3[13] = 6;
			array3[14] = 5;
			array3[15] = 5;
			array3[16] = 6;
			array3[17] = 7;
			array3[18] = 6;
			array3[19] = 0;
			array3[20] = 7;
			array3[21] = 7;
			array3[22] = 0;
			array3[23] = 1;
			mesh.vertices = array;
			mesh.normals = normals;
			mesh.uv = array2;
			mesh.triangles = array3;
			mesh.RecalculateBounds();
			return mesh;
		}
	}
}
