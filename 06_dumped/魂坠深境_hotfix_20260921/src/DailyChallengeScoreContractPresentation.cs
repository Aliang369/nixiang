using System;

[Serializable]
public sealed class DailyChallengeScoreContractPresentation
{
	[SaveField(null, null)]
	public string contractId;

	[SaveField(null, null)]
	public string displayName;

	[SaveField(null, null)]
	public string description;

	[SaveField(null, null)]
	public string scoringMethodText;

	[SaveField(null, null)]
	public string iconKey;

	public static DailyChallengeScoreContractPresentation FromMockAsset(ScoreCompetitionContractDef asset)
	{
		if (asset == null)
		{
			return null;
		}
		return new DailyChallengeScoreContractPresentation
		{
			contractId = asset.FullID,
			displayName = asset.displayName,
			description = asset.description,
			scoringMethodText = asset.scoringMethod
		};
	}
}
