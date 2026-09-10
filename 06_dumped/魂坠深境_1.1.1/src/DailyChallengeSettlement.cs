using System;
using System.Collections.Generic;

[Serializable]
public sealed class DailyChallengeSettlement
{
	[SaveField(null, null)]
	public string challengeId;

	[SaveField(null, null)]
	public DailyChallengeSettlementStatus status;

	[SaveField(null, null)]
	public string reasonCode;

	[SaveField(null, null)]
	public string statusText;

	[SaveField(null, null)]
	public bool challengeCompleted;

	[SaveField(null, null)]
	public bool hasChallenged;

	[SaveField(null, null)]
	public string scoreContractId;

	[SaveField(null, null)]
	public DailyChallengeScoreContractPresentation scoreContract;

	[SaveField(null, null)]
	public DailyChallengeScoreMetrics scoreMetrics = new DailyChallengeScoreMetrics();

	[SaveField(null, null)]
	public int baseExperience;

	[SaveField(null, null)]
	public int competitionScore;

	[SaveField(null, null)]
	public int preMultiplierScore;

	[SaveField(null, null)]
	public int finalBossMultiplierNumerator;

	[SaveField(null, null)]
	public int difficultyMultiplierTenths;

	[SaveField(null, null)]
	public int finalScore;

	[SaveField(null, null)]
	public bool completionRewardGranted;

	[SaveField(null, null)]
	public List<GlobalRewardInfo> completionRewardInfos = new List<GlobalRewardInfo>();

	public bool IsExpired => status == DailyChallengeSettlementStatus.ContractExpired;
}
