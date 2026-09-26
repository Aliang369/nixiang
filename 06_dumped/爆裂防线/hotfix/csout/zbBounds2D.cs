using UnityEngine;

public struct zbBounds2D
{
	public Vector2 center;

	public Vector2 halfSize;

	public float minX => center.x - halfSize.x;

	public float maxX => center.x + halfSize.x;

	public float minY => center.y - halfSize.y;

	public float maxY => center.y + halfSize.y;

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
		if (num <= maxX && num2 >= minX && num3 <= maxY)
		{
			return num4 >= minY;
		}
		return false;
	}

	public bool Overlaps(zbBounds2D other)
	{
		if (other.minX <= maxX && other.maxX >= minX && other.minY <= maxY)
		{
			return other.maxY >= minY;
		}
		return false;
	}
}
