using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbHeroHandler
{
	public HeroFragDecomposeAck heroFragDecomposeAck;

	public int heroSetTid;

	public List<BagSelectDetail> selectList = new List<BagSelectDetail>();

	private Dictionary<int, int> m_maxFollowLevels = new Dictionary<int, int>();

	private Dictionary<int, Dictionary<int, zbHeroFollowCsvRecord>> m_heroFollowMap = new Dictionary<int, Dictionary<int, zbHeroFollowCsvRecord>>();

	public const int MAX_QUALITY_STAR = 4;

	public void Init()
	{
		zbHeroFollowCsvRecord[] records = zbDataConfig.Instance.heroFollowCsvTable.GetRecords().records;
		m_maxFollowLevels.Clear();
		zbHeroFollowCsvRecord[] array = records;
		foreach (zbHeroFollowCsvRecord zbHeroFollowCsvRecord2 in array)
		{
			if (m_maxFollowLevels.TryGetValue(zbHeroFollowCsvRecord2.skillType, out var value))
			{
				if (zbHeroFollowCsvRecord2.lv > value)
				{
					m_maxFollowLevels[zbHeroFollowCsvRecord2.skillType] = zbHeroFollowCsvRecord2.lv;
				}
			}
			else
			{
				m_maxFollowLevels.Add(zbHeroFollowCsvRecord2.skillType, zbHeroFollowCsvRecord2.lv);
			}
		}
		m_heroFollowMap.Clear();
		array = records;
		foreach (zbHeroFollowCsvRecord zbHeroFollowCsvRecord3 in array)
		{
			if (!m_heroFollowMap.ContainsKey(zbHeroFollowCsvRecord3.skillType))
			{
				m_heroFollowMap[zbHeroFollowCsvRecord3.skillType] = new Dictionary<int, zbHeroFollowCsvRecord>();
			}
			m_heroFollowMap[zbHeroFollowCsvRecord3.skillType][zbHeroFollowCsvRecord3.lv] = zbHeroFollowCsvRecord3;
		}
	}

	public zbHeroCsvRecord GetHeroRecord(int tid)
	{
		return zbDataConfig.Instance.heroCsvTable.GetRecordByKey(tid);
	}

	public int GetHeroFollowMaxLevel(ESkillType skillType)
	{
		if (m_maxFollowLevels.TryGetValue((int)skillType, out var value))
		{
			return value;
		}
		return 0;
	}

	public zbHeroFollowCsvRecord GetHeroFollowRecord(ESkillType skillType, int level)
	{
		if (m_heroFollowMap.TryGetValue((int)skillType, out var value) && value.TryGetValue(level, out var value2))
		{
			return value2;
		}
		return null;
	}

	public float GetFollowDamages(ESkillType skillType)
	{
		float num = 0f;
		WarRoomDetail warRoomDetailBySkillType = zbSelf.Instance.heroHandler.GetWarRoomDetailBySkillType(skillType);
		if (warRoomDetailBySkillType != null)
		{
			foreach (zbHeroCsvRecord item in GetHeroRecordsBySkillType((int)skillType))
			{
				if (warRoomDetailBySkillType.HeroTid != item.id)
				{
					HeroDetail heroDetail = zbSelf.Instance.heroHandler.GetHeroDetail(item.id);
					if (heroDetail != null)
					{
						num += GetDamage(heroDetail.Quality, heroDetail.Rarity, heroDetail.QualityStar);
					}
				}
			}
		}
		HeroFollowDetail heroFollowDetail = ((IEnumerable<HeroFollowDetail>)zbServerData.sGameData?.HeroFollowInfo?.FollowList).FirstOrDefault((HeroFollowDetail x) => x.SkillType == skillType);
		if (heroFollowDetail != null)
		{
			zbHeroFollowCsvRecord heroFollowRecord = GetHeroFollowRecord(skillType, heroFollowDetail.FollowLevel);
			return num * heroFollowRecord.damageCoef;
		}
		return 0f;
	}

	public float GetDamage(EQuality quality, ERarityType rarity, int qualityStar)
	{
		zbHeroQualityUpgradeCsvRecord heroQualityUpgradeCsvRecord = GetHeroQualityUpgradeCsvRecord((int)quality, qualityStar);
		if (heroQualityUpgradeCsvRecord != null)
		{
			return rarity switch
			{
				ERarityType.B => heroQualityUpgradeCsvRecord.damage1, 
				ERarityType.A => heroQualityUpgradeCsvRecord.damage2, 
				ERarityType.S => heroQualityUpgradeCsvRecord.damage3, 
				ERarityType.Ss => heroQualityUpgradeCsvRecord.damage4, 
				ERarityType.Role => heroQualityUpgradeCsvRecord.damage10, 
				_ => 0f, 
			};
		}
		return 0f;
	}

	public bool CanFollow(ESkillType skillType)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroFollow))
		{
			return false;
		}
		int num = 0;
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			if (zbDataConfig.Instance.heroCsvTable.GetRecordByKey(hero.HeroTid).type == (int)skillType)
			{
				num++;
			}
		}
		return num >= 2;
	}

	public bool HasHeroFollowNotify(ESkillType skillType)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroFollow) || skillType == ESkillType.Qiang || !CanFollow(skillType))
		{
			return false;
		}
		int num = 0;
		RepeatedField<HeroFollowDetail> val = zbServerData.sGameData?.HeroFollowInfo?.FollowList;
		if (val != null)
		{
			for (int i = 0; i < val.Count; i++)
			{
				if (val[i].SkillType == skillType)
				{
					num = val[i].FollowLevel;
					break;
				}
			}
		}
		int heroFollowMaxLevel = GetHeroFollowMaxLevel(skillType);
		if (num >= heroFollowMaxLevel)
		{
			return false;
		}
		zbHeroFollowCsvRecord heroFollowRecord = zbSelf.Instance.heroHandler.GetHeroFollowRecord(skillType, num + 1);
		if (heroFollowRecord == null)
		{
			return false;
		}
		if (heroFollowRecord.spcCost != null && heroFollowRecord.spcCost.Length != 0 && !zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)heroFollowRecord.spcCost[0], heroFollowRecord.spcCost[1], heroFollowRecord.spcCost[2]))
		{
			return false;
		}
		int[][] costList = heroFollowRecord.costList;
		if (costList != null)
		{
			foreach (int[] array in costList)
			{
				if (!zbSelf.Instance.dataHandler.HasResourceEnough((EResourceType)array[0], array[1], array[2]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public void CheckHeroFollowConvert(ESkillType skillType)
	{
		int valueOrDefault = (((IEnumerable<HeroFollowDetail>)zbServerData.sGameData?.HeroFollowInfo?.FollowList).FirstOrDefault((HeroFollowDetail x) => x.SkillType == skillType)?.FollowLevel).GetValueOrDefault();
		int heroFollowMaxLevel = GetHeroFollowMaxLevel(skillType);
		if (valueOrDefault < heroFollowMaxLevel)
		{
			return;
		}
		zbHeroFollowCsvRecord heroFollowRecord = zbSelf.Instance.heroHandler.GetHeroFollowRecord(skillType, valueOrDefault);
		if (heroFollowRecord == null || heroFollowRecord.spcCost.Length == 0)
		{
			return;
		}
		long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount((EResourceType)heroFollowRecord.spcCost[0], heroFollowRecord.spcCost[1]);
		if (resourceCount > 0)
		{
			zbResourceConvertCsvRecord zbResourceConvertCsvRecord2 = zbDataConfig.Instance.resourceConvertCsvTable.GetRecords().records.Values.FirstOrDefault((zbResourceConvertCsvRecord x) => x.resType == heroFollowRecord.spcCost[0] && x.resId == heroFollowRecord.spcCost[1]);
			if (zbResourceConvertCsvRecord2 != null)
			{
				zbUIMgr.Instance.ShowAuxResourceConvert(zbResourceConvertCsvRecord2, (int)resourceCount / zbResourceConvertCsvRecord2.resCount);
			}
		}
	}

	public Sprite GetSkillIconByHeroTid(int tid)
	{
		zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(tid);
		zbSkillCsvRecord recordByKey2 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(recordByKey.skillId);
		return zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon);
	}

	public WarRoomDetail GetWarRoomDetailByHeroTid(int tid)
	{
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			if (room.HeroTid == tid)
			{
				return room;
			}
		}
		return null;
	}

	public ERarityType GetCurrentHeroRarityTypeBySkillType(ESkillType skillType)
	{
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			if (room.SkillType == skillType)
			{
				return (ERarityType)zbDataConfig.Instance.heroCsvTable.GetRecordByKey(room.HeroTid).rarity;
			}
		}
		return ERarityType.EmptyValue;
	}

	public WarRoomDetail GetWarRoomDetailBySkillType(ESkillType skillType)
	{
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			if (room.SkillType == skillType)
			{
				return room;
			}
		}
		return null;
	}

	public List<zbHeroCsvRecord> GetHeroRecordsBySkillType(int skillType)
	{
		List<zbHeroCsvRecord> list = new List<zbHeroCsvRecord>();
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (value.type == skillType)
			{
				list.Add(value);
			}
		}
		list.Sort(delegate(zbHeroCsvRecord a, zbHeroCsvRecord b)
		{
			int num = a.rarity.CompareTo(b.rarity);
			return (num != 0) ? num : b.sortId.CompareTo(a.sortId);
		});
		return list;
	}

	public (List<zbHeroCsvRecord> roomList, List<zbHeroCsvRecord> substituteList, List<zbHeroCsvRecord> fraglist) GetHeroList(int tid = -1)
	{
		List<zbHeroCsvRecord> list = new List<zbHeroCsvRecord>();
		List<zbHeroCsvRecord> list2 = new List<zbHeroCsvRecord>();
		List<(zbHeroCsvRecord, long)> list3 = new List<(zbHeroCsvRecord, long)>();
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (GetHeroDetail(value.id) != null)
			{
				if (GetWarRoomDetailByHeroTid(value.id) != null)
				{
					list.Add(value);
				}
				else
				{
					list2.Add(value);
				}
				continue;
			}
			long resourceCount = zbSelf.Instance.dataHandler.GetResourceCount(EResourceType.HeroFrag, value.id);
			if (resourceCount > 0)
			{
				list3.Add((value, resourceCount));
			}
		}
		list.Sort((zbHeroCsvRecord a, zbHeroCsvRecord b) => a.sortId.CompareTo(b.sortId));
		list2.Sort((zbHeroCsvRecord a, zbHeroCsvRecord b) => a.sortId.CompareTo(b.sortId));
		List<zbHeroCsvRecord> item = (from t in list3
			orderby t.count descending, t.record.sortId
			select t.record).ToList();
		return (roomList: list, substituteList: list2, fraglist: item);
	}

	public HeroDetail GetHeroDetail(int tid)
	{
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			if (hero.HeroTid == tid)
			{
				return hero;
			}
		}
		return null;
	}

	public int GetHeroLevel(int tid)
	{
		WarRoomDetail warRoomDetailByHeroTid = GetWarRoomDetailByHeroTid(tid);
		if (warRoomDetailByHeroTid != null)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailByHeroTid.RoomId);
			if (homelandRoomDetail != null)
			{
				return homelandRoomDetail.Level;
			}
		}
		return 0;
	}

	public long GetHeroFragCount(int tid)
	{
		foreach (ResourceDetail heroFrag in zbServerData.sGameData.HeroFragList)
		{
			if (heroFrag.Tid == tid)
			{
				return heroFrag.Count;
			}
		}
		return 0L;
	}

	public List<ResourceDetail> GetHeroFragListByRarity(List<int> rarityList)
	{
		List<ResourceDetail> list = new List<ResourceDetail>();
		foreach (ResourceDetail heroFrag in zbServerData.sGameData.HeroFragList)
		{
			zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroFrag.Tid);
			if (recordByKey != null && recordByKey.type != 1 && rarityList.Contains(recordByKey.rarity))
			{
				list.Add(heroFrag);
			}
		}
		list.Sort(delegate(ResourceDetail v1, ResourceDetail v2)
		{
			zbHeroCsvRecord recordByKey2 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(v1.Tid);
			zbHeroCsvRecord recordByKey3 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(v2.Tid);
			return (recordByKey2 != null && recordByKey3 != null) ? ((recordByKey2.rarity == recordByKey3.rarity) ? v1.Count.CompareTo(v2.Count) : recordByKey2.rarity.CompareTo(recordByKey3.rarity)) : v1.Count.CompareTo(v2.Count);
		});
		return list;
	}

	public long GetComposeCount(int heroTid)
	{
		zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroTid);
		int quality = 1;
		int qualityStar = 0;
		HeroDetail heroDetail = GetHeroDetail(heroTid);
		if (heroDetail != null)
		{
			quality = (int)heroDetail.Quality;
			qualityStar = heroDetail.QualityStar;
		}
		zbHeroQualityUpgradeCsvRecord heroQualityUpgradeCsvRecord = GetHeroQualityUpgradeCsvRecord(quality, qualityStar);
		if (heroQualityUpgradeCsvRecord != null)
		{
			return heroQualityUpgradeCsvRecord.costFrag;
		}
		return 0L;
	}

	public long GetComposeOrUpgradeCount(int heroTid)
	{
		long result = 0L;
		HeroDetail heroDetail = GetHeroDetail(heroTid);
		if (heroDetail == null)
		{
			result = GetComposeCount(heroTid);
		}
		else if (heroDetail.Quality < EQuality.Orange)
		{
			zbHeroQualityUpgradeCsvRecord heroNextQualityUpgradeRecord = GetHeroNextQualityUpgradeRecord((int)heroDetail.Quality, heroDetail.QualityStar);
			if (heroNextQualityUpgradeRecord != null)
			{
				result = heroNextQualityUpgradeRecord.costFrag;
			}
		}
		return result;
	}

	public bool HasHeroComposeNotify(int heroTid)
	{
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(heroTid);
		if (GetHeroDetail(heroTid) == null && heroFragCount >= GetComposeCount(heroTid))
		{
			return true;
		}
		return false;
	}

	public bool HasHeroStarFragNotify(int heroTid)
	{
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(heroTid);
		HeroDetail heroDetail = GetHeroDetail(heroTid);
		if (heroDetail != null && heroDetail.Quality < EQuality.Orange)
		{
			zbHeroQualityUpgradeCsvRecord heroNextQualityUpgradeRecord = GetHeroNextQualityUpgradeRecord((int)heroDetail.Quality, heroDetail.QualityStar);
			if (heroNextQualityUpgradeRecord != null && heroFragCount >= heroNextQualityUpgradeRecord.costFrag)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeroStarNotify(int heroTid)
	{
		long heroFragCount = zbSelf.Instance.heroHandler.GetHeroFragCount(heroTid);
		HeroDetail heroDetail = GetHeroDetail(heroTid);
		if (heroDetail != null && heroDetail.Quality < EQuality.Orange)
		{
			zbHeroQualityUpgradeCsvRecord heroNextQualityUpgradeRecord = GetHeroNextQualityUpgradeRecord((int)heroDetail.Quality, heroDetail.QualityStar);
			if (heroNextQualityUpgradeRecord != null && heroFragCount >= heroNextQualityUpgradeRecord.costFrag && zbSelf.Instance.dataHandler.GetCostListLackItem(heroNextQualityUpgradeRecord.costList) == null)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeroUpgradeNotify(int heroTid)
	{
		WarRoomDetail warRoomDetailByHeroTid = GetWarRoomDetailByHeroTid(heroTid);
		if (warRoomDetailByHeroTid != null)
		{
			HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailByHeroTid.RoomId);
			return zbSelf.Instance.homelandHandler.CanRoomUpgrade(homelandRoomDetail);
		}
		return false;
	}

	public bool HasNewPoint()
	{
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			if (zbSelf.Instance.newTagHandler.IsNewHero(hero.HeroTid))
			{
				return true;
			}
		}
		return false;
	}

	public zbHeroCsvRecord HasUpgradeEnterNotify()
	{
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (!HasHeroUpgradeNotify(value.id) && !HasHeroAwakenNotify(value.id) && !zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)value.type, value.id))
			{
				continue;
			}
			if (zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)value.type, value.id))
			{
				WarRoomDetail warRoomDetailBySkillType = GetWarRoomDetailBySkillType((ESkillType)value.type);
				if (warRoomDetailBySkillType != null && warRoomDetailBySkillType.HeroTid != 0)
				{
					return zbDataConfig.Instance.heroCsvTable.GetRecordByKey(warRoomDetailBySkillType.HeroTid);
				}
				return null;
			}
			return value;
		}
		foreach (WarRoomDetail room in zbServerData.sGameData.WarRoomInfo.RoomList)
		{
			if (room.HeroTid > 0 && HasHeroStarNotify(room.HeroTid))
			{
				return zbDataConfig.Instance.heroCsvTable.GetRecordByKey(room.HeroTid);
			}
		}
		return null;
	}

	public bool HasHeroListEnterNotify()
	{
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (HasHeroComposeNotify(value.id))
			{
				return true;
			}
		}
		for (ESkillType eSkillType = ESkillType.Qiang; eSkillType < ESkillType.Fengren; eSkillType++)
		{
			if (HasHeroFollowNotify(eSkillType))
			{
				return true;
			}
		}
		return false;
	}

	public bool HasHeroListEnterStarNotify()
	{
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (HasHeroStarFragNotify(value.id))
			{
				return true;
			}
		}
		return HasAnyHeroRarityNotify();
	}

	public bool HasRedPoint()
	{
		foreach (zbHeroCsvRecord value in zbDataConfig.Instance.heroCsvTable.GetRecords().records.Values)
		{
			if (HasHeroComposeNotify(value.id) || HasHeroUpgradeNotify(value.id) || HasHeroAwakenNotify(value.id) || (HasHeroStarNotify(value.id) && GetWarRoomDetailByHeroTid(value.id) != null) || zbSelf.Instance.geneHandler.HasHeroTabGeneNotify((ESkillType)value.type, value.id))
			{
				return true;
			}
		}
		return false;
	}

	public HeroAwakenDetail GetHeroAwakenDetail(ESkillType skillType)
	{
		if (zbServerData.sGameData.HeroAwakenInfo != null)
		{
			foreach (HeroAwakenDetail awaken in zbServerData.sGameData.HeroAwakenInfo.AwakenList)
			{
				if (awaken.SkillType == skillType)
				{
					return awaken;
				}
			}
		}
		return null;
	}

	public zbHeroAwakenCsvRecord GetHeroAwakenRecord(ESkillType skillType, int level)
	{
		zbHeroAwakenCsvRecord[] records = zbDataConfig.Instance.heroAwakenCsvTable.GetRecords().records;
		foreach (zbHeroAwakenCsvRecord zbHeroAwakenCsvRecord2 in records)
		{
			if (zbHeroAwakenCsvRecord2.skillType == (int)skillType && zbHeroAwakenCsvRecord2.awakenLevel == level)
			{
				return zbHeroAwakenCsvRecord2;
			}
		}
		return null;
	}

	public int GetHeroAwakenLevel(ESkillType skillType)
	{
		return GetHeroAwakenDetail(skillType)?.Level ?? 0;
	}

	public bool HasHeroAwakenNotify(int heroTid)
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroAwaken))
		{
			WarRoomDetail warRoomDetailByHeroTid = GetWarRoomDetailByHeroTid(heroTid);
			if (warRoomDetailByHeroTid != null)
			{
				HeroAwakenDetail heroAwakenDetail = GetHeroAwakenDetail(warRoomDetailByHeroTid.SkillType);
				HomelandRoomDetail homelandRoomDetail = zbSelf.Instance.homelandHandler.GetHomelandRoomDetail(warRoomDetailByHeroTid.RoomId);
				if (heroAwakenDetail == null)
				{
					return false;
				}
				zbHeroAwakenCsvRecord heroAwakenRecord = GetHeroAwakenRecord(warRoomDetailByHeroTid.SkillType, heroAwakenDetail.Level + 1);
				if (heroAwakenRecord != null && heroAwakenDetail != null)
				{
					if (heroAwakenDetail.AttrDetail.Value < heroAwakenDetail.AttrDetail.CurrentMax)
					{
						int itemIdByType = zbSelf.Instance.itemHandler.GetItemIdByType(EItemType.HeroAwakenUpgrade);
						if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, itemIdByType, heroAwakenRecord.upgradeCost))
						{
							return true;
						}
					}
					else if (homelandRoomDetail.Level >= heroAwakenRecord.warRoomLevel)
					{
						int itemIdByType2 = zbSelf.Instance.itemHandler.GetItemIdByType(EItemType.HeroAwakenAdvance);
						if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Item, itemIdByType2, heroAwakenRecord.advanceCost))
						{
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	public bool IsHeroRarityUnlock()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.HeroRarity))
		{
			foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
			{
				if (hero.Quality >= EQuality.Orange)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool HasHeroRarityNotify(int heroTid)
	{
		if (!IsHeroRarityUnlock())
		{
			return false;
		}
		if (zbSelf.Instance.newTagHandler.IsNotNotifyHeroRarity(heroTid))
		{
			return false;
		}
		HeroDetail heroDetail = GetHeroDetail(heroTid);
		if (heroDetail != null && heroDetail.Quality >= EQuality.Orange && heroDetail.Rarity < ERarityType.Ss)
		{
			int upgradeFrag = zbDataConfig.Instance.heroRarityCsvTable.GetRecordByKey((int)(heroDetail.Rarity + 1)).upgradeFrag;
			long num = 0L;
			foreach (ResourceDetail heroFrag in zbServerData.sGameData.HeroFragList)
			{
				zbHeroCsvRecord recordByKey = zbDataConfig.Instance.heroCsvTable.GetRecordByKey(heroFrag.Tid);
				HeroDetail heroDetail2 = GetHeroDetail(heroFrag.Tid);
				if (recordByKey.rarity == 4 && heroDetail2 != null && heroDetail2.Quality >= EQuality.Orange)
				{
					num += heroFrag.Count;
				}
			}
			if (num >= upgradeFrag - heroDetail.RarityUpgrade)
			{
				return true;
			}
		}
		return false;
	}

	public zbHeroQualityUpgradeCsvRecord GetHeroNextQualityUpgradeRecord(int Quality, int qualityStar)
	{
		zbHeroQualityUpgradeCsvRecord[] records = zbDataConfig.Instance.heroQualityUpgradeCsvTable.GetRecords().records;
		if (qualityStar != 4)
		{
			return records.FirstOrDefault((zbHeroQualityUpgradeCsvRecord r) => r.quality == Quality && r.qualityStar == qualityStar + 1);
		}
		return records.FirstOrDefault((zbHeroQualityUpgradeCsvRecord r) => r.quality == Quality + 1 && r.qualityStar == 0);
	}

	public zbHeroQualityUpgradeCsvRecord GetHeroQualityUpgradeCsvRecord(int Quality, int qualityStar)
	{
		return zbDataConfig.Instance.heroQualityUpgradeCsvTable.GetRecords().records.FirstOrDefault((zbHeroQualityUpgradeCsvRecord r) => r.quality == Quality && r.qualityStar == qualityStar);
	}

	public bool HasAnyHeroRarityNotify()
	{
		foreach (HeroDetail hero in zbServerData.sGameData.HeroList)
		{
			if (HasHeroRarityNotify(hero.HeroTid))
			{
				return true;
			}
		}
		return false;
	}
}
