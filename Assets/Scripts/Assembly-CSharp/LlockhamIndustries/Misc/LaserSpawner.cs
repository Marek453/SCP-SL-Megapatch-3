using System.Collections.Generic;
using UnityEngine;

namespace LlockhamIndustries.Misc
{
	public class LaserSpawner : MonoBehaviour
	{
		public GameObject laser;

		[SerializeField]
		private int laserCount = 1;

		[SerializeField]
		private int spawnRate = 5;

		private List<GameObject> lasers;

		private List<GameObject> laserPool;

		public int LaserCount
		{
			get
			{
				return laserCount;
			}
			set
			{
				laserCount = value;
				LaserCountChange();
			}
		}

		private void Awake()
		{
			lasers = new List<GameObject>();
			laserPool = new List<GameObject>();
		}

		private void Start()
		{
			LaserCountChange();
		}

		public GameObject RequestLaser()
		{
			GameObject gameObject = null;
			if (laserPool.Count > 0)
			{
				gameObject = laserPool[0];
				gameObject.SetActive(true);
				laserPool.RemoveAt(0);
			}
			else
			{
				gameObject = Object.Instantiate(laser, Vector3.zero, Quaternion.LookRotation(-Vector3.up, -Vector3.right), base.transform);
			}
			lasers.Add(gameObject);
			gameObject.transform.position = base.transform.position;
			return gameObject;
		}

		public void ReturnLaser(GameObject laser)
		{
			lasers.Remove(laser);
			laser.SetActive(false);
			laser.transform.position = Vector3.zero;
			laserPool.Add(laser);
		}

		public void LaserCountChange()
		{
			if (Application.isPlaying)
			{
				int num = 0;
				while (lasers != null && lasers.Count < laserCount && num < spawnRate)
				{
					RequestLaser();
					num++;
				}
				while (lasers != null && lasers.Count > laserCount)
				{
					ReturnLaser(lasers[lasers.Count - 1]);
				}
			}
		}
	}
}
