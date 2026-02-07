using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[ExecuteInEditMode]
	public class NineSprite : MonoBehaviour
	{
		[SerializeField]
		private ProjectionRenderer sprite;

		[SerializeField]
		private float borderPixelSize = 0.4f;

		[SerializeField]
		private float borderWorldSize = 0.2f;

		[SerializeField]
		private ProjectionRenderer[] spritePieces;

		public ProjectionRenderer Sprite
		{
			get
			{
				return sprite;
			}
			set
			{
				if (sprite != value)
				{
					sprite = value;
					UpdateProperties();
				}
			}
		}

		public float BorderPixelSize
		{
			get
			{
				return borderPixelSize;
			}
			set
			{
				if (borderPixelSize != value)
				{
					borderPixelSize = value;
					UpdateProperties();
				}
			}
		}

		public float BorderWorldSize
		{
			get
			{
				return borderWorldSize;
			}
			set
			{
				if (borderWorldSize != value)
				{
					borderWorldSize = value;
					UpdateTransforms();
				}
			}
		}

		private void OnDestroy()
		{
			ClearSprite();
		}

		public void UpdateProperties()
		{
			for (int i = 0; i < spritePieces.Length; i++)
			{
				spritePieces[i].Projection = sprite.Projection;
				spritePieces[i].Tiling = Tiling(i);
				spritePieces[i].Offset = Offset(i);
				spritePieces[i].MaskMethod = sprite.MaskMethod;
				spritePieces[i].MaskLayer1 = sprite.MaskLayer1;
				spritePieces[i].MaskLayer2 = sprite.MaskLayer2;
				spritePieces[i].MaskLayer3 = sprite.MaskLayer3;
				spritePieces[i].MaskLayer4 = sprite.MaskLayer4;
				spritePieces[i].Properties = sprite.Properties;
				spritePieces[i].UpdateProperties();
			}
		}

		public void UpdateTransforms()
		{
			for (int i = 0; i < spritePieces.Length; i++)
			{
				spritePieces[i].transform.localPosition = LocalPosition(i);
				spritePieces[i].transform.localRotation = Quaternion.identity;
				spritePieces[i].transform.localScale = LocalScale(i);
			}
		}

		private Vector2 Tiling(int Index)
		{
			switch (Index)
			{
			case 0:
				return new Vector2(borderPixelSize, borderPixelSize);
			case 1:
				return new Vector2(1f - 2f * borderPixelSize, borderPixelSize);
			case 2:
				return new Vector2(borderPixelSize, borderPixelSize);
			case 3:
				return new Vector2(borderPixelSize, 1f - 2f * borderPixelSize);
			case 4:
				return new Vector2(1f - 2f * borderPixelSize, 1f - 2f * borderPixelSize);
			case 5:
				return new Vector2(borderPixelSize, 1f - 2f * borderPixelSize);
			case 6:
				return new Vector2(borderPixelSize, borderPixelSize);
			case 7:
				return new Vector2(1f - 2f * borderPixelSize, borderPixelSize);
			case 8:
				return new Vector2(borderPixelSize, borderPixelSize);
			default:
				return Vector2.zero;
			}
		}

		private Vector2 Offset(int Index)
		{
			switch (Index)
			{
			case 0:
				return new Vector2(0f, 1f - borderPixelSize);
			case 1:
				return new Vector2(borderPixelSize, 1f - borderPixelSize);
			case 2:
				return new Vector2(1f - borderPixelSize, 1f - borderPixelSize);
			case 3:
				return new Vector2(0f, borderPixelSize);
			case 4:
				return new Vector2(borderPixelSize, borderPixelSize);
			case 5:
				return new Vector2(1f - borderPixelSize, borderPixelSize);
			case 6:
				return new Vector2(0f, 0f);
			case 7:
				return new Vector2(borderPixelSize, 0f);
			case 8:
				return new Vector2(1f - borderPixelSize, 0f);
			default:
				return Vector2.zero;
			}
		}

		private Vector3 LocalPosition(int Index)
		{
			float num = borderWorldSize / base.transform.localScale.x / 2f;
			float num2 = borderWorldSize / base.transform.localScale.y / 2f;
			switch (Index)
			{
			case 0:
				return new Vector3(-0.5f + num, 0.5f - num2, 0f);
			case 1:
				return new Vector3(0f, 0.5f - num2, 0f);
			case 2:
				return new Vector3(0.5f - num, 0.5f - num2, 0f);
			case 3:
				return new Vector3(-0.5f + num, 0f, 0f);
			case 4:
				return Vector3.zero;
			case 5:
				return new Vector3(0.5f - num, 0f, 0f);
			case 6:
				return new Vector3(-0.5f + num, -0.5f + num2, 0f);
			case 7:
				return new Vector3(0f, -0.5f + num2, 0f);
			case 8:
				return new Vector3(0.5f - num, -0.5f + num2, 0f);
			default:
				return Vector3.zero;
			}
		}

		private Vector3 LocalScale(int Index)
		{
			float num = borderWorldSize / base.transform.localScale.x;
			float num2 = borderWorldSize / base.transform.localScale.y;
			switch (Index)
			{
			case 0:
				return new Vector3(num, num2, 1f);
			case 1:
				return new Vector3(1f - 2f * num, num2, 1f);
			case 2:
				return new Vector3(num, num2, 1f);
			case 3:
				return new Vector3(num, 1f - 2f * num2, 1f);
			case 4:
				return new Vector3(1f - 2f * num, 1f - 2f * num2, 1f);
			case 5:
				return new Vector3(num, 1f - 2f * num2, 1f);
			case 6:
				return new Vector3(num, num2, 1f);
			case 7:
				return new Vector3(1f - 2f * num, num2, 1f);
			case 8:
				return new Vector3(num, num2, 1f);
			default:
				return Vector3.one;
			}
		}

		private void Generate()
		{
			spritePieces = new ProjectionRenderer[9];
			spritePieces[0] = GenerateRenderer("TopLeft");
			spritePieces[1] = GenerateRenderer("TopMiddle");
			spritePieces[2] = GenerateRenderer("TopRight");
			spritePieces[3] = GenerateRenderer("MiddleLeft");
			spritePieces[4] = GenerateRenderer("MiddleMiddle");
			spritePieces[5] = GenerateRenderer("MiddleRight");
			spritePieces[6] = GenerateRenderer("BottomLeft");
			spritePieces[7] = GenerateRenderer("BottomMiddle");
			spritePieces[8] = GenerateRenderer("BottomRight");
		}

		private ProjectionRenderer GenerateRenderer(string Name)
		{
			GameObject gameObject = new GameObject(Name);
			gameObject.transform.parent = base.transform;
			gameObject.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			gameObject.SetActive(false);
			ProjectionRenderer result = gameObject.AddComponent<ProjectionRenderer>();
			gameObject.AddComponent<NineSpritePiece>();
			return result;
		}

		public void UpdateNineSprite()
		{
			if (sprite != null && sprite.Projection != null)
			{
				if (spritePieces == null || spritePieces.Length != 9)
				{
					Generate();
					UpdateProperties();
					UpdateTransforms();
					for (int i = 0; i < spritePieces.Length; i++)
					{
						spritePieces[i].gameObject.SetActive(true);
					}
				}
				else
				{
					UpdateProperties();
					UpdateTransforms();
				}
			}
			else
			{
				ClearSprite();
			}
		}

		private void ClearSprite()
		{
			if (spritePieces != null)
			{
				for (int i = 0; i < spritePieces.Length; i++)
				{
					Object.DestroyImmediate(spritePieces[i]);
				}
				spritePieces = null;
			}
		}
	}
}
