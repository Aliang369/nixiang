using UnityEngine;

public struct zbBoundsCircle2D
{
	public Vector2 center;

	public float radius;

	public bool Overlaps(Bounds other)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Bounds)(ref other)).center;
		Vector3 extents = ((Bounds)(ref other)).extents;
		float num = val.x - extents.x;
		float num2 = val.x + extents.x;
		float num3 = val.y - extents.y;
		float num4 = val.y + extents.y;
		float num5 = Mathf.Clamp(center.x, num, num2);
		float num6 = Mathf.Clamp(center.y, num3, num4);
		float num7 = center.x - num5;
		float num8 = center.y - num6;
		return num7 * num7 + num8 * num8 <= radius * radius;
	}
}
