using System;

public static class StoreCardQualitySequence
{
	public static QualityLevel GetQualityAt(int itemIndex)
	{
		if (itemIndex < 0)
		{
			throw new ArgumentOutOfRangeException("itemIndex");
		}
		return (itemIndex % 3) switch
		{
			0 => QualityLevel.Common, 
			1 => QualityLevel.Uncommon, 
			_ => QualityLevel.Rare, 
		};
	}
}
