using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbPassPortHandler
{
	private zbZhanLingMainCsvTableData m_records;

	private List<int> m_cachedGoodIds;

	public void Init()
	{
		m_records = zbDataConfig.Instance.zhanLingMainCsvTable.GetRecords();
		m_cachedGoodIds = m_records.records.Values.Select((zbZhanLingMainCsvRecord r) => r.goodsId).Distinct().ToList();
	}

	private List<int> GetGoodIds()
	{
		return m_cachedGoodIds;
	}

	public List<zbZhanLingMainCsvRecord> GetZhanlingMainRecordsByStage(int stage)
	{
		if (stage <= 0)
		{
			return new List<zbZhanLingMainCsvRecord>();
		}
		List<int> goodIds = GetGoodIds();
		if (goodIds == null || stage > goodIds.Count)
		{
			return new List<zbZhanLingMainCsvRecord>();
		}
		int num = goodIds[stage - 1];
		List<zbZhanLingMainCsvRecord> list = new List<zbZhanLingMainCsvRecord>();
		if (m_records?.records != null)
		{
			foreach (KeyValuePair<int, zbZhanLingMainCsvRecord> record in m_records.records)
			{
				if (record.Value != null && record.Value.goodsId == num)
				{
					list.Add(record.Value);
				}
			}
		}
		return list;
	}

	public ZhanLingMainActiveDetail GetZhanlingMainActiveDetailByStage(int stage)
	{
		if (stage <= 0)
		{
			return null;
		}
		List<int> goodIds = GetGoodIds();
		if (goodIds == null || stage > goodIds.Count)
		{
			return null;
		}
		int num = goodIds[stage - 1];
		RepeatedField<ZhanLingMainActiveDetail> val = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingMainInfo?.ActiveList;
		if (val == null)
		{
			return null;
		}
		foreach (ZhanLingMainActiveDetail item in val)
		{
			if (item != null && num == item.GoodsId)
			{
				return item;
			}
		}
		return null;
	}

	public bool IsCurrentZhanling(int stage, int itemLevel)
	{
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(stage);
		if (zhanlingMainRecordsByStage == null || zhanlingMainRecordsByStage.Count == 0)
		{
			return false;
		}
		int reach = (zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingMainInfo?.Reach).GetValueOrDefault();
		zbZhanLingMainCsvRecord zbZhanLingMainCsvRecord2 = (from x in zhanlingMainRecordsByStage
			where x != null && x.mainLevel <= reach
			orderby x.mainLevel descending
			select x).FirstOrDefault();
		zbZhanLingMainCsvRecord zbZhanLingMainCsvRecord3 = zhanlingMainRecordsByStage.OrderBy((zbZhanLingMainCsvRecord x) => x?.mainLevel).LastOrDefault();
		if (zbZhanLingMainCsvRecord2 != null && zbZhanLingMainCsvRecord3 != null && zbZhanLingMainCsvRecord2.mainLevel == zbZhanLingMainCsvRecord3.mainLevel)
		{
			return false;
		}
		if (zbZhanLingMainCsvRecord2 != null)
		{
			return zbZhanLingMainCsvRecord2.mainLevel == itemLevel;
		}
		return false;
	}

	public int GetCurrentStage()
	{
		int result = 1;
		zbZhanLingMainCsvRecord recordByKey = zbDataConfig.Instance.zhanLingMainCsvTable.GetRecordByKey(zbServerData.sGameData.MainQuestInfo.MainLevel);
		List<int> goodIds = GetGoodIds();
		if (recordByKey != null)
		{
			if (goodIds.IndexOf(recordByKey.goodsId) <= -1)
			{
				return result;
			}
			return goodIds.IndexOf(recordByKey.goodsId) + 1;
		}
		return result;
	}

	public bool IsGrandPrize(int stage, int mainlevel)
	{
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(stage);
		if (zhanlingMainRecordsByStage == null || zhanlingMainRecordsByStage.Count == 0)
		{
			return false;
		}
		zbZhanLingMainCsvRecord obj = zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1];
		if (obj == null)
		{
			return false;
		}
		return obj.mainLevel == mainlevel;
	}

	public int GetCurrentItemIndex(int stage)
	{
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(stage);
		if (zhanlingMainRecordsByStage == null || zhanlingMainRecordsByStage.Count == 0)
		{
			return 0;
		}
		int reach = (zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingMainInfo?.Reach).GetValueOrDefault();
		int num = zhanlingMainRecordsByStage.FindLastIndex((zbZhanLingMainCsvRecord r) => r != null && r.mainLevel <= reach);
		if (num == -1)
		{
			return 0;
		}
		if (num == zhanlingMainRecordsByStage.Count - 1)
		{
			return 0;
		}
		if (num != 0)
		{
			num = Mathf.Min(num + 3, zhanlingMainRecordsByStage.Count - 3);
		}
		return num;
	}

	public bool HasRedPointByPassStage(int passStage)
	{
		ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingMainInfo;
		if (zhanLingMainInfo == null)
		{
			return false;
		}
		List<int> goodIds = GetGoodIds();
		if (goodIds == null || passStage <= 0 || passStage > goodIds.Count)
		{
			return false;
		}
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(passStage);
		if (zhanlingMainRecordsByStage == null || zhanlingMainRecordsByStage.Count == 0)
		{
			return false;
		}
		int mainLevel = zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel;
		int mainLevel2 = Mathf.Min(zhanLingMainInfo.Reach, mainLevel);
		ZhanLingMainActiveDetail zhanLingMainActiveDetail = null;
		if (zhanLingMainInfo.ActiveList != null)
		{
			foreach (ZhanLingMainActiveDetail active in zhanLingMainInfo.ActiveList)
			{
				if (active != null && active.Active == EBoolState.True && goodIds.IndexOf(active.GoodsId) == passStage - 1)
				{
					zhanLingMainActiveDetail = active;
					break;
				}
			}
		}
		if (zhanLingMainActiveDetail == null)
		{
			return HasNextCanReceiveAward(mainLevel2, zhanLingMainInfo.LowAward);
		}
		return HasNextCanReceiveAward(mainLevel2, zhanLingMainActiveDetail.HighAward);
	}

	public bool HasMainQuestRedPoint()
	{
		ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingMainInfo;
		if (zhanLingMainInfo == null)
		{
			return false;
		}
		if (HasNextCanReceiveAward(zhanLingMainInfo.Reach, zhanLingMainInfo.LowAward))
		{
			return true;
		}
		if (zhanLingMainInfo.ActiveList != null)
		{
			foreach (ZhanLingMainActiveDetail active in zhanLingMainInfo.ActiveList)
			{
				if (active == null || active.HighAward <= 0 || active.Active != EBoolState.True)
				{
					continue;
				}
				List<int> goodIds = GetGoodIds();
				if (goodIds == null)
				{
					continue;
				}
				int num = goodIds.IndexOf(active.GoodsId);
				if (num < 0)
				{
					continue;
				}
				List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(num + 1);
				if (zhanlingMainRecordsByStage != null && zhanlingMainRecordsByStage.Count != 0)
				{
					int mainLevel = zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel;
					int mainLevel2 = Mathf.Min(zhanLingMainInfo.Reach, mainLevel);
					if (HasNextCanReceiveAward(mainLevel2, active.HighAward))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasNextCanReceiveAward(int mainLevel, int receivedMainLevel)
	{
		foreach (zbZhanLingMainCsvRecord value in m_records.records.Values)
		{
			if (value.mainLevel > receivedMainLevel && value.mainLevel <= mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasPassportRedPoint()
	{
		PassportInfo passportInfo = zbServerData.sGameData.RechargeActiveInfo.PassportInfo;
		bool num = passportInfo.BaseInfo.Active == EBoolState.True;
		int level = passportInfo.LevelInfo.Level;
		if (!num)
		{
			return level > passportInfo.LevelInfo.LowAward;
		}
		if (level <= passportInfo.LevelInfo.LowAward)
		{
			return level > passportInfo.LevelInfo.HighAward;
		}
		return true;
	}

	public bool IsCurrentMainZhanlingDone(int stage)
	{
		List<zbZhanLingMainCsvRecord> zhanlingMainRecordsByStage = GetZhanlingMainRecordsByStage(stage);
		ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingMainInfo;
		return zhanlingMainRecordsByStage[zhanlingMainRecordsByStage.Count - 1].mainLevel <= zhanLingMainInfo.Reach;
	}
}
