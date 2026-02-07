using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextLanguageReplacer : MonoBehaviour
{
	private MenuMusicManager mng;

	[Multiline]
	public string englishVersion;

	public string keyName;

	public int index;

	public void UpdateString()
	{
		string text = TranslationReader.Get(keyName, index);
		while (text.Contains("\\n"))
		{
			text = text.Replace("\\n", Environment.NewLine);
		}
		if (text == "NO_TRANSLATION" || text == "TRANSLATION_ERROR")
		{
			Debug.Log("Missing translation! " + keyName + index);
			text = englishVersion;
		}
		if (GetComponent<TextMeshProUGUI>() != null)
		{
			GetComponent<TextMeshProUGUI>().text = text;
		}
		else
		{
			GetComponent<Text>().text = text;
		}
	}

	private void Awake()
	{
		UpdateString();
	}
}
