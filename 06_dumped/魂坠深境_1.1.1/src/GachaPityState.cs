public class GachaPityState
{
	[SaveField(null, null)]
	public string groupId;

	[SaveField(null, null)]
	public int totalDrawCount;

	[SaveField(null, null)]
	public int drawsAfterLastSoftPityTarget;

	[SaveField(null, null)]
	public int drawsAfterLastHardPityQualified;

	public bool NeverHitSoftPityTarget => drawsAfterLastSoftPityTarget == totalDrawCount;

	public bool NeverHitHardPityQualified => drawsAfterLastHardPityQualified == totalDrawCount;

	private GachaPityState()
	{
	}

	public GachaPityState(string groupId)
	{
		this.groupId = groupId;
	}
}
