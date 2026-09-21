using UnityEngine;

public static class NormalExtends
{
	public static Color Rotate(Color pixel, float radian)
	{
		Vector2 vector = new Vector2(pixel.r * 2f - 1f, pixel.g * 2f - 1f);
		Vector2 vector2 = new Vector2(vector.x * Mathf.Cos(radian) + vector.y * Mathf.Sin(radian), (0f - vector.x) * Mathf.Sin(radian) + vector.y * Mathf.Cos(radian));
		pixel.r = (vector2.x + 1f) / 2f;
		pixel.g = (vector2.y + 1f) / 2f;
		return pixel;
	}

	public static Color FlipX(Color pixel)
	{
		Vector2 vector = new Vector2(pixel.r * 2f - 1f, pixel.g * 2f - 1f) * new Vector2(-1f, 1f);
		pixel.r = (vector.x + 1f) / 2f;
		pixel.g = (vector.y + 1f) / 2f;
		return pixel;
	}

	public static Color FlipY(Color pixel)
	{
		Vector2 vector = new Vector2(pixel.r * 2f - 1f, pixel.g * 2f - 1f) * new Vector2(1f, -1f);
		pixel.r = (vector.x + 1f) / 2f;
		pixel.g = (vector.y + 1f) / 2f;
		return pixel;
	}
}
