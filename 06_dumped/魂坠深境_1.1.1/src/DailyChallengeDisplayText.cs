using System.Collections.Generic;

public static class DailyChallengeDisplayText
{
	private static TextDataPreset Text => ScriptableObjectSingleton<TextDataPreset>.instance;

	public static string GetScoreContractBodyText(DailyChallengeScoreContractPresentation contract)
	{
		if (contract == null)
		{
			return "";
		}
		return Text.term_daily_challenge_score_method_arg0_arg1.FormatWith(contract.description, GetTmpSafeText(contract.scoringMethodText));
	}

	public static string GetScoreContractBodyText(ScoreCompetitionContractDef contract)
	{
		return GetScoreContractBodyText(contract?.ToPresentation());
	}

	public static string GetScoreContractText(DailyChallengeScoreContractPresentation contract)
	{
		if (contract == null)
		{
			return "";
		}
		return contract.displayName + "\n" + GetScoreContractBodyText(contract);
	}

	public static string GetScoreContractText(ScoreCompetitionContractDef contract)
	{
		return GetScoreContractText(contract?.ToPresentation());
	}

	public static string GetScoreBreakdownText(DailyChallengeSettlement settlement)
	{
		if (settlement == null)
		{
			return "";
		}
		return Text.term_daily_challenge_score_breakdown_arg0_arg1_arg2_arg3_arg4_arg5.FormatWith(settlement.baseExperience, settlement.competitionScore, settlement.preMultiplierScore, GetBossMultiplierText(settlement), GetDifficultyMultiplierText(settlement), settlement.finalScore);
	}

	public static string GetSettlementTitleText(DailyChallengeScoreContractPresentation contract)
	{
		return Text.term_daily_challenge_settlement_title_arg0.FormatWith(contract?.displayName ?? Text.term_daily_challenge_score_contract_fallback);
	}

	public static string GetGameOverSettlementText(DailyChallengeSettlement settlement)
	{
		if (settlement == null)
		{
			return "";
		}
		if (settlement.IsExpired)
		{
			return GetSettlementStatusText(settlement);
		}
		DailyChallengeScoreContractPresentation scoreContract = settlement.scoreContract;
		List<string> list = new List<string>();
		list.Add(Text.term_daily_challenge_target_arg0.FormatWith(scoreContract?.description ?? "--"));
		list.Add(Text.term_daily_challenge_rule_arg0.FormatWith(GetTmpSafeText(scoreContract?.scoringMethodText) ?? "--"));
		list.Add(Text.term_daily_challenge_calculation_arg0.FormatWith(GetCompetitionScoreCalculationText(scoreContract, settlement)));
		List<string> list2 = list;
		string appliedMultiplierText = GetAppliedMultiplierText(settlement);
		if (!string.IsNullOrEmpty(appliedMultiplierText))
		{
			list2.Add(Text.term_daily_challenge_bonus_arg0.FormatWith(appliedMultiplierText));
		}
		list2.Add(Text.term_daily_challenge_final_score_arg0.FormatWith(settlement.finalScore));
		return string.Join("\n", list2);
	}

	public static string GetAppliedMultiplierText(DailyChallengeSettlement settlement)
	{
		if (settlement == null)
		{
			return "";
		}
		List<string> list = new List<string>();
		if (settlement.finalBossMultiplierNumerator > 2)
		{
			list.Add(Text.term_daily_challenge_boss_bonus_arg0.FormatWith(GetBossMultiplierText(settlement)));
		}
		if (settlement.difficultyMultiplierTenths > 10)
		{
			list.Add(Text.term_daily_challenge_difficulty_bonus_arg0.FormatWith(GetDifficultyMultiplierText(settlement)));
		}
		return string.Join(" · ", list);
	}

	public static string GetSettlementStatusText(DailyChallengeSettlement settlement)
	{
		if (settlement == null)
		{
			return "";
		}
		return settlement.status switch
		{
			DailyChallengeSettlementStatus.Completed => Text.term_daily_challenge_completed, 
			DailyChallengeSettlementStatus.NotCompleted => Text.term_daily_challenge_not_completed, 
			DailyChallengeSettlementStatus.ContractExpired => Text.term_daily_challenge_contract_expired, 
			_ => Text.term_daily_challenge_settlement, 
		};
	}

	public static string GetBossMultiplierText(DailyChallengeSettlement settlement)
	{
		if (settlement == null || settlement.finalBossMultiplierNumerator != 3)
		{
			return "1.0";
		}
		return "1.5";
	}

	public static string GetDifficultyMultiplierText(DailyChallengeSettlement settlement)
	{
		if (settlement != null)
		{
			return ((float)settlement.difficultyMultiplierTenths / 10f).ToString("0.0");
		}
		return "1.0";
	}

	public static string GetCompetitionScoreCalculationText(DailyChallengeScoreContractPresentation contract, DailyChallengeSettlement settlement)
	{
		if (settlement == null)
		{
			return "";
		}
		if (!settlement.challengeCompleted)
		{
			return Text.term_daily_challenge_failed_score;
		}
		DailyChallengeScoreMetrics dailyChallengeScoreMetrics = settlement.scoreMetrics ?? new DailyChallengeScoreMetrics();
		return contract?.contractId switch
		{
			"#SCORECONTRA#000001#" => $"max{{3600 - {dailyChallengeScoreMetrics.durationSeconds.GetValueOrDefault()}, 0}} = {settlement.competitionScore}", 
			"#SCORECONTRA#000002#" => $"{dailyChallengeScoreMetrics.deckCount.GetValueOrDefault()} x 50 = {settlement.competitionScore}", 
			"#SCORECONTRA#000003#" => $"max{{3000 - {dailyChallengeScoreMetrics.deckCount.GetValueOrDefault()} x 200, 0}} = {settlement.competitionScore}", 
			"#SCORECONTRA#000004#" => $"{dailyChallengeScoreMetrics.blessingCount.GetValueOrDefault()} x 150 = {settlement.competitionScore}", 
			"#SCORECONTRA#000005#" => $"max{{4000 - {dailyChallengeScoreMetrics.historicalRealHealthDamage.GetValueOrDefault()} x 20, 0}} = {settlement.competitionScore}", 
			"#SCORECONTRA#000006#" => $"{dailyChallengeScoreMetrics.finalGold.GetValueOrDefault()} x 25 + {dailyChallengeScoreMetrics.finalSoulCrystal.GetValueOrDefault()} x 50 = {settlement.competitionScore}", 
			"#SCORECONTRA#000007#" => $"{dailyChallengeScoreMetrics.finalSanity.GetValueOrDefault()} x 300 = {settlement.competitionScore}", 
			_ => settlement.competitionScore.ToString(), 
		};
	}

	public static string GetScoreBreakdownText(DailyChallengeMockScoreResult result)
	{
		if (result == null)
		{
			return "";
		}
		string text = ((result.finalBossMultiplierNumerator == 3) ? "1.5" : "1.0");
		string text2 = ((float)result.difficultyMultiplierTenths / 10f).ToString("0.0");
		return Text.term_daily_challenge_score_breakdown_arg0_arg1_arg2_arg3_arg4_arg5.FormatWith(result.baseExperience, result.competitionScore, result.preMultiplierScore, text, text2, result.finalScore);
	}

	public static string GetGameOverSettlementText(ScoreCompetitionContractDef contract, DailyChallengeSettlementSnapshot snapshot, DailyChallengeMockScoreResult result)
	{
		if (result == null)
		{
			return "";
		}
		string text = contract?.description ?? "--";
		string text2 = ((contract == null) ? "--" : GetTmpSafeText(contract.scoringMethod));
		string text3 = (result.challengeCompleted ? GetCompetitionScoreCalculationText(contract, snapshot, result) : Text.term_daily_challenge_failed_score);
		List<string> list = new List<string>();
		list.Add(Text.term_daily_challenge_target_arg0.FormatWith(text));
		list.Add(Text.term_daily_challenge_rule_arg0.FormatWith(text2));
		list.Add(Text.term_daily_challenge_calculation_arg0.FormatWith(text3));
		List<string> list2 = list;
		string appliedMultiplierText = GetAppliedMultiplierText(result);
		if (!string.IsNullOrEmpty(appliedMultiplierText))
		{
			list2.Add(Text.term_daily_challenge_bonus_arg0.FormatWith(appliedMultiplierText));
		}
		list2.Add(Text.term_daily_challenge_final_score_arg0.FormatWith(result.finalScore));
		return string.Join("\n", list2);
	}

	public static string GetChallengeCompletionText(DailyChallengeMockScoreResult result)
	{
		if (result == null || !result.challengeCompleted)
		{
			return Text.term_daily_challenge_not_completed;
		}
		return Text.term_daily_challenge_completed;
	}

	public static string GetCompetitionScoreCalculationText(ScoreCompetitionContractDef contract, DailyChallengeSettlementSnapshot snapshot, DailyChallengeMockScoreResult result)
	{
		if (result == null)
		{
			return "";
		}
		if (contract == null || snapshot == null)
		{
			return result.competitionScore.ToString();
		}
		return contract.rule switch
		{
			ScoreCompetitionRule.Speed => $"max{{3600 - {snapshot.completionSeconds}, 0}} = {result.competitionScore}", 
			ScoreCompetitionRule.LargeDeck => $"{snapshot.deckCount} x 50 = {result.competitionScore}", 
			ScoreCompetitionRule.SmallDeck => $"max{{3000 - {snapshot.deckCount} x 200, 0}} = {result.competitionScore}", 
			ScoreCompetitionRule.Blessings => $"{snapshot.blessingCount} x 150 = {result.competitionScore}", 
			ScoreCompetitionRule.Perfect => $"max{{4000 - {snapshot.realHealthDamageTaken} x 20, 0}} = {result.competitionScore}", 
			ScoreCompetitionRule.Wealth => $"{snapshot.finalCoinCount} x 25 + {snapshot.finalCrystalCount} x 50 = {result.competitionScore}", 
			ScoreCompetitionRule.Wisdom => $"{snapshot.GetWisdomScoreMetric()} x 300 = {result.competitionScore}", 
			_ => result.competitionScore.ToString(), 
		};
	}

	public static string GetBossMultiplierText(DailyChallengeMockScoreResult result)
	{
		if (result == null || result.finalBossMultiplierNumerator != 3)
		{
			return "1.0";
		}
		return "1.5";
	}

	public static string GetDifficultyMultiplierText(DailyChallengeMockScoreResult result)
	{
		if (result != null)
		{
			return ((float)result.difficultyMultiplierTenths / 10f).ToString("0.0");
		}
		return "1.0";
	}

	public static string GetAppliedMultiplierText(DailyChallengeMockScoreResult result)
	{
		if (result == null)
		{
			return "";
		}
		List<string> list = new List<string>();
		if (result.finalBossMultiplierNumerator > 2)
		{
			list.Add(Text.term_daily_challenge_boss_bonus_arg0.FormatWith(GetBossMultiplierText(result)));
		}
		if (result.difficultyMultiplierTenths > 10)
		{
			list.Add(Text.term_daily_challenge_difficulty_bonus_arg0.FormatWith(GetDifficultyMultiplierText(result)));
		}
		return string.Join(" · ", list);
	}

	public static string GetFinalScoreFormulaText(DailyChallengeMockScoreResult result, bool challengeCompleted)
	{
		if (result == null)
		{
			return "";
		}
		if (!challengeCompleted)
		{
			return Text.term_daily_challenge_failed_final_score;
		}
		return Text.term_daily_challenge_final_formula_arg0_arg1_arg2_arg3.FormatWith(result.baseExperience, result.competitionScore, GetBossMultiplierText(result), GetDifficultyMultiplierText(result));
	}

	public static string GetTmpSafeText(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			return text.Replace('×', 'x');
		}
		return "";
	}
}
