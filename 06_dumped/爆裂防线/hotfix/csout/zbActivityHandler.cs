using Proto.Enums;
using Proto.Types;

public class zbActivityHandler
{
	public bool m_firstRechargeIn;

	public void Init()
	{
	}

	public bool IsFirstRechargeEnd()
	{
		return zbSelf.Instance.operateHandler.IsNovicePackCompleted();
	}

	public bool IsFirstRechargeUnActive()
	{
		NovicePackInfo novicePackInfo = zbServerData.sGameData.RechargeActiveInfo?.NovicePackInfo;
		if (novicePackInfo != null)
		{
			return novicePackInfo.State != EBoolState.True;
		}
		return false;
	}

	public bool CanFirstRechargeGet()
	{
		return zbSelf.Instance.operateHandler.IsNovicePackClaimableToday();
	}

	public bool HasFirstRechargeNotify()
	{
		if (IsFirstRechargeEnd())
		{
			return false;
		}
		if (!IsFirstRechargeUnActive() || m_firstRechargeIn)
		{
			return CanFirstRechargeGet();
		}
		return true;
	}

	public bool HasDailySignNotify()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.DailySign))
		{
			return false;
		}
		DailySign dailySign = zbServerData.sGameData.ActivityInfo?.DailySign;
		if (dailySign == null)
		{
			return false;
		}
		if (dailySign.SignDay > 0 && dailySign.DailyState == ECommonState.Yes)
		{
			return true;
		}
		if (dailySign.TargetAward != null && dailySign.TargetAward.TargetState == ECommonState.Yes)
		{
			return true;
		}
		return false;
	}

	public bool HasAnyZhanLingMainActive()
	{
		foreach (ZhanLingMainActiveDetail active in zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo.ActiveList)
		{
			if (active.Active == EBoolState.True)
			{
				return true;
			}
		}
		return false;
	}
}
