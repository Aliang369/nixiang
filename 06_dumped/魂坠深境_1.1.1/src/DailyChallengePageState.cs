using System;
using System.Collections.Generic;

[Serializable]
public sealed class DailyChallengePageState
{
	public string challengeId;

	public ContractDef gameplayContract;

	public DailyChallengeScoreContractPresentation scoreCompetitionContract;

	public DateTime rotationResetUtc;

	public DateTime settlementUtc;

	public DateTime weeklyResetUtc;

	public int rewardClaimedThisWeek;

	public int rewardLimitPerWeek;

	public bool claimedToday;

	public List<DailyChallengeRankingRow> topRanking = new List<DailyChallengeRankingRow>();

	public DailyChallengeRankingRow playerRanking;

	public string leaderboardRewardText;

	public List<DailyChallengeRewardPresentation> completionRewards = new List<DailyChallengeRewardPresentation>();
}
