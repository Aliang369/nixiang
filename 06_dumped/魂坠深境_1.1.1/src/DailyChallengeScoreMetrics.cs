using System;

[Serializable]
public sealed class DailyChallengeScoreMetrics
{
	[SaveField(null, null)]
	public int? durationSeconds;

	[SaveField(null, null)]
	public int? deckCount;

	[SaveField(null, null)]
	public int? blessingCount;

	[SaveField(null, null)]
	public int? historicalRealHealthDamage;

	[SaveField(null, null)]
	public int? finalGold;

	[SaveField(null, null)]
	public int? finalSoulCrystal;

	[SaveField(null, null)]
	public int? finalSanity;
}
