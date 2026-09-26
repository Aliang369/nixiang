using Google.Protobuf.Collections;
using Proto.Enums;
using Proto.Types;

public class zbMainQuestHandler
{
	public bool shouldShowMainQuestTip = true;

	public bool isLevel2FirstResult;

	public void Init()
	{
	}

	public MainQuestDetail GetMainQuestDetail(int levelId)
	{
		foreach (MainQuestDetail quest in zbServerData.sGameData.MainQuestInfo.QuestList)
		{
			if (quest.LevelId == levelId)
			{
				return quest;
			}
		}
		return null;
	}

	public float GetQuestProgress(int level)
	{
		MainQuestDetail mainQuestDetail = GetMainQuestDetail(level);
		if (mainQuestDetail == null)
		{
			return 0f;
		}
		if (mainQuestDetail.Score == EBattleResultScore.Win2)
		{
			return 0.5f;
		}
		if (mainQuestDetail.Score != EBattleResultScore.Win3)
		{
			return 0f;
		}
		return 1f;
	}

	public bool CanClaimReward(int level, int boxIndex)
	{
		MainQuestDetail mainQuestDetail = GetMainQuestDetail(level);
		if (mainQuestDetail == null)
		{
			return false;
		}
		if ((int)mainQuestDetail.Score >= boxIndex)
		{
			return (int)mainQuestDetail.ScoreAward < boxIndex;
		}
		return false;
	}

	public int FindFirstImperfectLevel()
	{
		foreach (MainQuestDetail quest in zbServerData.sGameData.MainQuestInfo.QuestList)
		{
			if (quest.Score > EBattleResultScore.Fail && (quest.Score == EBattleResultScore.Win1 || quest.Score == EBattleResultScore.Win2))
			{
				return quest.LevelId;
			}
		}
		return zbServerData.sGameData.MainQuestInfo.NextLevel;
	}

	public bool HasRedPoint()
	{
		RepeatedField<MainQuestDetail> questList = zbServerData.sGameData.MainQuestInfo.QuestList;
		if (questList == null)
		{
			return false;
		}
		int count = questList.Count;
		for (int i = 0; i < count; i++)
		{
			MainQuestDetail mainQuestDetail = questList[i];
			if (mainQuestDetail != null && mainQuestDetail.Score > mainQuestDetail.ScoreAward)
			{
				return true;
			}
		}
		return false;
	}

	public bool isPerfectPass(int level, bool isElite = false)
	{
		if (!isElite)
		{
			MainQuestDetail mainQuestDetail = GetMainQuestDetail(level);
			if (mainQuestDetail != null)
			{
				return mainQuestDetail.Score == EBattleResultScore.Win3;
			}
			return false;
		}
		MainQuestDetail eliteQuestDetail = zbSelf.Instance.eliteQuestHandler.GetEliteQuestDetail(level);
		if (eliteQuestDetail != null)
		{
			return eliteQuestDetail.Score == EBattleResultScore.Win3;
		}
		return false;
	}

	public int GetSpineEffectFunc()
	{
		if (zbSelf.Instance.activityHandler.IsFirstRechargeUnActive())
		{
			return 1;
		}
		if (!zbSelf.Instance.vipHandler.ValidateHasWeekCard())
		{
			return 3;
		}
		if (zbServerData.sGameData.RechargeActiveInfo.PassportInfo.BaseInfo.Active == EBoolState.False && !zbSelf.Instance.activityHandler.HasAnyZhanLingMainActive())
		{
			return 4;
		}
		return 5;
	}
}
