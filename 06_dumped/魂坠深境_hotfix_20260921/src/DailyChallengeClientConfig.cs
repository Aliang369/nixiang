using System;
using System.Collections.Generic;
using System.Linq;
using ContainerOfSoul.Display.Fortress.IndexDisplay;
using UnityEngine;

[CreateAssetMenu(fileName = "每日挑战客户端配置", menuName = "每日挑战/客户端配置")]
public sealed class DailyChallengeClientConfig : ScriptableObject
{
	[Serializable]
	public sealed class LeaderboardRewardTier
	{
		public int maxRank = 1;

		public int rewardAmount;
	}

	public DailyChallengePanel panelPrefab;

	public DailyChallengeEntryView entryPrefab;

	public DailyChallengeTextPanel textPanelPrefab;

	public DailyChallengeRankingRowView rankingRowPrefab;

	public ReocrdInfoPanel recordInfoPanelPrefab;

	public GlobalItemDisplay rewardDisplayPrefab;

	public GameObject gameplayContractCardPrefab;

	public GameObject scoreContractCardPrefab;

	public GameObject settlementSummaryPrefab;

	public GameObject contractDropdownPrefab;

	public GameObject helpPanelPrefab;

	public DailyChallengeEntryLayoutView starMapEntryLayoutPrefab;

	public Sprite constellationPlaceholder;

	public Sprite defaultAvatar;

	public MatterDef completionRewardMatter;

	public int completionRewardAmount = 600;

	public int rewardLimitPerWeek = 3;

	public int leaderboardRowLimit = 10;

	public List<LeaderboardRewardTier> leaderboardRewardTiers = new List<LeaderboardRewardTier>();

	[Range(-12f, 14f)]
	public int utcOffsetHours = 8;

	[Range(0f, 23f)]
	public int dailyResetHour = 4;

	public DayOfWeek weeklyResetDay = DayOfWeek.Monday;

	public DifficultyLevel defaultDifficulty = DifficultyLevel.Normal;

	public List<ContractDef> gameplayContractPool = new List<ContractDef>();

	public List<ScoreCompetitionContractDef> scoreCompetitionContractPool = new List<ScoreCompetitionContractDef>();

	public DailyChallengeMockScenario defaultMockScenario = DailyChallengeMockScenario.SettlementShowcase;

	public string mockChallengeId = "mock-daily-challenge";

	public ContractDef mockGameplayContract;

	public ScoreCompetitionContractDef mockScoreCompetitionContract;

	public ContractDef GetMockGameplayContract()
	{
		return mockGameplayContract;
	}

	public ScoreCompetitionContractDef GetMockScoreContract()
	{
		return mockScoreCompetitionContract;
	}

	public ScoreCompetitionContractDef ResolveScoreContract(string contractId)
	{
		if (string.IsNullOrWhiteSpace(contractId))
		{
			return null;
		}
		ScoreCompetitionContractDef scoreCompetitionContractDef = scoreCompetitionContractPool.FirstOrDefault((ScoreCompetitionContractDef candidate) => candidate != null && string.Equals(candidate.FullID, contractId, StringComparison.Ordinal));
		if (scoreCompetitionContractDef != null && ScriptableObjectSingleton<LocalizationManager>.instance != null && ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization)
		{
			LocalizationManager.LocalizeTarget(scoreCompetitionContractDef, new TraverseLocalizeTermState(TraverseLocalizeTermState.EType.Translate));
		}
		return scoreCompetitionContractDef;
	}

	public string GetLeaderboardRewardText()
	{
		TextDataPreset text = ScriptableObjectSingleton<TextDataPreset>.instance;
		if (text == null)
		{
			return string.Empty;
		}
		string rewardName = text.term_daily_challenge_reward_name;
		return string.Join("\n", from tier in leaderboardRewardTiers
			where tier != null
			orderby tier.maxRank
			select (tier.maxRank != 1) ? text.term_daily_challenge_top_reward_arg0_arg1_arg2.FormatWith(tier.maxRank, rewardName, tier.rewardAmount) : text.term_daily_challenge_first_reward_arg0_arg1.FormatWith(rewardName, tier.rewardAmount));
	}
}
