using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbTaskHandler
{
	public List<TaskDetail> taskList = new List<TaskDetail>();

	public void Init()
	{
		UpdateDailyTaskInfo();
	}

	public void UpdatePeerageInfos()
	{
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Peerage_Data);
	}

	public void UpdateDailyTaskInfo()
	{
		taskList.Clear();
		foreach (TaskDetail task in zbServerData.sGameData.DailyTaskInfo.TaskList)
		{
			if (task.TaskType != 180)
			{
				taskList.Add(task);
			}
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_DailyTask_Data);
	}

	public bool CheckNeedShowPeerageInfo()
	{
		bool result = false;
		foreach (zbFunctionUnlockCsvRecord item in zbDataConfig.Instance.functionUnlockCsvTable.GetRecords().records.Values.Where((zbFunctionUnlockCsvRecord r) => r.unlockType == 2 && r.unlockVal == zbServerData.sGameData.PeerageInfo.PeerageId))
		{
			if (!zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)item.id))
			{
				result = true;
			}
		}
		return result;
	}

	public bool HasDailyTaskRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Task))
		{
			return false;
		}
		foreach (TaskDetail task in taskList)
		{
			if (task.State == EActiveState.Active)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPeerageRedPoint()
	{
		bool flag = false;
		bool result = true;
		if (IsTopPeerage())
		{
			return false;
		}
		foreach (TaskDetail task in zbServerData.sGameData.PeerageInfo.TaskList)
		{
			if (task.State == EActiveState.Active)
			{
				flag = true;
				break;
			}
			if (task.State != EActiveState.Done)
			{
				result = false;
			}
		}
		if (flag)
		{
			return true;
		}
		return result;
	}

	public bool IsTopPeerage()
	{
		return zbDataConfig.Instance.peerageCsvTable.GetRecordByKey(zbServerData.sGameData.PeerageInfo.PeerageId).ifMax == 1;
	}

	public bool HasRedPoint()
	{
		if (!HasDailyTaskRedPoint())
		{
			return HasPeerageRedPoint();
		}
		return true;
	}

	public DailyActiveDetail GetOperateDailyActiveDetail()
	{
		if (zbServerData.sGameData.OperateInfo.DailyActiveList != null)
		{
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(dailyActive.BaseInfo))
				{
					return dailyActive;
				}
			}
		}
		return null;
	}

	public FusionInfo GetFusionInfo()
	{
		if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo))
		{
			return zbServerData.sGameData.OperateInfo.FusionInfo;
		}
		return null;
	}
}
