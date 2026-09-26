using System.Collections.Generic;
using Proto.Enums;
using Proto.Types;
using UnityEngine;

public class zbEndlessHordeHandler
{
	public int maxLevel = 1;

	public int selfRank;

	public RankListAck rankListAck;

	public List<RankDetail> rankDetails = new List<RankDetail>();

	public void Init()
	{
		maxLevel = zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecords().records.Count;
	}

	public void UpdateEndlessHordeInfo(EndlessHordeInfo endlessHordeInfo)
	{
		zbServerData.sGameData.EndlessHordeInfo = endlessHordeInfo;
		zbGameEventMgr.Instance.SendEvent(EGameEvent.Event_Update_EndlessHorde);
	}

	public void UpdateEndlessHordeSelfRank(int rank)
	{
		selfRank = rank;
	}

	public (zbEndlessHordeLevelCsvRecord curRecord, zbEndlessHordeLevelCsvRecord maxRecord, int loopNum) GetRecord(int levelId)
	{
		int num = levelId / maxLevel;
		int num2 = levelId % maxLevel;
		if (num2 == 0)
		{
			num2 = maxLevel;
			num--;
		}
		zbEndlessHordeLevelCsvRecord zbEndlessHordeLevelCsvRecord2 = null;
		zbEndlessHordeLevelCsvRecord2 = zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecordByKey(maxLevel);
		return (curRecord: zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecordByKey(num2), maxRecord: zbEndlessHordeLevelCsvRecord2, loopNum: num);
	}

	public int GetDistance(int levelId)
	{
		var (zbEndlessHordeLevelCsvRecord2, zbEndlessHordeLevelCsvRecord3, num) = GetRecord(levelId);
		return zbEndlessHordeLevelCsvRecord2.distance + zbEndlessHordeLevelCsvRecord3.distance * num;
	}

	public void GetRankValueBySelf(float distance)
	{
		rankDetails.Clear();
		if (rankListAck == null)
		{
			return;
		}
		foreach (RankDetail rank in rankListAck.RankList)
		{
			if (distance > (float)rank.RankData.Value)
			{
				break;
			}
			if (rank.RoleBaseInfo.RoleId != zbSelf.Instance.GetRoleId())
			{
				if (rankDetails.Count < 3)
				{
					rankDetails.Add(rank);
					continue;
				}
				rankDetails.RemoveAt(0);
				rankDetails.Add(rank);
			}
		}
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EndlessHorde))
		{
			return false;
		}
		foreach (KeyValuePair<int, zbZhanLingEndlessCsvRecord> record in zbDataConfig.Instance.zhanLingEndlessCsvTable.GetRecords().records)
		{
			if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.Reach >= record.Value.distance && zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.LowAward < record.Value.distance)
			{
				return true;
			}
			if (zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.Active == EBoolState.True && zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.Reach >= record.Value.distance && zbServerData.sGameData.RechargeActiveInfo.ZhanLingEndlessInfo.HighAward < record.Value.distance)
			{
				return true;
			}
		}
		return zbServerData.sGameData.EndlessHordeInfo.DailyCount > zbServerData.sGameData.EndlessHordeInfo.CombatCount;
	}

	public List<zbItemData> GetEndlessHordePauseAwardByScroll(List<zbItemData> zbItemDatas)
	{
		zbEndlessHordeLevelCsvRecord recordByKey = zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecordByKey(zbCombatSystem.Instance.endlessHordeStartLevel);
		int[][] baseAward = recordByKey.baseAward;
		foreach (int[] array in baseAward)
		{
			zbItemDatas.Add(new zbItemData((EResourceType)array[0], array[1], array[2]));
		}
		float scrollTotalDistance = zbCombatSystem.Instance.scrollTotalDistance;
		if (scrollTotalDistance > 0f)
		{
			float num = Mathf.Min(scrollTotalDistance / (float)(zbCombatSystem.Instance.endlessHordeNextDistance - zbCombatSystem.Instance.endlessHordeCurrDistance), 1f);
			baseAward = recordByKey.growthAward;
			foreach (int[] array2 in baseAward)
			{
				int num2 = (int)(num * (float)array2[2]);
				if (num2 > 0)
				{
					zbItemDatas.Add(new zbItemData((EResourceType)array2[0], array2[1], num2));
				}
			}
			for (int j = 1; j <= 5; j++)
			{
				if (scrollTotalDistance < (float)recordByKey.GetFieldInt("spLvl" + j))
				{
					break;
				}
				baseAward = recordByKey.GetFieldIntArray("spAward" + j);
				foreach (int[] array3 in baseAward)
				{
					zbItemDatas.Add(new zbItemData((EResourceType)array3[0], array3[1], array3[2]));
				}
			}
		}
		int num3 = zbCombatSystem.Instance.battleInfo.BattleBaseInfo.EnemyId - 1;
		for (int k = zbCombatSystem.Instance.endlessHordeStartLevel; k <= num3; k++)
		{
			baseAward = zbDataConfig.Instance.endlessHordeLevelCsvTable.GetRecordByKey(k).passAward;
			foreach (int[] array4 in baseAward)
			{
				zbItemDatas.Add(new zbItemData((EResourceType)array4[0], array4[1], array4[2]));
			}
		}
		return zbItemDatas;
	}
}
