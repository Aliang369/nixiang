using System;
using System.Collections.Generic;

public static class DailyChallengeSettlementMockFactory
{
	public const string RecordIdPrefix = "mock-settlement-";

	public static readonly IReadOnlyList<DailyChallengeSettlementMockScenario> ShowcaseScenarios = new DailyChallengeSettlementMockScenario[6]
	{
		DailyChallengeSettlementMockScenario.FailedBeforeStage6,
		DailyChallengeSettlementMockScenario.ClearedStage6ThenDiedStage7,
		DailyChallengeSettlementMockScenario.ClearedStage6,
		DailyChallengeSettlementMockScenario.DefeatedStage7FinalBoss,
		DailyChallengeSettlementMockScenario.HighDifficultyClear,
		DailyChallengeSettlementMockScenario.ZeroCompetitionScore
	};

	public static DailyChallengeSettlementMockCase Create(DailyChallengeSettlementMockScenario scenario, ScoreCompetitionRule rule, DifficultyLevel defaultDifficulty)
	{
		DailyChallengeSettlementMockCase dailyChallengeSettlementMockCase = new DailyChallengeSettlementMockCase
		{
			displayName = GetDisplayName(scenario),
			runSuccess = true,
			challengeCompleted = true,
			stage = 6,
			difficulty = defaultDifficulty,
			snapshot = new DailyChallengeSettlementSnapshot
			{
				completionSeconds = 900,
				deckCount = 12,
				blessingCount = 8,
				realHealthDamageTaken = 20,
				finalCoinCount = 40,
				finalCrystalCount = 6,
				remainingWillPower = 7,
				hasMaximumUsedWillPower = true,
				maximumUsedWillPower = 7,
				baseExperience = 1200
			}
		};
		switch (scenario)
		{
		case DailyChallengeSettlementMockScenario.FailedBeforeStage6:
			dailyChallengeSettlementMockCase.runSuccess = false;
			dailyChallengeSettlementMockCase.challengeCompleted = false;
			dailyChallengeSettlementMockCase.stage = 5;
			break;
		case DailyChallengeSettlementMockScenario.ClearedStage6ThenDiedStage7:
			dailyChallengeSettlementMockCase.runSuccess = false;
			dailyChallengeSettlementMockCase.stage = 7;
			break;
		case DailyChallengeSettlementMockScenario.DefeatedStage7FinalBoss:
			dailyChallengeSettlementMockCase.stage = 7;
			dailyChallengeSettlementMockCase.snapshot.defeatedStage7FinalBoss = true;
			break;
		case DailyChallengeSettlementMockScenario.HighDifficultyClear:
			dailyChallengeSettlementMockCase.difficulty = DifficultyLevel.NightmareD3;
			break;
		case DailyChallengeSettlementMockScenario.ZeroCompetitionScore:
			ApplyZeroCompetitionSnapshot(dailyChallengeSettlementMockCase.snapshot, rule);
			break;
		default:
			throw new ArgumentOutOfRangeException("scenario", scenario, null);
		case DailyChallengeSettlementMockScenario.ClearedStage6:
		case DailyChallengeSettlementMockScenario.ContractExpired:
			break;
		}
		return dailyChallengeSettlementMockCase;
	}

	public static void ApplyToRecord(GameRecord record, DailyChallengeSettlementMockScenario scenario, ScoreCompetitionContractDef scoreContract, DifficultyLevel defaultDifficulty)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (scoreContract == null)
		{
			throw new ArgumentNullException("scoreContract");
		}
		DailyChallengeSettlementMockCase dailyChallengeSettlementMockCase = Create(scenario, scoreContract.rule, defaultDifficulty);
		record.success = dailyChallengeSettlementMockCase.runSuccess;
		record.finished = true;
		record.stage = dailyChallengeSettlementMockCase.stage;
		record.skipUnfinishedNewbieClear = !dailyChallengeSettlementMockCase.challengeCompleted;
		record.dailyChallengeSettlementSnapshot = dailyChallengeSettlementMockCase.snapshot;
		if (record.dailyChallengeContext == null)
		{
			record.dailyChallengeContext = new DailyChallengeRunContext();
		}
		record.dailyChallengeContext.isDailyChallenge = true;
		record.dailyChallengeContext.scoreCompetitionContract = scoreContract.ToPresentation();
		if (record.contract != null)
		{
			record.contract.difficultyLevel = dailyChallengeSettlementMockCase.difficulty;
		}
		if (scenario == DailyChallengeSettlementMockScenario.ContractExpired)
		{
			record.dailyChallengeSettlement = CreateExpiredSettlement(scoreContract, dailyChallengeSettlementMockCase.snapshot);
			record.rankScore = 0;
		}
		else
		{
			DailyChallengeMockScoreResult dailyChallengeMockScoreResult = DailyChallengeMockScoreCalculator.Calculate(scoreContract.rule, dailyChallengeSettlementMockCase.snapshot, dailyChallengeSettlementMockCase.difficulty, dailyChallengeSettlementMockCase.challengeCompleted);
			record.dailyChallengeSettlement = CreateServerShapedSettlement(scoreContract, dailyChallengeSettlementMockCase.snapshot, dailyChallengeMockScoreResult);
			record.rankScore = dailyChallengeMockScoreResult.finalScore;
		}
	}

	public static string GetRecordId(DailyChallengeSettlementMockScenario scenario)
	{
		return "mock-settlement-" + scenario;
	}

	public static bool TryGetScenario(string recordId, out DailyChallengeSettlementMockScenario scenario)
	{
		scenario = DailyChallengeSettlementMockScenario.FailedBeforeStage6;
		if (!string.IsNullOrEmpty(recordId) && recordId.StartsWith("mock-settlement-", StringComparison.Ordinal))
		{
			return Enum.TryParse<DailyChallengeSettlementMockScenario>(recordId.Substring("mock-settlement-".Length), out scenario);
		}
		return false;
	}

	public static DailyChallengeSettlement CreateServerShapedSettlement(ScoreCompetitionContractDef scoreContract, DailyChallengeSettlementSnapshot snapshot, DailyChallengeMockScoreResult result)
	{
		if (scoreContract == null || result == null)
		{
			return null;
		}
		return new DailyChallengeSettlement
		{
			status = ((!result.challengeCompleted) ? DailyChallengeSettlementStatus.NotCompleted : DailyChallengeSettlementStatus.Completed),
			reasonCode = (result.challengeCompleted ? "Completed" : "NotCompleted"),
			challengeCompleted = result.challengeCompleted,
			hasChallenged = (result.challengeCompleted && result.finalScore > 0),
			scoreContract = scoreContract.ToPresentation(),
			scoreMetrics = ToScoreMetrics(snapshot),
			baseExperience = result.baseExperience,
			competitionScore = result.competitionScore,
			preMultiplierScore = result.preMultiplierScore,
			finalBossMultiplierNumerator = result.finalBossMultiplierNumerator,
			difficultyMultiplierTenths = result.difficultyMultiplierTenths,
			finalScore = result.finalScore
		};
	}

	public static DailyChallengeSettlement CreateExpiredSettlement(ScoreCompetitionContractDef scoreContract, DailyChallengeSettlementSnapshot snapshot)
	{
		return new DailyChallengeSettlement
		{
			status = DailyChallengeSettlementStatus.ContractExpired,
			reasonCode = "ContractExpired",
			statusText = null,
			challengeCompleted = true,
			hasChallenged = false,
			scoreContract = scoreContract?.ToPresentation(),
			scoreMetrics = ToScoreMetrics(snapshot),
			baseExperience = Math.Max(0, snapshot?.baseExperience ?? 0),
			competitionScore = 0,
			preMultiplierScore = 0,
			finalBossMultiplierNumerator = 2,
			difficultyMultiplierTenths = 10,
			finalScore = 0
		};
	}

	private static DailyChallengeScoreMetrics ToScoreMetrics(DailyChallengeSettlementSnapshot snapshot)
	{
		if (snapshot == null)
		{
			snapshot = new DailyChallengeSettlementSnapshot();
		}
		return new DailyChallengeScoreMetrics
		{
			durationSeconds = snapshot.completionSeconds,
			deckCount = snapshot.deckCount,
			blessingCount = snapshot.blessingCount,
			historicalRealHealthDamage = snapshot.realHealthDamageTaken,
			finalGold = snapshot.finalCoinCount,
			finalSoulCrystal = snapshot.finalCrystalCount,
			finalSanity = snapshot.GetWisdomScoreMetric()
		};
	}

	public static string GetDisplayName(DailyChallengeSettlementMockScenario scenario)
	{
		return scenario switch
		{
			DailyChallengeSettlementMockScenario.FailedBeforeStage6 => "样例·未通过第6层", 
			DailyChallengeSettlementMockScenario.ClearedStage6ThenDiedStage7 => "样例·第6层后阵亡", 
			DailyChallengeSettlementMockScenario.ClearedStage6 => "样例·第6层通关", 
			DailyChallengeSettlementMockScenario.DefeatedStage7FinalBoss => "样例·击败最终Boss", 
			DailyChallengeSettlementMockScenario.HighDifficultyClear => "样例·高难度通关", 
			DailyChallengeSettlementMockScenario.ZeroCompetitionScore => "样例·竞赛项零分", 
			DailyChallengeSettlementMockScenario.ContractExpired => "样例·活动契约已过期", 
			_ => scenario.ToString(), 
		};
	}

	private static void ApplyZeroCompetitionSnapshot(DailyChallengeSettlementSnapshot snapshot, ScoreCompetitionRule rule)
	{
		switch (rule)
		{
		case ScoreCompetitionRule.Speed:
			snapshot.completionSeconds = 4000;
			break;
		case ScoreCompetitionRule.LargeDeck:
			snapshot.deckCount = 0;
			break;
		case ScoreCompetitionRule.SmallDeck:
			snapshot.deckCount = 15;
			break;
		case ScoreCompetitionRule.Blessings:
			snapshot.blessingCount = 0;
			break;
		case ScoreCompetitionRule.Perfect:
			snapshot.realHealthDamageTaken = 200;
			break;
		case ScoreCompetitionRule.Wealth:
			snapshot.finalCoinCount = 0;
			snapshot.finalCrystalCount = 0;
			break;
		case ScoreCompetitionRule.Wisdom:
			snapshot.maximumUsedWillPower = 0;
			break;
		default:
			throw new ArgumentOutOfRangeException("rule", rule, null);
		}
	}
}
