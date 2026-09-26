using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbWorldLairHandler
{
	private Dictionary<int, List<zbWorldLairKillAwardCsvRecord>> m_worldLairKillAwardRecordsDict;

	private zbWorldLairLevelCsvRecord m_worldLairLevelRecord;

	public int worldLairLevel;

	public WorldLairDetail m_worldLairInfo;

	public RoleBaseInfo m_worldLairHelpRoleBaseInfo;

	public string worldLairUuid = "";

	public int helpCount;

	private JToken m_worldBase;

	public string m_localDate;

	public WorldLairHelpInfo[] m_worldLairHelpInfos;

	public void Init()
	{
		m_worldBase = (JToken)(object)zbDataConfig.Instance.worldBaseJsonTable.GetRecord();
		m_localDate = zbGameSave.Instance.GetString("WorldLairFirstDate" + zbSelf.Instance.GetRoleId());
		zbWorldLairKillAwardCsvTableData records = zbDataConfig.Instance.worldLairKillAwardCsvTable.GetRecords();
		m_worldLairKillAwardRecordsDict = new Dictionary<int, List<zbWorldLairKillAwardCsvRecord>>();
		zbWorldLairKillAwardCsvRecord[] records2 = records.records;
		foreach (zbWorldLairKillAwardCsvRecord zbWorldLairKillAwardCsvRecord2 in records2)
		{
			if (!m_worldLairKillAwardRecordsDict.ContainsKey(zbWorldLairKillAwardCsvRecord2.id))
			{
				m_worldLairKillAwardRecordsDict.Add(zbWorldLairKillAwardCsvRecord2.id, new List<zbWorldLairKillAwardCsvRecord>());
			}
			m_worldLairKillAwardRecordsDict[zbWorldLairKillAwardCsvRecord2.id].Add(zbWorldLairKillAwardCsvRecord2);
		}
	}

	public void UpdateWorldLairHelpCount(int _helpCount)
	{
		helpCount = _helpCount;
	}

	public void UpdateWorldLairLevel(int level)
	{
		worldLairLevel = level;
		m_worldLairLevelRecord = zbDataConfig.Instance.worldLairLevelCsvTable.GetRecordByKey(level);
	}

	public List<zbItemData> GetLairPauseAwardByKilled(List<zbItemData> zbItemDatas)
	{
		if (m_worldLairLevelRecord == null || m_worldLairKillAwardRecordsDict == null || !m_worldLairKillAwardRecordsDict.ContainsKey(m_worldLairLevelRecord.killAwardId))
		{
			zbItemDatas.Clear();
			return zbItemDatas;
		}
		Dictionary<string, zbItemData> dictionary = new Dictionary<string, zbItemData>();
		foreach (zbWorldLairKillAwardCsvRecord item in m_worldLairKillAwardRecordsDict[m_worldLairLevelRecord.killAwardId])
		{
			if ((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount < item.killCount)
			{
				continue;
			}
			int[][] awardList = item.awardList;
			foreach (int[] array in awardList)
			{
				zbItemData zbItemData2 = new zbItemData((EResourceType)array[0], array[1], array[2]);
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

	public void UpdateWorldLairUuid(string uuid)
	{
		worldLairUuid = uuid;
	}

	public void UpdateWorldLairInfo(WorldLairDetail worldLairInfo)
	{
		m_worldLairInfo = worldLairInfo;
	}

	public Length KilledProgressToNextReward()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		if (m_worldLairLevelRecord == null || m_worldLairKillAwardRecordsDict == null || !m_worldLairKillAwardRecordsDict.ContainsKey(m_worldLairLevelRecord.killAwardId))
		{
			return new Length(0f, (LengthUnit)1);
		}
		List<zbWorldLairKillAwardCsvRecord> list = m_worldLairKillAwardRecordsDict[m_worldLairLevelRecord.killAwardId];
		list.Sort((zbWorldLairKillAwardCsvRecord a, zbWorldLairKillAwardCsvRecord b) => a.killCount.CompareTo(b.killCount));
		int num = 0;
		foreach (zbWorldLairKillAwardCsvRecord item in list)
		{
			if ((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount <= item.killCount)
			{
				int num2 = item.killCount - num;
				int num3 = (int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount - num;
				return new Length(Mathf.Clamp((num2 > 0) ? ((float)num3 / (float)num2 * 100f) : 100f, 0f, 100f), (LengthUnit)1);
			}
			num = item.killCount;
		}
		return new Length(100f, (LengthUnit)1);
	}

	public int ClaimedRewardCount()
	{
		if (m_worldLairLevelRecord == null || m_worldLairKillAwardRecordsDict == null || !m_worldLairKillAwardRecordsDict.ContainsKey(m_worldLairLevelRecord.killAwardId))
		{
			return 0;
		}
		List<zbWorldLairKillAwardCsvRecord> list = m_worldLairKillAwardRecordsDict[m_worldLairLevelRecord.killAwardId];
		int num = 0;
		foreach (zbWorldLairKillAwardCsvRecord item in list)
		{
			if ((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount >= item.killCount)
			{
				num++;
			}
		}
		return num;
	}

	public void RefreshHead(VisualElement headFrame, RoleBaseInfo roleBaseInfo)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(roleBaseInfo.HeadFrameId);
		headFrame.style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey.icon));
		zbPlayerHeadCsvRecord recordByKey2 = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(roleBaseInfo.HeadId);
		UQueryExtensions.Q<VisualElement>(headFrame, "head", (string)null).style.backgroundImage = new StyleBackground(zbAssetLoader.Instance.LoadResouce<Sprite>(recordByKey2.icon));
	}

	public bool HasWorldLairRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.WorldLair) || zbServerData.sGameData.WorldLairInfo == null || zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo == null)
		{
			return false;
		}
		if (zbServerData.sGameData.WorldLairInfo.SelfLairInfo == null)
		{
			return zbSelf.Instance.worldHandler.HasWorldRadarNotify();
		}
		if (Extensions.Value<int>((IEnumerable<JToken>)m_worldBase[(object)"lair"][(object)"freeNum"]) + zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.BuyNum - zbServerData.sGameData.WorldLairInfo.WorldLairBaseInfo.FightNum > 0 && zbServerData.sGameData.WorldLairInfo.SelfLairInfo.PassTarget > zbServerData.sGameData.WorldLairInfo.SelfLairInfo.KillCount)
		{
			return true;
		}
		if (m_localDate != zbSelf.Instance.timeHandler.GetServerDateStr())
		{
			return true;
		}
		return false;
	}

	public bool HasRedPoint()
	{
		return HasWorldLairRedPoint();
	}
}
