using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using Newtonsoft.Json.Linq;
using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public class zbLegionHandler
{
	public LegionInfo legionInfo;

	public RepeatedField<LegionBaseInfo> recommendList;

	public RepeatedField<LogDetail> logList;

	public RepeatedField<LegionBaseInfo> searchList;

	public RepeatedField<LegionMemberDetail> memberList;

	public RepeatedField<LegionApplyDetail> legionApplyList;

	public RepeatedField<AwardDetail> AuxDonateAwardList;

	public RepeatedField<AwardDetail> AuxDonateTargetList;

	public long recruitCd;

	private JToken legionBase;

	private Dictionary<int, List<zbLegionBossKillAwardCsvRecord>> m_legionBossKillAwardRecordsDict;

	private int m_legionDinnerUnLockLevel;

	private int m_legionBossUnLockLevel;

	public int selfRank;

	public void Init()
	{
		legionBase = (JToken)(object)zbDataConfig.Instance.legionBaseJsonTable.GetRecord();
		m_legionDinnerUnLockLevel = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(1).legionLvl;
		m_legionBossUnLockLevel = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl;
		zbLegionBossKillAwardCsvTableData records = zbDataConfig.Instance.legionBossKillAwardCsvTable.GetRecords();
		m_legionBossKillAwardRecordsDict = new Dictionary<int, List<zbLegionBossKillAwardCsvRecord>>();
		zbLegionBossKillAwardCsvRecord[] records2 = records.records;
		foreach (zbLegionBossKillAwardCsvRecord zbLegionBossKillAwardCsvRecord2 in records2)
		{
			if (!m_legionBossKillAwardRecordsDict.ContainsKey(zbLegionBossKillAwardCsvRecord2.bossid))
			{
				m_legionBossKillAwardRecordsDict.Add(zbLegionBossKillAwardCsvRecord2.bossid, new List<zbLegionBossKillAwardCsvRecord>());
			}
			m_legionBossKillAwardRecordsDict[zbLegionBossKillAwardCsvRecord2.bossid].Add(zbLegionBossKillAwardCsvRecord2);
		}
	}

	public int GetLegionZone()
	{
		return 1;
	}

	public List<zbItemData> GetLegionBossPauseAwardByKilled(List<zbItemData> zbItemdatas)
	{
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_legionBossKillAwardRecordsDict == null || !m_legionBossKillAwardRecordsDict.ContainsKey(enemyId))
		{
			zbItemdatas.Clear();
			return zbItemdatas;
		}
		List<zbLegionBossKillAwardCsvRecord> list = m_legionBossKillAwardRecordsDict[enemyId];
		Dictionary<string, zbItemData> dictionary = new Dictionary<string, zbItemData>();
		foreach (zbLegionBossKillAwardCsvRecord item in list)
		{
			if ((int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount >= item.killCount)
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
		zbItemdatas.Clear();
		zbItemdatas.AddRange(dictionary.Values);
		return zbItemdatas;
	}

	public int ClaimedRewardCount()
	{
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_legionBossKillAwardRecordsDict == null || !m_legionBossKillAwardRecordsDict.ContainsKey(enemyId))
		{
			return 0;
		}
		List<zbLegionBossKillAwardCsvRecord> list = m_legionBossKillAwardRecordsDict[enemyId];
		int num = 0;
		foreach (zbLegionBossKillAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == item.bossid && (int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount >= item.killCount)
			{
				num++;
			}
		}
		return num;
	}

	public Length KilledProgressToNextReward()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		int enemyId = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId;
		if (m_legionBossKillAwardRecordsDict == null || !m_legionBossKillAwardRecordsDict.ContainsKey(enemyId))
		{
			return new Length(0f, (LengthUnit)1);
		}
		List<zbLegionBossKillAwardCsvRecord> list = m_legionBossKillAwardRecordsDict[enemyId];
		list.Sort((zbLegionBossKillAwardCsvRecord a, zbLegionBossKillAwardCsvRecord b) => a.killCount.CompareTo(b.killCount));
		int num = 0;
		foreach (zbLegionBossKillAwardCsvRecord item in list)
		{
			if (zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId == item.bossid && (int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount <= item.killCount)
			{
				int num2 = item.killCount - num;
				int num3 = (int)zbCombatSystem.Instance.combatData.combatExp.monsterKilledCount - num;
				return new Length(Mathf.Clamp((num2 > 0) ? ((float)num3 / (float)num2 * 100f) : 100f, 0f, 100f), (LengthUnit)1);
			}
			num = item.killCount;
		}
		return new Length(100f, (LengthUnit)1);
	}

	public void GetLegionMembersExceptSelf(List<LegionMemberDetail> _memberList)
	{
		foreach (LegionMemberDetail member in memberList)
		{
			if (member.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
			{
				_memberList.Add(member);
			}
		}
	}

	public JToken GetLegionBase()
	{
		return legionBase;
	}

	public bool HasJoinLegion()
	{
		if (zbServerData.sGameData.LegionSelfInfo != null)
		{
			return zbServerData.sGameData.LegionSelfInfo.LegionId > 0;
		}
		return false;
	}

	public void UpdateLegionData(LegionInfo data)
	{
		legionInfo = data;
		if (data != null)
		{
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Data);
		}
	}

	public void UpdateLegionBaseInfo(LegionBaseInfo _legionBaseInfo)
	{
		if (legionInfo != null)
		{
			legionInfo.LegionBaseInfo = _legionBaseInfo;
			zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Data);
		}
	}

	public void UpdateLegionBossInfo(LegionBossInfo _legionBossInfo)
	{
		if (legionInfo != null)
		{
			legionInfo.LegionBossInfo = _legionBossInfo;
		}
	}

	public void UpdateLegionBossSelfRank(int _selfRank)
	{
		selfRank = _selfRank;
	}

	public void UpdateLegionSetInfo(LegionSetInfo _setBaseInfo)
	{
		legionInfo.LegionSetInfo = _setBaseInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Data);
	}

	public void UpdateLegionRecommend(RepeatedField<LegionBaseInfo> _recommendList)
	{
		recommendList = _recommendList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Recommend);
	}

	public void UpdateLegionLog(RepeatedField<LogDetail> _logList)
	{
		logList = _logList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Log);
	}

	public void UpdateLegionSearchList(RepeatedField<LegionBaseInfo> _searchList)
	{
		searchList = _searchList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Search);
	}

	public void UpdateLegionMembers(RepeatedField<LegionMemberDetail> _memberList)
	{
		memberList = _memberList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Members);
	}

	public void RemoveLegionMember(long PlayerId)
	{
		foreach (LegionMemberDetail member in memberList)
		{
			if (member.RoleBaseInfo.RoleId == PlayerId)
			{
				memberList.Remove(member);
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_Members);
				break;
			}
		}
	}

	public void UpdateLegionApplyList(RepeatedField<LegionApplyDetail> _applyList)
	{
		legionApplyList = _applyList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_ApplyList);
	}

	public void RemoveLegionApply(long PlayerId)
	{
		foreach (LegionApplyDetail legionApply in legionApplyList)
		{
			if (legionApply.RoleBaseInfo.RoleId == PlayerId)
			{
				legionApplyList.Remove(legionApply);
				zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_ApplyList);
				break;
			}
		}
	}

	public void UpdateAuxDonateAwardList(RepeatedField<AwardDetail> _awardList)
	{
		AuxDonateAwardList = _awardList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_AuxDonateAwardList);
	}

	public void UpdateAuxDonateTargetAwardList(RepeatedField<AwardDetail> _awardList)
	{
		AuxDonateTargetList = _awardList;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Legion_AuxDonateTargetList);
	}

	public ELegionOfficial GetOfficialById(int playerId)
	{
		if (legionInfo?.LegionSetInfo != null)
		{
			LegionSetInfo legionSetInfo = legionInfo.LegionSetInfo;
			if (legionSetInfo.LeaderId == playerId)
			{
				return ELegionOfficial.Leader;
			}
			if (((IEnumerable<long>)legionSetInfo.Adviser).ToList().Exists((long e) => e == playerId))
			{
				return ELegionOfficial.Adviser;
			}
			return ELegionOfficial.Member;
		}
		return ELegionOfficial.EmptyValue;
	}

	public string GetOfficialName(ELegionOfficial official)
	{
		return official switch
		{
			ELegionOfficial.Adviser => zbUtil.Language("lang.10830"), 
			ELegionOfficial.Leader => zbUtil.Language("lang.10831"), 
			ELegionOfficial.Member => zbUtil.Language("lang.10832"), 
			_ => zbUtil.Language("lang.10832"), 
		};
	}

	public bool HasDonateBtnRedPoint()
	{
		if (zbSelf.Instance.legionHandler.legionInfo != null)
		{
			int dailyCount = zbServerData.sGameData.LegionSelfInfo.Donate.DailyCount;
			if (Extensions.Value<int>((IEnumerable<JToken>)legionBase[(object)"donate"][(object)"dailyCount"]) - dailyCount > 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}

	public bool CheckCanKickMember()
	{
		LegionWarOpenInfo legionWarOpenInfo = zbServerData.sGameData.LegionWarBaseInfo.LegionWarOpenInfo;
		long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
		long num = legionWarOpenInfo.FightingStartTm - 3600;
		long num2 = legionWarOpenInfo.FightingEndTm + 3600;
		if (serverTime >= num && serverTime <= num2)
		{
			zbUIMgr.Instance.BubbleInfo(zbUtil.Language("lang.11184"));
			return false;
		}
		return true;
	}

	public bool HasDonateRedPoint()
	{
		if (zbSelf.Instance.legionHandler.legionInfo != null)
		{
			LegionSelfInfo legionSelfInfo = zbServerData.sGameData.LegionSelfInfo;
			int weekDonate = legionSelfInfo.Donate.WeekDonate;
			int dailyCount = legionSelfInfo.Donate.DailyCount;
			if (Extensions.Value<int>((IEnumerable<JToken>)legionBase[(object)"donate"][(object)"dailyCount"]) - dailyCount > 0)
			{
				return true;
			}
			RepeatedField<int> weekTarget = legionSelfInfo.Donate.WeekTarget;
			int[] array = new int[4] { 250, 500, 750, 1000 };
			foreach (int num in array)
			{
				if (weekDonate < num)
				{
					continue;
				}
				bool flag = false;
				foreach (int item in weekTarget)
				{
					if (item == num)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	public bool HasCallengeCountRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionBoss))
		{
			return false;
		}
		if (!CheckLegionFuncUnLock(ELegionFuncType.Boss))
		{
			return false;
		}
		if (zbServerData.sGameData.LegionSelfInfo == null || zbServerData.sGameData.LegionSelfInfo.BossInfo == null)
		{
			return false;
		}
		LegionSelfBoss bossInfo = zbServerData.sGameData.LegionSelfInfo.BossInfo;
		return bossInfo.DailyCount - bossInfo.CombatCount + bossInfo.BuyCombat > 0;
	}

	public bool HasCallengeAwardRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.LegionBoss))
		{
			return false;
		}
		LegionInfo legionInfo = zbSelf.Instance.legionHandler.legionInfo;
		if (legionInfo == null || !CheckLegionFuncUnLock(ELegionFuncType.Boss))
		{
			return false;
		}
		int level = legionInfo.LegionBaseInfo.Level;
		int legionLvl = zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(3).legionLvl;
		if (level < legionLvl)
		{
			return false;
		}
		LegionBossInfo legionBossInfo = legionInfo.LegionBossInfo;
		RepeatedField<int> passAward = zbServerData.sGameData.LegionSelfInfo.BossInfo.PassAward;
		int bossId = legionBossInfo.BossId;
		for (int i = 1; i < bossId; i++)
		{
			if (!passAward.Contains(i))
			{
				return true;
			}
		}
		if (legionBossInfo.PassState == EBoolState.True && !passAward.Contains(bossId))
		{
			return true;
		}
		return false;
	}

	public bool HasDinnerRedPoint()
	{
		if (zbSelf.Instance.legionHandler.legionInfo != null && CheckLegionFuncUnLock(ELegionFuncType.Dinner))
		{
			if (zbSelf.Instance.legionHandler.legionInfo.LegionBaseInfo.Level < zbDataConfig.Instance.legionUnlockCsvTable.GetRecordByKey(1).legionLvl)
			{
				return false;
			}
			RepeatedField<LegionSelfDinner> dinnerList = zbServerData.sGameData.LegionSelfInfo.DinnerList;
			long serverTime = zbSelf.Instance.timeHandler.GetServerTime();
			foreach (LegionSelfDinner item in dinnerList)
			{
				if (item.StartTm < serverTime && serverTime < item.EndTm && item.State != EActiveState.Active)
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private bool CheckLegionFuncUnLock(ELegionFuncType legionFuncType)
	{
		bool result = false;
		if (zbSelf.Instance.legionHandler.legionInfo != null)
		{
			switch (legionFuncType)
			{
			case ELegionFuncType.Dinner:
				result = legionInfo.LegionBaseInfo.Level >= m_legionDinnerUnLockLevel;
				break;
			case ELegionFuncType.Boss:
				result = legionInfo.LegionBaseInfo.Level >= m_legionBossUnLockLevel;
				break;
			}
		}
		return result;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.Legion))
		{
			return false;
		}
		if (!HasDonateRedPoint() && !HasCallengeAwardRedPoint() && !HasCallengeCountRedPoint())
		{
			return HasDinnerRedPoint();
		}
		return true;
	}

	public Sprite GetFlagIconById(int id)
	{
		return zbAssetLoader.Instance.LoadResouce<Sprite>("ui/image/icon/legion/" + id + ".png");
	}

	public string GetTodayRemainingCount()
	{
		int num = Extensions.Value<int>((IEnumerable<JToken>)legionBase[(object)"donate"][(object)"dailyCount"]) - zbServerData.sGameData.LegionSelfInfo.Donate.DailyCount;
		return string.Format("{0}/{1}", num, Extensions.Value<int>((IEnumerable<JToken>)legionBase[(object)"donate"][(object)"dailyCount"]));
	}
}
