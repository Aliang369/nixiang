using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using zbType;

public class zbGetWayHandler
{
	private Dictionary<string, List<int>> getWayDict = new Dictionary<string, List<int>>();

	private Dictionary<long, List<int>> mainQuestAwardIndex = new Dictionary<long, List<int>>();

	private Dictionary<long, List<int>> eliteQuestAwardIndex = new Dictionary<long, List<int>>();

	private Dictionary<long, List<int>> randBoxToBoxIds = new Dictionary<long, List<int>>();

	private int cachedMainLevel = -1;

	private Dictionary<long, int> mainQuestResultCache = new Dictionary<long, int>();

	private int cachedEliteLevel = -1;

	private Dictionary<long, int> eliteQuestResultCache = new Dictionary<long, int>();

	private Dictionary<int, List<zbBoxCsvRecord>> m_dictBoxRecordsNormal;

	private Dictionary<int, List<zbBoxCsvRecord>> m_dictBoxRecordsCross;

	private static readonly List<zbBoxCsvRecord> s_emptyBoxRecords = new List<zbBoxCsvRecord>();

	public int GetWayBuyIndexOrId = -1;

	private static long MakeIndexKey(EResourceType type, long id)
	{
		return ((long)type << 32) | (id & 0xFFFFFFFFu);
	}

	public void Init()
	{
		LoadRandBoxCsvTable();
		LoadBarPoolNormalCsvTable();
		LoadBarPoolSpecialCsvTable();
		LoadMainLevelCsvTable();
		LoadEliteLevelCsvTable();
		LoadPeerageTaskCsvTable();
		LoadDailyTaskCsvTable();
		LoadDailyTaskTargetCsvTable();
		LoadPassportLevelAwardCsvTable();
		LoadLegionDonateTargetCsvTable();
		LoadLegionBossChallengeCsvTable();
		LoadWorldEventBoxCsvTable();
		LoadWorldEventFightCsvTable();
		LoadWorldBossCsvTable();
		LoadWorldBossHurtAwardCsvTable();
		LoadWorldBossRankAwardCsvTable();
		LoadSignCsvTable();
		LoadVipDailyPackCsvTable();
		LoadVipShopCsvTable();
		LoadWorldLairKillAwardCsvTable();
		LoadWorldLairLevelCsvTable();
		LoadIdleAwardCsvTable();
		LoadElementTrialCsvTable();
		LoadZhanLingElementCsvTable();
		LoadZhanLingMainCsvTable();
		LoadEndlessHordeLevelCsvTable();
		LoadZhanLingEndlessCsvTable();
		LoadLegendaryLevelCsvTable();
		LoadZhanLingLegendaryCsvTable();
		LoadLadderCsvTable();
		LoadLadderTargetAwardCsvTable();
		LoadPeakRankAwardCsvTable();
		LoadBarPoolTreasureCsvTable();
		LoadBarPoolTreasurePlusCsvTable();
		LoadMineNodeCsvTable();
		LoadArenaGiftCsvTable();
		LoadLegionWarAwardCsvTable();
		LoadLegionWarLegionRankCsvTable();
		LoadLegionWarLevelCsvTable();
		LoadLegionWarMapCsvTable();
		LoadLegionWarPlayerRankCsvTable();
		LoadFacilityCsvTable();
		LoadVipLevelPackCsvTable();
		LoadFirstRechargeCsvTable();
		LoadVipTequanPackCsvTable();
		LoadOpenServerRankCsvTable();
		CleanupGetWayDictOperateValues();
		LoadPropList();
		LoadShopList();
		LoadGeneComposeGetWay();
		InsertOperateGetWay();
		InsertJourneyOneGetWay();
		InsertJourneyTwoGetWay();
		InsertJourneyThreeGetWay();
		AddToGetWayDict(EResourceType.Money, 11L, 15);
		AddToGetWayDict(EResourceType.Money, 12L, 16);
		AddToGetWayDict(EResourceType.Money, 4L, 21);
		AddToGetWayDict(EResourceType.Money, 4L, 31);
		AddToGetWayDict(EResourceType.Money, 2L, 35);
		AddToGetWayDict(EResourceType.Money, 3L, 35);
		AddToGetWayDict(EResourceType.Money, 2L, 49);
		AddToGetWayDict(EResourceType.Money, 17L, 49);
		AddToGetWayDict(EResourceType.Money, 21L, 57);
	}

	public void InvalidateQuestResultCache()
	{
		mainQuestResultCache.Clear();
		eliteQuestResultCache.Clear();
		cachedMainLevel = -1;
		cachedEliteLevel = -1;
	}

	public void AddToGetWayDict(EResourceType type, long id, int wayType)
	{
		string key = $"{type}_{id}";
		if (!getWayDict.TryGetValue(key, out var value))
		{
			value = new List<int>();
			getWayDict[key] = value;
		}
		foreach (int item in value)
		{
			if (item == wayType)
			{
				return;
			}
		}
		value.Add(wayType);
	}

	public void CheckHasGetWay()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<int, zbItemCsvRecord> record in zbDataConfig.Instance.itemCsvTable.GetRecords().records)
		{
			string key = $"Item_{record.Value.id}";
			if (!getWayDict.TryGetValue(key, out var _))
			{
				stringBuilder.AppendLine($"Item表 {record.Value.id} {record.Value.name} 没有获取途径");
			}
		}
		foreach (KeyValuePair<int, zbPropCsvRecord> record2 in zbDataConfig.Instance.propCsvTable.GetRecords().records)
		{
			string key2 = $"Prop_{record2.Value.id}";
			if (!getWayDict.TryGetValue(key2, out var _))
			{
				stringBuilder.AppendLine($"Prop表 {record2.Value.id} {record2.Value.name} 没有获取途径");
			}
		}
		foreach (KeyValuePair<int, zbMoneyCsvRecord> record3 in zbDataConfig.Instance.moneyCsvTable.GetRecords().records)
		{
			string key3 = $"Money_{record3.Value.id}";
			if (!getWayDict.TryGetValue(key3, out var _))
			{
				stringBuilder.AppendLine($"Money表 {record3.Value.id} {record3.Value.name} 没有获取途径");
			}
		}
		foreach (KeyValuePair<int, zbEquipCsvRecord> record4 in zbDataConfig.Instance.equipCsvTable.GetRecords().records)
		{
			string key4 = $"Equip_{record4.Value.id}";
			if (!getWayDict.TryGetValue(key4, out var _))
			{
				stringBuilder.AppendLine($"Equip表 {record4.Value.id} {record4.Value.name} 没有获取途径");
			}
		}
		foreach (KeyValuePair<int, zbHeroCsvRecord> record5 in zbDataConfig.Instance.heroCsvTable.GetRecords().records)
		{
			string key5 = $"Hero_{record5.Value.id}";
			if (!getWayDict.TryGetValue(key5, out var _))
			{
				stringBuilder.AppendLine($"Hero表 {record5.Value.id} {record5.Value.name} 没有获取途径");
			}
		}
		foreach (KeyValuePair<int, zbBoxCsvRecord> record6 in zbDataConfig.Instance.boxCsvTable.GetRecords().records)
		{
			string key6 = $"Box_{record6.Value.id}";
			if (!getWayDict.TryGetValue(key6, out var _))
			{
				stringBuilder.AppendLine($"Box表 {record6.Value.id} {record6.Value.name} 没有获取途径");
			}
		}
		File.WriteAllText(Path.Combine(Application.dataPath, "缺少获取途径.txt"), stringBuilder.ToString(), Encoding.UTF8);
	}

	public List<int> GetWayTypes(EResourceType type, long id)
	{
		string key = $"{type}_{id}";
		if (!getWayDict.TryGetValue(key, out var value))
		{
			switch (type)
			{
			case EResourceType.HeroFrag:
				key = $"{EResourceType.Hero}_{id}";
				if (!getWayDict.TryGetValue(key, out value))
				{
					return new List<int>();
				}
				break;
			case EResourceType.Hero:
				key = $"{EResourceType.HeroFrag}_{id}";
				if (!getWayDict.TryGetValue(key, out value))
				{
					return new List<int>();
				}
				break;
			default:
				return new List<int>();
			}
		}
		List<int> list = new List<int>(value);
		list.Sort(CompareGetWayTypes);
		return list;
	}

	private int CompareGetWayTypes(int a, int b)
	{
		int num = ((a <= 1000) ? 1 : 0);
		int num2 = ((b <= 1000) ? 1 : 0);
		if (num != num2)
		{
			return num.CompareTo(num2);
		}
		if (a > 1000 && b > 1000)
		{
			return a.CompareTo(b);
		}
		bool flag = IsGetWayLocked(a);
		bool flag2 = IsGetWayLocked(b);
		if (flag != flag2)
		{
			return flag.CompareTo(flag2);
		}
		int getWayIndex = GetGetWayIndex(a);
		int getWayIndex2 = GetGetWayIndex(b);
		return getWayIndex.CompareTo(getWayIndex2);
	}

	private bool IsGetWayLocked(int getWay)
	{
		zbGetWayCsvRecord recordByKey = zbDataConfig.Instance.getWayCsvTable.GetRecordByKey(getWay);
		if (recordByKey == null)
		{
			return true;
		}
		if (recordByKey.funcType == 0)
		{
			return false;
		}
		return zbSelf.Instance.funHandler.IsLockNoTip((EFuncType)recordByKey.funcType);
	}

	private int GetGetWayIndex(int getWay)
	{
		return zbDataConfig.Instance.getWayCsvTable.GetRecordByKey(getWay)?.index ?? int.MaxValue;
	}

	public void LoadRandBoxCsvTable()
	{
		zbRandBoxCsvTableData records = zbDataConfig.Instance.randBoxCsvTable.GetRecords();
		Dictionary<int, zbBoxCsvRecord>.ValueCollection values = zbDataConfig.Instance.boxCsvTable.GetRecords().records.Values;
		zbRandBoxCsvRecord[] records2 = records.records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records2)
		{
			long key = MakeIndexKey((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id);
			foreach (zbBoxCsvRecord item in values)
			{
				if (item.box == zbRandBoxCsvRecord2.pid)
				{
					AddQuestIndex(randBoxToBoxIds, key, item.id);
				}
			}
			zbWorldBossHurtAwardCsvRecord[] records3 = zbDataConfig.Instance.worldBossHurtAwardCsvTable.GetRecords().records;
			for (int j = 0; j < records3.Length; j++)
			{
				if (records3[j].pid == zbRandBoxCsvRecord2.pid)
				{
					AddToGetWayDict((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id, 28);
					break;
				}
			}
			zbLegionBossKillAwardCsvRecord[] records4 = zbDataConfig.Instance.legionBossKillAwardCsvTable.GetRecords().records;
			for (int j = 0; j < records4.Length; j++)
			{
				if (records4[j].pid == zbRandBoxCsvRecord2.pid)
				{
					AddToGetWayDict((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id, 23);
					break;
				}
			}
		}
	}

	public List<zbBoxCsvRecord> GetWayBoxs(EResourceType type, long id, bool isBoxCross)
	{
		if (type == EResourceType.HeroFrag)
		{
			type = EResourceType.Hero;
		}
		List<zbBoxCsvRecord> list = new List<zbBoxCsvRecord>();
		HashSet<long> hashSet = new HashSet<long>();
		zbRandBoxCsvRecord[] records = zbDataConfig.Instance.randBoxCsvTable.GetRecords().records;
		foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records)
		{
			if (zbRandBoxCsvRecord2.type != (int)type || zbRandBoxCsvRecord2.id != id)
			{
				continue;
			}
			foreach (zbBoxCsvRecord boxRecord in GetBoxRecords(zbRandBoxCsvRecord2.pid, isBoxCross))
			{
				if (hashSet.Add(boxRecord.id))
				{
					list.Add(boxRecord);
				}
			}
		}
		list.Sort((zbBoxCsvRecord a, zbBoxCsvRecord b) => b.quality.CompareTo(a.quality));
		return list;
	}

	private List<zbBoxCsvRecord> GetBoxRecords(int pid, bool isBoxCross)
	{
		Dictionary<int, List<zbBoxCsvRecord>> dictionary = (isBoxCross ? m_dictBoxRecordsCross : m_dictBoxRecordsNormal);
		if (dictionary == null)
		{
			dictionary = new Dictionary<int, List<zbBoxCsvRecord>>();
			foreach (zbBoxCsvRecord value3 in zbDataConfig.Instance.boxCsvTable.GetRecords().records.Values)
			{
				if (!zbVersionMgr.IsReleased(value3.preShow))
				{
					continue;
				}
				bool num;
				if (!isBoxCross)
				{
					num = value3.type == 1;
				}
				else
				{
					if (value3.type == 3 || value3.type == 5 || value3.type == 2)
					{
						goto IL_008b;
					}
					num = value3.type == 1;
				}
				if (!num)
				{
					continue;
				}
				goto IL_008b;
				IL_008b:
				if (!dictionary.TryGetValue(value3.box, out var value))
				{
					value = new List<zbBoxCsvRecord>();
					dictionary[value3.box] = value;
				}
				value.Add(value3);
			}
			if (isBoxCross)
			{
				m_dictBoxRecordsCross = dictionary;
			}
			else
			{
				m_dictBoxRecordsNormal = dictionary;
			}
		}
		if (!dictionary.TryGetValue(pid, out var value2))
		{
			return s_emptyBoxRecords;
		}
		return value2;
	}

	private void LoadBarPoolNormalCsvTable()
	{
		zbBarPoolNormalCsvRecord[] records = zbDataConfig.Instance.barPoolNormalCsvTable.GetRecords().records;
		foreach (zbBarPoolNormalCsvRecord zbBarPoolNormalCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbBarPoolNormalCsvRecord2.type, zbBarPoolNormalCsvRecord2.id, 1);
		}
	}

	private void LoadBarPoolSpecialCsvTable()
	{
		zbBarPoolSpecialCsvRecord[] records = zbDataConfig.Instance.barPoolSpecialCsvTable.GetRecords().records;
		foreach (zbBarPoolSpecialCsvRecord zbBarPoolSpecialCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbBarPoolSpecialCsvRecord2.type, zbBarPoolSpecialCsvRecord2.id, 2);
		}
	}

	private void LoadMainLevelCsvTable()
	{
		foreach (zbMainLevelCsvRecord value in zbDataConfig.Instance.mainLevelCsvTable.GetRecords().records.Values)
		{
			int[][][] array = new int[8][][] { value.awardShow, value.baseAward, value.growthAward, value.spAward1, value.spAward2, value.spAward3, value.spAward4, value.winAward };
			foreach (int[][] array2 in array)
			{
				if (array2 == null)
				{
					continue;
				}
				int[][] array3 = array2;
				foreach (int[] array4 in array3)
				{
					if (array4 != null && array4.Length != 0)
					{
						AddToGetWayDict((EResourceType)array4[0], array4[1], 7);
						AddQuestIndex(mainQuestAwardIndex, MakeIndexKey((EResourceType)array4[0], array4[1]), value.id);
					}
				}
			}
			array = new int[3][][] { value.awardStar1, value.awardStar2, value.awardStar3 };
			foreach (int[][] array5 in array)
			{
				if (array5 == null)
				{
					continue;
				}
				int[][] array3 = array5;
				foreach (int[] array6 in array3)
				{
					if (array6 != null && array6.Length != 0)
					{
						AddToGetWayDict((EResourceType)array6[0], array6[1], 8);
					}
				}
			}
		}
	}

	private void LoadEliteLevelCsvTable()
	{
		foreach (zbEliteLevelCsvRecord value in zbDataConfig.Instance.eliteLevelCsvTable.GetRecords().records.Values)
		{
			int[][][] array = new int[11][][]
			{
				value.awardStar1, value.awardStar2, value.awardStar3, value.awardShow, value.baseAward, value.growthAward, value.spAward1, value.spAward2, value.spAward3, value.spAward4,
				value.winAward
			};
			foreach (int[][] array2 in array)
			{
				if (array2 == null)
				{
					continue;
				}
				int[][] array3 = array2;
				foreach (int[] array4 in array3)
				{
					if (array4 != null && array4.Length != 0)
					{
						AddToGetWayDict((EResourceType)array4[0], array4[1], 9);
						AddQuestIndex(eliteQuestAwardIndex, MakeIndexKey((EResourceType)array4[0], array4[1]), value.id);
					}
				}
			}
		}
	}

	private void AddQuestIndex(Dictionary<long, List<int>> dict, long key, int levelId)
	{
		if (!dict.TryGetValue(key, out var value))
		{
			value = (dict[key] = new List<int>());
		}
		value.Add(levelId);
	}

	private void LoadPeerageTaskCsvTable()
	{
		foreach (zbPeerageTaskCsvRecord value in zbDataConfig.Instance.peerageTaskCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 11);
			}
		}
	}

	private void LoadDailyTaskCsvTable()
	{
		foreach (zbDailyTaskCsvRecord value in zbDataConfig.Instance.dailyTaskCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 12);
			}
		}
	}

	private void LoadDailyTaskTargetCsvTable()
	{
		zbDailyTaskTargetCsvRecord[] records = zbDataConfig.Instance.dailyTaskTargetCsvTable.GetRecords().records;
		for (int i = 0; i < records.Length; i++)
		{
			int[][] awardList = records[i].awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 13);
			}
		}
	}

	private void LoadPassportLevelAwardCsvTable()
	{
		foreach (zbPassportLevelAwardCsvRecord value in zbDataConfig.Instance.passportLevelAwardCsvTable.GetRecords().records.Values)
		{
			int[][] lowAward = value.lowAward;
			foreach (int[] array in lowAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 14);
			}
			lowAward = value.highAward;
			foreach (int[] array2 in lowAward)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 14);
			}
		}
	}

	private void LoadLegionDonateTargetCsvTable()
	{
		zbLegionDonateTargetCsvRecord[] records = zbDataConfig.Instance.legionDonateTargetCsvTable.GetRecords().records;
		for (int i = 0; i < records.Length; i++)
		{
			int[][] awardList = records[i].awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 22);
			}
		}
	}

	private void LoadLegionBossChallengeCsvTable()
	{
		foreach (zbLegionBossChallengeCsvRecord value in zbDataConfig.Instance.legionBossChallengeCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 23);
			}
		}
	}

	private void LoadWorldEventBoxCsvTable()
	{
		zbLegionBossKillAwardCsvRecord[] records = zbDataConfig.Instance.legionBossKillAwardCsvTable.GetRecords().records;
		foreach (zbLegionBossKillAwardCsvRecord zbLegionBossKillAwardCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbLegionBossKillAwardCsvRecord2.awardShow[0], zbLegionBossKillAwardCsvRecord2.awardShow[1], 25);
		}
	}

	private void LoadWorldEventFightCsvTable()
	{
		zbLegionBossKillAwardCsvRecord[] records = zbDataConfig.Instance.legionBossKillAwardCsvTable.GetRecords().records;
		foreach (zbLegionBossKillAwardCsvRecord zbLegionBossKillAwardCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbLegionBossKillAwardCsvRecord2.awardShow[0], zbLegionBossKillAwardCsvRecord2.awardShow[1], 25);
		}
	}

	private void LoadWorldBossCsvTable()
	{
		foreach (zbWorldBossCsvRecord value in zbDataConfig.Instance.worldBossCsvTable.GetRecords().records.Values)
		{
			int[][] awardShow = value.awardShow;
			foreach (int[] array in awardShow)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 28);
			}
		}
	}

	private void LoadWorldBossHurtAwardCsvTable()
	{
		zbWorldBossHurtAwardCsvRecord[] records = zbDataConfig.Instance.worldBossHurtAwardCsvTable.GetRecords().records;
		foreach (zbWorldBossHurtAwardCsvRecord zbWorldBossHurtAwardCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbWorldBossHurtAwardCsvRecord2.awardShow[0], zbWorldBossHurtAwardCsvRecord2.awardShow[1], 28);
		}
	}

	private void LoadWorldBossRankAwardCsvTable()
	{
		zbWorldBossRankAwardCsvRecord[] records = zbDataConfig.Instance.worldBossRankAwardCsvTable.GetRecords().records;
		for (int i = 0; i < records.Length; i++)
		{
			int[][] awardList = records[i].awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 28);
			}
		}
	}

	private void LoadSignCsvTable()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.DailySign))
		{
			return;
		}
		foreach (zbSignCsvRecord value in zbDataConfig.Instance.signCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.award[0], value.award[1], 30);
		}
		foreach (zbSignTargetCsvRecord value2 in zbDataConfig.Instance.signTargetCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value2.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 30);
			}
		}
	}

	private void LoadVipDailyPackCsvTable()
	{
		foreach (zbVipDailyPackCsvRecord value in zbDataConfig.Instance.vipDailyPackCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 33);
			}
		}
	}

	private void LoadVipShopCsvTable()
	{
		JObject record = zbDataConfig.Instance.vipBaseJsonTable.GetRecord();
		int num = (int)record["vipCard"][(object)"resource"][(object)"goodsId"];
		zbVipDailyPackCsvTableData records = zbDataConfig.Instance.vipDailyPackCsvTable.GetRecords();
		int[][] array = ((IEnumerable<JToken>)record["vipCard"][(object)"resource"][(object)"dailyAward"]).Select((JToken item) => ((IEnumerable<JToken>)item).Select((JToken x) => (int)x).ToArray()).ToArray();
		foreach (int[] array2 in array)
		{
			AddToGetWayDict((EResourceType)array2[0], array2[1], 56);
		}
		foreach (zbVipDailyPackCsvRecord value in records.records.Values)
		{
			if (value.id == num)
			{
				array = value.awardList;
				foreach (int[] array3 in array)
				{
					AddToGetWayDict((EResourceType)array3[0], array3[1], 56);
				}
			}
		}
	}

	private void LoadWorldLairKillAwardCsvTable()
	{
		zbWorldLairKillAwardCsvRecord[] records = zbDataConfig.Instance.worldLairKillAwardCsvTable.GetRecords().records;
		for (int i = 0; i < records.Length; i++)
		{
			int[][] awardList = records[i].awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 36);
			}
		}
	}

	private void LoadWorldLairLevelCsvTable()
	{
		foreach (zbWorldLairLevelCsvRecord value in zbDataConfig.Instance.worldLairLevelCsvTable.GetRecords().records.Values)
		{
			int[][] passAwardList = value.passAwardList;
			foreach (int[] array in passAwardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 36);
			}
			passAwardList = value.awardShow;
			foreach (int[] array2 in passAwardList)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 36);
			}
		}
	}

	private void LoadIdleAwardCsvTable()
	{
		AddToGetWayDict(EResourceType.Money, 1L, 37);
		zbIdleAwardCsvTableData records = zbDataConfig.Instance.idleAwardCsvTable.GetRecords();
		zbRandBoxCsvTableData records2 = zbDataConfig.Instance.randBoxCsvTable.GetRecords();
		foreach (zbIdleAwardCsvRecord value in records.records.Values)
		{
			zbRandBoxCsvRecord[] records3 = records2.records;
			foreach (zbRandBoxCsvRecord zbRandBoxCsvRecord2 in records3)
			{
				if (zbRandBoxCsvRecord2.pid == value.randPid)
				{
					AddToGetWayDict((EResourceType)zbRandBoxCsvRecord2.type, zbRandBoxCsvRecord2.id, 37);
				}
			}
		}
	}

	private void LoadElementTrialCsvTable()
	{
		foreach (zbElementTrialCsvRecord value in zbDataConfig.Instance.elementTrialCsvTable.GetRecords().records.Values)
		{
			int[][] awardShow = value.awardShow;
			foreach (int[] array in awardShow)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 38);
			}
			awardShow = value.growthAward;
			foreach (int[] array2 in awardShow)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 38);
			}
		}
	}

	private void LoadZhanLingElementCsvTable()
	{
		foreach (zbZhanlingElementCsvRecord value in zbDataConfig.Instance.zhanlingElementCsvTable.GetRecords().records.Values)
		{
			int[][] lowAward = value.lowAward;
			foreach (int[] array in lowAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 38);
			}
			lowAward = value.highAward;
			foreach (int[] array2 in lowAward)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 38);
			}
		}
	}

	private void LoadZhanLingMainCsvTable()
	{
		foreach (zbZhanLingMainCsvRecord value in zbDataConfig.Instance.zhanLingMainCsvTable.GetRecords().records.Values)
		{
			int[][] lowAward = value.lowAward;
			foreach (int[] array in lowAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 39);
			}
			lowAward = value.highAward;
			foreach (int[] array2 in lowAward)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 39);
			}
		}
	}

	private void LoadEndlessHordeLevelCsvTable()
	{
		foreach (zbEndlessHordeLevelCsvRecord value in zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecords().records.Values)
		{
			int[][][] array = new int[10][][] { value.passAward, value.awardShow, value.swAward, value.baseAward, value.growthAward, value.spAward1, value.spAward2, value.spAward3, value.spAward4, value.spAward5 };
			foreach (int[][] array2 in array)
			{
				if (array2 == null)
				{
					continue;
				}
				int[][] array3 = array2;
				foreach (int[] array4 in array3)
				{
					if (array4 != null && array4.Length != 0)
					{
						AddToGetWayDict((EResourceType)array4[0], array4[1], 46);
					}
				}
			}
		}
	}

	private void LoadZhanLingEndlessCsvTable()
	{
		foreach (zbZhanLingEndlessCsvRecord value in zbDataConfig.Instance.zhanLingEndlessCsvTable.GetRecords().records.Values)
		{
			int[][] lowAward = value.lowAward;
			foreach (int[] array in lowAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 46);
			}
			lowAward = value.highAward;
			foreach (int[] array2 in lowAward)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 46);
			}
		}
	}

	private void LoadLegendaryLevelCsvTable()
	{
		foreach (zbLegendaryLevelCsvRecord value in zbDataConfig.Instance.legendaryLevelCsvTable.GetRecords().records.Values)
		{
			int[][][] array = new int[8][][] { value.awardShow, value.baseAward, value.growthAward, value.spAward1, value.spAward2, value.spAward3, value.spAward4, value.winAward };
			foreach (int[][] array2 in array)
			{
				if (array2 == null)
				{
					continue;
				}
				int[][] array3 = array2;
				foreach (int[] array4 in array3)
				{
					if (array4 != null && array4.Length != 0)
					{
						AddToGetWayDict((EResourceType)array4[0], array4[1], 47);
					}
				}
			}
		}
	}

	private void LoadZhanLingLegendaryCsvTable()
	{
		foreach (zbZhanLingLegendaryCsvRecord value in zbDataConfig.Instance.zhanLingLegendaryCsvTable.GetRecords().records.Values)
		{
			int[][] lowAward = value.lowAward;
			foreach (int[] array in lowAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 48);
			}
			lowAward = value.highAward;
			foreach (int[] array2 in lowAward)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 48);
			}
		}
	}

	private void LoadLadderCsvTable()
	{
		foreach (zbLadderCsvRecord value in zbDataConfig.Instance.ladderCsvTable.GetRecords().records.Values)
		{
			int[][] settleAward = value.settleAward;
			foreach (int[] array in settleAward)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 52);
			}
		}
	}

	private void LoadLadderTargetAwardCsvTable()
	{
		foreach (zbLadderTargetAwardCsvRecord value in zbDataConfig.Instance.ladderTargetAwardCsvTable.GetRecords().records.Values)
		{
			int[][][] array = new int[12][][]
			{
				value.month1, value.month2, value.month3, value.month4, value.month5, value.month6, value.month7, value.month8, value.month9, value.month10,
				value.month11, value.month12
			};
			foreach (int[][] array2 in array)
			{
				if (array2 == null)
				{
					continue;
				}
				int[][] array3 = array2;
				foreach (int[] array4 in array3)
				{
					if (array4 != null && array4.Length != 0)
					{
						AddToGetWayDict((EResourceType)array4[0], array4[1], 52);
					}
				}
			}
		}
	}

	private void LoadPeakRankAwardCsvTable()
	{
		foreach (zbPeakRankAwardCsvRecord value in zbDataConfig.Instance.peakRankAwardCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 64);
			}
			AddToGetWayDict((EResourceType)value.bestAward[0], value.bestAward[1], 64);
		}
	}

	private void LoadBarPoolTreasureCsvTable()
	{
		zbBarPoolTreasureCsvRecord[] records = zbDataConfig.Instance.barPoolTreasureCsvTable.GetRecords().records;
		foreach (zbBarPoolTreasureCsvRecord zbBarPoolTreasureCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbBarPoolTreasureCsvRecord2.type, zbBarPoolTreasureCsvRecord2.id, 54);
		}
	}

	private void LoadBarPoolTreasurePlusCsvTable()
	{
		zbBarPoolTreasurePlusCsvRecord[] records = zbDataConfig.Instance.barPoolTreasurePlusCsvTable.GetRecords().records;
		foreach (zbBarPoolTreasurePlusCsvRecord zbBarPoolTreasurePlusCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbBarPoolTreasurePlusCsvRecord2.type, zbBarPoolTreasurePlusCsvRecord2.id, 54);
		}
	}

	private void LoadMineNodeCsvTable()
	{
		foreach (zbMineNodeCsvRecord value in zbDataConfig.Instance.mineNodeCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.resOut[0], value.resOut[1], 55);
			AddToGetWayDict((EResourceType)value.awardFight[0], value.awardFight[1], 55);
			AddToGetWayDict((EResourceType)value.crossAwardFight[0], value.crossAwardFight[1], 55);
		}
	}

	private void LoadArenaGiftCsvTable()
	{
		foreach (zbArenaGiftCsvRecord value in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.award[0], value.award[1], 51);
		}
	}

	private void LoadLegionWarAwardCsvTable()
	{
		foreach (zbLegionWarAwardCsvRecord value in zbDataConfig.Instance.legionWarAwardCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 70);
			}
		}
	}

	private void LoadLegionWarLegionRankCsvTable()
	{
		foreach (zbLegionWarLegionRankCsvRecord value in zbDataConfig.Instance.legionWarLegionRankCsvTable.GetRecords().records.Values)
		{
			int[][] winAwardList = value.winAwardList;
			foreach (int[] array in winAwardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 70);
			}
			winAwardList = value.failAwardList;
			foreach (int[] array2 in winAwardList)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 70);
			}
		}
	}

	private void LoadLegionWarLevelCsvTable()
	{
		foreach (zbLegionWarLevelCsvRecord value in zbDataConfig.Instance.legionWarLevelCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.awardLow[0], value.awardLow[1], 70);
			AddToGetWayDict((EResourceType)value.awardMedium[0], value.awardMedium[1], 70);
		}
	}

	private void LoadLegionWarMapCsvTable()
	{
		foreach (zbLegionWarMapCsvRecord value in zbDataConfig.Instance.legionWarMapCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 70);
			}
		}
	}

	private void LoadLegionWarPlayerRankCsvTable()
	{
		foreach (zbLegionWarPlayerRankCsvRecord value in zbDataConfig.Instance.legionWarPlayerRankCsvTable.GetRecords().records.Values)
		{
			int[][] winAwardList = value.winAwardList;
			foreach (int[] array in winAwardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 70);
			}
			winAwardList = value.failAwardList;
			foreach (int[] array2 in winAwardList)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 70);
			}
		}
	}

	private void LoadFacilityCsvTable()
	{
		zbFacilityCsvTableData records = zbDataConfig.Instance.facilityCsvTable.GetRecords();
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Facility))
		{
			return;
		}
		foreach (zbFacilityCsvRecord value in records.records.Values)
		{
			int[][] tierAwardList = value.tierAwardList1;
			foreach (int[] array in tierAwardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 71);
			}
			tierAwardList = value.tierAwardList2;
			foreach (int[] array2 in tierAwardList)
			{
				AddToGetWayDict((EResourceType)array2[0], array2[1], 71);
			}
			tierAwardList = value.tierAwardList3;
			foreach (int[] array3 in tierAwardList)
			{
				AddToGetWayDict((EResourceType)array3[0], array3[1], 71);
			}
		}
	}

	private void LoadVipLevelPackCsvTable()
	{
		foreach (zbItemData allVipDailyFreeGiftAward in zbSelf.Instance.vipHandler.GetAllVipDailyFreeGiftAwards())
		{
			AddToGetWayDict(allVipDailyFreeGiftAward.type, allVipDailyFreeGiftAward.id, 73);
		}
	}

	private void LoadVipTequanPackCsvTable()
	{
		foreach (zbItemData item in zbSelf.Instance.vipHandler.GetAllVipTqGiftAward())
		{
			AddToGetWayDict(item.type, item.id, 76);
		}
	}

	private void LoadFirstRechargeCsvTable()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		JToken val = zbDataConfig.Instance.vipBaseJsonTable.GetRecord()["novicePack"];
		if (val == null)
		{
			return;
		}
		foreach (JToken item in (JArray)val[(object)"dayAwardList"])
		{
			foreach (JArray item2 in (IEnumerable<JToken>)item)
			{
				JArray val2 = item2;
				AddToGetWayDict((EResourceType)(int)val2[0], (int)val2[1], 75);
			}
		}
	}

	private void LoadOpenServerRankCsvTable()
	{
		foreach (zbOpsMainQuestCsvRecord value in zbDataConfig.Instance.opsMainQuestCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 77);
			}
		}
	}

	private void LoadPropList()
	{
		foreach (zbPropCsvRecord value in zbDataConfig.Instance.propCsvTable.GetRecords().records.Values)
		{
			if (value.getWay == 1)
			{
				AddToGetWayDict(EResourceType.Prop, value.id, 37);
				AddToGetWayDict(EResourceType.Prop, value.id, 7);
				AddToGetWayDict(EResourceType.Prop, value.id, 9);
				AddToGetWayDict(EResourceType.Prop, value.id, 47);
				AddToGetWayDict(EResourceType.Prop, value.id, 27);
			}
			int[] operateId = value.operateId;
			foreach (int num in operateId)
			{
				switch ((EOperateType)num)
				{
				case EOperateType.LimitTimeRoulette:
					if (zbServerData.sGameData.OperateInfo.LimitTimeRouletteList != null && zbServerData.sGameData.OperateInfo.LimitTimeRouletteList.Count > 0 && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.LimitTimeRouletteList[0].BaseInfo))
					{
						AddOperateToGetWayDict(EResourceType.Prop, value.id, (EOperateType)num);
					}
					break;
				case EOperateType.LimitTimeVendor:
					if (zbServerData.sGameData.OperateInfo.LimitTimeVendorList != null && zbServerData.sGameData.OperateInfo.LimitTimeVendorList.Count > 0 && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.LimitTimeVendorList[0].BaseInfo))
					{
						AddOperateToGetWayDict(EResourceType.Prop, value.id, (EOperateType)num);
					}
					break;
				case EOperateType.LimitTimeTreasure:
					if (zbServerData.sGameData.OperateInfo.LimitTimeTreasureList != null && zbServerData.sGameData.OperateInfo.LimitTimeTreasureList.Count > 0 && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.LimitTimeTreasureList[0].BaseInfo))
					{
						AddOperateToGetWayDict(EResourceType.Prop, value.id, (EOperateType)num);
					}
					break;
				}
			}
		}
	}

	private void LoadShopList()
	{
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			foreach (ShopSlotDetail slot in shop.SlotList)
			{
				EGetWayType getWayTypeByShopType = zbSelf.Instance.shopHandler.GetGetWayTypeByShopType(shop.ShopType);
				AddToGetWayDict(slot.SlotItem.Type, slot.SlotItem.Tid, (int)getWayTypeByShopType);
			}
		}
	}

	private void LoadGeneComposeGetWay()
	{
		foreach (zbGeneCsvRecord value in zbDataConfig.Instance.geneCsvTable.GetRecords().records.Values)
		{
			if (value.quality > 2)
			{
				AddToGetWayDict(EResourceType.Gene, value.id, 58);
			}
		}
	}

	public void CleanupGetWayDictOperateValues()
	{
		foreach (string item in new List<string>(getWayDict.Keys))
		{
			List<int> list = getWayDict[item];
			list.RemoveAll((int v) => v > 1000);
			if (list.Count == 0)
			{
				getWayDict.Remove(item);
			}
		}
	}

	public void AddOperateToGetWayDict(EResourceType type, long id, EOperateType operateType)
	{
		AddToGetWayDict(type, id, (int)(1000 + operateType));
	}

	public int GetWayMaxMainQuest(EResourceType type, long id)
	{
		int mainLevel = zbServerData.sGameData.MainQuestInfo.MainLevel;
		if (cachedMainLevel != mainLevel)
		{
			mainQuestResultCache.Clear();
			cachedMainLevel = mainLevel;
		}
		long key = MakeIndexKey(type, id);
		if (mainQuestResultCache.TryGetValue(key, out var value))
		{
			return value;
		}
		int wayMaxMainQuestUncached = GetWayMaxMainQuestUncached(type, id);
		mainQuestResultCache[key] = wayMaxMainQuestUncached;
		return wayMaxMainQuestUncached;
	}

	private int GetWayMaxMainQuestUncached(EResourceType type, long id)
	{
		long key = MakeIndexKey(type, id);
		if (!mainQuestAwardIndex.TryGetValue(key, out var value) || value.Count == 0)
		{
			return GetWayMaxMainQuestCrossBox(type, id);
		}
		int num = 0;
		int num2 = 0;
		foreach (int item in value)
		{
			if (item <= cachedMainLevel)
			{
				if (item > num)
				{
					num = item;
				}
			}
			else if (num2 == 0 || item < num2)
			{
				num2 = item;
			}
		}
		if (num == 0)
		{
			num = num2;
		}
		if (num == 0)
		{
			return GetWayMaxMainQuestCrossBox(type, id);
		}
		return num;
	}

	public int GetWayMaxMainQuestCrossBox(EResourceType type, long id)
	{
		long key = MakeIndexKey(type, id);
		if (!randBoxToBoxIds.TryGetValue(key, out var value) || value.Count == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (int item in value)
		{
			int wayMaxMainQuest = GetWayMaxMainQuest(EResourceType.Box, item);
			if (wayMaxMainQuest <= cachedMainLevel)
			{
				if (wayMaxMainQuest > num)
				{
					num = wayMaxMainQuest;
				}
			}
			else if (num2 == 0 || wayMaxMainQuest < num2)
			{
				num2 = wayMaxMainQuest;
			}
		}
		if (num == 0)
		{
			num = num2;
		}
		return num;
	}

	public int GetWayMaxElite(EResourceType type, long id)
	{
		int eliteLevel = zbServerData.sGameData.EliteQuestInfo.EliteLevel;
		if (cachedEliteLevel != eliteLevel)
		{
			eliteQuestResultCache.Clear();
			cachedEliteLevel = eliteLevel;
		}
		long key = MakeIndexKey(type, id);
		if (eliteQuestResultCache.TryGetValue(key, out var value))
		{
			return value;
		}
		int wayMaxEliteUncached = GetWayMaxEliteUncached(type, id);
		eliteQuestResultCache[key] = wayMaxEliteUncached;
		return wayMaxEliteUncached;
	}

	private int GetWayMaxEliteUncached(EResourceType type, long id)
	{
		long key = MakeIndexKey(type, id);
		if (!eliteQuestAwardIndex.TryGetValue(key, out var value) || value.Count == 0)
		{
			return GetWayMaxEliteCrossBox(type, id);
		}
		int num = 0;
		int num2 = 0;
		foreach (int item in value)
		{
			if (item <= cachedEliteLevel)
			{
				if (item > num)
				{
					num = item;
				}
			}
			else if (num2 == 0 || item < num2)
			{
				num2 = item;
			}
		}
		if (num == 0)
		{
			num = num2;
		}
		if (num == 0)
		{
			return GetWayMaxEliteCrossBox(type, id);
		}
		return num;
	}

	public int GetWayMaxEliteCrossBox(EResourceType type, long id)
	{
		long key = MakeIndexKey(type, id);
		if (!randBoxToBoxIds.TryGetValue(key, out var value) || value.Count == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		foreach (int item in value)
		{
			int wayMaxElite = GetWayMaxElite(EResourceType.Box, item);
			if (wayMaxElite <= cachedEliteLevel)
			{
				if (wayMaxElite > num)
				{
					num = wayMaxElite;
				}
			}
			else if (num2 == 0 || wayMaxElite < num2)
			{
				num2 = wayMaxElite;
			}
		}
		if (num == 0)
		{
			num = num2;
		}
		return num;
	}

	public zbGetWayBuyPanelData GetGetWayBuyPanelData(EResourceType type, int id)
	{
		zbGetWayBuyPanelData zbGetWayBuyPanelData2 = GetDataFromRechargeGiftList(type, id);
		if (zbGetWayBuyPanelData2 == null)
		{
			zbGetWayBuyPanelData2 = GetDataFromDailyPackInfo(type, id);
			if (zbGetWayBuyPanelData2 == null)
			{
				zbGetWayBuyPanelData2 = GetDataFromArenaGift(type, id);
				if (zbGetWayBuyPanelData2 == null)
				{
					zbGetWayBuyPanelData2 = GetDataFromShop1(type, id);
				}
			}
		}
		return zbGetWayBuyPanelData2;
	}

	private zbGetWayBuyPanelData GetDataFromRechargeGiftList(EResourceType type, int id)
	{
		if (zbServerData.sGameData.OperateInfo.RechargeGiftList != null)
		{
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeGift.BaseInfo))
				{
					continue;
				}
				foreach (RechargeGoodsDetail goods in rechargeGift.GoodsList)
				{
					foreach (AwardDetail award in goods.AwardList)
					{
						if (award.Type == type && award.Tid == id && (goods.LimitNum == 0 || goods.LimitNum > goods.BoughtCount))
						{
							return new zbGetWayBuyPanelData(EGetWayBuyPanelType.RechargeGift, ((IEnumerable<AwardDetail>)goods.AwardList).ToArray(), goods.LimitNum, goods.BoughtCount, EMoneyType.EmptyValue, zbSelf.Instance.vipHandler.GetPriceByGoodsId(goods.GoodsId), goods.GoodsId, goods.Bonus, rechargeGift.BaseInfo);
						}
					}
				}
				foreach (RechargeDirectDetail direct in rechargeGift.DirectList)
				{
					zbDirectShopCsvRecord recordByKey = zbDataConfig.Instance.directShopCsvTable.GetRecordByKey(direct.DirectId);
					if (recordByKey.cost[0] != 1 || recordByKey.cost[1] != 2)
					{
						continue;
					}
					int[][] awardList = recordByKey.awardList;
					foreach (int[] array in awardList)
					{
						if (array[0] == (int)type && array[1] == id && (direct.LimitNum == 0 || direct.LimitNum > direct.BoughtCount))
						{
							List<AwardDetail> list = new List<AwardDetail>();
							int[][] awardList2 = recordByKey.awardList;
							foreach (int[] array2 in awardList2)
							{
								list.Add(new AwardDetail
								{
									Type = (EResourceType)array2[0],
									Tid = array2[1],
									Count = array2[2]
								});
							}
							return new zbGetWayBuyPanelData(EGetWayBuyPanelType.RechargeGiftDirect, list.ToArray(), direct.LimitNum, direct.BoughtCount, (EMoneyType)recordByKey.cost[1], recordByKey.cost[2].ToString(), direct.DirectId, recordByKey.bonus, rechargeGift.BaseInfo);
						}
					}
				}
			}
		}
		return null;
	}

	private zbGetWayBuyPanelData GetDataFromDailyPackInfo(EResourceType type, int id)
	{
		if (zbServerData.sGameData.RechargeActiveInfo != null && zbServerData.sGameData.RechargeActiveInfo.DailyPackInfo != null)
		{
			foreach (DailyPackDetail pack in zbServerData.sGameData.RechargeActiveInfo.DailyPackInfo.PackList)
			{
				if (pack.State == EActiveState.Done)
				{
					continue;
				}
				zbVipDailyPackCsvRecord recordByKey = zbDataConfig.Instance.vipDailyPackCsvTable.GetRecordByKey(pack.Id);
				int[][] awardList = recordByKey.awardList;
				foreach (int[] array in awardList)
				{
					if (array[0] == (int)type && array[1] == id)
					{
						List<AwardDetail> list = new List<AwardDetail>();
						int[][] awardList2 = recordByKey.awardList;
						foreach (int[] array2 in awardList2)
						{
							list.Add(new AwardDetail
							{
								Type = (EResourceType)array2[0],
								Tid = array2[1],
								Count = array2[2]
							});
						}
						return new zbGetWayBuyPanelData(EGetWayBuyPanelType.DailyPack, list.ToArray(), 1, pack.Count, EMoneyType.EmptyValue, zbSelf.Instance.vipHandler.GetPriceByGoodsId(recordByKey.goodsId), pack.Id, recordByKey.bonus);
					}
				}
			}
		}
		return null;
	}

	private zbGetWayBuyPanelData GetDataFromArenaGift(EResourceType type, int id)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Arena))
		{
			return null;
		}
		foreach (zbArenaGiftCsvRecord value in zbDataConfig.Instance.arenaGiftCsvTable.GetRecords().records.Values)
		{
			if (zbServerData.sGameData.ArenaInfo.BestRank <= value.layer && zbServerData.sGameData.ArenaInfo.BestRank > 0)
			{
				int arenaGiftSaleCount = zbSelf.Instance.arenaHandler.GetArenaGiftSaleCount(value.id);
				if (value.award[0] == (int)type && value.award[1] == id && (value.limitNum == 0 || value.limitNum > arenaGiftSaleCount))
				{
					List<AwardDetail> list = new List<AwardDetail>
					{
						new AwardDetail
						{
							Type = (EResourceType)value.award[0],
							Tid = value.award[1],
							Count = value.award[2]
						}
					};
					return new zbGetWayBuyPanelData(EGetWayBuyPanelType.ArenaGift, list.ToArray(), value.limitNum, arenaGiftSaleCount, (EMoneyType)value.moneyType, value.price.ToString(), value.id, value.bonus);
				}
			}
		}
		return null;
	}

	private zbGetWayBuyPanelData GetDataFromShop1(EResourceType type, int id)
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Shop1))
		{
			return null;
		}
		foreach (ShopDetail shop in zbServerData.sGameData.ShopList)
		{
			if (shop.ShopType != EShopType.Shop1)
			{
				continue;
			}
			foreach (ShopSlotDetail slot in shop.SlotList)
			{
				if (slot.MoneyType == EMoneyType.Diamond && slot.SlotItem.Type == type && slot.SlotItem.Tid == id && (slot.Limit == 0 || slot.Limit > slot.Count))
				{
					List<AwardDetail> list = new List<AwardDetail>
					{
						new AwardDetail
						{
							Type = slot.SlotItem.Type,
							Tid = slot.SlotItem.Tid,
							Count = slot.SlotItem.Count
						}
					};
					return new zbGetWayBuyPanelData(EGetWayBuyPanelType.Shop1, list.ToArray(), slot.Limit, slot.Count, slot.MoneyType, slot.Price.ToString(), slot.Index);
				}
			}
		}
		return null;
	}

	public void InsertJourneyOneGetWay()
	{
		if (zbSelf.Instance.journeyOneHandler.IsActive())
		{
			LoadChapterOneBarPoolCsvTable();
			LoadChapterOneBarTargetCsvTable();
			LoadChapterOneExchangeCsvTable();
			LoadChapterOneRechargeSumCsvTable();
			LoadChapterOneSignCsvTable();
			LoadChapterOneZhanLingCsvTable();
		}
	}

	private void LoadChapterOneBarPoolCsvTable()
	{
		zbChapterOneBarPoolCsvRecord[] records = zbDataConfig.Instance.chapterOneBarPoolCsvTable.GetRecords().records;
		foreach (zbChapterOneBarPoolCsvRecord zbChapterOneBarPoolCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterOneBarPoolCsvRecord2.type, zbChapterOneBarPoolCsvRecord2.id, 41);
		}
	}

	private void LoadChapterOneBarTargetCsvTable()
	{
		zbChapterOneBarTargetCsvRecord[] records = zbDataConfig.Instance.chapterOneBarTargetCsvTable.GetRecords().records;
		foreach (zbChapterOneBarTargetCsvRecord zbChapterOneBarTargetCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterOneBarTargetCsvRecord2.award[0], zbChapterOneBarTargetCsvRecord2.award[1], 41);
		}
	}

	private void LoadChapterOneExchangeCsvTable()
	{
		foreach (zbChapterOneExchangeCsvRecord value in zbDataConfig.Instance.chapterOneExchangeCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.award[0], value.award[1], 43);
		}
	}

	private void LoadChapterOneRechargeSumCsvTable()
	{
		foreach (zbChapterOneRechargeSumCsvRecord value in zbDataConfig.Instance.chapterOneRechargeSumCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 42);
			}
		}
	}

	private void LoadChapterOneSignCsvTable()
	{
		foreach (zbChapterOneSignCsvRecord value in zbDataConfig.Instance.chapterOneSignCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 45);
			}
		}
	}

	private void LoadChapterOneZhanLingCsvTable()
	{
		foreach (zbChapterOneZhanLingCsvRecord value in zbDataConfig.Instance.chapterOneZhanLingCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.lowAward[0], value.lowAward[1], 44);
			AddToGetWayDict((EResourceType)value.mediumAward[0], value.mediumAward[1], 44);
			AddToGetWayDict((EResourceType)value.highAward[0], value.highAward[1], 44);
		}
	}

	public void InsertJourneyThreeGetWay()
	{
		if (zbSelf.Instance.journeyThreeHandler.IsActive())
		{
			LoadChapterThreeBarPoolCsvTable();
			LoadChapterThreeBarTargetCsvTable();
			LoadChapterThreeExchangeCsvTable();
			LoadChapterThreeDrawCountCsvTable();
			LoadChapterThreeSignCsvTable();
			LoadChapterThreeZhanLingCsvTable();
		}
	}

	private void LoadChapterThreeBarPoolCsvTable()
	{
		zbChapterThreeBarPoolCsvRecord[] records = zbDataConfig.Instance.chapterThreeBarPoolCsvTable.GetRecords().records;
		foreach (zbChapterThreeBarPoolCsvRecord zbChapterThreeBarPoolCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterThreeBarPoolCsvRecord2.type, zbChapterThreeBarPoolCsvRecord2.id, 65);
		}
	}

	private void LoadChapterThreeBarTargetCsvTable()
	{
		zbChapterThreeBarTargetCsvRecord[] records = zbDataConfig.Instance.chapterThreeBarTargetCsvTable.GetRecords().records;
		foreach (zbChapterThreeBarTargetCsvRecord zbChapterThreeBarTargetCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterThreeBarTargetCsvRecord2.award[0], zbChapterThreeBarTargetCsvRecord2.award[1], 65);
		}
	}

	private void LoadChapterThreeExchangeCsvTable()
	{
		foreach (zbChapterThreeExchangeCsvRecord value in zbDataConfig.Instance.chapterThreeExchangeCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.award[0], value.award[1], 67);
		}
	}

	private void LoadChapterThreeDrawCountCsvTable()
	{
		foreach (zbChapterThreeDrawCountCsvRecord value in zbDataConfig.Instance.chapterThreeDrawCountCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 66);
			}
		}
	}

	private void LoadChapterThreeSignCsvTable()
	{
		foreach (KeyValuePair<int, zbChapterThreeSignCsvRecord> record in zbDataConfig.Instance.chapterThreeSignCsvTable.GetRecords().records)
		{
			int[][] awardList = record.Value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 68);
			}
		}
	}

	private void LoadChapterThreeZhanLingCsvTable()
	{
		foreach (zbChapterThreeZhanLingCsvRecord value in zbDataConfig.Instance.chapterThreeZhanLingCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.lowAward[0], value.lowAward[1], 69);
			AddToGetWayDict((EResourceType)value.mediumAward[0], value.mediumAward[1], 69);
			AddToGetWayDict((EResourceType)value.highAward[0], value.highAward[1], 69);
		}
	}

	public void InsertJourneyTwoGetWay()
	{
		if (zbSelf.Instance.journeyTwoHandler.IsActive())
		{
			LoadChapterTwoBarPoolCsvTable();
			LoadChapterTwoBarTargetCsvTable();
			LoadChapterTwoExchangeCsvTable();
			LoadChapterTwoDiamondCostCsvTable();
			LoadChapterTwoActiveCsvTable();
			LoadChapterTwoZhanLingCsvTable();
		}
	}

	private void LoadChapterTwoBarPoolCsvTable()
	{
		zbChapterTwoBarPoolCsvRecord[] records = zbDataConfig.Instance.chapterTwoBarPoolCsvTable.GetRecords().records;
		foreach (zbChapterTwoBarPoolCsvRecord zbChapterTwoBarPoolCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterTwoBarPoolCsvRecord2.type, zbChapterTwoBarPoolCsvRecord2.id, 59);
		}
	}

	private void LoadChapterTwoBarTargetCsvTable()
	{
		zbChapterTwoBarTargetCsvRecord[] records = zbDataConfig.Instance.chapterTwoBarTargetCsvTable.GetRecords().records;
		foreach (zbChapterTwoBarTargetCsvRecord zbChapterTwoBarTargetCsvRecord2 in records)
		{
			AddToGetWayDict((EResourceType)zbChapterTwoBarTargetCsvRecord2.award[0], zbChapterTwoBarTargetCsvRecord2.award[1], 59);
		}
	}

	private void LoadChapterTwoExchangeCsvTable()
	{
		foreach (zbChapterTwoExchangeCsvRecord value in zbDataConfig.Instance.chapterTwoExchangeCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.award[0], value.award[1], 61);
		}
	}

	private void LoadChapterTwoDiamondCostCsvTable()
	{
		foreach (zbChapterTwoDiamondCostCsvRecord value in zbDataConfig.Instance.chapterTwoDiamondCostCsvTable.GetRecords().records.Values)
		{
			int[][] awardList = value.awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 60);
			}
		}
	}

	private void LoadChapterTwoActiveCsvTable()
	{
		zbChapterTwoActiveCsvRecord[] records = zbDataConfig.Instance.chapterTwoActiveCsvTable.GetRecords().records;
		for (int i = 0; i < records.Length; i++)
		{
			int[][] awardList = records[i].awardList;
			foreach (int[] array in awardList)
			{
				AddToGetWayDict((EResourceType)array[0], array[1], 62);
			}
		}
	}

	private void LoadChapterTwoZhanLingCsvTable()
	{
		foreach (zbChapterTwoZhanLingCsvRecord value in zbDataConfig.Instance.chapterTwoZhanLingCsvTable.GetRecords().records.Values)
		{
			AddToGetWayDict((EResourceType)value.lowAward[0], value.lowAward[1], 63);
			AddToGetWayDict((EResourceType)value.mediumAward[0], value.mediumAward[1], 63);
			AddToGetWayDict((EResourceType)value.highAward[0], value.highAward[1], 63);
		}
	}

	public void InsertOperateGetWay()
	{
		if (zbServerData.sGameData.OperateInfo.RechargeDailyList != null)
		{
			foreach (RechargeDailyDetail rechargeDaily in zbServerData.sGameData.OperateInfo.RechargeDailyList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeDaily.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target in rechargeDaily.TargetList)
				{
					foreach (AwardDetail award in target.AwardList)
					{
						AddOperateToGetWayDict(award.Type, award.Tid, EOperateType.RechargeDaily);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeDailySumList != null)
		{
			foreach (RechargeSumDetail rechargeDailySum in zbServerData.sGameData.OperateInfo.RechargeDailySumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeDailySum.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target2 in rechargeDailySum.TargetList)
				{
					foreach (AwardDetail award2 in target2.AwardList)
					{
						AddOperateToGetWayDict(award2.Type, award2.Tid, EOperateType.RechargeDailySum);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeSumList != null)
		{
			foreach (RechargeSumDetail rechargeSum in zbServerData.sGameData.OperateInfo.RechargeSumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeSum.BaseInfo))
				{
					continue;
				}
				foreach (RechargeTargetDetail target3 in rechargeSum.TargetList)
				{
					foreach (AwardDetail award3 in target3.AwardList)
					{
						AddOperateToGetWayDict(award3.Type, award3.Tid, EOperateType.RechargeSum);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ConsumeDailySumList != null)
		{
			foreach (ConsumeSumDetail consumeDailySum in zbServerData.sGameData.OperateInfo.ConsumeDailySumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(consumeDailySum.BaseInfo))
				{
					continue;
				}
				foreach (ConsumeTargetDetail target4 in consumeDailySum.TargetList)
				{
					foreach (AwardDetail award4 in target4.AwardList)
					{
						AddOperateToGetWayDict(award4.Type, award4.Tid, EOperateType.ConsumeDailySum);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ConsumeSumList != null)
		{
			foreach (ConsumeSumDetail consumeSum in zbServerData.sGameData.OperateInfo.ConsumeSumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(consumeSum.BaseInfo))
				{
					continue;
				}
				foreach (ConsumeTargetDetail target5 in consumeSum.TargetList)
				{
					foreach (AwardDetail award5 in target5.AwardList)
					{
						AddOperateToGetWayDict(award5.Type, award5.Tid, EOperateType.ConsumeSum);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeHeroList != null)
		{
			foreach (LimitTimeHeroDetail limitTimeHero in zbServerData.sGameData.OperateInfo.LimitTimeHeroList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(limitTimeHero.BaseInfo))
				{
					continue;
				}
				AddOperateToGetWayDict(EResourceType.Hero, limitTimeHero.BaseData.HeroTid, EOperateType.LimitTimeHero);
				AddOperateToGetWayDict(EResourceType.HeroFrag, limitTimeHero.BaseData.HeroTid, EOperateType.LimitTimeHero);
				foreach (LimitTimeHeroScoreDetail score in limitTimeHero.ScoreList)
				{
					foreach (AwardDetail award6 in score.AwardList)
					{
						AddOperateToGetWayDict(award6.Type, award6.Tid, EOperateType.LimitTimeHero);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeShopList != null)
		{
			foreach (LimitTimeShopDetail limitTimeShop in zbServerData.sGameData.OperateInfo.LimitTimeShopList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(limitTimeShop.BaseInfo))
				{
					continue;
				}
				foreach (LimitTimeShopItemDetail item in limitTimeShop.ItemList)
				{
					AddOperateToGetWayDict(item.AwardDetail.Type, item.AwardDetail.Tid, EOperateType.LimitTimeShop);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LoginSumList != null)
		{
			foreach (LoginSumDetail loginSum in zbServerData.sGameData.OperateInfo.LoginSumList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(loginSum.BaseInfo))
				{
					continue;
				}
				foreach (OperateTargetDetail target6 in loginSum.TargetList)
				{
					foreach (AwardDetail award7 in target6.AwardList)
					{
						AddOperateToGetWayDict(award7.Type, award7.Tid, EOperateType.LoginSum);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.DailyActiveList != null)
		{
			foreach (DailyActiveDetail dailyActive in zbServerData.sGameData.OperateInfo.DailyActiveList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(dailyActive.BaseInfo))
				{
					continue;
				}
				foreach (AwardDetail award8 in dailyActive.AwardList)
				{
					AddOperateToGetWayDict(award8.Type, award8.Tid, EOperateType.DailyActive);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.BarDrawList != null)
		{
			foreach (BarDrawDetail barDraw in zbServerData.sGameData.OperateInfo.BarDrawList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(barDraw.BaseInfo))
				{
					continue;
				}
				foreach (OperateTargetDetail target7 in barDraw.TargetList)
				{
					foreach (AwardDetail award9 in target7.AwardList)
					{
						if (barDraw.BarType == EBarType.Normal)
						{
							AddOperateToGetWayDict(award9.Type, award9.Tid, EOperateType.BarDrawNormal);
						}
						else if (barDraw.BarType == EBarType.Special)
						{
							AddOperateToGetWayDict(award9.Type, award9.Tid, EOperateType.BarDrawSpecial);
						}
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.PropExchangeList != null)
		{
			foreach (PropExchangeDetail propExchange in zbServerData.sGameData.OperateInfo.PropExchangeList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(propExchange.BaseInfo))
				{
					continue;
				}
				foreach (OperateTargetDetail target8 in propExchange.TargetList)
				{
					foreach (AwardDetail award10 in target8.AwardList)
					{
						AddOperateToGetWayDict(award10.Type, award10.Tid, EOperateType.PropExchange);
					}
				}
				foreach (OperatePropExchangeDetail exchange in propExchange.ExchangeList)
				{
					foreach (AwardDetail award11 in exchange.AwardList)
					{
						AddOperateToGetWayDict(award11.Type, award11.Tid, EOperateType.PropExchange);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ScratchCardList != null)
		{
			foreach (ScratchCardDetail scratchCard in zbServerData.sGameData.OperateInfo.ScratchCardList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(scratchCard.BaseInfo))
				{
					continue;
				}
				foreach (OperateScratchCardDetail target9 in scratchCard.TargetList)
				{
					foreach (AwardDetail award12 in target9.AwardList)
					{
						AddOperateToGetWayDict(award12.Type, award12.Tid, EOperateType.ScratchCard);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.JiZiList != null)
		{
			foreach (JiZiDetail jiZi in zbServerData.sGameData.OperateInfo.JiZiList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(jiZi.BaseInfo))
				{
					continue;
				}
				foreach (OperateJiZiExchangeDetail exchange2 in jiZi.ExchangeList)
				{
					foreach (AwardDetail award13 in exchange2.AwardList)
					{
						AddOperateToGetWayDict(award13.Type, award13.Tid, EOperateType.JiZi);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.CipherBoxList != null)
		{
			foreach (CipherBoxDetail cipherBox in zbServerData.sGameData.OperateInfo.CipherBoxList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(cipherBox.BaseInfo))
				{
					continue;
				}
				foreach (CipherBoxTargetDetail target10 in cipherBox.TargetList)
				{
					AddOperateToGetWayDict(target10.Award.Type, target10.Award.Tid, EOperateType.CipherBox);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RechargeGiftList != null)
		{
			foreach (RechargeGiftDetail rechargeGift in zbServerData.sGameData.OperateInfo.RechargeGiftList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(rechargeGift.BaseInfo))
				{
					continue;
				}
				foreach (RechargeGoodsDetail goods in rechargeGift.GoodsList)
				{
					foreach (AwardDetail award14 in goods.AwardList)
					{
						AddOperateToGetWayDict(award14.Type, award14.Tid, EOperateType.RechargeGift);
					}
				}
				foreach (RechargeDirectDetail direct in rechargeGift.DirectList)
				{
					int[][] awardList = zbDataConfig.Instance.directShopCsvTable.GetRecordByKey(direct.DirectId).awardList;
					foreach (int[] array in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array[0], array[1], EOperateType.RechargeGift);
					}
				}
				foreach (FreeGiftDetail freeGift in zbServerData.sGameData.FreeGiftList)
				{
					if (rechargeGift.BaseInfo.Type != EOperatePlanType.Holiday && freeGift.FreeType == 1)
					{
						AddOperateToGetWayDict(freeGift.Award.Type, freeGift.Award.Tid, EOperateType.RechargeGift);
					}
					else if (rechargeGift.BaseInfo.Type == EOperatePlanType.Holiday && freeGift.FreeType == 2)
					{
						AddOperateToGetWayDict(freeGift.Award.Type, freeGift.Award.Tid, EOperateType.RechargeGift);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeCatcherList != null)
		{
			foreach (LimitTimeCatcherDetail limitTimeCatcher in zbServerData.sGameData.OperateInfo.LimitTimeCatcherList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(limitTimeCatcher.BaseInfo))
				{
					continue;
				}
				foreach (int hero in limitTimeCatcher.HeroList)
				{
					AddOperateToGetWayDict(EResourceType.Hero, hero, EOperateType.LimitTimeCatcher);
				}
				foreach (AwardDetail award15 in limitTimeCatcher.AwardList)
				{
					AddOperateToGetWayDict(award15.Type, award15.Tid, EOperateType.LimitTimeCatcher);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.BarrageCmdList != null)
		{
			foreach (BarrageCmdDetail barrageCmd in zbServerData.sGameData.OperateInfo.BarrageCmdList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(barrageCmd.BaseInfo))
				{
					continue;
				}
				foreach (AwardDetail award16 in barrageCmd.AwardList)
				{
					AddOperateToGetWayDict(award16.Type, award16.Tid, EOperateType.BarrageCmd);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.PrayLanternList != null)
		{
			foreach (PrayLanternDetail prayLantern in zbServerData.sGameData.OperateInfo.PrayLanternList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(prayLantern.BaseInfo))
				{
					continue;
				}
				foreach (OperateTargetDetail target11 in prayLantern.TargetList)
				{
					foreach (AwardDetail award17 in target11.AwardList)
					{
						AddOperateToGetWayDict(award17.Type, award17.Tid, EOperateType.PrayLantern);
					}
				}
				foreach (UltimateAwardDetail ultimateAward in prayLantern.Ultimate.UltimateAwardList)
				{
					AddOperateToGetWayDict(ultimateAward.Award.Type, ultimateAward.Award.Tid, EOperateType.PrayLantern);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ResourceExchangeList != null)
		{
			foreach (ResourceExchangeDetail resourceExchange in zbServerData.sGameData.OperateInfo.ResourceExchangeList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(resourceExchange.BaseInfo))
				{
					continue;
				}
				foreach (ExchangeItemDetail exchange3 in resourceExchange.ExchangeList)
				{
					foreach (AwardDetail dst in exchange3.DstList)
					{
						AddOperateToGetWayDict(dst.Type, dst.Tid, EOperateType.ResourceExchange);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.HolidayTaskList != null)
		{
			foreach (HolidayTaskDetail holidayTask in zbServerData.sGameData.OperateInfo.HolidayTaskList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(holidayTask.BaseInfo))
				{
					continue;
				}
				AddOperateToGetWayDict(EResourceType.Hero, holidayTask.HeroId, EOperateType.HolidayTask);
				foreach (HolidayTaskExchangeDetail exchange4 in holidayTask.ExchangeList)
				{
					foreach (AwardDetail award18 in exchange4.AwardList)
					{
						AddOperateToGetWayDict(award18.Type, award18.Tid, EOperateType.HolidayTask);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.FusionInfo != null && zbSelf.Instance.operateHandler.IsShowTime(zbServerData.sGameData.OperateInfo.FusionInfo.BaseInfo))
		{
			foreach (OperateTargetDetail target12 in zbServerData.sGameData.OperateInfo.FusionInfo.FusionBase.TargetList)
			{
				foreach (AwardDetail award19 in target12.AwardList)
				{
					AddOperateToGetWayDict(award19.Type, award19.Tid, EOperateType.Fusion);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.InfiniteGiftList != null)
		{
			foreach (InfiniteGiftDetail infiniteGift in zbServerData.sGameData.OperateInfo.InfiniteGiftList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(infiniteGift.BaseInfo))
				{
					continue;
				}
				foreach (InfiniteGiftGoodsDetail goods2 in infiniteGift.GoodsList)
				{
					foreach (AwardDetail award20 in goods2.AwardList)
					{
						AddOperateToGetWayDict(award20.Type, award20.Tid, EOperateType.InfiniteGift);
					}
				}
				foreach (OperateTargetDetail target13 in infiniteGift.TargetList)
				{
					foreach (AwardDetail award21 in target13.AwardList)
					{
						AddOperateToGetWayDict(award21.Type, award21.Tid, EOperateType.InfiniteGift);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.LimitTimeRouletteList != null)
		{
			foreach (LimitTimeRouletteDetail limitTimeRoulette in zbServerData.sGameData.OperateInfo.LimitTimeRouletteList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(limitTimeRoulette.BaseInfo))
				{
					continue;
				}
				foreach (LimitTimeRouletteRandDetail rand in limitTimeRoulette.RandList)
				{
					AddOperateToGetWayDict(rand.AwardDetail.Type, rand.AwardDetail.Tid, EOperateType.LimitTimeRoulette);
				}
				foreach (OperateTargetDetail target14 in limitTimeRoulette.TargetList)
				{
					foreach (AwardDetail award22 in target14.AwardList)
					{
						AddOperateToGetWayDict(award22.Type, award22.Tid, EOperateType.LimitTimeRoulette);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.RestaurantList != null)
		{
			foreach (RestaurantDetail restaurant in zbServerData.sGameData.OperateInfo.RestaurantList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(restaurant.BaseInfo))
				{
					continue;
				}
				foreach (AwardDetail award23 in restaurant.TargetDetail.AwardList)
				{
					AddOperateToGetWayDict(award23.Type, award23.Tid, EOperateType.Restaurant);
				}
				foreach (AwardDetail grandAward in restaurant.TargetNextGrand.GrandAwardList)
				{
					AddOperateToGetWayDict(grandAward.Type, grandAward.Tid, EOperateType.Restaurant);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ThemeTaskList != null)
		{
			foreach (ThemeTaskDetail themeTask in zbServerData.sGameData.OperateInfo.ThemeTaskList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(themeTask.BaseInfo))
				{
					continue;
				}
				foreach (OperateTargetDetail target15 in themeTask.TargetList)
				{
					foreach (AwardDetail award24 in target15.AwardList)
					{
						AddOperateToGetWayDict(award24.Type, award24.Tid, EOperateType.ThemeTask);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.ZhanLingKillList != null)
		{
			foreach (ZhanLingKillDetail zhanLingKill in zbServerData.sGameData.OperateInfo.ZhanLingKillList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(zhanLingKill.BaseInfo))
				{
					continue;
				}
				foreach (OperateZhanLingExtraDetail grandExtra in zhanLingKill.GrandExtraList)
				{
					AddOperateToGetWayDict(grandExtra.Award.Type, grandExtra.Award.Tid, EOperateType.ZhanLingKill);
				}
				foreach (zbZhanLingKillTargetCsvRecord value in zbDataConfig.Instance.zhanLingKillTargetCsvTable.GetRecords().records.Values)
				{
					int[][] awardList = value.lowAward;
					foreach (int[] array2 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array2[0], array2[1], EOperateType.ZhanLingKill);
					}
					awardList = value.highAward;
					foreach (int[] array3 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array3[0], array3[1], EOperateType.ZhanLingKill);
					}
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.SnowVeilList != null)
		{
			foreach (SnowVeilDetail snowVeil in zbServerData.sGameData.OperateInfo.SnowVeilList)
			{
				if (!zbSelf.Instance.operateHandler.IsShowTime(snowVeil.BaseInfo))
				{
					continue;
				}
				foreach (SnowVeilLandDetail land in snowVeil.LandList)
				{
					if (land.Item != null)
					{
						AddOperateToGetWayDict(land.Item.Type, land.Item.Tid, EOperateType.SnowVeil);
					}
				}
				foreach (SnowVeilTurntableSlotDetail slot in snowVeil.Turntable.SlotList)
				{
					AddOperateToGetWayDict(slot.Award.Type, slot.Award.Tid, EOperateType.SnowVeil);
				}
				foreach (AwardDetail award25 in snowVeil.TargetDetail.AwardList)
				{
					AddOperateToGetWayDict(award25.Type, award25.Tid, EOperateType.SnowVeil);
				}
				foreach (AwardDetail grandAward2 in snowVeil.TargetNextGrand.GrandAwardList)
				{
					AddOperateToGetWayDict(grandAward2.Type, grandAward2.Tid, EOperateType.SnowVeil);
				}
				AddOperateToGetWayDict(snowVeil.Rolling.SingleAward.Type, snowVeil.Rolling.SingleAward.Tid, EOperateType.SnowVeil);
			}
		}
		if (zbServerData.sGameData.OperateInfo.BossSubjectList == null)
		{
			return;
		}
		foreach (BossSubjectDetail bossSubject in zbServerData.sGameData.OperateInfo.BossSubjectList)
		{
			if (!zbSelf.Instance.operateHandler.IsShowTime(bossSubject.BaseInfo))
			{
				continue;
			}
			foreach (LimitTimeRouletteRandDetail slot2 in bossSubject.RouletteInfo.SlotList)
			{
				AddOperateToGetWayDict(slot2.AwardDetail.Type, slot2.AwardDetail.Tid, EOperateType.BossSubject);
			}
			foreach (OperateTargetDetail target16 in bossSubject.RouletteInfo.TargetList)
			{
				foreach (AwardDetail award26 in target16.AwardList)
				{
					AddOperateToGetWayDict(award26.Type, award26.Tid, EOperateType.BossSubject);
				}
			}
			foreach (KeyValuePair<int, zbBossLevelCsvRecord> record in zbDataConfig.Instance.bossLevelCsvTable.GetRecords().records)
			{
				int[][] awardList = record.Value.awardShow;
				foreach (int[] array4 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array4[0], array4[1], EOperateType.BossSubject);
				}
				awardList = record.Value.baseAward;
				foreach (int[] array5 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array5[0], array5[1], EOperateType.BossSubject);
				}
				awardList = record.Value.growthAward;
				foreach (int[] array6 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array6[0], array6[1], EOperateType.BossSubject);
				}
				awardList = record.Value.spAward1;
				foreach (int[] array7 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array7[0], array7[1], EOperateType.BossSubject);
				}
				awardList = record.Value.spAward2;
				foreach (int[] array8 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array8[0], array8[1], EOperateType.BossSubject);
				}
				awardList = record.Value.spAward3;
				foreach (int[] array9 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array9[0], array9[1], EOperateType.BossSubject);
				}
				awardList = record.Value.spAward4;
				foreach (int[] array10 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array10[0], array10[1], EOperateType.BossSubject);
				}
				awardList = record.Value.winAward;
				foreach (int[] array11 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array11[0], array11[1], EOperateType.BossSubject);
				}
			}
			foreach (KeyValuePair<int, zbBossTaskCsvRecord> record2 in zbDataConfig.Instance.bossTaskCsvTable.GetRecords().records)
			{
				int[][] awardList = record2.Value.award;
				foreach (int[] array12 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array12[0], array12[1], EOperateType.BossSubject);
				}
			}
			foreach (KeyValuePair<int, zbBossZhanlingCsvRecord> record3 in zbDataConfig.Instance.bossZhanlingCsvTable.GetRecords().records)
			{
				int[][] awardList = record3.Value.lowAward;
				foreach (int[] array13 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array13[0], array13[1], EOperateType.BossSubject);
				}
				awardList = record3.Value.highAward;
				foreach (int[] array14 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array14[0], array14[1], EOperateType.BossSubject);
				}
			}
			foreach (zbBossSubjectCsvRecord value2 in zbDataConfig.Instance.bossSubjectCsvTable.GetRecords().records.Values)
			{
				if (bossSubject.BossSubject.SubjectId == value2.id)
				{
					int[][] awardList = value2.zhanLingAward;
					foreach (int[] array15 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array15[0], array15[1], EOperateType.BossSubject);
					}
					AddOperateToGetWayDict((EResourceType)value2.grandPrizeAward[0], value2.grandPrizeAward[1], EOperateType.BossSubject);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.WallSubjectList == null)
		{
			return;
		}
		foreach (WallSubjectDetail wallSubject in zbServerData.sGameData.OperateInfo.WallSubjectList)
		{
			if (!zbSelf.Instance.operateHandler.IsShowTime(wallSubject.BaseInfo))
			{
				continue;
			}
			foreach (LimitTimeRouletteRandDetail slot3 in wallSubject.RouletteInfo.SlotList)
			{
				AddOperateToGetWayDict(slot3.AwardDetail.Type, slot3.AwardDetail.Tid, EOperateType.WallSubject);
			}
			foreach (OperateTargetDetail target17 in wallSubject.RouletteInfo.TargetList)
			{
				foreach (AwardDetail award27 in target17.AwardList)
				{
					AddOperateToGetWayDict(award27.Type, award27.Tid, EOperateType.WallSubject);
				}
			}
			zbWallSubjectLevelCsvRecord[] records = zbDataConfig.Instance.wallSubjectLevelCsvTable.GetRecords().records;
			for (int i = 0; i < records.Length; i++)
			{
				int[][] awardList = records[i].award;
				foreach (int[] array16 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array16[0], array16[1], EOperateType.WallSubject);
				}
			}
			foreach (KeyValuePair<int, zbWallSubjectZhanLingCsvRecord> record4 in zbDataConfig.Instance.wallSubjectZhanLingCsvTable.GetRecords().records)
			{
				int[][] awardList = record4.Value.lowAward;
				foreach (int[] array17 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array17[0], array17[1], EOperateType.WallSubject);
				}
				awardList = record4.Value.highAward;
				foreach (int[] array18 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array18[0], array18[1], EOperateType.WallSubject);
				}
			}
			foreach (zbWallSubjectCsvRecord value3 in zbDataConfig.Instance.wallSubjectCsvTable.GetRecords().records.Values)
			{
				if (wallSubject.SubjectId == value3.id)
				{
					int[][] awardList = value3.chapterGrandAward;
					foreach (int[] array19 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array19[0], array19[1], EOperateType.WallSubject);
					}
					awardList = value3.zhanLingAward;
					foreach (int[] array20 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array20[0], array20[1], EOperateType.WallSubject);
					}
					AddOperateToGetWayDict((EResourceType)value3.grandPrizeAward[0], value3.grandPrizeAward[1], EOperateType.WallSubject);
				}
			}
		}
		if (zbServerData.sGameData.OperateInfo.MechaSubjectList == null)
		{
			return;
		}
		foreach (MechaSubjectDetail mechaSubject in zbServerData.sGameData.OperateInfo.MechaSubjectList)
		{
			if (!zbSelf.Instance.operateHandler.IsShowTime(mechaSubject.BaseInfo))
			{
				continue;
			}
			foreach (LimitTimeRouletteRandDetail slot4 in mechaSubject.RouletteInfo.SlotList)
			{
				AddOperateToGetWayDict(slot4.AwardDetail.Type, slot4.AwardDetail.Tid, EOperateType.MechaSubject);
			}
			foreach (OperateTargetDetail target18 in mechaSubject.RouletteInfo.TargetList)
			{
				foreach (AwardDetail award28 in target18.AwardList)
				{
					AddOperateToGetWayDict(award28.Type, award28.Tid, EOperateType.MechaSubject);
				}
			}
			zbMechaSubjectLevelCsvRecord[] records2 = zbDataConfig.Instance.mechaSubjectLevelCsvTable.GetRecords().records;
			for (int i = 0; i < records2.Length; i++)
			{
				int[][] awardList = records2[i].award;
				foreach (int[] array21 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array21[0], array21[1], EOperateType.MechaSubject);
				}
			}
			foreach (KeyValuePair<int, zbMechaSubjectZhanLingCsvRecord> record5 in zbDataConfig.Instance.mechaSubjectZhanLingCsvTable.GetRecords().records)
			{
				int[][] awardList = record5.Value.lowAward;
				foreach (int[] array22 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array22[0], array22[1], EOperateType.MechaSubject);
				}
				awardList = record5.Value.highAward;
				foreach (int[] array23 in awardList)
				{
					AddOperateToGetWayDict((EResourceType)array23[0], array23[1], EOperateType.MechaSubject);
				}
			}
			foreach (zbMechaSubjectCsvRecord value4 in zbDataConfig.Instance.mechaSubjectCsvTable.GetRecords().records.Values)
			{
				if (mechaSubject.SubjectId == value4.id)
				{
					int[][] awardList = value4.chapterGrandAward;
					foreach (int[] array24 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array24[0], array24[1], EOperateType.MechaSubject);
					}
					awardList = value4.zhanLingAward;
					foreach (int[] array25 in awardList)
					{
						AddOperateToGetWayDict((EResourceType)array25[0], array25[1], EOperateType.MechaSubject);
					}
					AddOperateToGetWayDict((EResourceType)value4.grandPrizeAward[0], value4.grandPrizeAward[1], EOperateType.MechaSubject);
				}
			}
		}
	}
}
