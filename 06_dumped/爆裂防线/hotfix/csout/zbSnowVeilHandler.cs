using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;

public class zbSnowVeilHandler
{
	public int m_goAckStep;

	public int m_goAckfactor = 1;

	public void Init()
	{
	}

	public SnowVeilDetail GetSnowVeilDetail()
	{
		if (zbServerData.sGameData.OperateInfo.SnowVeilList != null)
		{
			foreach (SnowVeilDetail snowVeil in zbServerData.sGameData.OperateInfo.SnowVeilList)
			{
				if (zbSelf.Instance.operateHandler.IsShowTime(snowVeil.BaseInfo) && snowVeil.BaseInfo.Type == EOperatePlanType.Holiday)
				{
					return snowVeil;
				}
			}
		}
		return null;
	}

	public void UpdateSnowVeilDetail(string uuid, int totalScore, SnowVeilRoleState roleState, SnowVeilLandDetail[] landList = null, SnowVeilTurntableDetail turntable = null, SnowVeilRollingDetail rolling = null, OperateSnowVeilTargetDetail targetDetail = null, OperateTargetNextGrandDetail targetNextGrand = null)
	{
		foreach (SnowVeilDetail snowVeil in zbServerData.sGameData.OperateInfo.SnowVeilList)
		{
			if (snowVeil.BaseInfo.UUID == uuid)
			{
				if (totalScore != -1)
				{
					snowVeil.TotalScore = totalScore;
				}
				snowVeil.RoleState = roleState;
				if (landList != null)
				{
					snowVeil.LandList.Clear();
					snowVeil.LandList.AddRange((IEnumerable<SnowVeilLandDetail>)landList);
				}
				if (turntable != null)
				{
					snowVeil.Turntable = turntable;
				}
				if (rolling != null)
				{
					snowVeil.Rolling = rolling;
				}
				if (targetDetail != null)
				{
					snowVeil.TargetDetail = targetDetail;
				}
				if (targetNextGrand != null)
				{
					snowVeil.TargetNextGrand = targetNextGrand;
				}
				m_goAckStep = snowVeil.RoleState.Step;
				break;
			}
		}
	}

	public bool HasTargetNotify()
	{
		SnowVeilDetail snowVeilDetail = GetSnowVeilDetail();
		if (snowVeilDetail != null && snowVeilDetail.TotalScore >= snowVeilDetail.TargetDetail.CostScore)
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		SnowVeilDetail snowVeilDetail = GetSnowVeilDetail();
		if (snowVeilDetail == null)
		{
			return false;
		}
		if (snowVeilDetail.ActivateAward == EBoolState.False || zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, snowVeilDetail.Prop.PropId, 10))
		{
			return true;
		}
		return HasTargetNotify();
	}
}
