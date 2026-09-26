using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;

public class zbJourneyOneHandler
{
	private Dictionary<int, bool> m_shopRemindDict = new Dictionary<int, bool>();

	public int journeyOneMidGoodsId;

	public int journeyOneHighGoodsId;

	public void Init()
	{
		m_shopRemindDict.Clear();
		long roleId = zbSelf.Instance.GetRoleId();
		string key = $"journeyOneShop{roleId}_dict";
		string text = zbGameSave.Instance.GetString(key, string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			try
			{
				m_shopRemindDict = JsonConvert.DeserializeObject<Dictionary<int, bool>>(text);
			}
			catch
			{
				m_shopRemindDict = new Dictionary<int, bool>();
			}
		}
		journeyOneMidGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterOneBaseJsonTable.GetRecord()["zhanLing"][(object)"mediumGoodsId"]);
		journeyOneHighGoodsId = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterOneBaseJsonTable.GetRecord()["zhanLing"][(object)"highGoodsId"]);
	}

	public void UpdateJourneyInfo(JourneyInfo journeyInfo)
	{
		zbServerData.sGameData.JourneyInfo = journeyInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_JourneyInfo);
	}

	public bool IsActive()
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)zbDataConfig.Instance.chapterOneBaseJsonTable.GetRecord()["limitMainLevel"]);
		if (zbServerData.sGameData.MainQuestInfo.MainLevel < num)
		{
			return false;
		}
		if (zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterOne != null && zbServerData.sGameData.JourneyInfo.ChapterOne.BaseInfo != null && zbServerData.sGameData.JourneyInfo.ChapterOne.BaseInfo.StartTm < zbSelf.Instance.timeHandler.GetServerTime())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterOne.BaseInfo.EndTm > zbSelf.Instance.timeHandler.GetServerTime();
		}
		return false;
	}

	public long GetEndLeftTime()
	{
		if (IsActive())
		{
			return zbServerData.sGameData.JourneyInfo.ChapterOne.BaseInfo.EndTm - zbSelf.Instance.timeHandler.GetServerTime();
		}
		return 0L;
	}

	public bool IsZhanlingActive(int goodsId)
	{
		bool result = false;
		JourneyInfo journeyInfo = zbServerData.sGameData.JourneyInfo;
		if (journeyInfo == null || journeyInfo.ChapterOne == null)
		{
			return false;
		}
		JourneyZhanLingDetail zhanLing = journeyInfo.ChapterOne.ZhanLing;
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

	public bool HasRechargeFreePack()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 5 && freeGift.State == EBoolState.False)
			{
				return true;
			}
		}
		return false;
	}

	public bool CanReceiveZhanling()
	{
		if (!IsActive())
		{
			return false;
		}
		JourneyZhanLingDetail zhanLing = zbServerData.sGameData.JourneyInfo.ChapterOne.ZhanLing;
		bool flag = zbSelf.Instance.journeyOneHandler.IsZhanlingActive(journeyOneMidGoodsId);
		bool flag2 = zbSelf.Instance.journeyOneHandler.IsZhanlingActive(journeyOneHighGoodsId);
		if (zhanLing.Level <= zhanLing.LowAward && (!flag || zhanLing.Level <= zbSelf.Instance.journeyOneHandler.GetZhanlingDetailByGoodsId(journeyOneMidGoodsId).HighAward))
		{
			if (flag2)
			{
				return zhanLing.Level > zbSelf.Instance.journeyOneHandler.GetZhanlingDetailByGoodsId(journeyOneHighGoodsId).HighAward;
			}
			return false;
		}
		return true;
	}

	public JourneyZhanLingActiveDetail GetZhanlingDetailByGoodsId(int goodsId)
	{
		JourneyZhanLingActiveDetail result = null;
		foreach (JourneyZhanLingActiveDetail active in zbServerData.sGameData.JourneyInfo.ChapterOne.ZhanLing.ActiveList)
		{
			if (goodsId == active.GoodsId)
			{
				result = active;
				break;
			}
		}
		return result;
	}

	public bool HasBarRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		if (zbSelf.Instance.moneyHandler.HasEnoughMoney(34, 1L))
		{
			return true;
		}
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo)?.ChapterOne?.BarDraw?.TargetList;
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

	public bool HasSignRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyTargetRecordDetail> val = (zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Sign)?.TargetList;
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

	public bool HasZhanlingRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		return CanReceiveZhanling();
	}

	public bool GetShopRemindState(int goodsId)
	{
		if (m_shopRemindDict != null && m_shopRemindDict.ContainsKey(goodsId))
		{
			return m_shopRemindDict[goodsId];
		}
		return true;
	}

	public bool HasRechargeSumRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		if (HasRechargeFreePack())
		{
			return true;
		}
		JourneyStatisticsDetail journeyStatisticsDetail = zbServerData.sGameData?.JourneyInfo?.ChapterOne?.RechargeSum;
		if (journeyStatisticsDetail?.TargetList == null)
		{
			return false;
		}
		return ((IEnumerable<JourneyTargetRecordDetail>)journeyStatisticsDetail.TargetList).Any((JourneyTargetRecordDetail t) => t != null && t.State == ECommonState.Yes);
	}

	public void UpdateJourneyOneShopRemind(int goodsId, bool remind)
	{
		if (remind)
		{
			m_shopRemindDict.Remove(goodsId);
		}
		else
		{
			m_shopRemindDict[goodsId] = false;
		}
		string key = $"journeyOneShop{zbSelf.Instance.GetRoleId()}_dict";
		string value = JsonConvert.SerializeObject((object)m_shopRemindDict);
		zbGameSave.Instance.SetString(key, value);
	}

	public bool HasShopRedPoint()
	{
		if (!IsActive())
		{
			return false;
		}
		RepeatedField<JourneyCountRecordDetail> val = zbServerData.sGameData?.JourneyInfo?.ChapterOne?.Exchange?.ExchangeList;
		if (val == null)
		{
			return false;
		}
		foreach (JourneyCountRecordDetail item in val)
		{
			zbChapterOneExchangeCsvRecord recordByKey = zbDataConfig.Instance.chapterOneExchangeCsvTable.GetRecordByKey(item.Id);
			if (zbSelf.Instance.moneyHandler.HasEnoughMoney(recordByKey.moneyType, recordByKey.price) && recordByKey.limitNum - item.Count > 0)
			{
				bool flag = true;
				if (m_shopRemindDict != null && m_shopRemindDict.TryGetValue(item.Id, out var value))
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
		if (!HasBarRedPoint() && !HasSignRedPoint() && !HasZhanlingRedPoint() && !HasRechargeSumRedPoint())
		{
			return HasShopRedPoint();
		}
		return true;
	}

	public JourneyZhanLingDetail GetJourneyOneZhanLingDetail()
	{
		if (IsActive() && zbServerData.sGameData.JourneyInfo != null && zbServerData.sGameData.JourneyInfo.ChapterOne != null)
		{
			return zbServerData.sGameData.JourneyInfo.ChapterOne.ZhanLing;
		}
		return null;
	}
}
