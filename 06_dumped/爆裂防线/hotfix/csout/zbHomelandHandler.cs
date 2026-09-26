using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;
using zbType;

public class zbHomelandHandler
{
	public int DestroyRoomId;

	public List<ESkillType> warRoomTypeList = new List<ESkillType>();

	public int newFloorUnlock = -1;

	private long lastDataSyncTm;

	private readonly int SyncDeltaSecondTime = 180;

	private List<zbHomeFloorCsvRecord> m_sortedFloorRecords;

	private zbRoomUpgradeUtilityCsvRecord[] m_roomUpgradeUtilityRecords;

	private zbRoomUpgradeEffectCsvRecord[] m_roomUpgradeEffectRecords;

	public int[] replaceCost;

	public List<WorkerDetail> replaceList = new List<WorkerDetail>();

	public void Init()
	{
		zbHomeFloorCsvTableData records = zbDataConfig.Instance.homeFloorCsvTable.GetRecords();
		m_sortedFloorRecords = records.records.Values.ToList();
		m_sortedFloorRecords.Sort((zbHomeFloorCsvRecord a, zbHomeFloorCsvRecord b) => a.peerageId.CompareTo(b.peerageId));
		m_roomUpgradeUtilityRecords = zbDataConfig.Instance.roomUpgradeUtilityCsvTable.GetRecords().records;
		m_roomUpgradeEffectRecords = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records;
		replaceCost = zbDataConfig.Instance.homeBaseJsonTable.GetRecord()["worker"][(object)"replaceCost"].ToObject<int[]>();
	}

	public void UpdateWarRoomInfo(WarRoomInfo warRoomInfo)
	{
		zbSelf.Instance.homelandHandler.AddNewWarRoomUnlock(warRoomInfo);
		zbServerData.sGameData.WarRoomInfo = warRoomInfo;
		zbSelf.Instance.attrHandler.DataChangeWarRoom();
	}

	public void UpdateHomelandInfo(HomelandInfo homelandInfo)
	{
		CheckNewFloorUnlock(homelandInfo);
		zbServerData.sGameData.HomelandInfo = homelandInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_HomelandInfo);
		if (!zbHomeMgr.Instance.IsBuildOk())
		{
			zbHomeMgr.Instance.MarkNeedRefreshAfterBuild();
			if (zbSelf.Instance.homelandHandler.newFloorUnlock > 0)
			{
				zbHomeMgr.Instance.MarkNeedCheckFloorUnlockAfterBuild();
			}
		}
		else if (zbSelf.Instance.homelandHandler.newFloorUnlock > 0)
		{
			zbHomeMgr.Instance.floorMgr.CheakFloorUnlock();
		}
	}

	public void CheckNewFloorUnlock(HomelandInfo homelandInfo)
	{
		HomelandInfo homelandInfo2 = zbServerData.sGameData.HomelandInfo;
		newFloorUnlock = -1;
		if (homelandInfo2 == null || homelandInfo2.BaseInfo == null || homelandInfo.BaseInfo == null)
		{
			return;
		}
		Dictionary<int, EActiveState> dictionary = new Dictionary<int, EActiveState>();
		foreach (HomelandFloorDetail floor in homelandInfo2.BaseInfo.FloorList)
		{
			dictionary[floor.Floor] = floor.State;
		}
		foreach (HomelandFloorDetail floor2 in homelandInfo.BaseInfo.FloorList)
		{
			if (floor2.State == EActiveState.Done && (!dictionary.TryGetValue(floor2.Floor, out var value) || value != EActiveState.Done))
			{
				newFloorUnlock = floor2.Floor;
				break;
			}
		}
	}

	public void AddNewWarRoomUnlock(WarRoomInfo ackWarRoomInfo)
	{
		foreach (WarRoomDetail room in ackWarRoomInfo.RoomList)
		{
			RepeatedField<WarRoomDetail> val = zbServerData.sGameData.WarRoomInfo?.RoomList;
			WarRoomDetail warRoomDetail = null;
			if (val != null)
			{
				for (int i = 0; i < val.Count; i++)
				{
					if (val[i].RoomId == room.RoomId)
					{
						warRoomDetail = val[i];
						break;
					}
				}
			}
			if (warRoomDetail != null && warRoomDetail.HeroTid == 0 && room.HeroTid > 0 && !warRoomTypeList.Contains(room.SkillType))
			{
				warRoomTypeList.Add(room.SkillType);
			}
		}
	}

	public WorkerDetail GetHomelandWorkerDetail(int workerId)
	{
		foreach (WorkerDetail worker in zbServerData.sGameData.HomelandInfo.WorkerList)
		{
			if (worker.WorkerId == workerId)
			{
				return worker;
			}
		}
		return null;
	}

	public HomelandRoomDetail GetHomelandRoomDetail(int roomId)
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			if (room.RoomId == roomId)
			{
				return room;
			}
		}
		return null;
	}

	public int GetRoomCountByType(int typeId)
	{
		List<zbHomeRoomTypeCsvRecord> list = new List<zbHomeRoomTypeCsvRecord>();
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(typeId);
		foreach (zbHomeRoomTypeCsvRecord value in zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecords().records.Values)
		{
			if (recordByKey.type == value.type && recordByKey.subType == value.subType)
			{
				list.Add(value);
			}
		}
		int num = 0;
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			foreach (zbHomeRoomTypeCsvRecord item in list)
			{
				if (room.TypeId == item.id && room.Level > 0)
				{
					num += item.length;
				}
			}
		}
		return num;
	}

	public WarRoomDetail GetWarRoomDetail(int roomId)
	{
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			if (room.RoomId == roomId)
			{
				return room;
			}
		}
		return null;
	}

	public EActiveState GetFloorState(int floor)
	{
		foreach (HomelandFloorDetail floor2 in zbServerData.sGameData.HomelandInfo.BaseInfo.FloorList)
		{
			if (floor2.Floor == floor)
			{
				return floor2.State;
			}
		}
		return EActiveState.Pending;
	}

	public int GetNextActiveFloor()
	{
		foreach (HomelandFloorDetail floor in zbServerData.sGameData.HomelandInfo.BaseInfo.FloorList)
		{
			if (floor.State == EActiveState.Active)
			{
				return 0;
			}
		}
		foreach (zbHomeFloorCsvRecord sortedFloorRecord in m_sortedFloorRecords)
		{
			if (GetFloorState(sortedFloorRecord.floor) == EActiveState.Pending)
			{
				return sortedFloorRecord.floor;
			}
		}
		return 0;
	}

	public int GetCanUnlockFloor()
	{
		foreach (HomelandFloorDetail floor in zbServerData.sGameData.HomelandInfo.BaseInfo.FloorList)
		{
			if (GetFloorState(floor.Floor) == EActiveState.Active)
			{
				return floor.Floor;
			}
		}
		return 0;
	}

	public bool CanRoomUpgrade(HomelandRoomDetail roomData)
	{
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
		if (!zbVersionMgr.IsReleased(recordByKey.preShow))
		{
			return false;
		}
		if (zbServerData.sGameData.PeerageInfo.PeerageId < recordByKey.peerageId && recordByKey.peerageId < 100)
		{
			return false;
		}
		if (recordByKey.type == 1)
		{
			if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Explore))
			{
				return false;
			}
			zbRoomUpgradeUtilityCsvRecord[] records = zbDataConfig.Instance.roomUpgradeUtilityCsvTable.GetRecords().records;
			foreach (zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord2 in records)
			{
				if (zbRoomUpgradeUtilityCsvRecord2.id == roomData.TypeId && zbRoomUpgradeUtilityCsvRecord2.lv == roomData.Level + 1 && zbServerData.sGameData.PeerageInfo.PeerageId >= zbRoomUpgradeUtilityCsvRecord2.peerageId && zbSelf.Instance.dataHandler.GetCostListLackItem(zbRoomUpgradeUtilityCsvRecord2.costList) == null)
				{
					return true;
				}
			}
			return false;
		}
		if (recordByKey.type == 2 && roomData.Level > 0)
		{
			if (roomData.Level >= zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecords().records.Count)
			{
				return false;
			}
			zbRoomUpgradeWarCsvRecord recordByKey2 = zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecordByKey(roomData.Level + 1);
			if (recordByKey2 != null && zbSelf.Instance.GetLevel() >= recordByKey2.playerLv)
			{
				return HasCostListWithExtraItemEnough(recordByKey2.costList, recordByKey.itemId, recordByKey2.costItemNum);
			}
			return false;
		}
		if (recordByKey.type == 3)
		{
			zbRoomUpgradeEffectCsvRecord[] records2 = zbDataConfig.Instance.roomUpgradeEffectCsvTable.GetRecords().records;
			foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 in records2)
			{
				if (zbRoomUpgradeEffectCsvRecord2.id == recordByKey.length && zbRoomUpgradeEffectCsvRecord2.lv == roomData.Level + 1 && zbSelf.Instance.GetLevel() >= zbRoomUpgradeEffectCsvRecord2.playerLv && HasCostListWithExtraItemEnough(zbRoomUpgradeEffectCsvRecord2.costList, recordByKey.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool CanRoomRepaire()
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
			if (room.Level == 0 && CanRoomUpgrade(room) && (recordByKey == null || recordByKey.type != 2))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanRoomUpgradeFast()
	{
		foreach (HomelandRoomDetail room in zbServerData.sGameData.HomelandInfo.RoomList)
		{
			zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(room.TypeId);
			if (room.Level > 0 && CanRoomUpgrade(room) && recordByKey != null && recordByKey.type == 3)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateCostUI(int[][] costLists, Label[] costLabels, zbItem[] itemViews)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		if (costLists == null)
		{
			return;
		}
		for (int i = 0; i < costLists.Length; i++)
		{
			if (i >= costLabels.Length || i >= itemViews.Length)
			{
				break;
			}
			int resourceType = costLists[i][0];
			int itemTid = costLists[i][1];
			int requiredCount = costLists[i][2];
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)resourceType, itemTid);
			((VisualElement)costLabels[i]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)0);
			((TextElement)costLabels[i]).enableRichText = true;
			((TextElement)costLabels[i]).text = zbUtil.GetNeedHaveRichString(resourceCount, requiredCount);
			itemViews[i].Refresh(new zbItemData((EResourceType)resourceType, itemTid, requiredCount), isNameVis: false, isCountVis: false);
			if (requiredCount > resourceCount)
			{
				itemViews[i].SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip((EResourceType)resourceType, itemTid, requiredCount);
				});
			}
			else
			{
				itemViews[i].SetTouchCallback(delegate
				{
					zbUIMgr.Instance.ShowTip((EResourceType)resourceType, itemTid, 0L);
				});
			}
		}
		for (int num = costLists.Length; num < costLabels.Length; num++)
		{
			((VisualElement)costLabels[num]).style.display = StyleEnum<DisplayStyle>.op_Implicit((DisplayStyle)1);
		}
	}

	public int[][] AppendExtraCostItem(int[][] baseCostList, int extraItemId, int extraItemCount, int resourceType = 3)
	{
		if (baseCostList == null)
		{
			baseCostList = new int[0][];
		}
		int[][] array = new int[baseCostList.Length + 1][];
		Array.Copy(baseCostList, array, baseCostList.Length);
		array[baseCostList.Length] = new int[3] { resourceType, extraItemId, extraItemCount };
		return array;
	}

	public bool HasCostListWithExtraItemEnough(int[][] baseCostList, int extraItemId, int extraItemCount, int resourceType = 3)
	{
		zbDataHandler dataHandler = zbSelf.Instance.dataHandler;
		if (baseCostList != null)
		{
			foreach (int[] array in baseCostList)
			{
				if (array != null && array.Length >= 3 && !dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
				{
					return false;
				}
			}
		}
		return dataHandler.HasResourceEnough((EResourceType)resourceType, extraItemId, extraItemCount);
	}

	public List<ConditionCheckResult> GetRoomUpgradeConditions(HomelandRoomDetail roomData)
	{
		List<ConditionCheckResult> list = new List<ConditionCheckResult>();
		zbHomeRoomTypeCsvRecord recordByKey = zbDataConfig.Instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
		if (IsRoomMaxLevel(roomData))
		{
			list.Add(CheckIsMaxLevel());
			return list;
		}
		int peerageId = zbServerData.sGameData.PeerageInfo.PeerageId;
		if (roomData.Level == 0)
		{
			zbPeerageCsvRecord recordByKey2 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(recordByKey.peerageId);
			string text = ((recordByKey2 != null) ? recordByKey2.name : zbUtil.Language("lang.10195"));
			list.Add(new ConditionCheckResult(zbUtil.Language("lang.10194") + ":" + text, peerageId >= recordByKey.peerageId));
		}
		int num = roomData.Level + 1;
		switch ((EHomeRoomTypeV2)recordByKey.type)
		{
		case EHomeRoomTypeV2.Utility:
		{
			zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord2 = null;
			for (int k = 0; k < m_roomUpgradeUtilityRecords.Length; k++)
			{
				zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord3 = m_roomUpgradeUtilityRecords[k];
				if (zbRoomUpgradeUtilityCsvRecord3.id == roomData.TypeId && zbRoomUpgradeUtilityCsvRecord3.lv == num)
				{
					zbRoomUpgradeUtilityCsvRecord2 = zbRoomUpgradeUtilityCsvRecord3;
					break;
				}
			}
			if (zbRoomUpgradeUtilityCsvRecord2 != null)
			{
				zbPeerageCsvRecord recordByKey3 = zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(zbRoomUpgradeUtilityCsvRecord2.peerageId);
				string text2 = zbUtil.Language("lang.10195");
				string text3 = ((recordByKey3 != null) ? recordByKey3.name : text2);
				string description = zbUtil.Language("lang.10194") + ":" + text3;
				list.Add(new ConditionCheckResult(description, peerageId >= zbRoomUpgradeUtilityCsvRecord2.peerageId));
				list.Add(CheckMaterials(zbRoomUpgradeUtilityCsvRecord2.costList));
			}
			break;
		}
		case EHomeRoomTypeV2.WarRoom:
		{
			zbRoomUpgradeWarCsvRecord recordByKey4 = zbDataConfig.Instance.roomUpgradeWarCsvTable.GetRecordByKey(num);
			if (recordByKey4 != null)
			{
				list.Add(CheckPlayerLevel(recordByKey4.playerLv));
				List<int[]> list2 = recordByKey4.costList.ToList();
				list2.Add(new int[3] { 3, recordByKey.itemId, recordByKey4.costItemNum });
				list.Add(CheckMaterials(list2.ToArray()));
			}
			break;
		}
		case EHomeRoomTypeV2.Effect:
		{
			zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 = null;
			zbRoomUpgradeEffectCsvRecord[] roomUpgradeEffectRecords = m_roomUpgradeEffectRecords;
			foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord3 in roomUpgradeEffectRecords)
			{
				if (zbRoomUpgradeEffectCsvRecord3.id == recordByKey.length && zbRoomUpgradeEffectCsvRecord3.lv == num)
				{
					zbRoomUpgradeEffectCsvRecord2 = zbRoomUpgradeEffectCsvRecord3;
					break;
				}
			}
			if (zbRoomUpgradeEffectCsvRecord2 != null)
			{
				list.Add(CheckPlayerLevel(zbRoomUpgradeEffectCsvRecord2.playerLv));
				int[][] costList = zbRoomUpgradeEffectCsvRecord2.costList;
				int num2 = costList.Length;
				int[][] array = new int[num2 + 1][];
				for (int j = 0; j < num2; j++)
				{
					array[j] = costList[j];
				}
				array[num2] = new int[3] { 3, recordByKey.itemId, zbRoomUpgradeEffectCsvRecord2.costItemNum };
				list.Add(CheckMaterials(array));
			}
			break;
		}
		default:
			list.Add(new ConditionCheckResult("未知房间类型，无法判断升级条件", isMet: false));
			break;
		}
		return list;
	}

	public void ShowUpgradeConditions(List<ConditionCheckResult> conditions, Label lb)
	{
		ConditionCheckResult conditionCheckResult = null;
		ConditionCheckResult conditionCheckResult2 = null;
		for (int i = 0; i < conditions.Count; i++)
		{
			ConditionCheckResult conditionCheckResult3 = conditions[i];
			if (conditionCheckResult3.ShowInUI && !string.IsNullOrEmpty(conditionCheckResult3.Description))
			{
				if (conditionCheckResult == null)
				{
					conditionCheckResult = conditionCheckResult3;
				}
				if (!conditionCheckResult3.IsMet)
				{
					conditionCheckResult2 = conditionCheckResult3;
					break;
				}
			}
		}
		ConditionCheckResult conditionCheckResult4 = conditionCheckResult2 ?? conditionCheckResult;
		if (conditionCheckResult4 == null)
		{
			((TextElement)lb).text = string.Empty;
			return;
		}
		string arg = (conditionCheckResult4.IsMaxLevel ? "#1B8231" : (conditionCheckResult4.IsMet ? "#008415" : "#C50000"));
		int num = (conditionCheckResult4.IsMaxLevel ? 25 : 20);
		((TextElement)lb).text = $"<size={num}><color={arg}>{conditionCheckResult4.Description}</color></size>";
	}

	private ConditionCheckResult CheckPlayerLevel(int requiredLv)
	{
		int level = zbSelf.Instance.GetLevel();
		return new ConditionCheckResult(string.Format("{0}:{1}（{2}:{3}）", new object[4]
		{
			zbUtil.Language("lang.10192"),
			requiredLv,
			zbUtil.Language("lang.10193"),
			level
		}), level >= requiredLv);
	}

	private ConditionCheckResult CheckMaterials(int[][] costList)
	{
		zbItemData costListLackItem = zbSelf.Instance.dataHandler.GetCostListLackItem(costList);
		bool flag = costListLackItem == null;
		if (flag)
		{
			return new ConditionCheckResult(flag ? zbUtil.Language("lang.10189") : zbUtil.Language("lang.10190"), flag, null, isMaxLevel: false, showInUI: false);
		}
		return new ConditionCheckResult(flag ? zbUtil.Language("lang.10189") : zbUtil.Language("lang.10190"), flag, costListLackItem, isMaxLevel: false, showInUI: false);
	}

	private ConditionCheckResult CheckIsMaxLevel()
	{
		return new ConditionCheckResult(zbUtil.Language("lang.10188"), isMet: false, null, isMaxLevel: true);
	}

	private bool IsRoomMaxLevel(HomelandRoomDetail roomData)
	{
		zbDataConfig instance = zbDataConfig.Instance;
		zbHomeRoomTypeCsvRecord recordByKey = instance.homeRoomTypeCsvTable.GetRecordByKey(roomData.TypeId);
		if (recordByKey == null)
		{
			return false;
		}
		int num = roomData.Level + 1;
		switch ((EHomeRoomTypeV2)recordByKey.type)
		{
		case EHomeRoomTypeV2.Utility:
		{
			zbRoomUpgradeUtilityCsvRecord[] records2 = instance.roomUpgradeUtilityCsvTable.GetRecords().records;
			foreach (zbRoomUpgradeUtilityCsvRecord zbRoomUpgradeUtilityCsvRecord2 in records2)
			{
				if (zbRoomUpgradeUtilityCsvRecord2.id == roomData.TypeId && zbRoomUpgradeUtilityCsvRecord2.lv == num)
				{
					return false;
				}
			}
			return true;
		}
		case EHomeRoomTypeV2.WarRoom:
			return instance.roomUpgradeWarCsvTable.GetRecordByKey(num) == null;
		case EHomeRoomTypeV2.Effect:
		{
			zbRoomUpgradeEffectCsvRecord[] records = instance.roomUpgradeEffectCsvTable.GetRecords().records;
			int length = recordByKey.length;
			foreach (zbRoomUpgradeEffectCsvRecord zbRoomUpgradeEffectCsvRecord2 in records)
			{
				if (zbRoomUpgradeEffectCsvRecord2.id == length && zbRoomUpgradeEffectCsvRecord2.lv == num)
				{
					return false;
				}
			}
			return true;
		}
		default:
			return false;
		}
	}

	public void PositionByHomeRoomId(int roomId)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Camera mainCamera = zbCameraHelper.Instance.mainCamera;
		zbRoom room = zbHomeMgr.Instance.roomMgr.GetRoom(roomId);
		if (!((Object)(object)room == (Object)null) && !room.IsNotOpen())
		{
			((Component)mainCamera).transform.position = new Vector3(room.GetCenterPos().x, room.GetCenterPos().y, ((Component)mainCamera).transform.position.z);
		}
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Homeland))
		{
			return false;
		}
		if (!CanRoomRepaire() && !CanRoomUpgradeFast())
		{
			return zbSelf.Instance.redPointHandler.HasRedPoint(ERedPointType.Monster);
		}
		return true;
	}

	public void Update()
	{
		if (!zbSelf.Instance.IsLoading() && !zbGameStateMachine.Instance.IsFighting())
		{
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			if (serverTime > lastDataSyncTm)
			{
				zbWsMsgHandler.Instance.HomelandV2_DataSync_Req();
				lastDataSyncTm = serverTime + SyncDeltaSecondTime;
			}
		}
	}
}
