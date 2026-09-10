using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GizmosHelper
{
	public static void DrawString(string text, Vector3 worldPosition, Vector2 anchor, Color textColor, float textSize = 15f)
	{
	}

	public static void DrawString(string text, Vector3 worldPosition, float textSize = 15f)
	{
		DrawString(text, worldPosition, Vector2.zero, Gizmos.color, textSize);
	}

	public static void DrawArrowLine(Vector3 from, Vector3 to, Vector3 arrowUp)
	{
		Gizmos.DrawLine(from, to);
		Vector3 direction = to - from;
		DrawArrow(to, direction, arrowUp, direction.magnitude / 15f);
	}

	public static void DrawArrow(Vector3 pivot, Vector3 direction, Vector3 arrowUp, float size)
	{
		Vector3 vector = Quaternion.LookRotation(direction, arrowUp) * Quaternion.Euler(0f, 200f, 0f) * new Vector3(0f, 0f, 1f);
		Vector3 vector2 = Quaternion.LookRotation(direction, arrowUp) * Quaternion.Euler(0f, 160f, 0f) * new Vector3(0f, 0f, 1f);
		_ = direction.magnitude / 15f;
		Gizmos.DrawLine(pivot, pivot + vector * size);
		Gizmos.DrawLine(pivot, pivot + vector2 * size);
	}

	public static void DrawBezier(List<Vector3> points, int subPerSegment = 10)
	{
		List<Vector3> list = MathHelper.SampleBezierLinePoints(points, subPerSegment);
		for (int i = 0; i < list.Count - 1; i++)
		{
			Gizmos.DrawLine(list[i], list[i + 1]);
		}
	}

	public static void DrawBezierArrow(List<Vector3> points, Vector3 arrowUp, int subPerSegment = 10)
	{
		if (points.Count < 2)
		{
			return;
		}
		if (points.Count == 2)
		{
			DrawArrowLine(points[0], points[1], arrowUp);
			return;
		}
		List<Vector3> list = MathHelper.SampleBezierLinePoints(points, subPerSegment);
		for (int i = 0; i < list.Count - 1; i++)
		{
			Gizmos.DrawLine(list[i], list[i + 1]);
		}
		float size = (list.Last() - list.First()).magnitude / 10f;
		DrawArrow(list[list.Count - 1], list[list.Count - 1] - list[list.Count - 2], arrowUp, size);
	}
}
