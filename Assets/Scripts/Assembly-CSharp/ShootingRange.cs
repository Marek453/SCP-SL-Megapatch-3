using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;

public class ShootingRange : MonoBehaviour
{
	public bool isOnRange;

	private string curDamage;

	private float remainingTime;

	private Text txt;

	public void PrintDamage(float dmg)
	{
		if (isOnRange)
		{
			txt = GameObject.Find("ShootingText").GetComponent<Text>();
			curDamage = "-" + Mathf.Round(dmg * 100f) / 100f + " HP";
			remainingTime = 3f;
		}
	}

	private void Update()
	{
		if (isOnRange)
		{
			if (remainingTime > 0f)
			{
				txt.text = curDamage;
				remainingTime -= Time.deltaTime;
			}
			else if (txt != null)
			{
				txt.text = string.Empty;
			}
			Camera component = GetComponentInChildren<GlobalFog>().GetComponent<Camera>();
			bool flag = (base.transform.position.x > 1500f) & (base.transform.position.y > -10f);
			GetComponentInChildren<GlobalFog>().startDistance = (flag ? 200 : 0);
			GetComponent<FirstPersonController>().rangeSpeed = flag;
			component.farClipPlane = ((!flag) ? 47 : 1000);
		}
	}
}
