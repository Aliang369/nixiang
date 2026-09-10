using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
	public static int ReplaceIfZero(this int a, int b)
	{
		if (a == 0)
		{
			return b;
		}
		return a;
	}

	public static int gcf(int a, int b)
	{
		while (b != 0)
		{
			int num = b;
			b = a % b;
			a = num;
		}
		return a;
	}

	public static int lcm(int a, int b)
	{
		return a / gcf(a, b) * b;
	}

	public static int lcm(params int[] ls)
	{
		if (ls.Length == 0)
		{
			return 0;
		}
		int num = 1;
		foreach (int b in ls)
		{
			num = lcm(num, b);
		}
		return num;
	}

	public static int HamiltonDistance(Vector2Int a, Vector2Int b)
	{
		return Mathf.Abs(a.y - b.y) + Mathf.Abs(a.x - b.x);
	}

	public static int Clamp(this int f, int min, int max)
	{
		return Mathf.Clamp(f, min, max);
	}

	public static float Clamp(this float f, float min, float max)
	{
		return Mathf.Clamp(f, min, max);
	}

	public static Vector3 LocalPositionIn(this Transform trans, Transform parent)
	{
		return parent.worldToLocalMatrix.MultiplyPoint(trans.position);
	}

	public static void SetLocalPositionIn(this Transform trans, Vector3 posInParent, Transform parent)
	{
		trans.position = parent.localToWorldMatrix.MultiplyPoint(posInParent);
	}

	public static void SetWorldHeight(this RectTransform trans, float height)
	{
		trans.SetHeight(height / trans.lossyScale.y);
	}

	public static void SetWorldWidth(this RectTransform trans, float width)
	{
		trans.SetHeight(width / trans.lossyScale.x);
	}

	public static int SumOfDiffToMax(IReadOnlyList<int> values, Func<int, int> maxGetter)
	{
		int num = 0;
		for (int i = 0; i < values.Count; i++)
		{
			num += Mathf.Clamp(maxGetter(i) - values[i], 0, int.MaxValue);
		}
		return num;
	}

	public static List<Vector3> SampleBezierLinePoints(List<Vector3> controlPoints, int segment = 20)
	{
		if (controlPoints.Count < 3)
		{
			return new List<Vector3>(controlPoints);
		}
		List<Vector3> list = new List<Vector3>();
		for (int i = 0; i <= segment; i++)
		{
			list.Add(GetBezierPoint(controlPoints, (float)i / (float)segment));
		}
		return list;
	}

	public static Vector3 GetBezierPoint(List<Vector3> points, float progress)
	{
		if (points.Count == 1)
		{
			return points[0];
		}
		Vector3 bezierPoint = GetBezierPoint(points.GetRange(0, points.Count - 1), progress);
		Vector3 bezierPoint2 = GetBezierPoint(points.GetRange(1, points.Count - 1), progress);
		return Vector3.Lerp(bezierPoint, bezierPoint2, progress);
	}
}
