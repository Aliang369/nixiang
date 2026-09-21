using System;
using UnityEngine;

public class CustomLayoutGroup
{
	public enum Align
	{
		Left = 1,
		Center,
		Right
	}

	public Align align;

	public RectTransform baseRect;

	public Vector2 GetPosition(RectTransform rectTransform, int index, int count)
	{
		float width = rectTransform.GetWidth();
		float width2 = baseRect.GetWidth();
		int num = (int)(width2 / width);
		if (count <= num)
		{
			return align switch
			{
				Align.Left => new Vector2(((float)index + 0.5f - (float)num / 2f) * width, 0f), 
				Align.Center => new Vector2(((float)index - (float)count / 2f + 0.5f) * width, 0f), 
				Align.Right => new Vector2(((float)num / 2f - (float)index - 0.5f) * width, 0f), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		float num2 = width2 / (float)count;
		return new Vector2(((float)index - (float)count / 2f + 0.5f) * num2, 0f);
	}
}
