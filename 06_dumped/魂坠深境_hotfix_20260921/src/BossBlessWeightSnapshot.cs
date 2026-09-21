using System;
using System.Collections.Generic;

[Serializable]
public class BossBlessWeightSnapshot
{
	[SaveField(null, null)]
	public int StatDate;

	[SaveField(null, null)]
	public Dictionary<string, double> Weights;

	public bool HasWeights
	{
		get
		{
			if (Weights != null)
			{
				return Weights.Count > 0;
			}
			return false;
		}
	}

	public BossBlessWeightSnapshot Clone()
	{
		return new BossBlessWeightSnapshot
		{
			StatDate = StatDate,
			Weights = ((Weights == null) ? null : new Dictionary<string, double>(Weights))
		};
	}
}
