using UnityEngine;

public static class GeometryMathHelper
{
	public static Vector2 GetCirclePoint(Vector2 center, float radius, float alpha)
	{
		return center + new Vector2(radius * Mathf.Cos(alpha), radius * Mathf.Sin(alpha));
	}
}
