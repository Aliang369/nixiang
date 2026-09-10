using UnityEngine;
using UnityEngine.UI;

public static class ColorHelper
{
	public static Color IndexUnDiscoverColor = new Color(0f, 0f, 0f, 0.7f);

	public static void SetA(this Image image, float a)
	{
		image.color = image.color.SetA(a);
	}

	public static Color SetA(this Color color, float a)
	{
		color.a = a;
		return color;
	}
}
