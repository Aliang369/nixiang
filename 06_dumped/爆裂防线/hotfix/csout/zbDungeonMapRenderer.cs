using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public static class zbDungeonMapRenderer
{
	private const float CellSpacing = 30f;

	private const float PointSize = 16f;

	private static float MapHeight;

	public static void RenderMap(zbDungeonHandler dungeonBaseHandler, VisualElement container)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0426: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0500: Unknown result type (might be due to invalid IL or missing references)
		//IL_0505: Unknown result type (might be due to invalid IL or missing references)
		//IL_0507: Unknown result type (might be due to invalid IL or missing references)
		//IL_050c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0513: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_0539: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0549: Unknown result type (might be due to invalid IL or missing references)
		//IL_0564: Unknown result type (might be due to invalid IL or missing references)
		//IL_056e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_0580: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_0592: Unknown result type (might be due to invalid IL or missing references)
		//IL_0598: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0602: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Expected O, but got Unknown
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_044d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0460: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0464: Unknown result type (might be due to invalid IL or missing references)
		DungeonMapInfo dungeonMapInfo = dungeonBaseHandler.dungeonMapInfo;
		container.Clear();
		Dictionary<Vector2Int, DungeonMapCellDetail> dictionary = new Dictionary<Vector2Int, DungeonMapCellDetail>();
		List<DungeonMapCellDetail> list = new List<DungeonMapCellDetail>();
		Vector2Int? val = null;
		foreach (DungeonMapCellDetail cell in dungeonMapInfo.CellList)
		{
			Vector2Int key = ParseCellId(cell.CellId);
			dictionary[key] = cell;
			if (cell.FunType == EDungeonMapCellFunType.Elevator || cell.FunType == EDungeonMapCellFunType.Spawn || IsExplored(cell) || cell.State == EDungeonMapCellState.Visible)
			{
				list.Add(cell);
			}
		}
		if (dungeonBaseHandler.dungeonBaseInfo.RoleSiteInfo != null)
		{
			val = ParseCellId(dungeonBaseHandler.dungeonBaseInfo.RoleSiteInfo.CellId);
		}
		MapHeight = 0f;
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item3 in dictionary)
		{
			float mapHeight = MapHeight;
			Vector2Int key2 = item3.Key;
			MapHeight = Mathf.Max(mapHeight, (float)((Vector2Int)(ref key2)).y);
		}
		Dictionary<Vector2Int, Vector2> dictionary2 = new Dictionary<Vector2Int, Vector2>();
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item4 in dictionary)
		{
			Vector2Int key3 = item4.Key;
			DungeonMapCellDetail value = item4.Value;
			Vector2 roomCenter = GetRoomCenter(key3, value);
			dictionary2[key3] = roomCenter;
		}
		Vector2Int val3 = default(Vector2Int);
		Vector2Int val4 = default(Vector2Int);
		foreach (DungeonMapCellDetail item5 in list)
		{
			Vector2Int val2 = ParseCellId(item5.CellId);
			Vector2 centerA = dictionary2[val2];
			GetRoomWidth(item5);
			for (int i = 1; i <= 3; i++)
			{
				((Vector2Int)(ref val3))._002Ector(((Vector2Int)(ref val2)).x + i, ((Vector2Int)(ref val2)).y);
				if (dictionary2.TryGetValue(val3, out var value2))
				{
					DungeonMapCellDetail cellB = dictionary[val3];
					TryLink(item5, val2, centerA, cellB, val3, value2, container, dictionary);
				}
				((Vector2Int)(ref val4))._002Ector(((Vector2Int)(ref val2)).x - i, ((Vector2Int)(ref val2)).y);
				if (dictionary2.TryGetValue(val4, out var value3))
				{
					DungeonMapCellDetail cellB2 = dictionary[val4];
					TryLink(item5, val2, centerA, cellB2, val4, value3, container, dictionary);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			DungeonMapCellDetail dungeonMapCellDetail = list[j];
			if (dungeonMapCellDetail.FunType != EDungeonMapCellFunType.Elevator)
			{
				continue;
			}
			Vector2Int key4 = ParseCellId(dungeonMapCellDetail.CellId);
			Vector2 a = dictionary2[key4];
			for (int k = j + 1; k < list.Count; k++)
			{
				DungeonMapCellDetail dungeonMapCellDetail2 = list[k];
				if (dungeonMapCellDetail2.FunType == EDungeonMapCellFunType.Elevator)
				{
					Vector2Int key5 = ParseCellId(dungeonMapCellDetail2.CellId);
					if (((Vector2Int)(ref key4)).x == ((Vector2Int)(ref key5)).x)
					{
						Vector2 b = dictionary2[key5];
						DrawLine(container, a, b, Color.yellow);
					}
				}
			}
		}
		HashSet<(Vector2Int, Vector2Int)> hashSet = new HashSet<(Vector2Int, Vector2Int)>();
		Vector2Int val6 = default(Vector2Int);
		foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item6 in dictionary)
		{
			Vector2Int key6 = item6.Key;
			DungeonMapCellDetail value4 = item6.Value;
			if (!IsExplored(value4) && !IsElevatorActive(value4))
			{
				continue;
			}
			Vector2 val5 = dictionary2[key6];
			int roomWidth = GetRoomWidth(value4);
			((Vector2Int)(ref val6))._002Ector(((Vector2Int)(ref key6)).x + roomWidth, ((Vector2Int)(ref key6)).y);
			if (dictionary.TryGetValue(val6, out var value5) && (IsExplored(value5) || IsElevatorActive(value5)) && IsDoorMatch(value4, value5))
			{
				Vector2 b2 = dictionary2[val6];
				(Vector2Int, Vector2Int) item = (key6, val6);
				if (!hashSet.Contains(item))
				{
					DrawLine(container, val5, b2, Color.yellow);
					hashSet.Add(item);
				}
			}
			foreach (KeyValuePair<Vector2Int, DungeonMapCellDetail> item7 in dictionary)
			{
				Vector2Int key7 = item7.Key;
				DungeonMapCellDetail value6 = item7.Value;
				if (!IsExplored(value6) && !IsElevatorActive(value6))
				{
					continue;
				}
				int roomWidth2 = GetRoomWidth(value6);
				if (new Vector2Int(((Vector2Int)(ref key7)).x + roomWidth2, ((Vector2Int)(ref key7)).y) == key6 && IsDoorMatch(value6, value4))
				{
					Vector2 a2 = dictionary2[key7];
					(Vector2Int, Vector2Int) item2 = (key7, key6);
					if (!hashSet.Contains(item2))
					{
						DrawLine(container, a2, val5, Color.yellow);
						hashSet.Add(item2);
					}
				}
			}
		}
		foreach (DungeonMapCellDetail item8 in list)
		{
			Vector2Int val7 = ParseCellId(item8.CellId);
			Vector2 val8 = dictionary2[val7];
			bool isPlayerRoom = val.HasValue && val.Value == val7;
			float num = 16f;
			Color dotColor = GetDotColor(item8, isPlayerRoom);
			VisualElement val9 = new VisualElement();
			val9.style.position = StyleEnum<Position>.op_Implicit((Position)1);
			val9.style.left = StyleLength.op_Implicit(val8.x * 30f - num / 2f);
			val9.style.top = StyleLength.op_Implicit(FlipY(val8.y) * 30f - num / 2f);
			val9.style.width = StyleLength.op_Implicit(num);
			val9.style.height = StyleLength.op_Implicit(num);
			val9.style.backgroundColor = StyleColor.op_Implicit(dotColor);
			val9.style.borderBottomLeftRadius = new StyleLength(Length.Percent(50f));
			val9.style.borderBottomRightRadius = new StyleLength(Length.Percent(50f));
			val9.style.borderTopLeftRadius = new StyleLength(Length.Percent(50f));
			val9.style.borderTopRightRadius = new StyleLength(Length.Percent(50f));
			VisualElement val10 = val9;
			container.Add(val10);
		}
	}

	private static void TryLink(DungeonMapCellDetail cellA, Vector2Int posA, Vector2 centerA, DungeonMapCellDetail cellB, Vector2Int posB, Vector2 centerB, VisualElement container, Dictionary<Vector2Int, DungeonMapCellDetail> cellMap)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		bool num = IsExplored(cellA) || IsElevatorActive(cellA);
		bool flag = IsExplored(cellB) || IsElevatorActive(cellB);
		if ((num & flag) && IsDoorMatch(cellA, cellB))
		{
			DrawLine(container, centerA, centerB, Color.yellow);
			return;
		}
		bool num2 = IsExplored(cellA) && IsExplored(cellB) && IsDoorMatch(cellA, cellB);
		bool flag2 = (IsExplored(cellA) && IsVisible(cellB) && IsLineClear(posA, posB, cellMap)) || (IsExplored(cellB) && IsVisible(cellA) && IsLineClear(posB, posA, cellMap));
		if (num2)
		{
			DrawLine(container, centerA, centerB, Color.yellow);
		}
		else if (flag2)
		{
			DrawLine(container, centerA, centerB, Color.gray);
		}
	}

	private static bool IsLineClear(Vector2Int from, Vector2Int to, Dictionary<Vector2Int, DungeonMapCellDetail> cellMap)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.Min(((Vector2Int)(ref from)).x, ((Vector2Int)(ref to)).x);
		int num2 = Mathf.Max(((Vector2Int)(ref from)).x, ((Vector2Int)(ref to)).x);
		int y = ((Vector2Int)(ref from)).y;
		Vector2Int key = default(Vector2Int);
		for (int i = num + 1; i < num2; i++)
		{
			((Vector2Int)(ref key))._002Ector(i, y);
			if (cellMap.TryGetValue(key, out var value) && !IsExplored(value) && !IsVisible(value))
			{
				return false;
			}
		}
		return true;
	}

	private static float FlipY(float y)
	{
		return MapHeight - y;
	}

	private static Vector2Int ParseCellId(string id)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		string[] array = id.Split(':');
		return new Vector2Int(int.Parse(array[0]), int.Parse(array[1]));
	}

	private static Vector2 GetRoomCenter(Vector2Int pos, DungeonMapCellDetail cell)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int roomWidth = GetRoomWidth(cell);
		return new Vector2((float)((Vector2Int)(ref pos)).x + (float)(roomWidth - 1) / 2f, (float)((Vector2Int)(ref pos)).y);
	}

	private static int GetRoomWidth(DungeonMapCellDetail cell)
	{
		return Mathf.Clamp(cell.GridList?.Count ?? 1, 1, 3);
	}

	private static bool IsExplored(DungeonMapCellDetail cell)
	{
		if (cell.State != EDungeonMapCellState.LeftOpen && cell.State != EDungeonMapCellState.RightOpen)
		{
			return cell.State == EDungeonMapCellState.AllOpen;
		}
		return true;
	}

	private static bool IsDoorMatch(DungeonMapCellDetail left, DungeonMapCellDetail right)
	{
		bool flag = left.State == EDungeonMapCellState.RightOpen || left.State == EDungeonMapCellState.AllOpen;
		if (left.FunType == EDungeonMapCellFunType.Elevator || left.FunType == EDungeonMapCellFunType.Spawn)
		{
			flag = true;
		}
		bool flag2 = right.State == EDungeonMapCellState.LeftOpen || right.State == EDungeonMapCellState.AllOpen;
		if (right.FunType == EDungeonMapCellFunType.Elevator || right.FunType == EDungeonMapCellFunType.Spawn)
		{
			flag2 = true;
		}
		return flag & flag2;
	}

	private static Color GetDotColor(DungeonMapCellDetail cell, bool isPlayerRoom)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (isPlayerRoom)
		{
			return Color.blue;
		}
		if (IsExplored(cell) || cell.FunType == EDungeonMapCellFunType.Elevator || cell.FunType == EDungeonMapCellFunType.Spawn)
		{
			return Color.yellow;
		}
		if (cell.State == EDungeonMapCellState.Visible)
		{
			return Color.gray;
		}
		return Color.clear;
	}

	private static void DrawLine(VisualElement container, Vector2 a, Vector2 b, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Expected O, but got Unknown
		float num = a.x * 30f;
		float num2 = FlipY(a.y) * 30f;
		float num3 = b.x * 30f;
		float num4 = FlipY(b.y) * 30f;
		float num5 = Mathf.Min(num, num3);
		float num6 = Mathf.Min(num2, num4);
		float num7 = Mathf.Abs(num - num3);
		float num8 = Mathf.Abs(num2 - num4);
		VisualElement val = new VisualElement();
		val.style.position = StyleEnum<Position>.op_Implicit((Position)1);
		val.style.left = StyleLength.op_Implicit(num5);
		val.style.top = StyleLength.op_Implicit(num6);
		val.style.width = StyleLength.op_Implicit((num7 == 0f) ? 2f : num7);
		val.style.height = StyleLength.op_Implicit((num8 == 0f) ? 2f : num8);
		val.style.backgroundColor = StyleColor.op_Implicit(color);
		VisualElement val2 = val;
		container.Add(val2);
	}

	private static bool IsVisible(DungeonMapCellDetail cell)
	{
		return cell.State == EDungeonMapCellState.Visible;
	}

	private static bool IsElevatorActive(DungeonMapCellDetail cell)
	{
		if (cell.FunType == EDungeonMapCellFunType.Elevator)
		{
			return cell.State == EDungeonMapCellState.Activate;
		}
		return false;
	}
}
