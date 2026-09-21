using System;

[Serializable]
public class DailyChallengeRunContext
{
	[SaveField(null, null)]
	public string challengeId;

	[SaveField(null, null)]
	public bool isDailyChallenge;

	[SaveField(null, null)]
	public DailyChallengeScoreContractPresentation scoreCompetitionContract;

	[SaveField(null, null)]
	public DateTime rotationResetUtc;

	[SaveField(null, null)]
	public DateTime settlementUtc;
}
