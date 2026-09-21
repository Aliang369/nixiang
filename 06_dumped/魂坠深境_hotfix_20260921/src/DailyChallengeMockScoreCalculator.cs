using System;

public static class DailyChallengeMockScoreCalculator
{
	public static DailyChallengeMockScoreResult Calculate(ScoreCompetitionRule rule, DailyChallengeSettlementSnapshot snapshot, DifficultyLevel difficulty, bool challengeCompleted)
	{
		if (snapshot == null)
		{
			snapshot = new DailyChallengeSettlementSnapshot();
		}
		int num = Math.Max(0, snapshot.baseExperience);
		int num2 = (challengeCompleted ? CalculateCompetitionScore(rule, snapshot) : 0);
		long num3 = (long)num + (long)num2;
		DailyChallengeMockScoreResult dailyChallengeMockScoreResult = new DailyChallengeMockScoreResult
		{
			challengeCompleted = challengeCompleted,
			baseExperience = num,
			competitionScore = num2,
			preMultiplierScore = (int)((num3 > int.MaxValue) ? int.MaxValue : num3),
			finalBossMultiplierNumerator = (snapshot.defeatedStage7FinalBoss ? 3 : 2),
			difficultyMultiplierTenths = GetDifficultyMultiplierTenths(difficulty)
		};
		if (!challengeCompleted)
		{
			dailyChallengeMockScoreResult.preMultiplierScore = 0;
			dailyChallengeMockScoreResult.finalScore = 0;
			return dailyChallengeMockScoreResult;
		}
		long num4 = (long)dailyChallengeMockScoreResult.preMultiplierScore * (long)dailyChallengeMockScoreResult.finalBossMultiplierNumerator * dailyChallengeMockScoreResult.difficultyMultiplierTenths / 20;
		dailyChallengeMockScoreResult.finalScore = (int)((num4 > int.MaxValue) ? int.MaxValue : num4);
		return dailyChallengeMockScoreResult;
	}

	public static int GetDifficultyMultiplierTenths(DifficultyLevel difficulty)
	{
		int num = Math.Max(0, (int)(difficulty - 1));
		return checked(10 + num * 2);
	}

	private static int CalculateCompetitionScore(ScoreCompetitionRule rule, DailyChallengeSettlementSnapshot snapshot)
	{
		long num = rule switch
		{
			ScoreCompetitionRule.Speed => Math.Max(3600L - (long)snapshot.completionSeconds, 0L), 
			ScoreCompetitionRule.LargeDeck => (long)snapshot.deckCount * 50L, 
			ScoreCompetitionRule.SmallDeck => Math.Max(3000 - (long)snapshot.deckCount * 200L, 0L), 
			ScoreCompetitionRule.Blessings => (long)snapshot.blessingCount * 150L, 
			ScoreCompetitionRule.Perfect => Math.Max(4000 - (long)snapshot.realHealthDamageTaken * 20L, 0L), 
			ScoreCompetitionRule.Wealth => (long)snapshot.finalCoinCount * 25L + (long)snapshot.finalCrystalCount * 50L, 
			ScoreCompetitionRule.Wisdom => (long)snapshot.GetWisdomScoreMetric() * 300L, 
			_ => throw new ArgumentOutOfRangeException("rule", rule, null), 
		};
		if (num <= int.MaxValue)
		{
			return (int)Math.Max(num, 0L);
		}
		return int.MaxValue;
	}
}
