using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbLegendaryHandler
{
	public zbItemData[] m_costList;

	public long playerId;

	private zbZhanLingLegendaryCsvTableData m_tableData;

	public int m_zhanLingCount;

	private List<int> m_cachedGoodIds;

	public void Init()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		JToken val = zbDataConfig.Instance.questBaseJsonTable.GetRecord()["legedary"][(object)"costList"];
		JArray val2 = (JArray)(object)((val is JArray) ? val : null);
		if (val2 != null)
		{
			m_costList = new zbItemData[((JContainer)val2).Count];
			for (int i = 0; i < ((JContainer)val2).Count; i++)
			{
				JArray val3 = (JArray)val2[i];
				m_costList[i] = new zbItemData((EResourceType)Extensions.Value<int>((IEnumerable<JToken>)val3[0]), Extensions.Value<long>((IEnumerable<JToken>)val3[1]), Extensions.Value<long>((IEnumerable<JToken>)val3[2]));
			}
		}
		else
		{
			Debug.LogError((object)("costList is not a JArray: " + (object)val));
		}
		m_tableData = zbDataConfig.Instance.zhanLingLegendaryCsvTable.GetRecords();
		m_zhanLingCount = m_tableData.records.Count;
		m_cachedGoodIds = m_tableData.records.Values.Select((zbZhanLingLegendaryCsvRecord r) => r.goodsId).Distinct().ToList();
	}

	public void UpdateLegendaryInfo(LegendaryInfo legendaryInfo)
	{
		zbServerData.sGameData.LegendaryInfo = legendaryInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_LegendaryInfo);
	}

	private List<int> GetGoodIds()
	{
		return m_cachedGoodIds;
	}

	public List<zbZhanLingLegendaryCsvRecord> GetZhanlingRecordsByStage(int stage)
	{
		if (stage <= 0)
		{
			return new List<zbZhanLingLegendaryCsvRecord>();
		}
		List<int> goodIds = GetGoodIds();
		if (goodIds == null || stage > goodIds.Count)
		{
			return new List<zbZhanLingLegendaryCsvRecord>();
		}
		int num = goodIds[stage - 1];
		List<zbZhanLingLegendaryCsvRecord> list = new List<zbZhanLingLegendaryCsvRecord>();
		if (m_tableData?.records != null)
		{
			foreach (KeyValuePair<int, zbZhanLingLegendaryCsvRecord> record in m_tableData.records)
			{
				if (record.Value != null && record.Value.goodsId == num)
				{
					list.Add(record.Value);
				}
			}
		}
		return list;
	}

	public ZhanLingMainActiveDetail GetZhanlingActiveDetailByStage(int stage)
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
		RepeatedField<ZhanLingMainActiveDetail> val = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingLegendaryInfo?.ActiveList;
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
		List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(stage);
		if (zhanlingRecordsByStage == null || zhanlingRecordsByStage.Count == 0)
		{
			return false;
		}
		int reach = (zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingLegendaryInfo?.Reach).GetValueOrDefault();
		zbZhanLingLegendaryCsvRecord zbZhanLingLegendaryCsvRecord2 = (from x in zhanlingRecordsByStage
			where x != null && x.lv <= reach
			orderby x.lv descending
			select x).FirstOrDefault();
		zbZhanLingLegendaryCsvRecord zbZhanLingLegendaryCsvRecord3 = zhanlingRecordsByStage.OrderBy((zbZhanLingLegendaryCsvRecord x) => x?.lv).LastOrDefault();
		if (zbZhanLingLegendaryCsvRecord2 != null && zbZhanLingLegendaryCsvRecord3 != null && zbZhanLingLegendaryCsvRecord2.lv == zbZhanLingLegendaryCsvRecord3.lv)
		{
			return false;
		}
		if (zbZhanLingLegendaryCsvRecord2 != null)
		{
			return zbZhanLingLegendaryCsvRecord2.lv == itemLevel;
		}
		return false;
	}

	public int GetCurrentStage()
	{
		int result = 1;
		if (zbServerData.sGameData.LegendaryInfo.PassLevel > 0)
		{
			zbZhanLingLegendaryCsvRecord recordByKey = zbDataConfig.Instance.zhanLingLegendaryCsvTable.GetRecordByKey(zbServerData.sGameData.LegendaryInfo.PassLevel);
			List<int> goodIds = GetGoodIds();
			if (recordByKey != null)
			{
				if (goodIds.IndexOf(recordByKey.goodsId) <= -1)
				{
					return result;
				}
				return goodIds.IndexOf(recordByKey.goodsId) + 1;
			}
		}
		return result;
	}

	public bool IsInZhanlingGetWay(EResourceType type, long id)
	{
		for (int i = 1; i <= GetCurrentStage(); i++)
		{
			foreach (zbZhanLingLegendaryCsvRecord item in GetZhanlingRecordsByStage(i))
			{
				int[][] lowAward = item.lowAward;
				foreach (int[] array in lowAward)
				{
					if (array[0] == (int)type && array[1] == id)
					{
						return true;
					}
				}
				lowAward = item.highAward;
				foreach (int[] array2 in lowAward)
				{
					if (array2[0] == (int)type && array2[1] == id)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool IsGrandPrize(int stage, int mainlevel)
	{
		List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(stage);
		if (zhanlingRecordsByStage == null || zhanlingRecordsByStage.Count == 0)
		{
			return false;
		}
		foreach (zbZhanLingLegendaryCsvRecord item in zhanlingRecordsByStage)
		{
			if (item.lv == mainlevel && item.isGrand > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int GetCurrentItemIndex(int stage)
	{
		List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(stage);
		if (zhanlingRecordsByStage == null || zhanlingRecordsByStage.Count == 0)
		{
			return 0;
		}
		int reach = (zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingLegendaryInfo?.Reach).GetValueOrDefault();
		int num = zhanlingRecordsByStage.FindLastIndex((zbZhanLingLegendaryCsvRecord r) => r != null && r.lv <= reach);
		if (num == -1)
		{
			return 0;
		}
		if (num == zhanlingRecordsByStage.Count - 1)
		{
			return 0;
		}
		if (num != 0)
		{
			num = Mathf.Min(num + 3, zhanlingRecordsByStage.Count - 3);
		}
		return num;
	}

	public bool IsCurrentZhanlingDone(int stage)
	{
		List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(stage);
		ZhanLingMainInfo zhanLingLegendaryInfo = zbServerData.sGameData.RechargeActiveInfo.ZhanLingLegendaryInfo;
		return zhanlingRecordsByStage[zhanlingRecordsByStage.Count - 1].lv <= zhanLingLegendaryInfo.Reach;
	}

	public bool HasRedPointByPassStage(int passStage)
	{
		ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingLegendaryInfo;
		if (zhanLingMainInfo == null)
		{
			return false;
		}
		List<int> goodIds = GetGoodIds();
		if (goodIds == null || passStage <= 0 || passStage > goodIds.Count)
		{
			return false;
		}
		List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(passStage);
		if (zhanlingRecordsByStage == null || zhanlingRecordsByStage.Count == 0)
		{
			return false;
		}
		int lv = zhanlingRecordsByStage[zhanlingRecordsByStage.Count - 1].lv;
		int mainLevel = Mathf.Min(zhanLingMainInfo.Reach, lv);
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
			return HasNextCanReceiveAward(mainLevel, zhanLingMainInfo.LowAward);
		}
		return HasNextCanReceiveAward(mainLevel, zhanLingMainActiveDetail.HighAward);
	}

	public bool HasNextCanReceiveAward(int mainLevel, int receivedLevel)
	{
		foreach (zbZhanLingLegendaryCsvRecord value in m_tableData.records.Values)
		{
			if (value.lv > receivedLevel && value.lv <= mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasZhanLingNotify()
	{
		ZhanLingMainInfo zhanLingMainInfo = zbServerData.sGameData?.RechargeActiveInfo?.ZhanLingLegendaryInfo;
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
				if (active == null || active.Active != EBoolState.True)
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
				List<zbZhanLingLegendaryCsvRecord> zhanlingRecordsByStage = GetZhanlingRecordsByStage(num + 1);
				if (zhanlingRecordsByStage != null && zhanlingRecordsByStage.Count != 0)
				{
					int lv = zhanlingRecordsByStage[zhanlingRecordsByStage.Count - 1].lv;
					int mainLevel = Mathf.Min(zhanLingMainInfo.Reach, lv);
					if (HasNextCanReceiveAward(mainLevel, active.HighAward))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool HasFightNotify()
	{
		bool flag = true;
		zbItemData[] costList = zbSelf.Instance.legendaryHandler.m_costList;
		foreach (zbItemData zbItemData2 in costList)
		{
			if (!zbSelf.Instance.dataHandler.HasResourceEnough(zbItemData2.type, (int)zbItemData2.id, (int)zbItemData2.count))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			zbLegendaryLevelCsvRecord recordByKey = zbDataConfig.Instance.legendaryLevelCsvTable.GetRecordByKey(zbServerData.sGameData.LegendaryInfo.NextLevel);
			if (recordByKey != null && zbServerData.sGameData.MainQuestInfo.MainLevel >= recordByKey.mainLevel)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasFreeGiftNotify()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 6 && freeGift.State == EBoolState.False)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legendary))
		{
			return false;
		}
		if (!HasFreeGiftNotify() && !HasFightNotify())
		{
			return HasZhanLingNotify();
		}
		return true;
	}
}
