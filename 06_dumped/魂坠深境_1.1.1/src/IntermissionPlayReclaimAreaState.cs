public sealed class IntermissionPlayReclaimAreaState
{
	public int AvailableCount { get; }

	public bool CanReclaim { get; }

	public IntermissionPlayReclaimAreaState(int availableCount, bool canReclaim)
	{
		AvailableCount = availableCount;
		CanReclaim = canReclaim;
	}
}
