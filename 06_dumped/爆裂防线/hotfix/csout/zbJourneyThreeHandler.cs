using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbJourneyThreeHandler
{
	private Dictionary<int, bool> m_exchangeRemindDict = new Dictionary<int, bool>();

	public int journeyThreeMidGoodsId;

	public int journeyThreeHighGoodsId;

	public void Init()
	{
		m_exchangeRemindDict.Clear();
		long roleId = zbSelf.Instance.GetRoleId();
		string key = $"journeyThreeExchange{roleId}_dict";
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
		journeyThreeMidGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterThreeBaseJsonTable.GetRecord()["zhanLing"][(object)"mediumGoodsId"]);
		journeyThreeHighGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterThreeBaseJsonTable.GetRecord()["zhanLing"][(object)"highGoodsId"]);
	}

	public bool IsActive()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree != null && zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.StartTm < zbSelf.Instance.timeHandler.GetServerTime())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.EndTm > zbSelf.Instance.timeHandler.GetServerTime();
		}
		return false;
	}

	public bool IsEnd()
	{
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree != null && zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo != null && zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.EndTm > 0)
		{
			return zbSelf.Instance.timeHandler.GetServerTime() >= zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.EndTm;
		}
		return false;
	}

	public long GetEndLeftTime()
	{
		if (IsActive())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterThree.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return 0L;
	}

	public int GetSignTarget()
	{
		foreach (JourneyTargetRecordDetail target in zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.TargetList)
		{
			if (zbServerData.sGameData.JourneyInfo.ChapterThree.Sign.Reach < target.Target)
			{
				return target.Target;
			}
		}
		return 0;
	}

	public bool IsZhanlingActive(int goodsId)
	{
		bool result = false;
		JourneyInfo journeyInfo = zbServerData.sGameData.JourneyInfo;
		if (journeyInfo == null || journeyInfo.ChapterThree == null)
		{
			return false;
		}
		JourneyZhanLingDetail zhanLing = journeyInfo.ChapterThree.ZhanLing;
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
		JourneyZhanLingDetail journeyZhanLingDetail = zbServerData.sGameData.JourneyInfo?.ChapterThree?.ZhanLing;
		if (journeyZhanLingDetail == null)
		{
			return false;
		}
		bool flag = IsZhanlingActive(journeyThreeMidGoodsId);
		bool flag2 = IsZhanlingActive(journeyThreeHighGoodsId);
		if (journeyZhanLingDetail.Level <= journeyZhanLingDetail.LowAward && (!flag || journeyZhanLingDetail.Level <= GetZhanlingDetailByGoodsId(journeyThreeMidGoodsId).HighAward))
		{
			if (flag2)
			{
				return journeyZhanLingDetail.Level > GetZhanlingDetailByGoodsId(journeyThreeHighGoodsId).HighAward;
			}
			return false;
		}
		return true;
	}

	public JourneyZhanLingActiveDetail GetZhanlingDetailByGoodsId(int goodsId)
	{
		JourneyZhanLingActiveDetail result = null;
		foreach (JourneyZhanLingActiveDetail active in zbServerData.sGameData.JourneyInfo.ChapterThree.ZhanLing.ActiveList)
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
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo)?.ChapterThree?.BarDraw?.TargetList;
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

	public bool HasSignNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo?.ChapterThree?.Sign)?.TargetList;
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

	public bool HasDrawCountNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		JourneyStatisticsDetail journeyStatisticsDetail = zbServerData.sGameData?.JourneyInfo?.ChapterThree?.DrawCount;
		if (journeyStatisticsDetail?.TargetList == null)
		{
			return false;
		}
		return ((IEnumerable<JourneyTargetRecordDetail>)journeyStatisticsDetail.TargetList).Any((JourneyTargetRecordDetail t) => t != null && t.State == ECommonState.Yes);
	}

	public void UpdateJourneyThreeExchangeRemind(int goodsId, bool remind)
	{
		if (remind)
		{
			m_exchangeRemindDict.Remove(goodsId);
		}
		else
		{
			m_exchangeRemindDict[goodsId] = false;
		}
		string key = $"journeyThreeExchange{zbSelf.Instance.GetRoleId()}_dict";
		string value = JsonConvert.SerializeObject((object)m_exchangeRemindDict);
		zbGameSave.Instance.SetString(key, value);
	}

	public bool HasExchangeNotify()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyCountRecordDetail> val = zbServerData.sGameData?.JourneyInfo?.ChapterThree?.Exchange?.ExchangeList;
		if (val == null)
		{
			return false;
		}
		foreach (JourneyCountRecordDetail item in val)
		{
			zbChapterThreeExchangeCsvRecord recordByKey = zbDataConfig.Instance.chapterThreeExchangeCsvTable.GetRecordByKey(item.Id);
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
		if (!HasBarNotify() && !HasDrawCountNotify() && !HasExchangeNotify() && !HasSignNotify())
		{
			return zbSelf.Instance.facilityHandler.HasRedPoint(isMainWnd: false);
		}
		return true;
	}
}
