using System.Collections.Generic;
using System.Collections;
using MEC;
using UnityEngine;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
	public GameObject popup;

	public Image black;

	public Text youare;

	public Text wmi;

	public Text wihtd;

	public void PlayAnimation(int classID)
	{
		StopAllCoroutines();
		StopCoroutine(_Animate(classID));
		StartCoroutine(_Animate(classID));
	}

	private IEnumerator _Animate(int classID)
	{
		CanvasRenderer c1 = youare.GetComponent<CanvasRenderer>();
		CanvasRenderer c2 = wmi.GetComponent<CanvasRenderer>();
		CanvasRenderer c3 = wihtd.GetComponent<CanvasRenderer>();
		c1.SetAlpha(1);
			c2.SetAlpha(1);
			c3.SetAlpha(1);
		black.gameObject.SetActive(true);
		GameObject host = GameObject.Find("Host");
		CharacterClassManager ccm = host.GetComponent<CharacterClassManager>();
		Class klasa = ccm.klasy[classID];
		youare.text = ((!TutorialManager.status) ? TranslationReader.Get("Facility", 31) : string.Empty);
		wmi.text = klasa.fullName;
		wmi.GetComponent<Outline>().effectColor = ((!(klasa.classColor.r < 0.24f) || !(klasa.classColor.g < 0.24f) || !(klasa.classColor.b < 0.24f)) ? Color.black : new Color(0.35f, 0.35f, 0.35f));
		wmi.color = klasa.classColor;
		wihtd.text = klasa.description;
		while (popup.transform.localScale.x < 1f)
		{
			popup.transform.localScale += Vector3.one * 0.02f * 2f;
			if (popup.transform.localScale.x > 1f)
			{
				popup.transform.localScale = Vector3.one;
			}
			yield return 0f;
		}
		while (black.color.a > 0f)
		{
			black.color = new Color(0f, 0f, 0f, black.color.a - 0.02f);
			yield return 0f;
		}
		yield return new WaitForSeconds(6f);
		HintManager.singleton.AddHint(0);
		while (c1.GetAlpha() > 0f)
		{
			c1.SetAlpha(c1.GetAlpha() - 0.0039999997f);
			c2.SetAlpha(c2.GetAlpha() - 0.0039999997f);
			c3.SetAlpha(c3.GetAlpha() - 0.0039999997f);
			yield return 0f;
		}
		black.gameObject.SetActive(false);
	}
}
