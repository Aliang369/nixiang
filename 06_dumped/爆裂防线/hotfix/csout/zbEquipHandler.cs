using System.Collections.Generic;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbEquipHandler
{
	private string m_equipLevelUpgradeKey;

	private long m_equipLevelUpgradeKeyLastSaveTime;

	public bool m_todayRedPointUsed;

	private static readonly List<EquipDetail> filteredEquips = new List<EquipDetail>(128);

	private static readonly HashSet<int> seenTidSet = new HashSet<int>();

	public void Init()
	{
		m_equipLevelUpgradeKey = "isEquipLevelUpgradeRedDotShow" + zbSelf.Instance.GetRoleId();
		m_equipLevelUpgradeKeyLastSaveTime = zbGameSave.Instance.GetLong(m_equipLevelUpgradeKey, 0L);
		m_todayRedPointUsed = m_equipLevelUpgradeKeyLastSaveTime < zbSelf.Instance.timeHandler.GetRefreshTime();
	}

	public void UpdateEquipSlotInfo(EquipSlotInfo EquipSlotInfo)
	{
		zbServerData.sGameData.EquipSlotInfo = EquipSlotInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_EquipSlotInfo);
	}

	public EquipSlotDetail GetEquipSlotInfo(EEquipType equipType, EquipSlotInfo slotInfo = null)
	{
		foreach (EquipSlotDetail equipSlot in (slotInfo ?? zbServerData.sGameData.EquipSlotInfo).EquipSlotList)
		{
			if (equipSlot.EquipType == equipType)
			{
				return equipSlot;
			}
		}
		return null;
	}

	public EquipDetail GetEquipSlotEquip(EEquipType equipType)
	{
		return GetEquipSlotInfo(equipType)?.Equip;
	}

	public EquipDetail GetEquipDetail(string equipId)
	{
		foreach (EquipDetail equip in zbServerData.sGameData.EquipList)
		{
			if (equip.EquipId == equipId)
			{
				return equip;
			}
		}
		return null;
	}

	public float GetEquipMainAttrValue(int equipTid, EQuality quality, bool isNextLevel = false)
	{
		float result = 0f;
		zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equipTid);
		zbEquipQualityCsvRecord recordByKey2 = zbDataConfig.Instance.equipQualityCsvTable.GetRecordByKey((int)quality);
		if (recordByKey != null)
		{
			EquipSlotDetail equipSlotInfo = GetEquipSlotInfo((EEquipType)recordByKey.type);
			result = (isNextLevel ? ((float)recordByKey.teamAtk[0] + (float)(equipSlotInfo.Level * recordByKey.teamAtk[1]) * recordByKey2.rate) : ((float)recordByKey.teamAtk[0] + (float)((equipSlotInfo.Level - 1) * recordByKey.teamAtk[1]) * recordByKey2.rate));
		}
		return result;
	}

	public bool HasEquipSuit()
	{
		foreach (EquipSlotDetail equipSlot in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			if (equipSlot.Equip == null)
			{
				return false;
			}
		}
		return true;
	}

	public float RemainingEquipSetActivationPercent()
	{
		int num = 0;
		int num2 = 0;
		foreach (EquipSlotDetail equipSlot in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			num++;
			if (equipSlot.Equip != null)
			{
				num2++;
			}
		}
		if (!HasEquipSuit())
		{
			if (num == 0)
			{
				return 100f;
			}
			return (1f - (float)num2 / (float)num) * 100f;
		}
		EQuality equipSuitQuality = GetEquipSuitQuality();
		if (equipSuitQuality >= EQuality.Orange)
		{
			return 0f;
		}
		EQuality eQuality = equipSuitQuality + 1;
		int num3 = 0;
		foreach (EquipSlotDetail equipSlot2 in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			if (equipSlot2.Equip != null && equipSlot2.Equip.Quality >= eQuality)
			{
				num3++;
			}
		}
		if (num2 == 0)
		{
			return 100f;
		}
		return (1f - (float)num3 / (float)num2) * 100f;
	}

	public EQuality GetEquipSuitQuality()
	{
		EQuality eQuality = EQuality.Orange;
		foreach (EquipSlotDetail equipSlot in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			if (equipSlot.Equip != null && equipSlot.Equip.Quality < eQuality)
			{
				eQuality = equipSlot.Equip.Quality;
			}
		}
		return eQuality;
	}

	public bool HasGemSuit()
	{
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid <= 0)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public float RemainingGemSetActivationPercent()
	{
		int num = 0;
		int num2 = 0;
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					num++;
					if (gemSlot.GemTid > 0)
					{
						num2++;
					}
				}
			}
			break;
		}
		if (!HasGemSuit())
		{
			if (num == 0)
			{
				return 100f;
			}
			return (1f - (float)num2 / (float)num) * 100f;
		}
		EQuality gemSuitQuality = GetGemSuitQuality();
		if (gemSuitQuality >= EQuality.Orange)
		{
			return 0f;
		}
		EQuality eQuality = gemSuitQuality + 1;
		int num3 = 0;
		foreach (GemPlanDetail gemPlan2 in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan2.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot2 in gemPlan2.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot2 in gemPlanSlot2.GemSlotList)
				{
					if (gemSlot2.GemTid > 0)
					{
						zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemSlot2.GemTid);
						if (recordByKey != null && recordByKey.quality >= (int)eQuality)
						{
							num3++;
						}
					}
				}
			}
			break;
		}
		if (num2 == 0)
		{
			return 100f;
		}
		return (1f - (float)num3 / (float)num2) * 100f;
	}

	public EQuality GetGemSuitQuality()
	{
		EQuality eQuality = EQuality.Orange;
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid > 0)
					{
						zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemSlot.GemTid);
						if (recordByKey != null && recordByKey.quality < (int)eQuality)
						{
							eQuality = (EQuality)recordByKey.quality;
						}
					}
				}
			}
		}
		return eQuality;
	}

	public RepeatedField<GemSlotDetail> GetGemSlotList(EEquipType equipType, EquipSlotInfo slotInfo = null)
	{
		foreach (GemPlanDetail gemPlan in (slotInfo ?? zbServerData.sGameData.EquipSlotInfo).GemPlanList)
		{
			if (gemPlan.PlanId != (slotInfo ?? zbServerData.sGameData.EquipSlotInfo).GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				if (gemPlanSlot.EquipType != equipType)
				{
					continue;
				}
				using IEnumerator<GemSlotDetail> enumerator3 = gemPlanSlot.GemSlotList.GetEnumerator();
				if (enumerator3.MoveNext())
				{
					_ = enumerator3.Current;
					return gemPlanSlot.GemSlotList;
				}
			}
		}
		return null;
	}

	public bool IsGemByPlan(int gemId, int ignorePlanId = 0)
	{
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId == ignorePlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid == gemId)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public int GetGroupGemIndex(int gemId)
	{
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemId);
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid > 0)
					{
						zbGemCsvRecord recordByKey2 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemSlot.GemTid);
						if (recordByKey2 != null && recordByKey.type == (int)gemSlot.GemType && recordByKey.groupId == recordByKey2.groupId)
						{
							return gemSlot.Index;
						}
					}
				}
			}
		}
		return -1;
	}

	public bool CanGemInsert(int gemId)
	{
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemId);
		if (recordByKey == null)
		{
			return false;
		}
		int gemPlanId = zbServerData.sGameData.EquipSlotInfo.GemPlanId;
		GemPlanDetail gemPlanDetail = null;
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId == gemPlanId)
			{
				gemPlanDetail = gemPlan;
			}
		}
		if (gemPlanDetail == null)
		{
			return false;
		}
		bool result = false;
		foreach (GemPlanSlotDetail gemPlanSlot in gemPlanDetail.GemPlanSlotList)
		{
			foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
			{
				if (gemSlot.GemTid > 0)
				{
					zbGemCsvRecord recordByKey2 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemSlot.GemTid);
					if (gemSlot.GemTid == recordByKey.id || recordByKey.groupId == recordByKey2.groupId)
					{
						return false;
					}
				}
				else if (recordByKey.type == (int)gemSlot.GemType)
				{
					result = true;
				}
			}
		}
		return result;
	}

	public bool CanGemReplace(int gemId)
	{
		zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemId);
		if (recordByKey == null)
		{
			return false;
		}
		int gemPlanId = zbServerData.sGameData.EquipSlotInfo.GemPlanId;
		GemPlanDetail gemPlanDetail = null;
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId == gemPlanId)
			{
				gemPlanDetail = gemPlan;
			}
		}
		if (gemPlanDetail == null)
		{
			return false;
		}
		foreach (GemPlanSlotDetail gemPlanSlot in gemPlanDetail.GemPlanSlotList)
		{
			foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
			{
				if (gemSlot.GemTid > 0)
				{
					zbGemCsvRecord recordByKey2 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gemSlot.GemTid);
					if (recordByKey2 != null && recordByKey.type == (int)gemSlot.GemType && recordByKey.groupId == recordByKey2.groupId && recordByKey.quality > recordByKey2.quality)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool CanGemCompose()
	{
		GameData sGameData = zbServerData.sGameData;
		RepeatedField<ResourceDetail> gemList = sGameData.GemList;
		if (gemList == null || gemList.Count == 0)
		{
			return false;
		}
		HashSet<int> hashSet = new HashSet<int>();
		foreach (int gem in sGameData.LockInfo.GemList)
		{
			hashSet.Add(gem);
		}
		HashSet<int> hashSet2 = new HashSet<int>();
		EquipSlotInfo equipSlotInfo = sGameData.EquipSlotInfo;
		if (equipSlotInfo != null && equipSlotInfo.GemPlanList != null)
		{
			foreach (GemPlanDetail gemPlan in equipSlotInfo.GemPlanList)
			{
				if (gemPlan?.GemPlanSlotList == null)
				{
					continue;
				}
				foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
				{
					if (gemPlanSlot?.GemSlotList == null)
					{
						continue;
					}
					foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
					{
						if (gemSlot != null && gemSlot.GemTid > 0)
						{
							hashSet2.Add(gemSlot.GemTid);
						}
					}
				}
			}
		}
		long[,] array = new long[7, 7];
		zbGemCsvTable gemCsvTable = zbDataConfig.Instance.gemCsvTable;
		foreach (ResourceDetail item in gemList)
		{
			if (hashSet.Contains(item.Tid))
			{
				continue;
			}
			zbGemCsvRecord recordByKey = gemCsvTable.GetRecordByKey(item.Tid);
			if (recordByKey == null)
			{
				continue;
			}
			long num = item.Count;
			if (hashSet2.Contains(item.Tid))
			{
				num--;
			}
			if (num > 0 && recordByKey.quality < 7)
			{
				array[recordByKey.type, recordByKey.quality] += num;
				if (array[recordByKey.type, recordByKey.quality] >= 5)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsEquipExist(string EquipId)
	{
		foreach (EquipSlotDetail equipSlot in zbServerData.sGameData.EquipSlotInfo.EquipSlotList)
		{
			if (equipSlot.Equip.EquipId == EquipId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsGemLock(int gemTid)
	{
		foreach (int gem in zbServerData.sGameData.LockInfo.GemList)
		{
			if (gem == gemTid)
			{
				return true;
			}
		}
		return false;
	}

	public long getGemCount(int gemId)
	{
		long num = 0L;
		foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
		{
			if (gem.Tid == gemId)
			{
				num = gem.Count;
				break;
			}
		}
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlan.PlanId != zbServerData.sGameData.EquipSlotInfo.GemPlanId)
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid == gemId)
					{
						return num - 1;
					}
				}
			}
		}
		return num;
	}

	public bool IsGemInsert(int gemId, int gemPlanId = 0)
	{
		foreach (GemPlanDetail gemPlan in zbServerData.sGameData.EquipSlotInfo.GemPlanList)
		{
			if (gemPlanId != 0 && (gemPlanId == 0 || gemPlanId != gemPlan.PlanId))
			{
				continue;
			}
			foreach (GemPlanSlotDetail gemPlanSlot in gemPlan.GemPlanSlotList)
			{
				foreach (GemSlotDetail gemSlot in gemPlanSlot.GemSlotList)
				{
					if (gemSlot.GemTid == gemId)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private long getCanComposeGemCount(int gemType, int quality)
	{
		long num = 0L;
		foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
		{
			if (!IsGemLock(gem.Tid))
			{
				zbGemCsvRecord recordByKey = zbDataConfig.Instance.gemCsvTable.GetRecordByKey(gem.Tid);
				if (recordByKey.type == gemType && recordByKey.quality == quality)
				{
					num = ((!IsGemInsert(gem.Tid)) ? (num + gem.Count) : (num + gem.Count - 1));
				}
			}
		}
		return num;
	}

	public bool HasGemNotify()
	{
		foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
		{
			if (CanGemInsert(gem.Tid) || CanGemReplace(gem.Tid))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNewGemNotify()
	{
		foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewGem(gem.Tid.ToString()))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasNewEquipNotify()
	{
		foreach (EquipDetail equip in zbServerData.sGameData.EquipList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewEquip(equip.EquipId))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanEquipSet(string equipId)
	{
		EquipDetail equipDetail = GetEquipDetail(equipId);
		if (equipDetail == null)
		{
			return false;
		}
		zbEquipCsvRecord recordByKey = zbDataConfig.Instance.equipCsvTable.GetRecordByKey(equipDetail.EquipTid);
		if (recordByKey != null)
		{
			EquipDetail equipSlotEquip = GetEquipSlotEquip((EEquipType)recordByKey.type);
			if (equipSlotEquip == null)
			{
				return true;
			}
			if (equipDetail != null)
			{
				return equipDetail.Quality > equipSlotEquip.Quality;
			}
		}
		return false;
	}

	public bool HasEquipNotify()
	{
		FilterEquip();
		foreach (EquipDetail filteredEquip in filteredEquips)
		{
			if (CanEquipSet(filteredEquip.EquipId))
			{
				return true;
			}
		}
		return false;
	}

	private void FilterEquip()
	{
		filteredEquips.Clear();
		seenTidSet.Clear();
		foreach (EquipDetail equip in zbServerData.sGameData.EquipList)
		{
			if (seenTidSet.Add(equip.EquipTid))
			{
				filteredEquips.Add(equip);
			}
		}
	}

	public bool CanEquipSlotUpgrade(EEquipType equipType)
	{
		EquipSlotDetail equipSlotInfo = GetEquipSlotInfo(equipType);
		if (equipSlotInfo != null)
		{
			zbEquipSlotUpgradeCsvRecord[] records = zbDataConfig.Instance.equipSlotUpgradeCsvTable.GetRecords().records;
			foreach (zbEquipSlotUpgradeCsvRecord zbEquipSlotUpgradeCsvRecord2 in records)
			{
				if (zbEquipSlotUpgradeCsvRecord2.type == (int)equipType && zbEquipSlotUpgradeCsvRecord2.lv == equipSlotInfo.Level + 1 && zbSelf.Instance.dataHandler.GetCostListLackItem(zbEquipSlotUpgradeCsvRecord2.costList) == null)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquipSlotUpgrade()
	{
		for (int i = 1; i <= 3; i++)
		{
			if (CanEquipSlotUpgrade((EEquipType)i))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanEquipLevelUpgrade(EEquipType equipType)
	{
		EquipDetail equipSlotEquip = GetEquipSlotEquip(equipType);
		if (equipSlotEquip != null)
		{
			zbEquipUpgradeSubCsvRecord[] records = zbDataConfig.Instance.equipUpgradeSubCsvTable.GetRecords().records;
			foreach (zbEquipUpgradeSubCsvRecord zbEquipUpgradeSubCsvRecord2 in records)
			{
				if (zbEquipUpgradeSubCsvRecord2.id == (int)equipSlotEquip.Quality && zbEquipUpgradeSubCsvRecord2.lv == equipSlotEquip.SubLevel + 1 && zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 12, zbEquipUpgradeSubCsvRecord2.exp))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasEquipLevelUpgrade()
	{
		if (m_todayRedPointUsed)
		{
			return false;
		}
		return HasEquipUpgradeAvailable();
	}

	private bool HasEquipUpgradeAvailable()
	{
		for (int i = 1; i <= 3; i++)
		{
			if (CanEquipLevelUpgrade((EEquipType)i))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasEquipSlotNotify()
	{
		if (!HasEquipSlotUpgrade())
		{
			return HasEquipLevelUpgrade();
		}
		return true;
	}

	public bool HasRedPoint()
	{
		if (!HasGemNotify() && !CanGemCompose() && !HasEquipNotify())
		{
			return HasEquipSlotNotify();
		}
		return true;
	}

	public void SetTodayRedPointUse()
	{
		m_todayRedPointUsed = true;
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		zbGameSave.Instance.SetLong(m_equipLevelUpgradeKey, serverTime);
	}

	public void ResetTodayRedPointUse()
	{
		m_todayRedPointUsed = false;
		zbGameSave.Instance.SetLong(m_equipLevelUpgradeKey, 0L);
	}
}
