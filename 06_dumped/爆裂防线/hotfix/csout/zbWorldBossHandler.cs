using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldBossHandler
{
	public RankData worldBossSelfRank;

	public WorldBossInfo worldBossInfo;

	private Dictionary<int, List<zbWorldBossHurtAwardCsvRecord>> m_worldBossHurtAwardRecordsDict;

	public int worldBossuuid;

	public int selfRank;

	public void Init()
	{
		zbWorldBossHurtAwardCsvTableData records = zbDataConfig.Instance.worldBossHurtAwardCsvTable.GetRecords();
		m_worldBossHurtAwardRecordsDict = new Dictionary<int, List<zbWorldBossHurtAwardCsvRecord>>();
		zbWorldBossHurtAwardCsvRecord[] records2 = records.records;
		foreach (zbWorldBossHurtAwardCsvRecord zbWorldBossHurtAwardCsvRecord2 in records2)
		{
			if (!m_worldBossHurtAwardRecordsDict.ContainsKey(zbWorldBossHurtAwardCsvRecord2.bossid))
			{
				m_worldBossHurtAwardRecordsDict.Add(zbWorldBossHurtAwardCsvRecord2.bossid, new List<zbWorldBossHurtAwardCsvRecord>());
			}
			m_worldBossHurtAwardRecordsDict[zbWorldBossHurtAwardCsvRecord2.bossid].Add(zbWorldBossHurtAwardCsvRecord2);
		}
	}

	public void UpdateWorldBossSelfInfo(WorldBossSelfInfo _worldBossSelfInfo)
	{
		zbServerData.sGameData.WorldBossSelfInfo = _worldBossSelfInfo;
	}

	public void UpdateWorldBossSelfRank(RankData _worldBossSelfRank)
	{
		worldBossSelfRank = _worldBossSelfRank;
	}

	public void UpdateWorldBossSelfRank(int _selfRank)
	{
		selfRank = _selfRank;
	}

	public void UpdateWorldBossInfo(WorldBossInfo _worldBossInfo)
	{
		worldBossInfo = _worldBossInfo;
		worldBossuuid = _worldBossInfo.UUID;
	}

	public void UpdateWorldBossUUid(int uuid)
	{
		worldBossuuid = uuid;
	}

	public bool IsWorldBossOpen()
	{
		if (zbServerData.sGameData.WorldBossSelfInfo == null)
		{
			return false;
		}
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		if (serverTime >= zbServerData.sGameData.WorldBossSelfInfo.StartTm && serverTime <= zbServerData.sGameData.WorldBossSelfInfo.EndTm)
		{
			return true;
		}
		return false;
	}

	public List<zbItemData> GetWorldBossPauseAwardByHurt(List<zbItemData> zbItemDatas)
	{
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_worldBossHurtAwardRecordsDict == null || !m_worldBossHurtAwardRecordsDict.ContainsKey(enemyId))
		{
			zbItemDatas.Clear();
			return zbItemDatas;
		}
		List<zbWorldBossHurtAwardCsvRecord> list = m_worldBossHurtAwardRecordsDict[enemyId];
		Dictionary<string, zbItemData> dictionary = new Dictionary<string, zbItemData>();
		foreach (zbWorldBossHurtAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum >= item.hurt)
			{
				zbItemData zbItemData2 = new zbItemData((EResourceType)item.awardShow[0], item.awardShow[1], item.awardShow[2]);
				string key = $"{zbItemData2.type}_{zbItemData2.id}";
				if (dictionary.ContainsKey(key))
				{
					dictionary[key].count += zbItemData2.count;
				}
				else
				{
					dictionary.Add(key, zbItemData2);
				}
			}
		}
		zbItemDatas.Clear();
		zbItemDatas.AddRange(dictionary.Values);
		return zbItemDatas;
	}

	public int ClaimedRewardCount()
	{
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_worldBossHurtAwardRecordsDict == null || !m_worldBossHurtAwardRecordsDict.ContainsKey(enemyId))
		{
			return 0;
		}
		List<zbWorldBossHurtAwardCsvRecord> list = m_worldBossHurtAwardRecordsDict[enemyId];
		int num = 0;
		foreach (zbWorldBossHurtAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum >= item.hurt)
			{
				num++;
			}
		}
		return num;
	}

	public Length KilledProgressToNextReward()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_worldBossHurtAwardRecordsDict == null || !m_worldBossHurtAwardRecordsDict.ContainsKey(enemyId))
		{
			return new Length(0f, (LengthUnit)1);
		}
		List<zbWorldBossHurtAwardCsvRecord> list = m_worldBossHurtAwardRecordsDict[enemyId];
		list.Sort((zbWorldBossHurtAwardCsvRecord a, zbWorldBossHurtAwardCsvRecord b) => a.hurt.CompareTo(b.hurt));
		long num = 0L;
		foreach (zbWorldBossHurtAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum <= item.hurt)
			{
				long num2 = item.hurt - num;
				long num3 = zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum - num;
				return new Length(Mathf.Clamp((num2 > 0) ? ((float)num3 / (float)num2 * 100f) : 100f, 0f, 100f), (LengthUnit)1);
			}
			num = item.hurt;
		}
		return new Length(100f, (LengthUnit)1);
	}

	public long GetNextStepDmg(int id, long dmg)
	{
		if (m_worldBossHurtAwardRecordsDict == null || !m_worldBossHurtAwardRecordsDict.ContainsKey(id))
		{
			return 0L;
		}
		List<zbWorldBossHurtAwardCsvRecord> list = m_worldBossHurtAwardRecordsDict[id];
		list.Sort((zbWorldBossHurtAwardCsvRecord a, zbWorldBossHurtAwardCsvRecord b) => a.hurt.CompareTo(b.hurt));
		foreach (zbWorldBossHurtAwardCsvRecord item in list)
		{
			if (item.hurt > dmg)
			{
				return item.hurt;
			}
		}
		return 0L;
	}

	public zbItemData GetNextStepAward()
	{
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_worldBossHurtAwardRecordsDict == null || !m_worldBossHurtAwardRecordsDict.ContainsKey(enemyId))
		{
			return null;
		}
		List<zbWorldBossHurtAwardCsvRecord> list = m_worldBossHurtAwardRecordsDict[enemyId];
		list.Sort((zbWorldBossHurtAwardCsvRecord a, zbWorldBossHurtAwardCsvRecord b) => a.hurt.CompareTo(b.hurt));
		foreach (zbWorldBossHurtAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum > item.hurt && zbCombatSystem.Instance.combatData.masterTeam.combatStat.damageBossSum < item.hurt)
			{
				return new zbItemData((EResourceType)item.awardShow[0], item.awardShow[1], item.awardShow[2]);
			}
		}
		return null;
	}

	public bool HasRedPoint()
	{
		if (!zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldBoss) && IsWorldBossOpen() && zbServerData.sGameData.WorldBossSelfInfo?.DailyCount > zbServerData.sGameData.WorldBossSelfInfo?.CombatCount)
		{
			return true;
		}
		return false;
	}
}
