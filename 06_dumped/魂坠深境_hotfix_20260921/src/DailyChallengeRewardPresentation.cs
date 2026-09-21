using System;

[Serializable]
public sealed class DailyChallengeRewardPresentation
{
	public string type;

	public string assetId;

	public int count;

	[NonSerialized]
	public ICTAsset asset;
}
