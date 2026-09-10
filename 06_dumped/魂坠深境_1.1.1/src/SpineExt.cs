using Spine;
using UnityEngine;

public static class SpineExt
{
	public static Color GetColor(this Slot slot)
	{
		return new Color(slot.R, slot.G, slot.B);
	}

	public static Color GetColor2(this Slot slot)
	{
		return new Color(slot.R2, slot.G2, slot.B2);
	}

	public static void SetColor(this Slot slot, Color color)
	{
		slot.R = color.r;
		slot.G = color.g;
		slot.B = color.b;
	}

	public static void SetColor2(this Slot slot, Color color)
	{
		slot.R2 = color.r;
		slot.G2 = color.g;
		slot.B2 = color.b;
	}
}
