using System;

public class RankingScorePoint
{
	public enum ScroeType
	{
		WinScore,
		DifficultyScore,
		TimeScore
	}

	[SaveField(null, null)]
	public ProtectedInt score;

	[SaveField(null, null)]
	public ScroeType type;

	private const int maxSkillWordCount = 18;

	private RankingScorePoint()
	{
	}

	public RankingScorePoint(ScroeType type, GameRecord record)
	{
		this.type = type;
		score = GetScore(record);
	}

	public string GetDescription(ApplicationStateInfo applicationState, GameRecord record)
	{
		switch (type)
		{
		case ScroeType.WinScore:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_rank_score_source_finish_success;
		case ScroeType.DifficultyScore:
		{
			DifficultyInfo difficultyInfo = record.contract.GetDifficultyInfo(applicationState);
			if (difficultyInfo == null)
			{
				return "";
			}
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_rank_score_source_finish_difficulty_arg0.FormatWith(difficultyInfo.difficultyName);
		}
		case ScroeType.TimeScore:
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_rank_score_source_finish_time_arg0.FormatWith(TextDisplayHelper.GetDetailTimeSpanText(record.gameTime));
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public int GetScore(GameRecord record)
	{
		switch (type)
		{
		case ScroeType.WinScore:
			if (record.success)
			{
				return ScriptableObjectSingleton<FortressDataManager>.instance.winGameRankScore;
			}
			return 0;
		case ScroeType.DifficultyScore:
			if (!record.success)
			{
				return 0;
			}
			return (int)(record.contract.difficultyLevel - 1) * ScriptableObjectSingleton<FortressDataManager>.instance.rankScorePerDifficultyLevel;
		case ScroeType.TimeScore:
		{
			double num = ScriptableObjectSingleton<FortressDataManager>.instance.rankTimeScoreMaxTimeTicks;
			double num2 = ScriptableObjectSingleton<FortressDataManager>.instance.rankTimeScoreMinTimeTicks;
			double num3 = Math.Clamp(record.gameTime, num2, num);
			return (int)((double)ScriptableObjectSingleton<FortressDataManager>.instance.rankTimeScoreSum * (1.0 - (num3 - num2) / (num - num2)));
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}
}
