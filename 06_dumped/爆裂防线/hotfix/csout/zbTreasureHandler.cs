using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbTreasureHandler
{
	public Dictionary<int, zbTreasureCsvRecord[]> dictionaryRecords = new Dictionary<int, zbTreasureCsvRecord[]>();

	public TreasureLevelChange[] m_changes;

	public int targetNum;

	public void Init()
	{
		zbTreasureCsvRecord[] source = zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values.Where((zbTreasureCsvRecord r) => zbVersionMgr.IsReleased(r.version)).ToArray();
		dictionaryRecords = (from r in source
			group r by r.quality into g
			orderby g.Key descending
			select g).ToDictionary((IGrouping<int, zbTreasureCsvRecord> g) => g.Key, (IGrouping<int, zbTreasureCsvRecord> g) => g.ToArray());
		targetNum = zbDataConfig.Instance.treasurePrivilegeCsvTable.GetRecordByKey(1).val;
	}

	public int GetMaxLevel(int quality)
	{
		return zbDataConfig.Instance.treasureQualityCsvTable.GetRecords().records.FirstOrDefault((zbTreasureQualityCsvRecord r) => r.quality == quality)?.maxLvl ?? 20;
	}

	public void UpdateTreasureInfo(TreasureInfo treasureInfo)
	{
		TreasureInfo treasureInfo2 = zbServerData.sGameData.TreasureInfo;
		m_changes = CompareTreasureInfo(treasureInfo2, treasureInfo);
		zbServerData.sGameData.TreasureInfo = treasureInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_TreasureInfo);
	}

	public static TreasureLevelChange[] CompareTreasureInfo(TreasureInfo oldInfo, TreasureInfo newInfo)
	{
		if (newInfo == null || newInfo.TreasureList == null)
		{
			return Array.Empty<TreasureLevelChange>();
		}
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		if (oldInfo != null && oldInfo.TreasureList != null)
		{
			foreach (TreasureDetail treasure in oldInfo.TreasureList)
			{
				dictionary[treasure.Id] = treasure.Level;
			}
		}
		List<TreasureLevelChange> list = new List<TreasureLevelChange>();
		foreach (TreasureDetail treasure2 in newInfo.TreasureList)
		{
			int value = 0;
			bool flag = dictionary.TryGetValue(treasure2.Id, out value);
			if (!flag || value != treasure2.Level)
			{
				list.Add(new TreasureLevelChange
				{
					Id = treasure2.Id,
					OldLevel = (flag ? value : 0),
					NewLevel = treasure2.Level
				});
			}
		}
		list.Sort(delegate(TreasureLevelChange a, TreasureLevelChange b)
		{
			int quality = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(a.Id).quality;
			return zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(b.Id).quality.CompareTo(quality);
		});
		return list.ToArray();
	}

	public int GetTreasureLevel(int treasureId)
	{
		if (zbServerData.sGameData?.TreasureInfo?.TreasureList != null)
		{
			foreach (TreasureDetail treasure in zbServerData.sGameData.TreasureInfo.TreasureList)
			{
				if (treasure.Id == treasureId)
				{
					return treasure.Level;
				}
			}
		}
		return 0;
	}

	public bool HasTreasureActiveNotify(int treasureId)
	{
		if (GetTreasureLevel(treasureId) == 0)
		{
			zbTreasureCsvRecord recordByKey = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(treasureId);
			zbTreasureUpgradeCsvRecord recordByKey2 = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(1);
			if (zbSelf.Instance.GetLevel() >= recordByKey2.roleLvl && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, recordByKey2.costNum))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTreasureUpgradeNotify(int treasureId)
	{
		int treasureLevel = GetTreasureLevel(treasureId);
		zbTreasureCsvRecord recordByKey = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(treasureId);
		if (treasureLevel > 0 && treasureLevel < GetMaxLevel(recordByKey.quality))
		{
			zbTreasureUpgradeCsvRecord recordByKey2 = zbDataConfig.Instance.treasureUpgradeCsvTable.GetRecordByKey(treasureLevel + 1);
			if (zbSelf.Instance.GetLevel() >= recordByKey2.roleLvl && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, recordByKey2.costNum))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyTreasureActiveNotify()
	{
		foreach (zbTreasureCsvRecord value in zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values)
		{
			if (HasTreasureActiveNotify(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasAnyTreasureUpgradeNotify()
	{
		foreach (zbTreasureCsvRecord value in zbDataConfig.Instance.treasureCsvTable.GetRecords().records.Values)
		{
			if (HasTreasureUpgradeNotify(value.id))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasTreasureNofity()
	{
		if (!HasAnyTreasureActiveNotify())
		{
			return HasAnyTreasureUpgradeNotify();
		}
		return true;
	}

	public bool HasFreePackRedDot()
	{
		foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
		{
			if (freeGift.FreeType == 8 && freeGift.State == EBoolState.False && !zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Treasure))
		{
			return false;
		}
		if (!HasTreasureNofity() && !HasFreePackRedDot())
		{
			return zbSelf.Instance.barHandler.HasTreasureBarRedPoint();
		}
		return true;
	}

	public bool IsTreasurePrivilegeActive()
	{
		RechargeActiveInfo rechargeActiveInfo = zbServerData.sGameData.RechargeActiveInfo;
		if (rechargeActiveInfo == null)
		{
			return false;
		}
		return rechargeActiveInfo.TreasurePrivilegeInfo.Active == EBoolState.True;
	}

	public List<TreasureDetail> GetTreasureShouldConvert()
	{
		List<TreasureDetail> list = new List<TreasureDetail>();
		if (zbServerData.sGameData.TreasureInfo != null)
		{
			foreach (TreasureDetail treasure in zbServerData.sGameData.TreasureInfo.TreasureList)
			{
				zbTreasureCsvRecord recordByKey = zbDataConfig.Instance.treasureCsvTable.GetRecordByKey(treasure.Id);
				if (recordByKey != null)
				{
					bool flag = zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Prop, recordByKey.propId, 1);
					if ((treasure.Level >= GetMaxLevel(recordByKey.quality)) & flag)
					{
						list.Add(treasure);
					}
				}
			}
		}
		return list;
	}
}
