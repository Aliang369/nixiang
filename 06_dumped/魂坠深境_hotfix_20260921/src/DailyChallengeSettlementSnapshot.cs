using System;

[Serializable]
public class DailyChallengeSettlementSnapshot
{
	[SaveField(null, null)]
	public int completionSeconds;

	[SaveField(null, null)]
	public int deckCount;

	[SaveField(null, null)]
	public int blessingCount;

	[SaveField(null, null)]
	public int realHealthDamageTaken;

	[SaveField(null, null)]
	public int finalCoinCount;

	[SaveField(null, null)]
	public int finalCrystalCount;

	[SaveField(null, null)]
	public int remainingWillPower;

	[SaveField(null, null)]
	public bool hasMaximumUsedWillPower;

	[SaveField(null, null)]
	public int maximumUsedWillPower;

	[SaveField(null, null)]
	public int baseExperience;

	[SaveField(null, null)]
	public bool defeatedStage7FinalBoss;

	public int GetWisdomScoreMetric()
	{
		return DailyChallengeWillPowerMetrics.ResolveMaximum(hasMaximumUsedWillPower, maximumUsedWillPower, remainingWillPower);
	}
}
