using UnityEngine;
using UnityEngine.UI;

public class VSyncFPSlimit : MonoBehaviour
{
	public void Check()
	{
		if (base.gameObject.GetComponent<Slider>().value == 0f)
		{
			int @int = PlayerPrefs.GetInt("MaxFramerate", 969);
			if (@int == 969)
			{
				Application.targetFrameRate = -1;
			}
			else
			{
				Application.targetFrameRate = @int;
			}
		}
	}
}
