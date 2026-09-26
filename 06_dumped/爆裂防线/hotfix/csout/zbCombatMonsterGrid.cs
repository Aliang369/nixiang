using System.Collections.Generic;
using UnityEngine;

public class zbCombatMonsterGrid
{
	private readonly HashSet<int> tmpAdded = new HashSet<int>();

	private readonly Vector2[] tmpCorners = (Vector2[])(object)new Vector2[4];

	private readonly int cellSize = 1;

	private readonly float invCellSize;

	private readonly int worldMinX = -4;

	private readonly int worldMaxX = 4;

	private readonly int worldMinY = -10;

	private readonly int worldMaxY = 12;

	private readonly int offsetX;

	private readonly int offsetY;

	private readonly HashSet<zbMonster>[,] grid;

	public zbCombatMonsterGrid()
	{
		invCellSize = 1f / (float)cellSize;
		offsetX = -worldMinX;
		offsetY = -worldMinY;
		int num = worldMaxX - worldMinX + 1;
		int num2 = worldMaxY - worldMinY + 1;
		grid = new HashSet<zbMonster>[num, num2];
	}

	public void Clear()
	{
		for (int i = 0; i < grid.GetLength(0); i++)
		{
			for (int j = 0; j < grid.GetLength(1); j++)
			{
				grid[i, j]?.Clear();
			}
		}
	}

	public void AddMonster(zbMonster m)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CalcCellRange(m.monsterBoxBounds, out var minCx, out var maxCx, out var minCy, out var maxCy);
		m.minCx = minCx;
		m.maxCx = maxCx;
		m.minCy = minCy;
		m.maxCy = maxCy;
		for (int i = minCx; i <= maxCx; i++)
		{
			for (int j = minCy; j <= maxCy; j++)
			{
				HashSet<zbMonster> hashSet = grid[i, j];
				if (hashSet == null)
				{
					hashSet = new HashSet<zbMonster>();
					grid[i, j] = hashSet;
				}
				hashSet.Add(m);
			}
		}
	}

	public void RemoveMonster(zbMonster m)
	{
		if (m.minCx == int.MinValue)
		{
			return;
		}
		for (int i = m.minCx; i <= m.maxCx; i++)
		{
			for (int j = m.minCy; j <= m.maxCy; j++)
			{
				grid[i, j]?.Remove(m);
			}
		}
	}

	private void CalcCellRange(Bounds b, out int minCx, out int maxCx, out int minCy, out int maxCy)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		minCx = Mathf.FloorToInt(((Bounds)(ref b)).min.x * invCellSize) + offsetX;
		maxCx = Mathf.CeilToInt(((Bounds)(ref b)).max.x * invCellSize) + offsetX;
		minCy = Mathf.FloorToInt(((Bounds)(ref b)).min.y * invCellSize) + offsetY;
		maxCy = Mathf.CeilToInt(((Bounds)(ref b)).max.y * invCellSize) + offsetY;
		minCx = Mathf.Clamp(minCx, 0, grid.GetLength(0) - 1);
		maxCx = Mathf.Clamp(maxCx, 0, grid.GetLength(0) - 1);
		minCy = Mathf.Clamp(minCy, 0, grid.GetLength(1) - 1);
		maxCy = Mathf.Clamp(maxCy, 0, grid.GetLength(1) - 1);
	}

	public Vector2Int FindBestClusterCell()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		int num = 0;
		Vector2Int result = default(Vector2Int);
		((Vector2Int)(ref result))._002Ector(-1, -1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				HashSet<zbMonster> hashSet = grid[i, j];
				if (hashSet != null && hashSet.Count != 0)
				{
					HashSet<zbMonster> hashSet2 = grid[i, j];
					if (hashSet2.Count > num)
					{
						num = hashSet2.Count;
						((Vector2Int)(ref result))._002Ector(i - offsetX, j - offsetY);
					}
				}
			}
		}
		return result;
	}

	public void UpdateMonster(zbMonster m)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		CalcCellRange(m.monsterBoxBounds, out var minCx, out var maxCx, out var minCy, out var maxCy);
		if (minCx == m.minCx && maxCx == m.maxCx && minCy == m.minCy && maxCy == m.maxCy)
		{
			return;
		}
		RemoveMonster(m);
		m.minCx = minCx;
		m.maxCx = maxCx;
		m.minCy = minCy;
		m.maxCy = maxCy;
		for (int i = minCx; i <= maxCx; i++)
		{
			for (int j = minCy; j <= maxCy; j++)
			{
				HashSet<zbMonster> hashSet = grid[i, j];
				if (hashSet == null)
				{
					hashSet = new HashSet<zbMonster>();
					grid[i, j] = hashSet;
				}
				hashSet.Add(m);
			}
		}
	}

	private void Query(in zbBounds2D area, HashSet<int> ignoreList, List<zbMonster> results, bool stopAtFirst = false)
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		tmpAdded.Clear();
		int num = Mathf.FloorToInt(area.minX / (float)cellSize) + offsetX;
		int num2 = Mathf.CeilToInt(area.maxX / (float)cellSize) + offsetX;
		int num3 = Mathf.FloorToInt(area.minY / (float)cellSize) + offsetY;
		int num4 = Mathf.CeilToInt(area.maxY / (float)cellSize) + offsetY;
		int num5 = Mathf.Clamp(num, 0, grid.GetLength(0) - 1);
		num2 = Mathf.Clamp(num2, 0, grid.GetLength(0) - 1);
		num3 = Mathf.Clamp(num3, 0, grid.GetLength(1) - 1);
		num4 = Mathf.Clamp(num4, 0, grid.GetLength(1) - 1);
		for (int i = num5; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				HashSet<zbMonster> hashSet = grid[i, j];
				if (hashSet == null)
				{
					continue;
				}
				foreach (zbMonster item in hashSet)
				{
					if (item.isGoDestroyed || !item.boxColliderEnable)
					{
						continue;
					}
					int uUID = item.GetUUID();
					if ((ignoreList == null || !ignoreList.Contains(uUID)) && !tmpAdded.Contains(uUID) && area.Overlaps(item.monsterBoxBounds))
					{
						results.Add(item);
						tmpAdded.Add(uUID);
						if (stopAtFirst)
						{
							return;
						}
					}
				}
			}
		}
	}

	private void Query(in zbBoundsCircle2D area, HashSet<int> ignoreList, List<zbMonster> results, bool stopAtFirst = false)
	{
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		tmpAdded.Clear();
		float num = area.center.x - area.radius;
		float num2 = area.center.x + area.radius;
		float num3 = area.center.y - area.radius;
		float num4 = area.center.y + area.radius;
		int num5 = Mathf.FloorToInt(num / (float)cellSize) + offsetX;
		int num6 = Mathf.CeilToInt(num2 / (float)cellSize) + offsetX;
		int num7 = Mathf.FloorToInt(num3 / (float)cellSize) + offsetY;
		int num8 = Mathf.CeilToInt(num4 / (float)cellSize) + offsetY;
		int num9 = Mathf.Clamp(num5, 0, grid.GetLength(0) - 1);
		num6 = Mathf.Clamp(num6, 0, grid.GetLength(0) - 1);
		num7 = Mathf.Clamp(num7, 0, grid.GetLength(1) - 1);
		num8 = Mathf.Clamp(num8, 0, grid.GetLength(1) - 1);
		for (int i = num9; i <= num6; i++)
		{
			for (int j = num7; j <= num8; j++)
			{
				HashSet<zbMonster> hashSet = grid[i, j];
				if (hashSet == null)
				{
					continue;
				}
				foreach (zbMonster item in hashSet)
				{
					if (item.isGoDestroyed || !item.boxColliderEnable)
					{
						continue;
					}
					int uUID = item.GetUUID();
					if ((ignoreList == null || !ignoreList.Contains(uUID)) && !tmpAdded.Contains(uUID) && area.Overlaps(item.monsterBoxBounds))
					{
						results.Add(item);
						tmpAdded.Add(uUID);
						if (stopAtFirst)
						{
							return;
						}
					}
				}
			}
		}
	}

	private void Query(in zbOBB2D area, HashSet<int> ignoreList, List<zbMonster> results, bool stopAtFirst = false)
	{
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		results.Clear();
		tmpAdded.Clear();
		area.GetCorners(tmpCorners);
		float x = tmpCorners[0].x;
		float x2 = tmpCorners[0].x;
		float y = tmpCorners[0].y;
		float y2 = tmpCorners[0].y;
		for (int i = 1; i < 4; i++)
		{
			if (tmpCorners[i].x < x)
			{
				x = tmpCorners[i].x;
			}
			if (tmpCorners[i].x > x2)
			{
				x2 = tmpCorners[i].x;
			}
			if (tmpCorners[i].y < y)
			{
				y = tmpCorners[i].y;
			}
			if (tmpCorners[i].y > y2)
			{
				y2 = tmpCorners[i].y;
			}
		}
		int num = Mathf.FloorToInt(x / (float)cellSize) + offsetX;
		int num2 = Mathf.CeilToInt(x2 / (float)cellSize) + offsetX;
		int num3 = Mathf.FloorToInt(y / (float)cellSize) + offsetY;
		int num4 = Mathf.CeilToInt(y2 / (float)cellSize) + offsetY;
		int num5 = Mathf.Clamp(num, 0, grid.GetLength(0) - 1);
		num2 = Mathf.Clamp(num2, 0, grid.GetLength(0) - 1);
		num3 = Mathf.Clamp(num3, 0, grid.GetLength(1) - 1);
		num4 = Mathf.Clamp(num4, 0, grid.GetLength(1) - 1);
		for (int j = num5; j <= num2; j++)
		{
			for (int k = num3; k <= num4; k++)
			{
				HashSet<zbMonster> hashSet = grid[j, k];
				if (hashSet == null)
				{
					continue;
				}
				foreach (zbMonster item in hashSet)
				{
					if (item.isGoDestroyed || !item.boxColliderEnable)
					{
						continue;
					}
					int uUID = item.GetUUID();
					if ((ignoreList == null || !ignoreList.Contains(uUID)) && !tmpAdded.Contains(uUID) && area.Overlaps(item.monsterBoxBounds))
					{
						results.Add(item);
						tmpAdded.Add(uUID);
						if (stopAtFirst)
						{
							return;
						}
					}
				}
			}
		}
	}

	public void QueryFirst(in zbBounds2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results, stopAtFirst: true);
	}

	public void QueryFirst(in zbBoundsCircle2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results, stopAtFirst: true);
	}

	public void QueryFirst(in zbOBB2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results, stopAtFirst: true);
	}

	public void QueryList(in zbBounds2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results);
	}

	public void QueryList(in zbBoundsCircle2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results);
	}

	public void QueryList(in zbOBB2D area, HashSet<int> ignoreList, List<zbMonster> results)
	{
		Query(in area, ignoreList, results);
	}
}
