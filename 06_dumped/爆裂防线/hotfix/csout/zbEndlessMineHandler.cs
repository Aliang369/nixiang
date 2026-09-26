using Proto.Enums;
using Proto.Types;
using zbType;

public class zbEndlessMineHandler
{
	public EndlessMineInfo endlessMineInfo;

	public void Init()
	{
	}

	public void UpdateEndlessMineInfo(EndlessMineInfo _endlessMineInfo)
	{
		endlessMineInfo = _endlessMineInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_EndlessMineInfo);
		if (zbServerData.sGameData.OperateInfo.EndlessMineInfo == null || !zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo))
		{
			return;
		}
		foreach (AwardDetail award in endlessMineInfo.EndlessMineBaseInfo.ExchangeDetail.AwardList)
		{
			zbSelf.Instance.getWayHandler.AddOperateToGetWayDict(award.Type, award.Tid, EOperateType.EndlessMine);
		}
		foreach (OperateTargetDetail target in endlessMineInfo.EndlessMineBaseInfo.TargetList)
		{
			foreach (AwardDetail award2 in target.AwardList)
			{
				zbSelf.Instance.getWayHandler.AddOperateToGetWayDict(award2.Type, award2.Tid, EOperateType.EndlessMine);
			}
		}
		foreach (AwardDetail grandAward in endlessMineInfo.EndlessMineBaseInfo.ExchangeNextGrand.GrandAwardList)
		{
			zbSelf.Instance.getWayHandler.AddOperateToGetWayDict(grandAward.Type, grandAward.Tid, EOperateType.EndlessMine);
		}
	}

	public void UpdateEndlessMineBaseInfo(EndlessMineBaseInfo _endlessMineBaseInfo)
	{
		endlessMineInfo.EndlessMineBaseInfo = _endlessMineBaseInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_EndlessMineInfo);
	}

	public EndlessMineDetail GetEndlessMineDetail()
	{
		if (zbServerData.sGameData.OperateInfo.EndlessMineInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.EndlessMineInfo.BaseInfo))
		{
			return zbServerData.sGameData.OperateInfo.EndlessMineInfo;
		}
		return null;
	}

	public EndlessMinePropDetail GetEndlessMinePropDetail(int propId)
	{
		if (endlessMineInfo != null)
		{
			foreach (EndlessMinePropDetail prop in endlessMineInfo.EndlessMineBaseInfo.PropList)
			{
				if (propId == prop.PropId)
				{
					return prop;
				}
			}
		}
		return null;
	}

	public int GetEndlessMinePropCount(int propId)
	{
		return GetEndlessMinePropDetail(propId)?.Count ?? 0;
	}

	public OperateTargetDetail GetTargetDetail()
	{
		if (endlessMineInfo != null)
		{
			OperateTargetDetail operateTargetDetail = null;
			OperateTargetDetail operateTargetDetail2 = null;
			OperateTargetDetail operateTargetDetail3 = null;
			foreach (OperateTargetDetail target in endlessMineInfo.EndlessMineBaseInfo.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					operateTargetDetail = target;
					break;
				}
				if (target.State == ECommonState.No && operateTargetDetail2 == null)
				{
					operateTargetDetail2 = target;
				}
				if (target.State == ECommonState.End)
				{
					operateTargetDetail3 = target;
				}
			}
			if (operateTargetDetail == null)
			{
				operateTargetDetail = ((operateTargetDetail2 != null) ? operateTargetDetail2 : operateTargetDetail3);
			}
			return operateTargetDetail;
		}
		return null;
	}

	public bool HasExchangeNotify()
	{
		if (GetEndlessMineDetail() != null && endlessMineInfo != null && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, 10002, endlessMineInfo.EndlessMineBaseInfo.ExchangeDetail.Cost))
		{
			return true;
		}
		return false;
	}

	public bool HasTargetNotify()
	{
		if (GetEndlessMineDetail() != null && endlessMineInfo != null)
		{
			foreach (OperateTargetDetail target in endlessMineInfo.EndlessMineBaseInfo.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool HasPropsNotify()
	{
		if (GetEndlessMineDetail() != null && endlessMineInfo != null)
		{
			int[] array = new int[3] { 10003, 10004, 10005 };
			foreach (int propId in array)
			{
				if (GetEndlessMinePropCount(propId) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasExchangeNotify() && !HasTargetNotify())
		{
			return HasPropsNotify();
		}
		return true;
	}
}
