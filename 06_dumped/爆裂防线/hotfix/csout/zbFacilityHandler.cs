using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbFacilityHandler
{
	public bool isFirstRecommend = true;

	public FacilityBaseInfo[] recommendList;

	public FacilityMapInfo facilityMapInfo;

	public Dictionary<int, zbFacilityCsvRecord> records;

	private int[] m_createItemIds;

	public int tiliCost;

	private int limitCount;

	public int curRoomId;

	public FacilityRoomDetail triggerAckRoomDetail;

	public AwardDetail[] triggerAckAwardList;

	public FacilityShopInfo triggerAckShopInfo;

	public FacilityEventMonsterAck triggerAckMonster;

	public JObject facilityBaseRecord { get; set; }

	public void Init()
	{
		records = zbDataConfig.Instance.facilityCsvTable.GetRecords().records;
		m_createItemIds = records.Values.Select((zbFacilityCsvRecord r) => r.createCostItem).ToArray();
		facilityBaseRecord = zbDataConfig.Instance.facilityBaseJsonTable.GetRecord();
		tiliCost = Extensions.Value<int>((IEnumerable<JToken>)facilityBaseRecord["roomExplore"][(object)"tiliCost"]);
		limitCount = Extensions.Value<int>((IEnumerable<JToken>)facilityBaseRecord["roomExplore"][(object)"limitCount"]);
	}

	public void UpdateFacilitySelfInfo(FacilitySelfInfo facilitySelfInfo)
	{
		zbServerData.sGameData.FacilitySelfInfo = facilitySelfInfo;
	}

	public void UpdateFacilityMapInfo(FacilityMapInfo _facilityMapInfo)
	{
		facilityMapInfo = _facilityMapInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
	}

	public void ResetFacilityTrigger()
	{
		triggerAckRoomDetail = null;
		triggerAckAwardList = null;
		triggerAckShopInfo = null;
		triggerAckMonster = null;
	}

	public void UpdateFacilityTrigger(FacilityRoomDetail ackRoomDetail, AwardDetail[] ackAwardList, FacilityShopInfo ackShopInfo = null, FacilityEventMonsterAck ackMonster = null)
	{
		triggerAckRoomDetail = ackRoomDetail;
		triggerAckAwardList = ackAwardList;
		triggerAckShopInfo = ackShopInfo;
		triggerAckMonster = ackMonster;
	}

	public void UpdateFacilityRoomDetail(FacilityRoomDetail roomDetail)
	{
		if (facilityMapInfo == null || facilityMapInfo.RoomList == null)
		{
			return;
		}
		for (int i = 0; i < facilityMapInfo.RoomList.Count; i++)
		{
			if (facilityMapInfo.RoomList[i].RoomId == roomDetail.RoomId)
			{
				facilityMapInfo.RoomList[i] = roomDetail;
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_Facility);
				break;
			}
		}
	}

	public FacilityRoomDetail GetFacilityRoomDetail(int roomId)
	{
		foreach (FacilityRoomDetail room in facilityMapInfo.RoomList)
		{
			if (room.RoomId == roomId)
			{
				return room;
			}
		}
		return null;
	}

	public int GetLeftShareRoomCount(zbFacilityCsvRecord record, FacilityBaseInfo baseInfo)
	{
		int val = record.tierTarget3 - baseInfo.ExploreTotal;
		int val2 = Math.Max(record.shareCount - baseInfo.ExploreShare, 0);
		return Math.Min(val, val2);
	}

	public (int[], int) GetCurrentGirdSpec(zbFacilityCsvRecord record, int currentRoom)
	{
		int num = 0;
		int num2 = 0;
		int[][] gridSpec = record.gridSpec;
		foreach (int[] array in gridSpec)
		{
			num2 = array[0] * array[1] + num2;
			if (currentRoom > num2)
			{
				num++;
			}
		}
		int num3 = 1;
		if (num > 0)
		{
			for (int j = 0; j < num; j++)
			{
				num3 += record.gridSpec[j][0] * record.gridSpec[j][1];
			}
		}
		return (record.gridSpec[num], num3);
	}

	public (int, int) GetFloorAndAllFloor(zbFacilityCsvRecord record, int roomId)
	{
		int num = 0;
		int num2 = 0;
		int item = record.gridSpec.Length;
		int[][] gridSpec = record.gridSpec;
		foreach (int[] array in gridSpec)
		{
			num2 = array[0] * array[1] + num2;
			if (roomId > num2)
			{
				num++;
			}
		}
		return (num, item);
	}

	public bool CanAroundRoomExplore(int[] gridSpec, int startRoomId, int roomId)
	{
		if (IsEntranceStair(roomId))
		{
			return true;
		}
		int num = gridSpec[0];
		int num2 = gridSpec[1];
		int num3 = roomId - startRoomId;
		int num4 = num3 % num;
		int num5 = num3 / num;
		int[,] array = new int[8, 2]
		{
			{ -1, 0 },
			{ 1, 0 },
			{ 0, -1 },
			{ 0, 1 },
			{ -1, -1 },
			{ 1, -1 },
			{ -1, 1 },
			{ 1, 1 }
		};
		for (int i = 0; i < array.GetLength(0); i++)
		{
			int num6 = num4 + array[i, 0];
			int num7 = num5 + array[i, 1];
			if (num6 >= 0 && num6 < num && num7 >= 0 && num7 < num2)
			{
				int roomId2 = startRoomId + num6 + num7 * num;
				FacilityRoomDetail facilityRoomDetail = GetFacilityRoomDetail(roomId2);
				if ((facilityRoomDetail != null && facilityRoomDetail.RoomState > EFacilityRoomState.Ready) || IsEntranceStair(roomId2))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEntranceStair(int roomId)
	{
		foreach (FacilityRoomDetail room in facilityMapInfo.RoomList)
		{
			if (roomId == room.RoomId && room.RoomType == EFacilityRoomType.EntranceStair)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCanExploreCount()
	{
		if (facilityMapInfo.BaseInfo.FacilityCreator.RoleId == zbSelf.Instance.GetRoleId())
		{
			return zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(facilityMapInfo.BaseInfo.FacilityType).creatorExploreLimit - facilityMapInfo.ExploreCount;
		}
		return limitCount - facilityMapInfo.ExploreCount;
	}

	public (int, int) GetCanExploreAndAllCount()
	{
		int num = 0;
		foreach (FacilityRoomDetail room in facilityMapInfo.RoomList)
		{
			if (room.RoomState == EFacilityRoomState.Locked || room.RoomState == EFacilityRoomState.Ready)
			{
				num++;
			}
		}
		return (num, facilityMapInfo.RoomList.Count);
	}

	public bool HasFacility()
	{
		if (zbServerData.sGameData.FacilitySelfInfo != null && zbServerData.sGameData.FacilitySelfInfo.Create != null && zbServerData.sGameData.FacilitySelfInfo.Create.FacilityType > 0)
		{
			return zbServerData.sGameData.FacilitySelfInfo.Create.ExpireTm > zbSelf.Instance.timeHandler.GetServerTime();
		}
		return false;
	}

	public int[][] GetMainTabChallengeAwardList()
	{
		List<int[]> list = new List<int[]>();
		int[] array = new int[2] { 4, 2 };
		foreach (int key in array)
		{
			zbFacilityCsvRecord recordByKey = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(key);
			int[][] tierAwardList = recordByKey.tierAwardList3;
			foreach (int[] item in tierAwardList)
			{
				list.Add(item);
			}
			tierAwardList = recordByKey.tierAwardList2;
			foreach (int[] item2 in tierAwardList)
			{
				list.Add(item2);
			}
			tierAwardList = recordByKey.tierAwardList1;
			foreach (int[] item3 in tierAwardList)
			{
				list.Add(item3);
			}
		}
		return list.ToArray();
	}

	public bool IsFacilityOpen()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree != null && zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.StartTm > 0 && zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.StartTm)
		{
			return true;
		}
		return false;
	}

	public bool HasTargetNotify()
	{
		FacilityBaseInfo facilityBaseInfo = null;
		if (HasFacility())
		{
			facilityBaseInfo = zbServerData.sGameData.FacilitySelfInfo.Create;
		}
		if (facilityMapInfo != null && facilityMapInfo.BaseInfo.FacilityCreator.RoleId == zbSelf.Instance.GetRoleId() && ((facilityBaseInfo != null && facilityBaseInfo.ExploreTotal < facilityMapInfo.BaseInfo.ExploreTotal) || facilityBaseInfo == null))
		{
			facilityBaseInfo = facilityMapInfo.BaseInfo;
		}
		if (facilityBaseInfo != null)
		{
			zbFacilityCsvRecord recordByKey = zbDataConfig.Instance.facilityCsvTable.GetRecordByKey(facilityBaseInfo.FacilityType);
			if (recordByKey != null && facilityBaseInfo.ExploreTotal >= recordByKey.tierTarget3)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasCreateNotify()
	{
		if (!HasFacility() && zbSelf.Instance.moneyHandler.HasEnoughMoney(4, tiliCost * 10))
		{
			int[] createItemIds = m_createItemIds;
			foreach (int tid in createItemIds)
			{
				if (zbSelf.Instance.itemHandler.GetCountByItemTid(tid) > 0)
				{
					return true;
				}
			}
		}
		return HasTargetNotify();
	}

	public bool HasSearchNotify()
	{
		if ((isFirstRecommend || recommendList.Length != 0) && zbSelf.Instance.moneyHandler.HasEnoughMoney(4, tiliCost * 2))
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint(bool isMainWnd = true)
	{
		if (!IsFacilityOpen() || zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Facility))
		{
			return false;
		}
		if (isMainWnd && zbSelf.Instance.journeyThreeHandler.IsActive())
		{
			return false;
		}
		if (zbSelf.Instance.journeyThreeHandler.HasZhanLingNotify())
		{
			return true;
		}
		if (!HasCreateNotify())
		{
			return HasSearchNotify();
		}
		return true;
	}
}
