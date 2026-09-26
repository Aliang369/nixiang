using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbDungeonHandler
{
	public DungeonBaseInfo dungeonBaseInfo;

	public DungeonMapInfo dungeonMapInfo;

	public DungeonAwardInfo dungeonAwardInfo;

	public DungeonResultInfo dungeonResultInfo;

	public AwardDetail[] dungeonAwardList;

	public DungeonLogInfo dungeonLogInfo;

	public BattleInfo triggerBattleInfo;

	public bool isFirstPassPop;

	public void Init()
	{
		dungeonBaseInfo = zbServerData.sGameData.WorldInfo.DungeonBaseInfo;
		isFirstPassPop = false;
	}

	public void OpenDungeon(int gridId, EDifficultyLevel dungeonLevel)
	{
		isFirstPassPop = false;
		if (dungeonBaseInfo == null || dungeonBaseInfo.PassState == EPassState.NotStarted)
		{
			WorldReconDetail worldReconDetailByGridId = zbSelf.Instance.worldHandler.GetWorldReconDetailByGridId(gridId);
			if (worldReconDetailByGridId != null)
			{
				zbSelf.Instance.worldHandler.curReconId = worldReconDetailByGridId.ReconId;
			}
			zbUIMgr.Instance.ShowDungeonFormation(this, EDungeonType.World, gridId, dungeonLevel);
		}
		else if (dungeonBaseInfo.PassState == EPassState.InProgress)
		{
			zbWsMsgHandler.Instance.WorldDungeon_Continue_Req();
		}
		else if (dungeonBaseInfo.PassState == EPassState.Failed || dungeonBaseInfo.PassState == EPassState.Passed)
		{
			zbUIMgr.Instance.ShowDungeonExit(this);
		}
	}

	public void OnWorldDungeonOpenAck(DungeonBaseInfo baseInfo, DungeonMapInfo mapInfo, DungeonAwardInfo awardInfo)
	{
		UpDateDungeonMapInfo(baseInfo, mapInfo, awardInfo);
		zbUIMgr.Instance.ShowDungeon(this);
	}

	public bool IsFightEvent(EDungeonEventType type)
	{
		if (type != EDungeonEventType.EasyFight && type != EDungeonEventType.RiotFight)
		{
			return type == EDungeonEventType.BossFight;
		}
		return true;
	}

	public void OnDungeonPassStateCheck()
	{
		if (dungeonBaseInfo != null)
		{
			bool flag = false;
			if (dungeonBaseInfo.PassState == EPassState.Passed || dungeonBaseInfo.TriggerCount == dungeonBaseInfo.EventCount)
			{
				flag = true;
			}
			if (dungeonBaseInfo.PassState == EPassState.Failed)
			{
				zbUIMgr.Instance.ShowDungeonExit(this);
			}
			else if (flag && !isFirstPassPop)
			{
				isFirstPassPop = true;
				zbUIMgr.Instance.ShowDungeonExit(this);
			}
		}
	}

	public void OnTriggerEventCheck()
	{
		if (dungeonBaseInfo != null && dungeonBaseInfo.TriggerEvent.EventType != EDungeonEventType.EmptyValue)
		{
			if (IsFightEvent(dungeonBaseInfo.TriggerEvent.EventType))
			{
				zbDungeonMgr.Instance.dungeonHandler.EventTrigger_Req(dungeonBaseInfo.TriggerEvent.CellId, dungeonBaseInfo.TriggerEvent.GridId);
			}
			else if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.Popup)
			{
				OnTriggerPopUp();
			}
			else if (dungeonBaseInfo.TriggerEvent.EventType == EDungeonEventType.ModRand)
			{
				OnTriggerModRand();
			}
		}
	}

	public void OnTriggerModRand()
	{
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Trigger);
	}

	public void OnTriggerPopUp()
	{
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Trigger);
	}

	public void OnTriggerFight(BattleInfo battleInfo)
	{
		triggerBattleInfo = battleInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Trigger);
	}

	public bool IsRoomHaveBoss(DungeonMapCellDetail dungeonMapCellDetail)
	{
		if (dungeonMapCellDetail == null)
		{
			return false;
		}
		foreach (DungeonMapGridDetail grid in dungeonMapCellDetail.GridList)
		{
			if (grid.EventType == EDungeonEventType.BossFight)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsGridInRoom(DungeonMapCellDetail dungeonMapCellDetail, int gridId)
	{
		if (dungeonMapCellDetail == null || dungeonMapCellDetail.GridList.Count == 0)
		{
			return false;
		}
		return ((IEnumerable<DungeonMapGridDetail>)dungeonMapCellDetail.GridList).Any((DungeonMapGridDetail cell) => cell.GridId == gridId);
	}

	public bool IsDoorOpen(DungeonMapCellDetail dungeonMapCellDetail, bool isLeft)
	{
		if (dungeonMapCellDetail == null)
		{
			return false;
		}
		if (dungeonMapCellDetail.State == EDungeonMapCellState.AllOpen)
		{
			return true;
		}
		if (isLeft)
		{
			return dungeonMapCellDetail.State == EDungeonMapCellState.LeftOpen;
		}
		return dungeonMapCellDetail.State == EDungeonMapCellState.RightOpen;
	}

	public int GetMaxGridId(DungeonMapCellDetail dungeonMapCellDetail)
	{
		if (dungeonMapCellDetail == null || dungeonMapCellDetail.GridList.Count == 0)
		{
			return -1;
		}
		return ((IEnumerable<DungeonMapGridDetail>)dungeonMapCellDetail.GridList).Max((DungeonMapGridDetail cell) => cell.GridId);
	}

	public int GetMinGridId(DungeonMapCellDetail dungeonMapCellDetail)
	{
		if (dungeonMapCellDetail == null || dungeonMapCellDetail.GridList.Count == 0)
		{
			return -1;
		}
		return ((IEnumerable<DungeonMapGridDetail>)dungeonMapCellDetail.GridList).Min((DungeonMapGridDetail cell) => cell.GridId);
	}

	public DungeonMapCellDetail GetNextCellDetail(DungeonMapCellDetail cur, bool left)
	{
		int gridId = (left ? (GetMinGridId(cur) - 1) : (GetMaxGridId(cur) + 1));
		foreach (DungeonMapCellDetail cell in dungeonMapInfo.CellList)
		{
			if (IsGridInRoom(cell, gridId))
			{
				return cell;
			}
		}
		return null;
	}

	public DungeonRoleSiteInfo GetRoleSiteInfo()
	{
		if (dungeonBaseInfo != null)
		{
			return dungeonBaseInfo.RoleSiteInfo;
		}
		return null;
	}

	public Vector3 GetRolePosition(DungeonRoleSiteInfo dungeonRoleSiteInfo, zbDungeonRoom room)
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		int num = ((dungeonMapInfo == null) ? 25 : dungeonMapInfo.MaxX);
		float num2 = dungeonRoleSiteInfo.GridId % num;
		num2 = ((num2 == 0f) ? ((float)num) : num2);
		float num3 = (num2 - (float)room.dungeonMapCellDetail.X) * 3f + 0.75f;
		float num4 = ((Component)room).gameObject.transform.position.x + num3;
		return new Vector3(num4, ((Component)room).gameObject.transform.position.y, ((Component)room).gameObject.transform.position.z);
	}

	public Vector3 GetNewRoomTargetPosition(zbDungeonRoom currentRoom, zbDungeonRoom newRoom, DungeonRoleSiteInfo siteInfo)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)currentRoom == (Object)null)
		{
			return GetRolePosition(siteInfo, newRoom);
		}
		int minGridId = GetMinGridId(currentRoom.dungeonMapCellDetail);
		int minGridId2 = GetMinGridId(newRoom.dungeonMapCellDetail);
		if (minGridId >= minGridId2)
		{
			return newRoom.GetRightDoorPos();
		}
		return newRoom.GetLeftDoorPos();
	}

	public void SaveNewRoleSiteInfo(zbDungeonRoom currentRoom, zbDungeonRoom newRoom, DungeonRoleSiteInfo siteInfo)
	{
		if ((Object)(object)currentRoom != (Object)null && (Object)(object)newRoom != (Object)null && siteInfo != null)
		{
			siteInfo.CellId = newRoom.dungeonMapCellDetail.CellId;
			int minGridId = GetMinGridId(newRoom.dungeonMapCellDetail);
			int minGridId2 = GetMinGridId(currentRoom.dungeonMapCellDetail);
			siteInfo.GridId = ((minGridId > minGridId2) ? minGridId : GetMaxGridId(newRoom.dungeonMapCellDetail));
		}
	}

	public bool IsFloorDungeon()
	{
		if (dungeonBaseInfo != null)
		{
			zbDungeonCsvRecord recordByKey = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(dungeonBaseInfo.DungeonId);
			if (recordByKey != null && recordByKey.type == 1)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsWorldDungeon()
	{
		if (dungeonBaseInfo != null)
		{
			zbDungeonCsvRecord recordByKey = zbDataConfig.Instance.dungeonCsvTable.GetRecordByKey(dungeonBaseInfo.DungeonId);
			if (recordByKey != null && recordByKey.type == 2)
			{
				return true;
			}
		}
		return false;
	}

	public void UpdateDungeonLogInfo(DungeonLogInfo _dungeonLogInfo)
	{
		dungeonLogInfo = _dungeonLogInfo;
	}

	public void UpDateDungeonMapInfo(DungeonBaseInfo baseInfo, DungeonMapInfo mapInfo, DungeonAwardInfo awardInfo = null, AwardDetail[] awardList = null, DungeonResultInfo resultInfo = null)
	{
		zbServerData.sGameData.WorldInfo.DungeonBaseInfo = baseInfo;
		dungeonBaseInfo = baseInfo;
		dungeonMapInfo = mapInfo;
		if (awardInfo != null)
		{
			dungeonAwardInfo = awardInfo;
		}
		if (resultInfo != null)
		{
			dungeonResultInfo = resultInfo;
		}
		if (awardList != null)
		{
			dungeonAwardList = awardList;
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Dungeon_Move);
	}

	public void SetRoleSiteInfo_Req(DungeonRoleSiteInfo dungeonRoleSiteInfo)
	{
		if (dungeonBaseInfo != null)
		{
			dungeonBaseInfo.RoleSiteInfo = dungeonRoleSiteInfo;
			zbWsMsgHandler.Instance.WorldDungeon_Move_Req(dungeonRoleSiteInfo);
		}
	}

	public void OpenCell_Req(string FromCellId, string OpenCellId)
	{
		zbWsMsgHandler.Instance.WorldDungeon_OpenCell_Req(FromCellId, OpenCellId);
	}

	public void EventTrigger_Req(string CellId, int GridId)
	{
		zbWsMsgHandler.Instance.WorldDungeon_EventTrigger_Req(CellId, GridId);
	}

	public void DungeonEventModSelect_Req(int modId)
	{
		zbWsMsgHandler.Instance.WorldDungeon_EventModSelect_Req(modId);
	}

	public void DungeonEventPopConfirm_Req()
	{
		zbWsMsgHandler.Instance.WorldDungeon_EventPopupConfirm_Req();
	}

	public void CloseDungeon_Req()
	{
		zbWsMsgHandler.Instance.WorldDungeon_Close_Req();
	}

	public void OpenDungeon_Req(int param1, EDifficultyLevel param2, int[] SelectList)
	{
		zbWsMsgHandler.Instance.WorldDungeon_Open_Req(param1, param2, SelectList);
	}
}
