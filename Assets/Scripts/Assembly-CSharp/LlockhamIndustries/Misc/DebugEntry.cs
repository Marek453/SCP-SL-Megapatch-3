using UnityEngine;
using UnityEngine.UI;

namespace LlockhamIndustries.Misc
{
	internal class DebugEntry
	{
		public Text text;

		public RectTransform transform;

		public string title;

		public string log;

		public DebugEntry(Font Font, RectTransform LogRect, float LogHeight, int Index)
		{
			GameObject gameObject = new GameObject("Entry");
			transform = gameObject.AddComponent<RectTransform>();
			transform.SetParent(LogRect, false);
			transform.anchorMin = new Vector2(0f, 1f - LogHeight / LogRect.rect.height * (float)Index);
			transform.anchorMax = new Vector2(0f, 1f - LogHeight / LogRect.rect.height * (float)Index);
			transform.offsetMin = new Vector2(0f, 0f - LogHeight);
			transform.offsetMax = new Vector2(LogRect.rect.width, 0f);
			text = gameObject.AddComponent<Text>();
			text.alignment = TextAnchor.MiddleLeft;
			text.color = Color.white;
			text.fontSize = 20;
			text.font = Font;
			text.text = string.Empty;
		}

		public void Update(string Title, string Log)
		{
			title = Title;
			log = Log;
			if (Title != string.Empty || Log != string.Empty)
			{
				text.text = title + " : " + log;
			}
			else
			{
				text.text = string.Empty;
			}
		}
	}
}
