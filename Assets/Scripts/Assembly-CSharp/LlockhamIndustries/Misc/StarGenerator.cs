using UnityEngine;

namespace LlockhamIndustries.Misc
{
	[RequireComponent(typeof(MeshFilter))]
	public class StarGenerator : MonoBehaviour
	{
		public int seed = 64;

		public int count = 1000;

		public float radius = 500f;

		public int octaves = 3;

		public float size = 1f;

		private MeshFilter meshfilter;

		public void GenerateQuadStars()
		{
			Random.InitState(seed);
			meshfilter = GetComponent<MeshFilter>();
			if (meshfilter.sharedMesh == null)
			{
				Mesh mesh = new Mesh();
				mesh.name = "Stars";
				meshfilter.sharedMesh = mesh;
			}
			Mesh sharedMesh = meshfilter.sharedMesh;
			sharedMesh.Clear();
			Vector3[] array = new Vector3[count * 4];
			Vector3[] normals = new Vector3[count * 4];
			Vector2[] array2 = new Vector2[count * 4];
			int[] array3 = new int[count * 6];
			for (int i = 0; i < count; i++)
			{
				GenerateQuadStar(array, normals, array2, array3, i);
			}
			sharedMesh.vertices = array;
			sharedMesh.normals = normals;
			sharedMesh.uv = array2;
			sharedMesh.triangles = array3;
			sharedMesh.RecalculateBounds();
		}

		private void GenerateQuadStar(Vector3[] Verts, Vector3[] Normals, Vector2[] UVs, int[] Tris, int Index)
		{
			Vector3 vector = new Vector3(Random.Range(-1f, 1f), Random.Range(0f, 1f), Random.Range(-1f, 1f));
			vector.Normalize();
			if (vector != Vector3.zero)
			{
				float num = radius * 0.0006f;
				float num2 = size;
				for (int i = 0; i < octaves; i++)
				{
					num2 *= Random.Range(0.1f, 1f);
				}
				float num3 = Mathf.Max(num2, num);
				float x = Mathf.Clamp01(num2 / num);
				Vector3 vector2 = base.transform.position + vector * radius;
				Quaternion quaternion = Quaternion.LookRotation(-vector);
				Verts[Index * 4] = vector2 + quaternion * new Vector3(-1f, -1f, 0f) * num3;
				UVs[Index * 4] = new Vector2(x, 0f);
				Normals[Index * 4] = -vector;
				Verts[Index * 4 + 1] = vector2 + quaternion * new Vector3(-1f, 1f, 0f) * num3;
				UVs[Index * 4 + 1] = new Vector2(x, 0f);
				Normals[Index * 4 + 1] = -vector;
				Verts[Index * 4 + 2] = vector2 + quaternion * new Vector3(1f, 1f, 0f) * num3;
				UVs[Index * 4 + 2] = new Vector2(x, 0f);
				Normals[Index * 4 + 2] = -vector;
				Verts[Index * 4 + 3] = vector2 + quaternion * new Vector3(1f, -1f, 0f) * num3;
				UVs[Index * 4 + 3] = new Vector2(x, 0f);
				Normals[Index * 4 + 3] = -vector;
				Tris[Index * 6] = Index * 4;
				Tris[Index * 6 + 1] = Index * 4 + 2;
				Tris[Index * 6 + 2] = Index * 4 + 1;
				Tris[Index * 6 + 3] = Index * 4;
				Tris[Index * 6 + 4] = Index * 4 + 3;
				Tris[Index * 6 + 5] = Index * 4 + 2;
			}
		}
	}
}
