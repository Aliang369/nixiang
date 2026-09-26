using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbCrossPeakHandler
{
	public int buyLimit;

	public int buyDiamond;

	public int maxPeakId;

	public int currRank;

	public int currPlayerId;

	public CrossPeakRankInfo crossPeakRankInfo;

	public bool isUpgradeAck;

	public SettleRankInfo settleRankInfo;

	private const int arenaResultTime = 25200;

	private long m_arenaResultTm;

	public CrossPeakRankDetail[] combatList;

	public RoleBaseInfo peakTop;

	public void Init()
	{
		JObject record = zbDataConfig.Instance.peakBaseJsonTable.GetRecord();
		buyLimit = Extensions.Value<int>((IEnumerable<JToken>)record["buyLimit"]);
		buyDiamond = Extensions.Value<int>((IEnumerable<JToken>)record["buyDiamond"]);
	}

	public void UpdateCrossPeakTop(RoleBaseInfo roseBaseInfo)
	{
		peakTop = roseBaseInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_ArenaRankTop);
	}

	public int GetBuyLimit()
	{
		if (zbSelf.Instance.crossLadderHandler.ValidateHasCrossLadderCard())
		{
			zbVipCrossLadderCsvRecord recordByKey = zbDataConfig.Instance.vipCrossLadderCsvTable.GetRecordByKey(4);
			if (recordByKey != null)
			{
				return buyLimit + recordByKey.val;
			}
		}
		return buyLimit;
	}

	public void UpdateCrossPeakInfo(CrossPeakSelfInfo _crossPeakInfo, CrossPeakRankInfo _crossPeakRankInfo)
	{
		if (_crossPeakRankInfo != null)
		{
			crossPeakRankInfo = _crossPeakRankInfo;
		}
		if (_crossPeakInfo != null)
		{
			zbServerData.sGameData.CrossPeakSelfInfo = _crossPeakInfo;
		}
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_CrossPeakInfo);
	}

	public void UpdateRankList(CrossPeakRankDetail[] list)
	{
		combatList = list;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_CrossPeakCombatList);
	}

	private void RefreshArenaResultTm()
	{
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long nextRefresh = zbServerData.sGameData.RoleCommonInfo.NextRefresh;
		if (serverTime < nextRefresh - 25200)
		{
			m_arenaResultTm = nextRefresh - 25200;
		}
		else
		{
			m_arenaResultTm = nextRefresh + 86400 - 25200;
		}
	}

	public string GetArenaRefreshTimeStr()
	{
		long num = m_arenaResultTm - zbSelf.Instance.timeHandler.GetServerTime();
		if (num < 0)
		{
			RefreshArenaResultTm();
			num = m_arenaResultTm - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return zbUtil.FormatTimeHHMMSS(num);
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.PeakArenaRecord))
		{
			return true;
		}
		return false;
	}
}
