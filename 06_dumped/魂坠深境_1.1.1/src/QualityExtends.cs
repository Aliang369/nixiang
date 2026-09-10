public static class QualityExtends
{
	public static int QualityToLevel(this QualityLevel qualityLevel)
	{
		return qualityLevel switch
		{
			QualityLevel.Base => 2, 
			QualityLevel.Common => 3, 
			QualityLevel.Uncommon => 4, 
			QualityLevel.Rare => 5, 
			_ => 6, 
		};
	}
}
