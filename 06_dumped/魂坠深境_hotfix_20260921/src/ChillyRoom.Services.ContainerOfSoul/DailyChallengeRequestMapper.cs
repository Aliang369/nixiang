namespace ChillyRoom.Services.ContainerOfSoul;

public static class DailyChallengeRequestMapper
{
	public static void ApplyStartBinding(StartGameRequest request, DailyChallengeRunContext context, DifficultyLevel difficulty)
	{
		if (request != null && context != null && context.isDailyChallenge)
		{
			request.ChallengeId = context.challengeId;
			request.Difficulty = difficulty;
		}
	}

	public static DailyChallengeGameRecord ToGameRecord(global::GameRecord record)
	{
		if (record == null || record.dailyChallengeContext?.isDailyChallenge != true)
		{
			return null;
		}
		DailyChallengeSettlementSnapshot dailyChallengeSettlementSnapshot = record.dailyChallengeSettlementSnapshot ?? new DailyChallengeSettlementSnapshot();
		return new DailyChallengeGameRecord
		{
			ChallengeId = record.dailyChallengeContext.challengeId,
			HistoricalRealHealthDamage = dailyChallengeSettlementSnapshot.realHealthDamageTaken,
			FinalGold = dailyChallengeSettlementSnapshot.finalCoinCount,
			FinalSoulCrystal = dailyChallengeSettlementSnapshot.finalCrystalCount,
			FinalSanity = dailyChallengeSettlementSnapshot.GetWisdomScoreMetric()
		};
	}
}
