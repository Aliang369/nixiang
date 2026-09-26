using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbMineWarHandler
{
	public int buyLimit;

	public int buyDiamond;

	public MineWarMapInfo mineWarMapInfo;

	public MineWarBaseInfo mineWarBaseInfo;

	public CrossMineServerDetail[] crossMineServerList;

	public float mineWarRecordTime;

	public float crossMineRecordTime;

	public MineWarRecordDetail[] mineWarRecordList;

	public CrossMineRecordDetail[] crossMineRecordList;

	public int currServerId;

	public int currPlayerId;

	public int currPointId;

	public bool openWnd;

	public bool openEnemyWnd;

	public int selfPointId;

	public int selfLayerNum;

	public int canCuihuiLayer;

	public stChangeInfo changeInfo = new stChangeInfo();

	public void Init()
	{
		JObject record = zbDataConfig.Instance.mineBaseJsonTable.GetRecord();
		buyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["buyAttackLimit"]);
		buyDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["buyAttackDiamond"]);
		currServerId = zbSelf.Instance.GetServerId();
	}

	public int GetBuyLimit()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			zbTreasurePrivilegeCsvRecord recordByKey = zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecordByKey(3);
			if (recordByKey != null)
			{
				return buyLimit + recordByKey.val;
			}
		}
		return buyLimit;
	}

	public int GetMineWarCollect()
	{
		if (zbServerData.sGameData.RechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True)
		{
			zbTreasurePrivilegeCsvRecord recordByKey = zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecordByKey(2);
			if (recordByKey != null)
			{
				return recordByKey.val;
			}
		}
		return 0;
	}

	public void UpdateMineWarInfo(MineWarBaseInfo _mineWarBaseInfo, MineWarMapInfo _mineWarMapInfo, bool isCross = false)
	{
		if (_mineWarBaseInfo != null)
		{
			mineWarBaseInfo = _mineWarBaseInfo;
			zbServerData.sGameData.MineWarBaseInfo = _mineWarBaseInfo;
		}
		if (_mineWarMapInfo != null)
		{
			mineWarMapInfo = _mineWarMapInfo;
			canCuihuiLayer = CrossCanCuihuiLayer(mineWarMapInfo);
			if (!isCross)
			{
				selfPointId = 0;
				foreach (MineWarPointDetail point in _mineWarMapInfo.PointList)
				{
					if (point.PlayerId == zbSelf.Instance.GetRoleId())
					{
						selfPointId = point.PointId;
						break;
					}
				}
				selfLayerNum = GetLayerByPoint(selfPointId);
			}
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_MineWarBaseInfo);
	}

	public int GetCostNum(int serverId)
	{
		if (crossMineServerList == null)
		{
			return 1;
		}
		CrossMineServerDetail[] array = crossMineServerList;
		foreach (CrossMineServerDetail crossMineServerDetail in array)
		{
			if (crossMineServerDetail.ServerId == serverId)
			{
				return crossMineServerDetail.CostCombatNum;
			}
		}
		return 1;
	}

	public int GetLayerByPoint(int pointId)
	{
		switch (pointId)
		{
		case 1:
			return 1;
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
			return 2;
		default:
			if (pointId > 9 && pointId <= 25)
			{
				return 3;
			}
			if (pointId > 25 && pointId <= 49)
			{
				return 4;
			}
			return 5;
		}
	}

	public int CrossCanCuihuiLayer(MineWarMapInfo mineWarMapInfo)
	{
		int num = 0;
		foreach (MineWarPointDetail point in mineWarMapInfo.PointList)
		{
			if (point.DestroyTm <= zbSelf.Instance.timeHandler.GetServerTime())
			{
				num = Math.Max(GetLayerByPoint(point.PointId), num);
				if (num == 4)
				{
					return 4;
				}
			}
		}
		return num;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.MineWar))
		{
			return false;
		}
		if ((float)(zbSelf.Instance.timeHandler.GetServerTime() - zbServerData.sGameData.MineWarBaseInfo.MineWarSelfInfo.CollectTm) > 3600f)
		{
			return true;
		}
		if (zbServerData.sGameData.MineWarBaseInfo.MineWarSelfInfo.CombatNum > 0)
		{
			return true;
		}
		if (zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.MineRecord) || zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.CrossMineRecord))
		{
			return true;
		}
		return zbSelf.Instance.treasureHandler.HasRedPoint();
	}
}
