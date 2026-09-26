using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbHomeEditHandler
{
	public zbHomeRoomTypeCsvRecord createRoomRecord;

	public int[][] createRoomCostList;

	public int curEditRoomId;

	private Dictionary<int, Stack<(int x, int y)>> roomEditHistories = new Dictionary<int, Stack<(int, int)>>();

	public List<HomelandRoomDetail> roomList = new List<HomelandRoomDetail>();

	public List<HomelandRoomDetail> roomBagList = new List<HomelandRoomDetail>();

	public void Init()
	{
		curEditRoomId = 0;
		roomList.Clear();
		roomBagList.Clear();
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			HomelandRoomDetail homelandRoomDetail = new HomelandRoomDetail();
			homelandRoomDetail.RoomId = room.RoomId;
			homelandRoomDetail.TypeId = room.TypeId;
			homelandRoomDetail.X = room.X;
			homelandRoomDetail.Y = room.Y;
			homelandRoomDetail.Level = room.Level;
			roomList.Add(homelandRoomDetail);
		}
	}

	public HomelandRoomDetail GetCurEditRoomData()
	{
		if (curEditRoomId == 0)
		{
			return null;
		}
		foreach (HomelandRoomDetail room in roomList)
		{
			if (room.RoomId == curEditRoomId)
			{
				return room;
			}
		}
		foreach (HomelandRoomDetail roomBag in roomBagList)
		{
			if (roomBag.RoomId == curEditRoomId)
			{
				return roomBag;
			}
		}
		return null;
	}

	public void StartEditRoom(HomelandRoomDetail room)
	{
		if (room != null)
		{
			int roomId = room.RoomId;
			if (!roomEditHistories.ContainsKey(roomId))
			{
				roomEditHistories[roomId] = new Stack<(int, int)>();
			}
			roomEditHistories[roomId].Clear();
			curEditRoomId = roomId;
		}
	}

	public void RecordPositionBeforeChange(int x, int y)
	{
		if (curEditRoomId == 0)
		{
			return;
		}
		HomelandRoomDetail curEditRoomData = GetCurEditRoomData();
		if (curEditRoomData == null)
		{
			return;
		}
		int roomId = curEditRoomData.RoomId;
		if (!roomEditHistories.ContainsKey(roomId))
		{
			roomEditHistories[roomId] = new Stack<(int, int)>();
		}
		Stack<(int, int)> stack = roomEditHistories[roomId];
		(int, int) tuple = (curEditRoomData.X, curEditRoomData.Y);
		if (stack.Count != 0)
		{
			(int, int) tuple2 = stack.Peek();
			(int, int) tuple3 = tuple;
			if (tuple2.Item1 == tuple3.Item1 && tuple2.Item2 == tuple3.Item2)
			{
				return;
			}
		}
		stack.Push(tuple);
	}

	public bool UndoLastEdit(HomelandRoomDetail room)
	{
		if (room == null || !roomEditHistories.TryGetValue(room.RoomId, out Stack<(int, int)> value) || value.Count == 0)
		{
			return false;
		}
		var (x, y) = value.Pop();
		UpdateRoomDetail(x, y);
		return true;
	}

	public void ClearHistory(int roomId)
	{
		if (roomId > 0)
		{
			roomEditHistories.Remove(roomId);
		}
	}

	public void AddToBag(HomelandRoomDetail room)
	{
		roomBagList.Add(room);
		for (int num = roomList.Count - 1; num >= 0; num--)
		{
			HomelandRoomDetail homelandRoomDetail = roomList[num];
			if (room.RoomId == homelandRoomDetail.RoomId)
			{
				roomList.RemoveAt(num);
				break;
			}
		}
		curEditRoomId = 0;
	}

	public void RemoveFromBag(HomelandRoomDetail room)
	{
		roomList.Add(room);
		for (int num = roomBagList.Count - 1; num >= 0; num--)
		{
			HomelandRoomDetail homelandRoomDetail = roomBagList[num];
			if (room.RoomId == homelandRoomDetail.RoomId)
			{
				roomBagList.RemoveAt(num);
				break;
			}
		}
	}

	public void UpdateRoomDetail(int x, int y)
	{
		foreach (HomelandRoomDetail room in roomList)
		{
			if (room.RoomId == curEditRoomId)
			{
				room.X = x;
				room.Y = y;
				break;
			}
		}
	}

	public bool IsInEditBag(int roomId)
	{
		foreach (HomelandRoomDetail roomBag in roomBagList)
		{
			if (roomBag.RoomId == roomId)
			{
				return true;
			}
		}
		return false;
	}

	public HomelandRoomEditDetail[] GetEditedRoomList()
	{
		List<HomelandRoomEditDetail> list = new List<HomelandRoomEditDetail>();
		foreach (HomelandRoomDetail room in roomList)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(room.RoomId);
			if (homelandRoomDetail != null && (room.X != homelandRoomDetail.X || room.Y != homelandRoomDetail.Y))
			{
				HomelandRoomEditDetail homelandRoomEditDetail = new HomelandRoomEditDetail();
				homelandRoomEditDetail.RoomId = room.RoomId;
				homelandRoomEditDetail.X = room.X;
				homelandRoomEditDetail.Y = room.Y;
				list.Add(homelandRoomEditDetail);
			}
		}
		return list.ToArray();
	}

	public HomelandRoomEditDetail[] GetAllRoomList()
	{
		List<HomelandRoomEditDetail> list = new List<HomelandRoomEditDetail>();
		foreach (HomelandRoomDetail room in roomList)
		{
			HomelandRoomEditDetail homelandRoomEditDetail = new HomelandRoomEditDetail();
			homelandRoomEditDetail.RoomId = room.RoomId;
			homelandRoomEditDetail.X = room.X;
			homelandRoomEditDetail.Y = room.Y;
			list.Add(homelandRoomEditDetail);
		}
		return list.ToArray();
	}

	public HomelandRoomDetail GetHomelandRoomDetail(int x, int y)
	{
		foreach (HomelandRoomDetail room in roomList)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
			if (recordByKey != null && room.Y == y && x >= room.X && x <= room.X + recordByKey.length - 1)
			{
				return room;
			}
		}
		return null;
	}

	private bool CanRoomEdited(int x, int y, HomelandRoomDetail roomData)
	{
		if (GetHomelandRoomDetail(x, y) == null || GetHomelandRoomDetail(x, y) == roomData)
		{
			return true;
		}
		return false;
	}

	public int GetEditedRoomX(int roomX, int roomY)
	{
		HomelandRoomDetail homelandRoomDetail = null;
		foreach (HomelandRoomDetail room in roomList)
		{
			if (room.RoomId == curEditRoomId)
			{
				homelandRoomDetail = room;
			}
		}
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(homelandRoomDetail.TypeId);
		if (recordByKey == null)
		{
			return 0;
		}
		switch (recordByKey.length)
		{
		case 1:
			return roomX;
		case 2:
		{
			bool flag2 = (roomX <= 2 || roomX >= 4) && (GetHomelandRoomDetail(roomX + 1, roomY) == null || GetHomelandRoomDetail(roomX + 1, roomY) == homelandRoomDetail);
			bool flag3 = ((roomX >= 2 && roomX <= 3) || (roomX >= 5 && roomX <= 6)) && GetHomelandRoomDetail(roomX - 1, roomY) == null;
			if (flag2 && roomX == 6)
			{
				return 5;
			}
			if (!flag2)
			{
				if (!flag3)
				{
					return 0;
				}
				return roomX - 1;
			}
			return roomX;
		}
		case 3:
		{
			bool num = roomX >= 1 && roomX <= 3 && GetHomelandRoomDetail(1, roomY) == null && GetHomelandRoomDetail(2, roomY) == null && GetHomelandRoomDetail(3, roomY) == null;
			bool flag = roomX >= 4 && roomX <= 6 && GetHomelandRoomDetail(4, roomY) == null && GetHomelandRoomDetail(5, roomY) == null && GetHomelandRoomDetail(6, roomY) == null;
			if (num)
			{
				return 1;
			}
			if (flag)
			{
				return 4;
			}
			break;
		}
		}
		return 0;
	}

	public zbRoomCreateData[] GetCanCreateRoomList(List<(int x, int y)> emptyRoomXYs)
	{
		List<zbRoomCreateData> list = new List<zbRoomCreateData>();
		if (createRoomRecord != null)
		{
			foreach (var emptyRoomXY in emptyRoomXYs)
			{
				if (zbHomeMgr.Instance.roomMgr.IsMustNull(emptyRoomXY.x, emptyRoomXY.y) || zbSelf.Instance.homelandHandler.GetFloorState(emptyRoomXY.y) != EActiveState.Done)
				{
					continue;
				}
				if (createRoomRecord.length == 1)
				{
					list.Add(new zbRoomCreateData(createRoomRecord, emptyRoomXY.x, emptyRoomXY.y));
				}
				else if (createRoomRecord.length == 2)
				{
					for (int i = 1; i <= 2; i++)
					{
						if (IsEmptyRoom(emptyRoomXYs, i, emptyRoomXY.y) && IsEmptyRoom(emptyRoomXYs, i + 1, emptyRoomXY.y))
						{
							list.Add(new zbRoomCreateData(createRoomRecord, i, emptyRoomXY.y));
						}
					}
					for (int j = 3; j <= 4; j++)
					{
						if (IsEmptyRoom(emptyRoomXYs, j, emptyRoomXY.y) && IsEmptyRoom(emptyRoomXYs, j + 1, emptyRoomXY.y))
						{
							list.Add(new zbRoomCreateData(createRoomRecord, j, emptyRoomXY.y));
						}
					}
				}
				else
				{
					if (createRoomRecord.length != 3)
					{
						continue;
					}
					int[] array = new int[2] { 1, 4 };
					foreach (int num in array)
					{
						if (IsEmptyRoom(emptyRoomXYs, num, emptyRoomXY.y) && IsEmptyRoom(emptyRoomXYs, num + 1, emptyRoomXY.y) && IsEmptyRoom(emptyRoomXYs, num + 2, emptyRoomXY.y))
						{
							list.Add(new zbRoomCreateData(createRoomRecord, num, emptyRoomXY.y));
						}
					}
				}
			}
		}
		return list.ToArray();
	}

	private bool IsEmptyRoom(List<(int x, int y)> emptyRoomXYs, int x, int y)
	{
		foreach (var emptyRoomXY in emptyRoomXYs)
		{
			if (y == emptyRoomXY.y && x == emptyRoomXY.x)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanRoomCreate()
	{
		if (HaveEmptyRoom() && RoomCreateResourceEnough())
		{
			return true;
		}
		return false;
	}

	public bool RoomCreateResourceEnough()
	{
		foreach (zbHomeRoomTypeCsvRecord value in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			if (value.sortIndex <= 0 || zbServerData.sGameData.PeerageInfo.PeerageId < value.peerageId)
			{
				continue;
			}
			zbRoomUpgradeEffectCsvRecord[] records = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records;
			foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 in records)
			{
				if (zbRoomUpgradeEffectCsvRecord2.id != value.length || zbRoomUpgradeEffectCsvRecord2.lv != 1)
				{
					continue;
				}
				int[][] array = zbSelf.Instance.homelandHandler.AppendExtraCostItem(zbRoomUpgradeEffectCsvRecord2.costList, value.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum);
				bool flag = true;
				int[][] array2 = array;
				foreach (int[] array3 in array2)
				{
					if (zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)array3[0], array3[1]) < array3[2])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HaveEmptyRoom()
	{
		List<(int, int)> list = new List<(int, int)>();
		for (int num = 3; num >= -8; num--)
		{
			for (int i = 1; i <= 6; i++)
			{
				if (zbHomeMgr.Instance.roomMgr != null && !zbHomeMgr.Instance.roomMgr.IsMustNull(i, num) && !zbHomeMgr.Instance.roomMgr.IsExistRoom(i, num))
				{
					list.Add((i, num));
				}
			}
		}
		foreach (var item in list)
		{
			if (!zbHomeMgr.Instance.roomMgr.IsMustNull(item.Item1, item.Item2) && zbSelf.Instance.homelandHandler.GetFloorState(item.Item2) == EActiveState.Done)
			{
				return true;
			}
		}
		return false;
	}
}
