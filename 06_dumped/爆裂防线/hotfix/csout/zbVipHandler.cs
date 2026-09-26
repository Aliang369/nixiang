using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using zbType;

public class zbVipHandler
{
	private Dictionary<int, List<zbVipShopChannelCsvRecord>> m_cidRecords;

	public JObject m_vipBaseRecord;

	public int maxVipLv = 12;

	public void Init()
	{
		m_cidRecords = new Dictionary<int, List<zbVipShopChannelCsvRecord>>();
		zbVipShopChannelCsvTableData records = zbDataConfig.Instance.vipShopChannelCsvTable.GetRecords();
		if (records != null && records.records != null)
		{
			zbVipShopChannelCsvRecord[] records2 = records.records;
			foreach (zbVipShopChannelCsvRecord zbVipShopChannelCsvRecord2 in records2)
			{
				if (!m_cidRecords.ContainsKey(zbVipShopChannelCsvRecord2.cid))
				{
					m_cidRecords[zbVipShopChannelCsvRecord2.cid] = new List<zbVipShopChannelCsvRecord>();
				}
				m_cidRecords[zbVipShopChannelCsvRecord2.cid].Add(zbVipShopChannelCsvRecord2);
			}
		}
		m_vipBaseRecord = zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
	}

	public void openVipTqWnd()
	{
		zbWsMsgHandler.Instance.Vip_Data_Req();
	}

	public string GetIapIdByGoodsId(int cid, int goodsId)
	{
		if (m_cidRecords.ContainsKey(cid))
		{
			foreach (zbVipShopChannelCsvRecord item in m_cidRecords[cid])
			{
				if (item.goodsId == goodsId)
				{
					return item.cgoodsId;
				}
			}
		}
		return string.Empty;
	}

	public int GetGoodsIdByIapId(int cid, string cgoodsId)
	{
		if (m_cidRecords.ContainsKey(cid))
		{
			foreach (zbVipShopChannelCsvRecord item in m_cidRecords[cid])
			{
				if (item.cgoodsId == cgoodsId)
				{
					return item.goodsId;
				}
			}
		}
		return 0;
	}

	public string GetPriceByGoodsId(int goodId, bool needSymbol = true)
	{
		string result = "";
		int key = goodId;
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(key);
		if (recordByKey != null && recordByKey.referId != 0)
		{
			key = recordByKey.referId;
		}
		zbVipGoodsPriceCsvRecord recordByKey2 = zbDataConfig.Instance.vipGoodsPriceCsvTable.GetRecordByKey(key);
		if (recordByKey2 != null)
		{
			result = (needSymbol ? $"¥{recordByKey2.price}" : recordByKey2.price.ToString());
		}
		return result;
	}

	public float GetFloatPriceByGoodsId(int goodId)
	{
		float result = 0f;
		int key = goodId;
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(key);
		if (recordByKey != null && recordByKey.referId != 0)
		{
			key = recordByKey.referId;
		}
		zbVipGoodsPriceCsvRecord recordByKey2 = zbDataConfig.Instance.vipGoodsPriceCsvTable.GetRecordByKey(key);
		if (recordByKey2 != null)
		{
			result = recordByKey2.price;
		}
		return result;
	}

	public string GetGoodsName(int goodId)
	{
		string text = "";
		int num = goodId;
		zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(num);
		if (recordByKey != null && recordByKey.referId != 0)
		{
			num = recordByKey.referId;
		}
		zbVipShopCsvRecord recordByKey2 = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(num);
		if (recordByKey2 != null)
		{
			text = recordByKey2.goodsName;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = GetPriceByGoodsId(num);
		}
		return text;
	}

	public bool HasDailyPackInfoNotify()
	{
		if (zbServerData.sGameData.RechargeActiveInfo == null)
		{
			return false;
		}
		RechargeActiveInfo rechargeActiveInfo = zbServerData.sGameData.RechargeActiveInfo;
		if (rechargeActiveInfo.DailyPackInfo != null)
		{
			foreach (DailyPackDetail pack in rechargeActiveInfo.DailyPackInfo.PackList)
			{
				if (pack.State == EActiveState.Active)
				{
					return true;
				}
			}
		}
		return false;
	}

	public VipCardShopDetail GetVipCardShopDetail(int goodsId)
	{
		if (zbServerData.sGameData != null && zbServerData.sGameData.RechargeShopInfo != null)
		{
			foreach (VipCardShopDetail item in zbServerData.sGameData.RechargeShopInfo.VipCardShop)
			{
				if (goodsId == item.GoodsId)
				{
					return item;
				}
			}
		}
		return null;
	}

	public EFuncType GetDoubleAwardFunType()
	{
		if (zbServerData.sGameData != null && zbServerData.sGameData.RechargeActiveInfo != null && zbServerData.sGameData.RechargeActiveInfo.ResourceCardDoubleInfo != null)
		{
			return zbServerData.sGameData.RechargeActiveInfo.ResourceCardDoubleInfo.FuncId;
		}
		return EFuncType.EmptyValue;
	}

	public bool ValidateHasWeekCard()
	{
		int goodsId = m_vipBaseRecord["vipCard"][(object)"resource"].Value<int>((object)"goodsId");
		VipCardShopDetail vipCardShopDetail = GetVipCardShopDetail(goodsId);
		if (vipCardShopDetail != null && vipCardShopDetail.ExpireTm > zbSelf.Instance.timeHandler.GetServerTime())
		{
			return true;
		}
		return false;
	}

	public DailyPackDetail GetBatchDailyPackDetail()
	{
		RechargeActiveInfo rechargeActiveInfo = zbServerData.sGameData.RechargeActiveInfo;
		if (rechargeActiveInfo != null && rechargeActiveInfo.DailyPackInfo != null)
		{
			foreach (DailyPackDetail pack in rechargeActiveInfo.DailyPackInfo.PackList)
			{
				if (zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(pack.Id).type == 3)
				{
					return pack;
				}
			}
		}
		return null;
	}

	public int CheckVipFuncLimited(EVipFuncType funcType, int vipLevel)
	{
		zbVipCsvRecord recordByKey = zbDataConfig.Instance.vipCsvTable.GetRecordByKey(vipLevel);
		if (recordByKey == null)
		{
			return 0;
		}
		return funcType switch
		{
			EVipFuncType.HighSpeed => recordByKey.highSpeed, 
			EVipFuncType.WorldRecon2 => recordByKey.worldRecon2, 
			EVipFuncType.FightModRefresh => recordByKey.fightModRefresh, 
			EVipFuncType.IdleSweep => recordByKey.idleSweep, 
			EVipFuncType.LegionBossSweep => recordByKey.legionBossSweep, 
			EVipFuncType.WorldBossSweep => recordByKey.worldBossSweep, 
			EVipFuncType.QuickBuy => recordByKey.quickBuy, 
			EVipFuncType.AutoFight => recordByKey.autoFight, 
			EVipFuncType.TiliBuy => recordByKey.tiliBuy, 
			EVipFuncType.WorldLairBuy => recordByKey.worldLairBuy, 
			EVipFuncType.LegionBossBuy => recordByKey.legionBossBuy, 
			EVipFuncType.WorldBossBuy => recordByKey.worldBossBuy, 
			EVipFuncType.EndlessHordeBuy => recordByKey.endlessHordeBuy, 
			EVipFuncType.ElementTrialBuy => recordByKey.elementTrialBuy, 
			EVipFuncType.EliteQuestBuy => recordByKey.eliteQuestBuy, 
			EVipFuncType.PvpModRefresh => recordByKey.pvpModRefresh, 
			_ => 0, 
		};
	}

	public int CheckVipFuncUnlock(EVipFuncType funcType, int number = 1)
	{
		for (int i = 0; i < 12; i++)
		{
			if (CheckVipFuncLimited(funcType, i) >= number)
			{
				return i;
			}
		}
		return 0;
	}

	public bool HasVipCardShopNotifyByGoodsId(int goodsId)
	{
		VipCardShopDetail vipCardShopDetail = GetVipCardShopDetail(goodsId);
		if (vipCardShopDetail != null && vipCardShopDetail.DailyAward == EActiveState.Active)
		{
			return true;
		}
		return false;
	}

	public bool HasVipCardShopNotify()
	{
		for (int i = 0; i < 5; i++)
		{
			if (HasVipCardNotifyByTab((EVipCardTab)i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasVipCardNotifyByTab(EVipCardTab tab)
	{
		switch (tab)
		{
		case EVipCardTab.Week:
		{
			int num = (int)m_vipBaseRecord["vipCard"][(object)"resource"][(object)"goodsId"];
			if (zbServerData.sGameData == null || zbServerData.sGameData.RechargeShopInfo == null)
			{
				break;
			}
			foreach (VipCardShopDetail item in zbServerData.sGameData.RechargeShopInfo.VipCardShop)
			{
				if (item.GoodsId == num && HasVipCardShopNotifyByGoodsId(item.GoodsId))
				{
					return true;
				}
			}
			break;
		}
		case EVipCardTab.Truck:
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Truck) && zbSelf.Instance.truckHandler.IsTruckOpen())
			{
				return zbSelf.Instance.truckHandler.HasFreePackRedDot();
			}
			break;
		case EVipCardTab.Treasure:
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure))
			{
				return zbSelf.Instance.treasureHandler.HasFreePackRedDot();
			}
			break;
		case EVipCardTab.CrossLadder:
			if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.CrossLadder) && zbSelf.Instance.crossLadderHandler.IsCrossLadderOpen())
			{
				return zbSelf.Instance.crossLadderHandler.HasFreePackRedDot();
			}
			break;
		}
		return false;
	}

	public bool HasRechargeGift()
	{
		foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
		{
			if (zbSelf.Instance.operateHandler.IsShowTime(rechargeGift.BaseInfo) && rechargeGift.BaseInfo.Type != EOperatePlanType.Holiday)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRechargeNotify()
	{
		JourneyTargetRecordDetail journeyRechargeSumTargetDetail = GetJourneyRechargeSumTargetDetail();
		if (journeyRechargeSumTargetDetail == null || journeyRechargeSumTargetDetail.State != ECommonState.Yes)
		{
			RechargeTargetData rechargeTargetData = GetRechargeTargetData();
			if (rechargeTargetData == null)
			{
				return false;
			}
			return rechargeTargetData.TargetDetail.State == ECommonState.Yes;
		}
		return true;
	}

	public JourneyTargetRecordDetail GetJourneyRechargeSumTargetDetail()
	{
		if (zbSelf.Instance.journeyOneHandler.GetEndLeftTime() > 0)
		{
			foreach (JourneyTargetRecordDetail target in zbServerData.sGameData.JourneyInfo.ChapterOne.RechargeSum.TargetList)
			{
				if (target.State == ECommonState.Yes)
				{
					return target;
				}
			}
			foreach (JourneyTargetRecordDetail target2 in zbServerData.sGameData.JourneyInfo.ChapterOne.RechargeSum.TargetList)
			{
				if (target2.State == ECommonState.No)
				{
					return target2;
				}
			}
		}
		return null;
	}

	public RechargeTargetData GetRechargeTargetData()
	{
		RechargeTargetData rechargeTargetData = null;
		if (zbServerData.sGameData.OperateInfo.RechargeSumList != null)
		{
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeSum.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target in rechargeSum.TargetList)
				{
					if (target.State == ECommonState.Yes)
					{
						return new RechargeTargetData(target, rechargeSum.Count, rechargeSum.BaseInfo.UUID, EOperateType.RechargeSum);
					}
				}
				foreach (RechargeTargetDetail target2 in rechargeSum.TargetList)
				{
					if (target2.State == ECommonState.No)
					{
						rechargeTargetData = new RechargeTargetData(target2, rechargeSum.Count, rechargeSum.BaseInfo.UUID, EOperateType.RechargeSum);
						break;
					}
				}
			}
		}
		RechargeTargetData rechargeTargetData2 = null;
		if (zbServerData.sGameData.OperateInfo.RechargeDailySumList != null)
		{
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeDailySum.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target3 in rechargeDailySum.TargetList)
				{
					if (target3.State == ECommonState.Yes)
					{
						return new RechargeTargetData(target3, rechargeDailySum.Count, rechargeDailySum.BaseInfo.UUID, EOperateType.RechargeDailySum);
					}
				}
				foreach (RechargeTargetDetail target4 in rechargeDailySum.TargetList)
				{
					if (target4.State == ECommonState.No)
					{
						rechargeTargetData2 = new RechargeTargetData(target4, rechargeDailySum.Count, rechargeDailySum.BaseInfo.UUID, EOperateType.RechargeDailySum);
						break;
					}
				}
			}
		}
		RechargeTargetData rechargeTargetData3 = null;
		if (zbServerData.sGameData.OperateInfo.RechargeDailyList != null)
		{
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeDaily.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target5 in rechargeDaily.TargetList)
				{
					if (target5.State == ECommonState.Yes)
					{
						return new RechargeTargetData(target5, rechargeDaily.Count, rechargeDaily.BaseInfo.UUID, EOperateType.RechargeDaily);
					}
				}
				foreach (RechargeTargetDetail target6 in rechargeDaily.TargetList)
				{
					if (target6.State == ECommonState.No)
					{
						rechargeTargetData3 = new RechargeTargetData(target6, rechargeDaily.Count, rechargeDaily.BaseInfo.UUID, EOperateType.RechargeDaily);
						break;
					}
				}
			}
		}
		return CompareRechargeTargetDatas(rechargeTargetData, rechargeTargetData2, rechargeTargetData3);
	}

	private RechargeTargetData CompareRechargeTargetDatas(params RechargeTargetData[] datas)
	{
		int num = int.MaxValue;
		RechargeTargetData result = null;
		foreach (RechargeTargetData rechargeTargetData in datas)
		{
			if (rechargeTargetData != null)
			{
				int num2 = rechargeTargetData.TargetDetail.Target - rechargeTargetData.Count;
				if (num2 > 0 && num2 < num)
				{
					num = num2;
					result = rechargeTargetData;
				}
			}
		}
		return result;
	}

	public List<zbViptdescCsvRecord> GetVipTqDescByLv(int viplv)
	{
		zbViptdescCsvTableData records = zbDataConfig.Instance.viptdescCsvTable.GetRecords();
		if (records == null || records.records == null)
		{
			return null;
		}
		return (from rec in records.records
			where rec.level == viplv
			orderby rec.id
			select rec).ToList();
	}

	public int GetDayFreeExp(int day, bool bGetNextDay = false)
	{
		int result = 0;
		if (bGetNextDay)
		{
			int num = day;
			foreach (KeyValuePair<int, zbVipFreeExpCsvRecord> record in zbDataConfig.Instance.vipFreeExpCsvTable.GetRecords().records)
			{
				if (record.Value.day > num)
				{
					num = record.Value.day;
				}
			}
			day++;
			if (day > num)
			{
				day = 1;
			}
		}
		zbVipFreeExpCsvRecord recordByKey = zbDataConfig.Instance.vipFreeExpCsvTable.GetRecordByKey(day);
		if (recordByKey != null)
		{
			result = recordByKey.vipExp;
		}
		return result;
	}

	public List<zbItemData> GetReceivedDailyFreeGiftAward(int from)
	{
		if (from < 0)
		{
			return new List<zbItemData>();
		}
		return MergeDailyFreeGiftAward(0, from);
	}

	public List<zbItemData> GetDailyFreeGiftAward(int from, int to)
	{
		return MergeDailyFreeGiftAward(from, to);
	}

	public List<zbItemData> GetAllVipDailyFreeGiftAwards()
	{
		return MergeDailyFreeGiftAward(0, 15);
	}

	public List<zbItemData> MergeDailyFreeGiftAward(int startLv, int endLv)
	{
		List<zbItemData> list = new List<zbItemData>();
		zbVipLevelPackCsvTableData records = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecords();
		if (records == null || records.records == null)
		{
			return list;
		}
		Dictionary<long, zbItemData> dictionary = new Dictionary<long, zbItemData>();
		for (int i = startLv; i <= endLv; i++)
		{
			if (!records.records.TryGetValue(i, out var value) || value.dailyFreeAward == null)
			{
				continue;
			}
			int[][] dailyFreeAward = value.dailyFreeAward;
			foreach (int[] array in dailyFreeAward)
			{
				if (array != null && array.Length >= 3)
				{
					long key = ((long)array[0] << 32) | (uint)array[1];
					if (dictionary.TryGetValue(key, out var value2))
					{
						value2.count += array[2];
					}
					else
					{
						dictionary[key] = new zbItemData((EResourceType)array[0], array[1], array[2]);
					}
				}
			}
		}
		list.AddRange(dictionary.Values);
		return list;
	}

	public List<zbItemData> GetAllVipTqGiftAward()
	{
		List<zbItemData> list = new List<zbItemData>();
		zbVipLevelPackCsvTableData records = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecords();
		if (records == null || records.records == null)
		{
			return list;
		}
		Dictionary<long, zbItemData> dictionary = new Dictionary<long, zbItemData>();
		foreach (zbVipLevelPackCsvRecord value2 in records.records.Values)
		{
			if (value2 == null)
			{
				continue;
			}
			zbVipShopCsvRecord recordByKey = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(value2.goodsId);
			if (recordByKey == null || recordByKey.awardList == null)
			{
				continue;
			}
			int[][] awardList = recordByKey.awardList;
			foreach (int[] array in awardList)
			{
				if (array != null && array.Length >= 3)
				{
					long key = ((long)array[0] << 32) | (uint)array[1];
					if (dictionary.TryGetValue(key, out var value))
					{
						value.count += array[2];
					}
					else
					{
						dictionary[key] = new zbItemData((EResourceType)array[0], array[1], array[2]);
					}
				}
			}
		}
		list.AddRange(dictionary.Values);
		return list;
	}

	public List<zbItemData> GetVipTqGiftAward(int vipLv)
	{
		List<zbItemData> list = new List<zbItemData>();
		zbVipLevelPackCsvRecord recordByKey = zbDataConfig.Instance.vipLevelPackCsvTable.GetRecordByKey(vipLv);
		if (recordByKey == null)
		{
			return list;
		}
		zbVipShopCsvRecord recordByKey2 = zbDataConfig.Instance.vipShopCsvTable.GetRecordByKey(recordByKey.goodsId);
		if (recordByKey2 == null || recordByKey2.awardList == null)
		{
			return list;
		}
		int[][] awardList = recordByKey2.awardList;
		foreach (int[] array in awardList)
		{
			if (array != null && array.Length >= 3)
			{
				list.Add(new zbItemData((EResourceType)array[0], array[1], array[2]));
			}
		}
		return list;
	}

	public bool hasVipFreeExp()
	{
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		if (vipInfo.VipFreeExp != null)
		{
			return vipInfo.VipFreeExp.State == EBoolState.False;
		}
		return true;
	}

	public bool hasDailyFreeGift()
	{
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		int vipLevel = zbSelf.Instance.GetVipLevel();
		if (vipLevel <= 0)
		{
			return false;
		}
		return vipLevel > vipInfo.DailyFree;
	}

	public bool hasDailyFreeGift(int tabVipLv)
	{
		if (!hasDailyFreeGift())
		{
			return false;
		}
		VipInfo vipInfo = zbServerData.sGameData.VipInfo;
		int vipLevel = zbSelf.Instance.GetVipLevel();
		if (tabVipLv > vipInfo.DailyFree)
		{
			return tabVipLv <= vipLevel;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (!HasDailyPackInfoNotify() && !HasVipCardShopNotify() && (!HasRechargeGift() || !zbSelf.Instance.operateHandler.HasRechargeGiftNotify(1)))
		{
			return HasRechargeNotify();
		}
		return true;
	}
}
