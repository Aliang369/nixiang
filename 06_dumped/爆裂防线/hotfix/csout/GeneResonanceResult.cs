using System.Collections.Generic;

public struct GeneResonanceResult
{
	public int MaxCount;

	public int MaxPotentialCount;

	public List<int> ResonanceEdges;

	public List<int> OpenResonanceEdges;

	public List<int> ResonanceSlotIndices;

	public List<int> FilledResonanceSlots;

	public List<int> EmptyResonanceSlots;
}
