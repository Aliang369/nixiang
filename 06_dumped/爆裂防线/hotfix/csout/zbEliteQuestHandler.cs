using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Types;

public class zbEliteQuestHandler
{
	public bool m_firstEliteIn;

	public int fightCount;

	public long playerId;

	public HelpRecordDetail[] eliteQuestHelpRecords;

	public void Init()
	{
	}

	public MainQuestDetail GetEliteQuestDetail(int levelId)
	{
		foreach (MainQuestDetail elite in zbServerData.sGameData.EliteQuestInfo.EliteList)
		{
			if (elite.LevelId == levelId)
			{
				return elite;
			}
		}
		return null;
	}

	public float GetQuestProgress(int level)
	{
		MainQuestDetail eliteQuestDetail = GetEliteQuestDetail(level);
		if (eliteQuestDetail == null)
		{
			return 0f;
		}
		if (eliteQuestDetail.Score == EBattleResultScore.Win2)
		{
			return 0.5f;
		}
		if (eliteQuestDetail.Score != EBattleResultScore.Win3)
		{
			return 0f;
		}
		return 1f;
	}

	public bool CanClaimReward(int level, int boxIndex)
	{
		MainQuestDetail eliteQuestDetail = GetEliteQuestDetail(level);
		if (eliteQuestDetail == null)
		{
			return false;
		}
		if ((int)eliteQuestDetail.Score >= boxIndex)
		{
			return (int)eliteQuestDetail.ScoreAward < boxIndex;
		}
		return false;
	}

	public int FindFirstImperfectLevel()
	{
		foreach (MainQuestDetail elite in zbServerData.sGameData.EliteQuestInfo.EliteList)
		{
			if (elite.Score == EBattleResultScore.Win1 || elite.Score == EBattleResultScore.Win2)
			{
				return elite.LevelId;
			}
		}
		return zbServerData.sGameData.EliteQuestInfo.NextLevel;
	}

	public int GetHelperedNumber(long roleId)
	{
		int num = 0;
		foreach (long helper in zbServerData.sGameData.HelpInfo.HelperList)
		{
			if (helper == roleId)
			{
				num++;
			}
		}
		return num;
	}

	public bool HasRedPoint()
	{
		if (zbSelf.Instance.funHandler.IsLockNoTip(EFuncType.EliteQuest))
		{
			return false;
		}
		if (zbSelf.Instance.dataHandler.HasResourceEnough(EResourceType.Money, 4, 20) && !m_firstEliteIn)
		{
			zbEliteLevelCsvRecord recordByKey = zbDataConfig.Instance.eliteLevelCsvTable.GetRecordByKey(zbServerData.sGameData.EliteQuestInfo.NextLevel);
			if (recordByKey != null && zbServerData.sGameData.MainQuestInfo.MainLevel >= recordByKey.mainLevel)
			{
				return true;
			}
		}
		if (zbSelf.Instance.reminderHandler.HasReminderNotify(EReminderType.HelpAward))
		{
			return true;
		}
		return ((IEnumerable<MainQuestDetail>)zbServerData.sGameData.EliteQuestInfo.EliteList).Any((MainQuestDetail m) => m.Score > EBattleResultScore.Fail && m.Score - m.ScoreAward > 0);
	}
}
