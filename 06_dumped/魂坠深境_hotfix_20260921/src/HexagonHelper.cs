using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public static class HexagonHelper
{
	public static float HexCellSize = 36f;

	public static int HexShapeMaxRadius = 3;

	public static Vector2 GetHexPointPosition(Vector2Int hexPoint, float hexSize)
	{
		int x = hexPoint.x;
		int y = hexPoint.y;
		float x2 = hexSize * (Mathf.Sqrt(3f) * ((float)x + (float)y / 2f));
		float y2 = hexSize * (1.5f * (float)y);
		return new Vector2(x2, y2);
	}

	public static List<Vector2Int> GetHexShapeIntersectPoints(List<HexShape> shapes)
	{
		List<Vector2Int> list = new List<Vector2Int>();
		if (shapes == null || shapes.Count < 2)
		{
			return list;
		}
		Dictionary<Vector2Int, int> dictionary = new Dictionary<Vector2Int, int>();
		foreach (HexShape shape in shapes)
		{
			if (shape?.points == null)
			{
				continue;
			}
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
			foreach (Vector2Int point in shape.points)
			{
				Vector2Int vector2Int = point + shape.offset;
				if (hashSet.Add(vector2Int) && !dictionary.TryAdd(vector2Int, 1))
				{
					dictionary[vector2Int]++;
				}
			}
		}
		foreach (KeyValuePair<Vector2Int, int> item in dictionary)
		{
			if (item.Value >= 2)
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	public static bool GetIsHexShapeIntersect(List<HexShape> shapes, Vector2Int pos)
	{
		if (shapes == null)
		{
			return false;
		}
		new Dictionary<Vector2Int, int>();
		foreach (HexShape shape in shapes)
		{
			if (shape?.points == null)
			{
				continue;
			}
			foreach (Vector2Int point in shape.points)
			{
				Vector2Int vector2Int = point + shape.offset;
				if (pos == vector2Int)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool CheckHexShapeContainsSubShapes(HexShape mainShape, List<HexShape> subShapes)
	{
		if (mainShape?.points == null || subShapes == null || subShapes.Count == 0)
		{
			return false;
		}
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		foreach (Vector2Int point in mainShape.points)
		{
			hashSet.Add(point + mainShape.offset);
		}
		foreach (HexShape subShape in subShapes)
		{
			if (subShape?.points == null)
			{
				continue;
			}
			foreach (Vector2Int point2 in subShape.points)
			{
				Vector2Int item = point2 + subShape.offset;
				if (!hashSet.Contains(item))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static string GenerateHexShapePreviewText(HexShape hexShape)
	{
		if (hexShape?.points == null || hexShape.points.Count == 0)
		{
			return "";
		}
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		foreach (Vector2Int point in hexShape.points)
		{
			hashSet.Add(point + hexShape.offset);
		}
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		foreach (Vector2Int item2 in hashSet)
		{
			num = Mathf.Min(num, item2.x);
			num2 = Mathf.Max(num2, item2.x);
			num3 = Mathf.Min(num3, item2.y);
			num4 = Mathf.Max(num4, item2.y);
		}
		num--;
		num2++;
		num3--;
		num4++;
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = num3; i <= num4; i++)
		{
			if (i % 2 != 0)
			{
				stringBuilder.Append(" ");
			}
			for (int j = num; j <= num2; j++)
			{
				Vector2Int item = new Vector2Int(j, i);
				if (hashSet.Contains(item))
				{
					stringBuilder.Append("x");
				}
				else
				{
					stringBuilder.Append("o");
				}
				if (j < num2)
				{
					stringBuilder.Append(" ");
				}
			}
			if (i < num4)
			{
				stringBuilder.AppendLine();
			}
		}
		return stringBuilder.ToString();
	}

	public static HexShape GenerateRandomSpreadHexShape(int pointCount, int maxRadius)
	{
		HexShape hexShape = new HexShape();
		hexShape.offset = Vector2Int.zero;
		if (pointCount <= 0)
		{
			return hexShape;
		}
		hexShape.points.Add(Vector2Int.zero);
		if (pointCount == 1)
		{
			return hexShape;
		}
		Vector2Int[] hexNeighborDirections = GetHexNeighborDirections();
		List<Vector2Int> list = new List<Vector2Int>();
		Vector2Int[] array = hexNeighborDirections;
		foreach (Vector2Int vector2Int in array)
		{
			if (IsWithinHexRadius(vector2Int, maxRadius))
			{
				list.Add(vector2Int);
			}
		}
		if (list.Count == 0)
		{
			return hexShape;
		}
		Vector2Int item = list[UnityEngine.Random.Range(0, list.Count)];
		hexShape.points.Add(item);
		for (int j = 2; j < pointCount; j++)
		{
			List<Vector2Int> list2 = new List<Vector2Int>();
			HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>(hexShape.points);
			HashSet<Vector2Int> hashSet2 = new HashSet<Vector2Int>();
			foreach (Vector2Int point in hexShape.points)
			{
				array = hexNeighborDirections;
				foreach (Vector2Int vector2Int2 in array)
				{
					Vector2Int vector2Int3 = point + vector2Int2;
					if (hashSet2.Contains(vector2Int3))
					{
						continue;
					}
					hashSet2.Add(vector2Int3);
					if (hashSet.Contains(vector2Int3) || !IsWithinHexRadius(vector2Int3, maxRadius))
					{
						continue;
					}
					int num = 0;
					foreach (Vector2Int point2 in hexShape.points)
					{
						if (IsHexAdjacent(vector2Int3, point2))
						{
							num++;
						}
					}
					if (num >= 2)
					{
						list2.Add(vector2Int3);
					}
				}
			}
			if (list2.Count == 0)
			{
				break;
			}
			Vector2Int item2 = list2[UnityEngine.Random.Range(0, list2.Count)];
			hexShape.points.Add(item2);
		}
		return hexShape;
	}

	public static HexShape GenerateMutatedHexShape(HexShape currentShape, int targetPointCount, int maxRadius, int maxLocalMoves = 2)
	{
		if (targetPointCount <= 0)
		{
			return GenerateRandomSpreadHexShape(targetPointCount, maxRadius);
		}
		if (!TryNormalizeShapePoints(currentShape, out var normalizedPoints))
		{
			return GenerateRandomSpreadHexShape(targetPointCount, maxRadius);
		}
		if (normalizedPoints.Count < 2 || !IsConnected(normalizedPoints))
		{
			return GenerateRandomSpreadHexShape(targetPointCount, maxRadius);
		}
		if (TryBuildMutatedShape(normalizedPoints, targetPointCount, maxRadius, maxLocalMoves, allowBoundaryMoves: true, out var shape))
		{
			return shape;
		}
		if (TryBuildMutatedShape(normalizedPoints, targetPointCount, maxRadius, maxLocalMoves, allowBoundaryMoves: false, out shape))
		{
			return shape;
		}
		return GenerateRandomSpreadHexShape(targetPointCount, maxRadius);
	}

	private static bool TryBuildMutatedShape(HashSet<Vector2Int> basePoints, int targetPointCount, int maxRadius, int maxLocalMoves, bool allowBoundaryMoves, out HexShape shape)
	{
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>(basePoints);
		int value = targetPointCount - hashSet.Count;
		if (allowBoundaryMoves)
		{
			int a = Mathf.Clamp(maxLocalMoves - Mathf.Abs(value), 0, maxLocalMoves);
			a = Mathf.Min(a, Mathf.Max(0, hashSet.Count - 1));
			for (int i = 0; i < a; i++)
			{
				if (!TryMoveOneBoundaryCell(hashSet, maxRadius))
				{
					break;
				}
			}
		}
		if (!TryResizeShape(hashSet, targetPointCount, maxRadius))
		{
			shape = null;
			return false;
		}
		if (!IsConnected(hashSet))
		{
			shape = null;
			return false;
		}
		shape = CreateHexShape(hashSet);
		return true;
	}

	private static bool TryNormalizeShapePoints(HexShape currentShape, out HashSet<Vector2Int> normalizedPoints)
	{
		normalizedPoints = new HashSet<Vector2Int>();
		if (currentShape?.points == null || currentShape.points.Count == 0)
		{
			return false;
		}
		foreach (Vector2Int point in currentShape.points)
		{
			normalizedPoints.Add(point + currentShape.offset);
		}
		return normalizedPoints.Count > 0;
	}

	private static bool TryResizeShape(HashSet<Vector2Int> points, int targetPointCount, int maxRadius)
	{
		int num = 0;
		int num2 = Mathf.Max(8, (points.Count + targetPointCount + 1) * 4);
		while (points.Count < targetPointCount && num++ < num2)
		{
			if (!TryAddOneCell(points, maxRadius))
			{
				return false;
			}
		}
		while (points.Count > targetPointCount && num++ < num2)
		{
			if (!TryRemoveOneCell(points))
			{
				return false;
			}
		}
		return points.Count == targetPointCount;
	}

	private static bool TryMoveOneBoundaryCell(HashSet<Vector2Int> points, int maxRadius)
	{
		List<Vector2Int> removableBoundaryPoints = GetRemovableBoundaryPoints(points);
		if (removableBoundaryPoints.Count == 0)
		{
			return false;
		}
		Vector2 shapeCenter = GetShapeCenter(points);
		List<int> list = new List<int>(removableBoundaryPoints.Count);
		foreach (Vector2Int item in removableBoundaryPoints)
		{
			list.Add(GetRemoveWeight(points, item, shapeCenter));
		}
		if (!TrySelectWeightedPoint(removableBoundaryPoints, list, out var selectedPoint))
		{
			return false;
		}
		if (!points.Remove(selectedPoint))
		{
			return false;
		}
		if (!TryAddOneCell(points, maxRadius, selectedPoint))
		{
			points.Add(selectedPoint);
			return false;
		}
		return true;
	}

	private static bool TryAddOneCell(HashSet<Vector2Int> points, int maxRadius, Vector2Int? preferredCenterPoint = null)
	{
		List<Vector2Int> candidateAddPoints = GetCandidateAddPoints(points, maxRadius, 2);
		if (candidateAddPoints.Count == 0)
		{
			candidateAddPoints = GetCandidateAddPoints(points, maxRadius, 1);
		}
		if (candidateAddPoints.Count == 0)
		{
			return false;
		}
		Vector2 shapeCenter = GetShapeCenter(points);
		List<int> list = new List<int>(candidateAddPoints.Count);
		foreach (Vector2Int item in candidateAddPoints)
		{
			list.Add(GetAddWeight(points, item, shapeCenter, preferredCenterPoint));
		}
		if (!TrySelectWeightedPoint(candidateAddPoints, list, out var selectedPoint))
		{
			return false;
		}
		return points.Add(selectedPoint);
	}

	private static bool TryRemoveOneCell(HashSet<Vector2Int> points)
	{
		List<Vector2Int> removableBoundaryPoints = GetRemovableBoundaryPoints(points);
		if (removableBoundaryPoints.Count == 0)
		{
			return false;
		}
		Vector2 shapeCenter = GetShapeCenter(points);
		List<int> list = new List<int>(removableBoundaryPoints.Count);
		foreach (Vector2Int item in removableBoundaryPoints)
		{
			list.Add(GetRemoveWeight(points, item, shapeCenter));
		}
		if (!TrySelectWeightedPoint(removableBoundaryPoints, list, out var selectedPoint))
		{
			return false;
		}
		return points.Remove(selectedPoint);
	}

	private static List<Vector2Int> GetCandidateAddPoints(HashSet<Vector2Int> points, int maxRadius, int minAdjacentCount)
	{
		List<Vector2Int> list = new List<Vector2Int>();
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		Vector2Int[] hexNeighborDirections = GetHexNeighborDirections();
		foreach (Vector2Int point in points)
		{
			Vector2Int[] array = hexNeighborDirections;
			foreach (Vector2Int vector2Int in array)
			{
				Vector2Int vector2Int2 = point + vector2Int;
				if (hashSet.Add(vector2Int2) && !points.Contains(vector2Int2) && IsWithinHexRadius(vector2Int2, maxRadius) && CountAdjacentPoints(points, vector2Int2) >= minAdjacentCount)
				{
					list.Add(vector2Int2);
				}
			}
		}
		return list;
	}

	private static List<Vector2Int> GetRemovableBoundaryPoints(HashSet<Vector2Int> points)
	{
		List<Vector2Int> list = new List<Vector2Int>();
		if (points.Count <= 1)
		{
			return list;
		}
		foreach (Vector2Int point in points)
		{
			if (IsBoundaryPoint(points, point))
			{
				HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>(points);
				hashSet.Remove(point);
				if (IsConnected(hashSet))
				{
					list.Add(point);
				}
			}
		}
		list.Sort(delegate(Vector2Int a, Vector2Int b)
		{
			bool flag = a == Vector2Int.zero;
			bool flag2 = b == Vector2Int.zero;
			if (flag != flag2)
			{
				if (!flag)
				{
					return -1;
				}
				return 1;
			}
			int num = CountAdjacentPoints(points, a).CompareTo(CountAdjacentPoints(points, b));
			if (num != 0)
			{
				return num;
			}
			int num2 = a.y.CompareTo(b.y);
			return (num2 == 0) ? a.x.CompareTo(b.x) : num2;
		});
		return list;
	}

	private static bool IsBoundaryPoint(HashSet<Vector2Int> points, Vector2Int point)
	{
		return CountAdjacentPoints(points, point) < GetHexNeighborDirections().Length;
	}

	private static int CountAdjacentPoints(HashSet<Vector2Int> points, Vector2Int point)
	{
		int num = 0;
		Vector2Int[] hexNeighborDirections = GetHexNeighborDirections();
		foreach (Vector2Int vector2Int in hexNeighborDirections)
		{
			if (points.Contains(point + vector2Int))
			{
				num++;
			}
		}
		return num;
	}

	private static bool IsConnected(HashSet<Vector2Int> points)
	{
		if (points.Count <= 1)
		{
			return true;
		}
		HashSet<Vector2Int>.Enumerator enumerator = points.GetEnumerator();
		enumerator.MoveNext();
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		Queue<Vector2Int> queue = new Queue<Vector2Int>();
		queue.Enqueue(enumerator.Current);
		hashSet.Add(enumerator.Current);
		while (queue.Count > 0)
		{
			Vector2Int vector2Int = queue.Dequeue();
			Vector2Int[] hexNeighborDirections = GetHexNeighborDirections();
			foreach (Vector2Int vector2Int2 in hexNeighborDirections)
			{
				Vector2Int item = vector2Int + vector2Int2;
				if (points.Contains(item) && !hashSet.Contains(item))
				{
					hashSet.Add(item);
					queue.Enqueue(item);
				}
			}
		}
		return hashSet.Count == points.Count;
	}

	private static Vector2 GetShapeCenter(HashSet<Vector2Int> points)
	{
		if (points.Count == 0)
		{
			return Vector2.zero;
		}
		float num = 0f;
		float num2 = 0f;
		foreach (Vector2Int point in points)
		{
			num += (float)point.x;
			num2 += (float)point.y;
		}
		return new Vector2(num / (float)points.Count, num2 / (float)points.Count);
	}

	private static int GetAddWeight(HashSet<Vector2Int> points, Vector2Int candidatePoint, Vector2 center, Vector2Int? preferredCenterPoint)
	{
		int num = CountAdjacentPoints(points, candidatePoint);
		int hexDistance = GetHexDistance(candidatePoint, new Vector2Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y)));
		int num2 = 10 + num * 20 + Mathf.Max(0, 8 - hexDistance) * 4;
		if (preferredCenterPoint.HasValue)
		{
			int hexDistance2 = GetHexDistance(candidatePoint, preferredCenterPoint.Value);
			num2 += Mathf.Max(0, 6 - hexDistance2) * 6;
		}
		return Mathf.Max(1, num2);
	}

	private static int GetRemoveWeight(HashSet<Vector2Int> points, Vector2Int point, Vector2 center)
	{
		int num = CountAdjacentPoints(points, point);
		int hexDistance = GetHexDistance(point, new Vector2Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y)));
		int num2 = 10 + (6 - num) * 20 + hexDistance * 6;
		if (point == Vector2Int.zero)
		{
			num2 = Mathf.Max(1, num2 / 10);
		}
		return Mathf.Max(1, num2);
	}

	private static int GetHexDistance(Vector2Int pointA, Vector2Int pointB)
	{
		Vector2Int vector2Int = pointA - pointB;
		return Mathf.Max(Mathf.Abs(vector2Int.x), Mathf.Abs(vector2Int.y), Mathf.Abs(vector2Int.x + vector2Int.y));
	}

	private static bool TrySelectWeightedPoint(List<Vector2Int> points, List<int> weights, out Vector2Int selectedPoint)
	{
		selectedPoint = Vector2Int.zero;
		if (points == null || weights == null || points.Count == 0 || points.Count != weights.Count)
		{
			return false;
		}
		int num = 0;
		for (int i = 0; i < weights.Count; i++)
		{
			num += Mathf.Max(0, weights[i]);
		}
		if (num <= 0)
		{
			return false;
		}
		int num2 = UnityEngine.Random.Range(0, num);
		int num3 = 0;
		for (int j = 0; j < points.Count; j++)
		{
			num3 += Mathf.Max(0, weights[j]);
			if (num2 < num3)
			{
				selectedPoint = points[j];
				return true;
			}
		}
		selectedPoint = points[points.Count - 1];
		return true;
	}

	private static HexShape CreateHexShape(HashSet<Vector2Int> points)
	{
		HexShape hexShape = new HexShape();
		hexShape.offset = Vector2Int.zero;
		hexShape.points = new List<Vector2Int>(points);
		hexShape.points.Sort(delegate(Vector2Int a, Vector2Int b)
		{
			int num = a.y.CompareTo(b.y);
			return (num == 0) ? a.x.CompareTo(b.x) : num;
		});
		return hexShape;
	}

	private static Vector2Int[] GetHexNeighborDirections()
	{
		return new Vector2Int[6]
		{
			new Vector2Int(1, 0),
			new Vector2Int(1, -1),
			new Vector2Int(0, -1),
			new Vector2Int(-1, 0),
			new Vector2Int(-1, 1),
			new Vector2Int(0, 1)
		};
	}

	private static bool IsHexAdjacent(Vector2Int point1, Vector2Int point2)
	{
		Vector2Int vector2Int = point1 - point2;
		Vector2Int[] hexNeighborDirections = GetHexNeighborDirections();
		foreach (Vector2Int vector2Int2 in hexNeighborDirections)
		{
			if (vector2Int == vector2Int2)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsWithinHexRadius(Vector2Int point, int maxRadius)
	{
		int x = point.x;
		int y = point.y;
		return Mathf.Max(Mathf.Abs(x), Mathf.Abs(y), Mathf.Abs(x + y)) <= maxRadius;
	}

	public static List<(int, HexShape)> GetAllRotatedShapes(HexShape hexShape)
	{
		List<(int, HexShape)> list = new List<(int, HexShape)>();
		if (hexShape?.points == null)
		{
			return list;
		}
		for (int i = 0; i < 6; i++)
		{
			HexShape item = RotateHexShape(hexShape, i);
			list.Add((i, item));
		}
		return list;
	}

	public static HexShape RotateHexShape(HexShape shape, int rotateIndex)
	{
		if (shape?.points == null)
		{
			return new HexShape();
		}
		HexShape hexShape = new HexShape();
		hexShape.offset = shape.offset;
		rotateIndex = (rotateIndex % 6 + 6) % 6;
		foreach (Vector2Int point in shape.points)
		{
			Vector2Int item = RotateHexPoint(point, rotateIndex);
			hexShape.points.Add(item);
		}
		return hexShape;
	}

	public static Vector2Int RotateHexPoint(Vector2Int point, int rotateIndex)
	{
		int num = point.x;
		int num2 = point.y;
		int num3 = -num - num2;
		rotateIndex = (rotateIndex % 6 + 6) % 6;
		for (int i = 0; i < rotateIndex; i++)
		{
			int num4 = -num2;
			int num5 = -num3;
			int num6 = -num;
			num = num4;
			num2 = num5;
			num3 = num6;
		}
		return new Vector2Int(num, num2);
	}

	public static void GenerateHexShapeLayout(Dictionary<Vector2Int, Image> cachedImages, Image templateImage, RectTransform parentRect, HexShape shape, float hexPixelSize = 32f)
	{
		foreach (Vector2Int point in shape.points)
		{
			Vector2Int vector2Int = point + shape.offset;
			Vector2 hexPointPosition = GetHexPointPosition(vector2Int, hexPixelSize);
			if (!cachedImages.TryGetValue(vector2Int, out var value))
			{
				value = UnityEngine.Object.Instantiate(templateImage, parentRect);
				cachedImages.Add(vector2Int, value);
			}
			value.gameObject.SetActive(value: true);
			value.transform.localPosition = hexPointPosition;
			value.rectTransform.sizeDelta = Vector2.one * (hexPixelSize * 2f);
			value.sprite = templateImage.sprite;
		}
	}

	public static Texture2D GenerateHexShapeTexture(HexShape hexShape, int hexPixelSize = 32, float gap = 2f, Color hexColor = default(Color), Color bgColor = default(Color), Color centerColor = default(Color))
	{
		if (hexShape?.points == null || hexShape.points.Count == 0)
		{
			return null;
		}
		if (hexColor == default(Color))
		{
			hexColor = Color.white;
		}
		if (bgColor == default(Color))
		{
			bgColor = new Color(0f, 0f, 0f, 0f);
		}
		if (centerColor == default(Color))
		{
			centerColor = hexColor;
		}
		int num = 3;
		float num2 = (float)hexPixelSize / 2f;
		float num3 = Mathf.Sqrt(3f) * num2;
		float num4 = 2f * num2;
		int num5 = Mathf.CeilToInt((float)(2 * num + 1) * num3 * 0.75f + num3 * 0.25f);
		int num6 = Mathf.CeilToInt((float)(2 * num + 1) * num4 * 0.5f + num4 * 0.5f);
		Texture2D texture2D = new Texture2D(num5, num6, TextureFormat.RGBA32, mipChain: false);
		texture2D.filterMode = FilterMode.Point;
		Color[] array = new Color[num5 * num6];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = bgColor;
		}
		texture2D.SetPixels(array);
		Vector2 vector = new Vector2((float)num5 / 2f, (float)num6 / 2f);
		float size = Mathf.Max(1f, num2 * 0.75f - gap);
		HashSet<Vector2Int> hashSet = new HashSet<Vector2Int>();
		foreach (Vector2Int point in hexShape.points)
		{
			hashSet.Add(point + hexShape.offset);
		}
		foreach (Vector2Int item in hashSet)
		{
			Vector2 hexPointPosition = GetHexPointPosition(item, num2 * 0.75f);
			Vector2 center = vector + hexPointPosition;
			Color color = ((item.x == 0 && item.y == 0) ? centerColor : hexColor);
			DrawFilledHexagon(texture2D, center, size, color);
		}
		texture2D.Apply();
		return texture2D;
	}

	private static void DrawFilledHexagon(Texture2D tex, Vector2 center, float size, Color color)
	{
		Vector2[] array = new Vector2[6];
		for (int i = 0; i < 6; i++)
		{
			float f = (float)Math.PI / 180f * (float)(60 * i - 30);
			array[i] = center + new Vector2(size * Mathf.Cos(f), size * Mathf.Sin(f));
		}
		int num = Mathf.FloorToInt(Mathf.Min(array[0].x, array[1].x, array[2].x, array[3].x, array[4].x, array[5].x));
		int num2 = Mathf.CeilToInt(Mathf.Max(array[0].x, array[1].x, array[2].x, array[3].x, array[4].x, array[5].x));
		int num3 = Mathf.FloorToInt(Mathf.Min(array[0].y, array[1].y, array[2].y, array[3].y, array[4].y, array[5].y));
		int num4 = Mathf.CeilToInt(Mathf.Max(array[0].y, array[1].y, array[2].y, array[3].y, array[4].y, array[5].y));
		for (int j = num3; j <= num4; j++)
		{
			for (int k = num; k <= num2; k++)
			{
				if (PointInPolygon(new Vector2((float)k + 0.5f, (float)j + 0.5f), array) && k >= 0 && k < tex.width && j >= 0 && j < tex.height)
				{
					tex.SetPixel(k, j, color);
				}
			}
		}
	}

	private static bool PointInPolygon(Vector2 p, Vector2[] verts)
	{
		int num = verts.Length;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector = verts[i];
			Vector2 vector2 = verts[(i + 1) % num] - vector;
			Vector2 vector3 = p - vector;
			if (vector2.x * vector3.y - vector2.y * vector3.x < 0f)
			{
				return false;
			}
		}
		return true;
	}

	public static Vector2 ScreenPosToLocalRectPos(Vector2 screenPos, RectTransform rectTransform, Camera camera)
	{
		Vector2 result = default(Vector2);
		RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPos, camera, ref result);
		return result;
	}

	public static Vector2Int LocalRectToHexOffset(Vector2 localPos, float hexSize, int minQ, int maxQ, int minR, int maxR)
	{
		float x = (Mathf.Sqrt(3f) / 3f * localPos.x - 1f / 3f * localPos.y) / hexSize;
		float y = 2f / 3f * localPos.y / hexSize;
		Vector2Int result = HexRound(new Vector2(x, y));
		result.x = Mathf.Clamp(result.x, minQ, maxQ);
		result.y = Mathf.Clamp(result.y, minR, maxR);
		return result;
	}

	private static Vector2Int HexRound(Vector2 hexFloat)
	{
		float x = hexFloat.x;
		float y = hexFloat.y;
		float num = 0f - x - y;
		int num2 = Mathf.RoundToInt(x);
		int num3 = Mathf.RoundToInt(num);
		int num4 = Mathf.RoundToInt(y);
		float num5 = Mathf.Abs((float)num2 - x);
		float num6 = Mathf.Abs((float)num3 - num);
		float num7 = Mathf.Abs((float)num4 - y);
		if (num5 > num6 && num5 > num7)
		{
			num2 = -num3 - num4;
		}
		else if (num6 > num7)
		{
			num3 = -num2 - num4;
		}
		else
		{
			num4 = -num2 - num3;
		}
		return new Vector2Int(num2, num4);
	}
}
