using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ServerInfo : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	private Canvas canvas;

	public GameObject root;

	public TextMeshProUGUI text;

	public static void ShowInfo(string id)
	{
		ServerInfo serverInfo = Object.FindObjectOfType<ServerInfo>();
		Timing.RunCoroutine(serverInfo._Show(id), Segment.FixedUpdate);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		int num = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);
		if (num != -1)
		{
			TMP_LinkInfo tMP_LinkInfo = text.textInfo.linkInfo[num];
			Application.OpenURL(tMP_LinkInfo.GetLinkID());
		}
	}

	public IEnumerator<float> _Show(string id)
	{
		root.SetActive(true);
		text.text = string.Empty;
		if (id.Contains("/"))
		{
			text.text = "The URL isn't directing to pastebin site. Please contact server owner.";
		}
		WWW www = new WWW("https://pastebin.com/raw/" + id);
		yield return Timing.WaitUntilDone(www);
		if (string.IsNullOrEmpty(www.error))
		{
			text.text = www.text;
		}
		else
		{
			text.text = www.error;
		}
	}

	public void Close()
	{
		Object.FindObjectOfType<ServerInfo>().root.SetActive(false);
	}
}
