using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbJourneyTwoHandler
{
	private Dictionary<int, bool> m_exchangeRemindDict = new Dictionary<int, bool>();

	public int journeyTwoMidGoodsId;

	public int journeyTwoHighGoodsId;

	public void Init()
	{
		m_exchangeRemindDict.Clear();
		long roleId = zbSelf.Instance.GetRoleId();
		string key = $"journeyTwoExchange{roleId}_dict";
		string text = zbGameSave.Instance.GetString(key, string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				m_exchangeRemindDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(text);
			}
			catch
			{
				m_exchangeRemindDict = new Dictionary<int, bool>();
			}
		}
		journeyTwoMidGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterTwoBaseJsonTable.GetRecord()["zhanLing"][(object)"mediumGoodsId"]);
		journeyTwoHighGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterTwoBaseJsonTable.GetRecord()["zhanLing"][(object)"highGoodsId"]);
	}

	public bool IsActive()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.StartTm > 0 && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.StartTm < zbSelf.Instance.timeHandler.GetServerTime())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.EndTm > zbSelf.Instance.timeHandler.GetServerTime();
		}
		return false;
	}

	public bool IsEnd()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo != null && zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.EndTm > 0)
		{
			return zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.EndTm;
		}
		return false;
	}

	public long GetEndLeftTime()
	{
		if (IsActive())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterTwo.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return 0L;
	}

	public int GetActiveTarget()
	{
		if (IsActive())
		{
			foreach (JourneyTargetRecordDetail target in zbServerData.sGameData.JourneyInfo.ChapterTwo.Active.TargetList)
			{
				if (zbServerData.sGameData.JourneyInfo.ChapterTwo.Active.Reach < target.Target)
				{
					return target.Target;
				}
			}
			foreach (JourneyTargetRecordDetail target2 in zbServerData.sGameData.JourneyInfo.ChapterTwo.Active.TargetList)
			{
				if (target2.State != ECommonState.End)
				{
					return target2.Target;
				}
			}
		}
		return 0;
	}

	public bool IsZhanlingActive(int goodsId)
	{
		bool result = false;
		JourneyInfo journeyInfo = zbServerData.sGameData.JourneyInfo;
		if (journeyInfo == null || journeyInfo.ChapterTwo == null)
		{
			return false;
		}
		JourneyZhanLingDetail zhanLing = journeyInfo.ChapterTwo.ZhanLing;
		if (zhanLing == null || zhanLing.ActiveList == null)
		{
			return false;
		}
		foreach (JourneyZhanLingActiveDetail active in zhanLing.ActiveList)
		{
			if (goodsId == active.GoodsId && active.Active == EBoolState.True)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public bool CanReceiveZhanling()
	{
		if (!IsActive())
		{
			return false;
		}
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing;
		bool flag = IsZhanlingActive(journeyTwoMidGoodsId);
		bool flag2 = IsZhanlingActive(journeyTwoHighGoodsId);
		if (zhanLing.Level <= zhanLing.LowAward && (!flag || zhanLing.Level <= GetZhanlingDetailByGoodsId(journeyTwoMidGoodsId).HighAward))
		{
			if (flag2)
			{
				return zhanLing.Level > GetZhanlingDetailByGoodsId(journeyTwoHighGoodsId).HighAward;
			}
			return false;
		}
		return true;
	}

	public JourneyZhanLingActiveDetail GetZhanlingDetailByGoodsId(int goodsId)
	{
		JourneyZhanLingActiveDetail result = null;
		foreach (JourneyZhanLingActiveDetail active in zbServerData.sGameData.JourneyInfo.ChapterTwo.ZhanLing.ActiveList)
		{
			if (goodsId == active.GoodsId)
			{
				result = active;
				break;
			}
		}
		return result;
	}

	public bool HasBarNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L))
		{
			return true;
		}
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo)?.ChapterTwo?.BarDraw?.TargetList;
		if (val == null)
		{
			return false;
		}
		int count = val.Count;
		for (int i = 0; i < count; i++)
		{
			JourneyTargetRecordDetail journeyTargetRecordDetail = val[i];
			if (journeyTargetRecordDetail != null && journeyTargetRecordDetail.State == ECommonState.Yes)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo?.ChapterTwo?.Active)?.TargetList;
		if (val == null)
		{
			return false;
		}
		int count = val.Count;
		for (int i = 0; i < count; i++)
		{
			JourneyTargetRecordDetail journeyTargetRecordDetail = val[i];
			if (journeyTargetRecordDetail != null && journeyTargetRecordDetail.State == ECommonState.Yes)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasZhanLingNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		return CanReceiveZhanling();
	}

	public bool GetExchangeRemindState(int goodsId)
	{
		if (m_exchangeRemindDict != null && m_exchangeRemindDict.ContainsKey(goodsId))
		{
			return m_exchangeRemindDict[goodsId];
		}
		return true;
	}

	public bool HasDiamondCostNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		JourneyStatisticsDetail journeyStatisticsDetail = zbServerData.sGameData?.JourneyInfo?.ChapterTwo?.DiamondCost;
		if (journeyStatisticsDetail?.TargetList == null)
		{
			return false;
		}
		return ((IEnumerable<JourneyTargetRecordDetail>)journeyStatisticsDetail.TargetList).Any((JourneyTargetRecordDetail t) => t != null && t.State == ECommonState.Yes);
	}

	public void UpdateJourneyTwoExchangeRemind(int goodsId, bool remind)
	{
		if (remind)
		{
			m_exchangeRemindDict.Remove(goodsId);
		}
		else
		{
			m_exchangeRemindDict[goodsId] = false;
		}
		string key = $"journeyTwoExchange{zbSelf.Instance.GetRoleId()}_dict";
		string value = JsonConvert.SerializeObject((object)m_exchangeRemindDict);
		zbGameSave.Instance.SetString(key, value);
	}

	public bool HasExchangeNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyCountRecordDetail> val = zbServerData.sGameData?.JourneyInfo?.ChapterTwo?.Exchange?.ExchangeList;
		if (val == null)
		{
			return false;
		}
		foreach (JourneyCountRecordDetail item in val)
		{
			zbChapterTwoExchangeCsvRecord recordByKey = zbDataConfig.Instance.chapterTwoExchangeCsvTable.GetRecordByKey(item.Id);
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(recordByKey.moneyType, recordByKey.price) && recordByKey.limitNum - item.Count > 0)
			{
				bool flag = true;
				if (m_exchangeRemindDict != null && m_exchangeRemindDict.TryGetValue(item.Id, out var value))
				{
					flag = value;
				}
				if (flag)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		if (!HasBarNotify() && !HasDiamondCostNotify() && !HasExchangeNotify() && !HasActiveNotify())
		{
			return zbSelf.Instance.truckHandler.HasRedPoint(isArenaMain: false);
		}
		return true;
	}
}
