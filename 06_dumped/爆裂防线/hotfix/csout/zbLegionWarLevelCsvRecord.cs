using System;

[Serializable]
public class zbLegionWarLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int score { get; set; }

	public int[] awardLow { get; set; }

	public int[] awardMedium { get; set; }

	public int awardHighType { get; set; }

	public float awardHigh { get; set; }

	public int legionScore { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarLevelCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarLevelCsvRecord
		{
			level = r.ReadInt(),
			score = r.ReadInt(),
			awardLow = r.ReadIntArray(),
			awardMedium = r.ReadIntArray(),
			awardHighType = r.ReadInt(),
			awardHigh = r.ReadFloat(),
			legionScore = r.ReadInt()
		};
	}
}
