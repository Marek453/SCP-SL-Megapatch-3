using UnityEngine;

namespace LlockhamIndustries.Decals
{
	[RequireComponent(typeof(ProjectionRenderer))]
	public class SheetAnimator : MonoBehaviour
	{
		[Tooltip("The number of collumns in the sprite sheet being sampled.")]
		[Header("Basics")]
		public int collumns = 1;

		[Tooltip("The number of rows in the sprite sheet being sampled.")]
		public int rows = 1;

		[Tooltip("The playback speed, in frames per second.")]
		public float speed = 30f;

		[Tooltip("Skip the first x frames of the animation.")]
		[Header("Advanced")]
		public int skipFirst;

		[Tooltip("Skip the last x frames of the animation.")]
		public int skipLast;

		[Tooltip("Sample frames from the bottom instead of the top.")]
		public bool invertY;

		[Tooltip("Destroy the projection when the animator has finished its first loop.")]
		public bool destroyOnComplete;

		private ProjectionRenderer projection;

		private float time;

		private bool paused;

		private void Awake()
		{
			projection = GetComponent<ProjectionRenderer>();
		}

		private void Update()
		{
			int num = collumns * rows - (skipFirst + skipLast);
			if (!paused)
			{
				time += Time.deltaTime * speed;
			}
			if (time > (float)num)
			{
				if (destroyOnComplete)
				{
					projection.Destroy();
					return;
				}
				time -= num;
			}
			int num2 = skipFirst + Mathf.FloorToInt(time);
			Vector2 vector = new Vector2(1f / (float)collumns, 1f / (float)rows);
			int num3 = num2 / collumns;
			int num4 = num2 % collumns;
			float x = vector.x * (float)num4;
			float num5 = vector.y * (float)num3;
			if (!invertY)
			{
				num5 = 1f - vector.y - num5;
			}
			projection.Tiling = new Vector2(vector.x, vector.y);
			projection.Offset = new Vector2(x, num5);
			projection.UpdateProperties();
		}

		public void Play()
		{
			paused = false;
		}

		public void Pause()
		{
			paused = true;
		}

		public void Stop()
		{
			paused = true;
			time = 0f;
		}
	}
}
