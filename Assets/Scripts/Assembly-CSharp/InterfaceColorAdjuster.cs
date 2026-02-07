using UnityEngine;
using UnityEngine.UI;

public class InterfaceColorAdjuster : MonoBehaviour
{
	public Graphic[] graphicsToChange;

	public void ChangeColor(Color color)
	{
		Graphic[] array = graphicsToChange;
		foreach (Graphic graphic in array)
		{
			if (graphic != null)
			{
				Color color2 = new Color(color.r, color.g, color.b, graphic.color.a);
				graphic.color = color2;
			}
		}
        Color color3= new Color(color.r, color.g, color.b, 0.5F);
        GetComponent<InventoryDisplay>().highlightColor = color3;
        Color color4 = new Color(color.r, color.g, color.b, 1);
        GetComponent<InventoryDisplay>().selectedColor = color4;

    }
}
